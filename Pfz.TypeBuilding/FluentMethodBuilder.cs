using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Reflection;
using System.Data;
using System.Threading;
using LinqCompileToInstanceMethod;
using Pfz.TypeBuilding.Blocks;

namespace Pfz.TypeBuilding;

/// <summary>
/// Instances of this class are capable of creating methods at run-time using a
/// fluent interface combined with the .NET expressions that is very
/// easy to use.
/// </summary>
public class FluentMethodBuilder
{
    /// <summary>
    /// A class that is used to "box" values so they can be passed as
    /// references even if the LINQ expressions don't accept that.
    /// You should not use this class directly.
    /// </summary>
    public sealed class Box<T>
    {
        /// <summary>
        /// This is the value that is "boxed". Note that this is a public field
        /// on purpose, as this keeps field accesses as field accesses when
        /// generating dynamic code.
        /// </summary>
        public T Value = default(T)!;
    }

    internal readonly int _creatorThreadId;
    internal readonly _Visitor _visitor;
    internal readonly LabelTarget _returnTarget;
    internal int _delegateIndex;
    internal bool _modifiable = true;
    internal bool _respectVisibility;

    internal FluentMethodBuilder(
        _IFluentTypeBuilder typeBuilder,
        string name,
        Type returnType,
        bool isStatic,
        int creatorThreadId,
        int delegateIndex,
        bool respectVisibility
    )
    {
        _creatorThreadId = creatorThreadId;
        _visitor = new _Visitor(this);
        _returnTarget = Expression.Label(returnType);
        TypeBuilder = typeBuilder;
        _body = new FluentBodyBuilder(this);
        Name = name;
        ReturnType = returnType;
        IsStatic = isStatic;
        _delegateIndex = delegateIndex;
        _respectVisibility = respectVisibility;
    }

    /// <summary>
    /// Creates a new method builder.
    /// </summary>
    /// <param name="returnType">The type of the return of the method that will be generated.</param>
    public FluentMethodBuilder(Type returnType)
    {
        if (returnType == null)
        {
            throw new ArgumentNullException("returnType");
        }

        _delegateIndex = -1;
        _creatorThreadId = Thread.CurrentThread.ManagedThreadId;
        _visitor = new _Visitor(this);
        _returnTarget = Expression.Label(returnType);
        _body = new FluentBodyBuilder(this);
        ReturnType = returnType;
        IsStatic = true;
    }

    /// <summary>
    /// Creates a new FluentMethodBuilder that has the interface of an Action.
    /// That is: It does not receive any parameters and does not return values.
    /// </summary>
    public static FluentMethodBuilder CreateAction()
    {
        var result = new FluentMethodBuilder(typeof(void));
        result.MakeParametersReadOnly();
        return result;
    }

    /// <summary>
    /// Creates a new FluentMethodBuilder based on the given action expression.
    /// </summary>
    public static FluentMethodBuilder CreateAction(Expression<Action> expression)
    {
        if (expression == null)
        {
            throw new ArgumentNullException("expression");
        }

        var result = CreateAction();
        result.Body.Do(expression);
        return result;
    }

    internal _IFluentTypeBuilder? TypeBuilder { get; private set; }

    /// <summary>
    /// Gets the Name of the method that is being built.
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    /// Gets the ReturnType of the method that is being built.
    /// </summary>
    public Type ReturnType { get; private set; }

    /// <summary>
    /// Gets a value indicating if the generated method will be static.
    /// </summary>
    public bool IsStatic { get; private set; }

    private readonly FluentBodyBuilder _body;

    /// <summary>
    /// Gets the Body block of the method, which is the entry point to
    /// built it.
    /// </summary>
    public FluentBodyBuilder Body
    {
        get { return _body; }
    }

    internal readonly Dictionary<KeyValuePair<FieldInfo, object>, ParameterExpression> _parameters =
        new Dictionary<KeyValuePair<FieldInfo, object>, ParameterExpression>();

    /// <summary>
    /// Adds a new parameter to this method.
    /// </summary>
    /// <param name="expression">
    /// An expression that access a local variable that represents the parameter to be added.
    /// All other references to such local variable will be compiled as an access
    /// to the parameter.
    /// </param>
    /// <param name="kind">
    /// Determines if the parameter is input only or if it can be used as output too.
    /// </param>
#pragma warning disable MA0051 // Method is too long
    public FluentMethodBuilder AddParameter<T>(
#pragma warning restore MA0051 // Method is too long
        Expression<Func<T>> expression,
        FluentParameterKind kind = FluentParameterKind.Input
    )
    {
        if (expression == null)
        {
            throw new ArgumentNullException("expression");
        }

        if (_areParametersReadOnly)
        {
            throw new ReadOnlyException("New parameters can't be added to this method.");
        }

        var body = expression.Body;
        if (body.NodeType != ExpressionType.MemberAccess)
        {
            throw new ArgumentException(
                "A call to AddParameter must access a local variable of your code to work properly."
            );
        }

        MemberExpression memberExpression = (MemberExpression)body;
        MemberInfo member = memberExpression.Member;
        if (member.MemberType != MemberTypes.Field)
        {
            throw new ArgumentException(
                "A call to AddParameter must access a local variable of your code to work properly."
            );
        }

        Expression untypedSource = memberExpression.Expression;
        if (untypedSource != null && untypedSource.NodeType != ExpressionType.Constant)
        {
            throw new ArgumentException(
                "A call to AddParameter must access a local variable of your code to work properly."
            );
        }

        if (kind != FluentParameterKind.Input && TypeBuilder == null)
        {
            throw new ArgumentException(
                "A FluentMethodBuilder that's not bound to a FluentMethodBuilder can't have a Kind different than Input."
            );
        }

        _CheckModifiable();

        object source = null;
        if (untypedSource != null)
        {
            var sourceExpression = (ConstantExpression)untypedSource;
            source = sourceExpression.Value;
        }

        var field = (FieldInfo)member;
        var fieldType = field.FieldType;

        switch (kind)
        {
            case FluentParameterKind.Input:
                break;

            case FluentParameterKind.InputAndOutput:
                // for some reason, a ref parameter causes an exception when compiling the
                // expression telling that a non-collectible assembly can't reference
                // a collectible one, so we use a Box on such situation.

                if (_respectVisibility || TypeBuilder?.IsCollectible == false)
                {
                    fieldType = fieldType.MakeByRefType();
                }
                else
                {
                    fieldType = typeof(Box<>).MakeGenericType(fieldType);
                }

                break;

            default:
                throw new ArgumentException("Invalid kind.", "kind");
        }

        var parameterExpression = Expression.Parameter(fieldType, field.Name);

        object fieldValue = field.GetValue(source);
        if (!EqualityComparer<T>.Default.Equals((T)fieldValue, default(T)))
        {
            throw new ArgumentException(
                "The local variable used to declare a parameter variable must have its default value at this moment."
            );
        }

        _parameters.Add(new KeyValuePair<FieldInfo, object>(field, source), parameterExpression);

        return this;
    }

    internal readonly Dictionary<KeyValuePair<FieldInfo, object>, ParameterExpression> _locals =
        new Dictionary<KeyValuePair<FieldInfo, object>, ParameterExpression>();

    /// <summary>
    /// Adds a new local to this method.
    /// </summary>
    /// <param name="expression">
    /// An expression that access a local variable (of the generator method) that
    /// represents the local variable that will exist on the generated method.
    /// All other references to such local variable will be compiled as an access
    /// to a real local variable on the generated method, without sharing the
    /// variable of the generator method.
    /// </param>
    public FluentMethodBuilder AddLocal<T>(Expression<Func<T>> expression)
    {
        if (expression == null)
        {
            throw new ArgumentNullException("expression");
        }

        var body = expression.Body;
        if (body.NodeType != ExpressionType.MemberAccess)
        {
            throw new ArgumentException(
                "A call to AddVariable must access a local variable of your code to work properly."
            );
        }

        MemberExpression memberExpression = (MemberExpression)body;
        MemberInfo member = memberExpression.Member;
        if (member.MemberType != MemberTypes.Field)
        {
            throw new ArgumentException(
                "A call to AddVariable must access a local variable of your code to work properly."
            );
        }

        Expression untypedSource = memberExpression.Expression;
        if (untypedSource?.NodeType != ExpressionType.Constant)
        {
            throw new ArgumentException(
                "A call to AddVariable must access a local variable of your code to work properly."
            );
        }

        _CheckModifiable();

        var sourceExpression = (ConstantExpression)untypedSource;
        var field = (FieldInfo)member;
        var variableExpression = Expression.Variable(field.FieldType, field.Name);
        var source = sourceExpression.Value;

        object fieldValue = field.GetValue(source);
        if (!EqualityComparer<T>.Default.Equals((T)fieldValue, default(T)))
        {
            throw new ArgumentException(
                "The local variable used to declare a method variable must have its default value at this moment."
            );
        }

        _locals.Add(new KeyValuePair<FieldInfo, object>(field, source), variableExpression);

        return this;
    }

    internal readonly HashSet<KeyValuePair<FieldInfo, object>> _sharedVariables =
        new HashSet<KeyValuePair<FieldInfo, object>>();

    /// <summary>
    /// Allows an external variable (including locals of the generator method) to be accessed by
    /// the method that will be generated.
    /// </summary>
    /// <param name="expression">
    /// An expression that access a variable that will be acessible to the
    /// generated method.
    /// </param>
    public FluentMethodBuilder AddExternal<T>(Expression<Func<T>> expression)
    {
        if (expression == null)
        {
            throw new ArgumentNullException("expression");
        }

        var body = expression.Body;
        if (body.NodeType != ExpressionType.MemberAccess)
        {
            throw new ArgumentException(
                "A call to AddExternal must access a local variable of your code to work properly."
            );
        }

        MemberExpression memberExpression = (MemberExpression)body;
        MemberInfo member = memberExpression.Member;
        if (member.MemberType != MemberTypes.Field)
        {
            throw new ArgumentException(
                "A call to AddExternal must access a local variable of your code to work properly."
            );
        }

        Expression untypedSource = memberExpression.Expression;
        if (untypedSource?.NodeType != ExpressionType.Constant)
        {
            throw new ArgumentException(
                "A call to AddExternal must access a local variable of your code to work properly."
            );
        }

        _CheckModifiable();

        var sourceExpression = (ConstantExpression)untypedSource;
        var field = (FieldInfo)member;
        var source = sourceExpression.Value;

        _sharedVariables.Add(new KeyValuePair<FieldInfo, object>(field, source));

        return this;
    }

    private void _CheckModifiable()
    {
        _CheckThread();

        if (!_modifiable)
        {
            throw new ReadOnlyException(
                "This method builder was already compiled and can't be modified anymore."
            );
        }
    }

    private static void _DoNothing() { }

    private static readonly Expression<Action> _doNothing = () => _DoNothing();

    internal Type _Precompile()
    {
        _CheckThread();
        _modifiable = false;

        ParameterExpression[] parameterExpressions = null;
        int countParameters = _parameters.Count;
        if (countParameters > 0)
        {
            parameterExpressions = _parameters.Values.ToArray();
        }

        Expression resultExpression;

        if (ReturnType == typeof(void))
        {
            resultExpression = _doNothing.Body;
        }
        else
        {
            resultExpression = Expression.Default(ReturnType);
        }

        Expression lambdaExpression = Expression.Lambda(resultExpression, parameterExpressions);
        return lambdaExpression.Type;
    }

    /// <summary>
    /// Compiles this method. You can optionally tell to which
    /// delegate type it should be compiled.
    /// </summary>
    public Delegate Compile(Type? delegateType = null)
    {
        if (TypeBuilder != null)
        {
            throw new InvalidOperationException(
                "You can only call Compile() on FluentMethodBuilders that aren't bound to a type-builder."
            );
        }

        return _Compile(delegateType, null);
    }

    internal Delegate _Compile(Type? delegateType, MethodBuilder? compileToMethod)
    {
        if (delegateType != null && !delegateType.IsSubclassOf(typeof(Delegate)))
        {
            throw new ArgumentException(
                delegateType.FullName + " is not a delegate type.",
                "delegateType"
            );
        }

        _CheckThread();

        _modifiable = false;
        ParameterExpression[] parameterExpressions = null;
        int countParameters = _parameters.Count;
        if (countParameters > 0)
        {
            parameterExpressions = _parameters.Values.ToArray();
        }

        var body = _body._CompileToExpression();
        body = _visitor.Visit(body);

        LambdaExpression lambda;

        if (delegateType != null)
        {
            lambda = Expression.Lambda(delegateType, body, parameterExpressions);
        }
        else
        {
            lambda = Expression.Lambda(body, parameterExpressions);
        }

        if (compileToMethod != null)
        {
            compileToMethod.
            lambda.CompileToMethod(compileToMethod);
            return null;
        }

        Delegate result = lambda.Compile();
        return result;
    }

    internal void _CheckThread()
    {
        if (Thread.CurrentThread.ManagedThreadId != _creatorThreadId)
        {
            throw new InvalidOperationException(
                "Only the thread that created this method builder can invoke this method."
            );
        }
    }

    /// <summary>
    /// Compiles this method to an specific delegate type.
    /// </summary>
    public T Compile<T>()
    {
        object result = Compile(typeof(T));
        return (T)result;
    }

    private bool _areParametersReadOnly;

    /// <summary>
    /// After a call to this method the parameters will not be modifiable anymore.
    /// This is useful if you create the "method signature" but want to let the
    /// implementation to be done by some other code.
    /// </summary>
    public void MakeParametersReadOnly()
    {
        _CheckThread();
        _areParametersReadOnly = true;
    }
}

/// <summary>
/// Typed version of the FluentMethodBuilder.
/// The T is the type of results this method will generate.
/// </summary>
public sealed class FluentMethodBuilder<T> : FluentMethodBuilder
{
    /// <summary>
    /// Creates a new type builder instance.
    /// </summary>
    public FluentMethodBuilder() : base(typeof(T)) { }

    internal FluentMethodBuilder(
        _IFluentTypeBuilder typeBuilder,
        string name,
        bool isStatic,
        int creatorThreadId,
        int delegateIndex,
        bool respectVisibility
    )
        : base(
            typeBuilder,
            name,
            typeof(T),
            isStatic,
            creatorThreadId,
            delegateIndex,
            respectVisibility
        )
    {
    }

    /// <summary>
    /// This method is here only to make it possible to generate expressions that call it.
    /// If you invoke it directly an InvalidOperationException will be thrown.
    /// </summary>
    public T Call(params object[] parameters)
    {
        throw new InvalidOperationException(
            "This method is here only to build expressions that do a call to other methods that are being built."
        );
    }
}
