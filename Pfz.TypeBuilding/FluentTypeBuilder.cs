using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Pfz.TypeBuilding;

/// <summary>
/// This class with the FluentMethodBuider are the most important classes of this
/// namespace. With this class you can create entire new types at run-time using
/// easy to understand methods like AddMethod, AddProperty and AddEvent.
/// </summary>
/// <typeparam name="TBase">The type from which the new type will inherit. If you don't require any special type, use object.</typeparam>
public class FluentTypeBuilder<TBase> : _IFluentTypeBuilder
{
    private static readonly Expression<Func<TBase>> _thisVariable;

    static FluentTypeBuilder()
    {
        if (!typeof(TBase).IsVisible)
        {
            throw new InvalidOperationException(
                "The FluentTypeBuilder can only implement externally visible types."
            );
        }

        if (typeof(TBase).IsSealed)
        {
            throw new InvalidOperationException(
                "The FluentTypeBuilder can't implement new types based on sealed ones."
            );
        }

        _thisVariable = () => This;
    }

    /// <summary>
    /// Use this field when building your expressions if you want the generated code to access
    /// the "this" instance.
    /// This is published as a public field on purpose. as it is required to build the right expression.
    /// </summary>
    public static readonly TBase? This;

    /// <summary>
    /// Gets an already built expression that access the "this" instance when
    /// a method is compiled.
    /// </summary>
    public static Expression<Func<TBase>> ThisExpression
    {
        get { return _thisVariable; }
    }

    private readonly int _creatorThreadId;
    private readonly TypeBuilder _type;
    private readonly FieldBuilder _delegatesField;
    private Delegate[]? _delegates;
    private readonly Expression<Func<Delegate[]>> _delegatesExpression;
    private readonly Type[] _interfaceTypes;
    private readonly bool _isCollectible;

    /// <summary>
    /// Creates a new fluent type builder using the name of the TBase as the
    /// name of the generated type and optionally telling which interfaces
    /// should be implemented.
    /// </summary>
    public FluentTypeBuilder(params Type[] interfaceTypes)
        : this(typeof(TBase).Name, interfaceTypes) { }

    /// <summary>
    /// Creates a new fluent type builder using the given name
    /// and optionally telling which interfaces should be implemented.
    /// </summary>
    public FluentTypeBuilder(string name, params Type[] interfaceTypes)
        : this(name, true, interfaceTypes) { }

    /// <summary>
    /// Creates a new fluent type builder using the given name, telling if the
    /// generated type will be collectible (if you use the other overloads,
    /// this value is true) and optionally telling which interfaces should be
    /// implemented.
    /// </summary>
    public FluentTypeBuilder(string name, bool isCollectible, params Type[] interfaceTypes)
    {
        _delegatesExpression = () => _delegates;
        _creatorThreadId = Thread.CurrentThread.ManagedThreadId;
        _isCollectible = isCollectible;

        Type baseType;
        HashSet<Type> allInterfaces = new HashSet<Type>();
        if (typeof(TBase).IsInterface)
        {
            baseType = typeof(object);
            allInterfaces.Add(typeof(TBase));
        }
        else
        {
            baseType = typeof(TBase);
        }

        foreach (var interfaceType in typeof(TBase).GetInterfaces())
        {
            allInterfaces.Add(interfaceType);
        }

        if (interfaceTypes != null)
        {
            foreach (var interfaceType in interfaceTypes)
            {
                if (interfaceType == null)
                {
                    throw new ArgumentException(
                        "interfaceTypes can't contain null values.",
                        "interfaceTypes"
                    );
                }

                if (!interfaceType.IsInterface)
                {
                    throw new ArgumentException(
                        "interfaceTypes can't contain non-interface types.",
                        "interfaceTypes"
                    );
                }

                if (!interfaceType.IsVisible)
                {
                    throw new ArgumentException(
                        "interfaceTypes must only contain visible interfaces (public interfaces declared directly in namespaces or inside other public types).",
                        "interfaceTypes"
                    );
                }

                allInterfaces.Add(interfaceType);
                foreach (var baseInterface in interfaceType.GetInterfaces())
                {
                    allInterfaces.Add(baseInterface);
                }
            }
        }

        AssemblyBuilderAccess access = AssemblyBuilderAccess.Run;
        if (isCollectible)
        {
            access = AssemblyBuilderAccess.RunAndCollect;
        }

        _interfaceTypes = allInterfaces.ToArray();
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), access);
        var module = assembly.DefineDynamicModule(name);

        _type = module.DefineType(name, TypeAttributes.NotPublic, baseType, _interfaceTypes);
        _type.DefineDefaultConstructor(MethodAttributes.Public);
        _delegatesField = _type.DefineField(
            ".delegates",
            typeof(Delegate[]),
            FieldAttributes.Private | FieldAttributes.Static
        );
    }

    private List<FluentMethodBuilder> _methods = new List<FluentMethodBuilder>();

    private static readonly MethodInfo _addMethod = typeof(FluentTypeBuilder<TBase>).GetMethod(
        "AddMethod",
        new Type[] { typeof(string), typeof(bool), typeof(bool) }
    );

    /// <summary>
    /// Adds a new method to this type.
    /// </summary>
    public FluentMethodBuilder AddMethod(
        Type returnType,
        string name,
        bool isStatic = false,
        bool respectVisibility = false
    )
    {
        if (returnType == null)
        {
            throw new ArgumentNullException("returnType");
        }

        if (returnType == typeof(void))
        {
            return AddVoidMethod(name, isStatic, respectVisibility);
        }

        var method = _addMethod.MakeGenericMethod(returnType);
        var result = method.Invoke(this, new object[] { name, isStatic, respectVisibility });
        return (FluentMethodBuilder)result;
    }

    /// <summary>
    /// Adds a new method that has a return type of void to this method.
    /// </summary>
    public FluentVoidMethodBuilder AddVoidMethod(
        string name,
        bool isStatic = false,
        bool respectVisibility = false
    )
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }

        _CheckUncompiled();

        var result = new FluentVoidMethodBuilder(
            this,
            name,
            isStatic,
            _creatorThreadId,
            _methods.Count,
            respectVisibility
        );
        result.AddExternal(_delegatesExpression);

        if (!isStatic)
        {
            result.AddParameter(_thisVariable);
        }

        _methods.Add(result);
        return result;
    }

    /// <summary>
    /// Adds a method to this type.
    /// </summary>
    /// <typeparam name="T">The type of the result this method will generate.</typeparam>
    public FluentMethodBuilder<T> AddMethod<T>(
        string name,
        bool isStatic = false,
        bool respectVisibility = false
    )
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }

        _CheckUncompiled();

        var result = new FluentMethodBuilder<T>(
            this,
            name,
            isStatic,
            _creatorThreadId,
            _methods.Count,
            respectVisibility
        );
        result.AddExternal(_delegatesExpression);

        if (!isStatic)
        {
            result.AddParameter(_thisVariable);
        }

        _methods.Add(result);
        return result;
    }

    private Type? _compiledType;

    /// <summary>
    /// Compiles the type that is being built and returns its run-time Type.
    /// If you want to create instances directly, see the GetConstructorDelegate.
    /// </summary>
    public Type? Compile()
    {
        if (_compiledType == null)
        {
            _CheckThread();

            _delegates = new Delegate[_methods.Count];

            var methodBuilders = new Dictionary<string, MethodBuilder>();
            int index = -1;
            foreach (var method in _methods)
            {
                index++;
                var methodBuilder = _Compile(method, index);
                methodBuilders[method.Name] = methodBuilder;
            }

            foreach (var property in _properties.Values)
            {
                string name = property.Name;
                var propertyBuilder = _type.DefineProperty(
                    name,
                    PropertyAttributes.None,
                    property.PropertyType,
                    Type.EmptyTypes
                );
                propertyBuilder.SetGetMethod(methodBuilders["get_" + name]);
                propertyBuilder.SetSetMethod(methodBuilders["set_" + name]);
            }

            foreach (var eventInfo in _events.Values)
            {
                string name = eventInfo.Name;
                var eventBuilder = _type.DefineEvent(name, EventAttributes.None, eventInfo.Type);

                string addName = "add_" + name;
                string removeName = "remove_" + name;

                var addMethod = methodBuilders[addName];
                var removeMethod = methodBuilders[removeName];
                eventBuilder.SetAddOnMethod(addMethod);
                eventBuilder.SetRemoveOnMethod(removeMethod);

                foreach (var interfaceType in _interfaceTypes)
                {
                    var method = interfaceType.GetMethod(addName);
                    if (method != null)
                    {
                        _type.DefineMethodOverride(addMethod, method);
                    }

                    method = interfaceType.GetMethod(removeName);
                    if (method != null)
                    {
                        _type.DefineMethodOverride(removeMethod, method);
                    }
                }
            }

            var compiledType = _type.CreateType();

            var thisExpression = Expression.Parameter(compiledType, "this");
            var fields = _fields.ToArray();
            foreach (var fieldPair in fields)
            {
                var leftPair = fieldPair.Key;
                var rightPair = fieldPair.Value;
                string fieldName = rightPair.Value;
                var expression = Expression.MakeMemberAccess(
                    thisExpression,
                    compiledType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                );
                _fields[leftPair] = new KeyValuePair<MemberExpression, string>(
                    expression,
                    fieldName
                );
            }

            index = -1;
            foreach (var method in _methods)
            {
                index++;

                if (method._respectVisibility)
                {
                    continue;
                }

                if (!method.IsStatic)
                {
                    var firstParameter = method._parameters.Keys.First();
                    method._parameters[firstParameter] = thisExpression;
                }

                var compiledMethod = method._Compile(null, null);
                _delegates[index] = compiledMethod;
            }

            var delegatesField = compiledType.GetField(
                ".delegates",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
            );
            delegatesField!.SetValue(null, _delegates);

            _compiledType = compiledType;
        }

        return _compiledType;
    }

    private Func<TBase>? _constructorDelegate;

    /// <summary>
    /// Compiles the type and returns a delegate capable of creating new instances of that type
    /// without the performance overhead of reflection.
    /// </summary>
    public Func<TBase>? GetConstructorDelegate()
    {
        if (_constructorDelegate == null)
        {
            var type = Compile();
            var newExpression = Expression.New(type);
            Expression<Func<TBase>?> lambda = Expression.Lambda<Func<TBase>>(newExpression);
            _constructorDelegate = lambda.Compile();
        }

        return _constructorDelegate;
    }

    private MethodBuilder _Compile(FluentMethodBuilder method, int index)
    {
        var compiledMethodType = method._Precompile();

        MethodAttributes methodAttributes = MethodAttributes.Public;
        if (method.IsStatic)
        {
            methodAttributes |= MethodAttributes.Static;
        }
        else
        {
            methodAttributes |= MethodAttributes.Virtual;
        }

        // the originalParameterTypes don't include a Box<> or ByRef because
        // they come from a field.
        Type[] originalParameterTypes;

        // the parameterTypesForDelegate may have Box<> types.
        Type[] parameterTypesForDelegate;

        // the parameterTypesForGeneratedMethod will replace Box<> parameter by ref parameters.
        Type[] parameterTypesForGeneratedMethod;

        int parameterCount = method._parameters.Count;
        int parameterIndex = -1;
        using (var enumerator = method._parameters.GetEnumerator())
        {
            if (!method.IsStatic)
            {
                parameterCount--;
                enumerator.MoveNext();
            }

            originalParameterTypes = new Type[parameterCount];
            parameterTypesForDelegate = new Type[parameterCount];
            parameterTypesForGeneratedMethod = new Type[parameterCount];

            while (enumerator.MoveNext())
            {
                parameterIndex++;
                var pair = enumerator.Current;
                var fieldInfoAndSourcePair = pair.Key;
                var parameterExpression = pair.Value;

                var fieldType = fieldInfoAndSourcePair.Key.FieldType;
                var parameterType = parameterExpression.Type;
                originalParameterTypes[parameterIndex] = fieldType;
                parameterTypesForDelegate[parameterIndex] = parameterType;

                var parameterTypeForGeneratedMethod = fieldType;
                if (parameterExpression.IsByRef || fieldType != parameterType)
                {
                    parameterTypeForGeneratedMethod = fieldType.MakeByRefType();
                }

                parameterTypesForGeneratedMethod[parameterIndex] = parameterTypeForGeneratedMethod;
            }
        }

        var methodBuilder = _type.DefineMethod(
            method.Name,
            methodAttributes,
            method.ReturnType,
            parameterTypesForGeneratedMethod
        );

        foreach (var interfaceType in _interfaceTypes)
        {
            var methodToOverride = interfaceType.GetMethod(method.Name, originalParameterTypes);
            if (methodToOverride != null)
            {
                _type.DefineMethodOverride(methodBuilder, methodToOverride);
            }
        }

        var methodGenerator = methodBuilder.GetILGenerator();

        int count = originalParameterTypes.Length;
        if (!method.IsStatic)
        {
            count++;
        }

        if (method._respectVisibility)
        {
            if (method.IsStatic)
            {
                method._Compile(null, methodBuilder);
            }
            else
            {
                var allParameterTypes = new Type[parameterTypesForGeneratedMethod.Length + 1];
                allParameterTypes[0] = _type;
                parameterTypesForGeneratedMethod.CopyTo(allParameterTypes, 1);

                var otherMethod = _type.DefineMethod(
                    "." + method._delegateIndex,
                    MethodAttributes.Static,
                    method.ReturnType,
                    allParameterTypes
                );
                method._Compile(null, otherMethod);
                _EmitDirectCall(methodBuilder, otherMethod, allParameterTypes.Length);
            }
        }
        else
        {
            methodGenerator.Emit(OpCodes.Ldsfld, _delegatesField);
            methodGenerator.Emit(OpCodes.Ldc_I4, index);
            methodGenerator.Emit(OpCodes.Ldelem, compiledMethodType);

            var locals = new List<LocalBuilder>();
            _GeneratePreCall(
                method,
                originalParameterTypes,
                methodGenerator,
                count,
                locals,
                parameterTypesForDelegate
            );

            methodGenerator.Emit(OpCodes.Callvirt, compiledMethodType.GetMethod("Invoke"));

            if (locals.Count > 0)
            {
                _GeneratePosCall(
                    method,
                    originalParameterTypes,
                    methodGenerator,
                    count,
                    locals,
                    parameterTypesForDelegate
                );
            }

            methodGenerator.Emit(OpCodes.Ret);
        }

        return methodBuilder;
    }

    private void _EmitDirectCall(
        MethodBuilder method,
        MethodBuilder otherMethod,
        int parameterCount
    )
    {
        var generator = method.GetILGenerator();
        for (int i = 0; i < parameterCount; i++)
        {
            generator.Emit(OpCodes.Ldarg, i);
        }

        generator.Emit(OpCodes.Call, otherMethod);
        generator.Emit(OpCodes.Ret);
    }

    private static void _GeneratePreCall(
        FluentMethodBuilder method,
        Type[] originalParameterTypes,
        ILGenerator methodGenerator,
        int count,
        List<LocalBuilder> locals,
        Type[] parameterTypesForDelegate
    )
    {
        for (int i = 0; i < count; i++)
        {
            int realIndex;
            if (method.IsStatic)
            {
                realIndex = i;
            }
            else
            {
                realIndex = i - 1;
            }

            if (realIndex >= 0)
            {
                var originalParameterType = originalParameterTypes[realIndex];
                var parameterType = parameterTypesForDelegate[realIndex];
                if (
                    originalParameterType != parameterType
                    && parameterType.IsGenericType
                    && parameterType.GetGenericTypeDefinition() == typeof(FluentMethodBuilder.Box<>)
                )
                {
                    var local = methodGenerator.DeclareLocal(parameterType);
                    locals.Add(local);
                    methodGenerator.Emit(
                        OpCodes.Newobj,
                        parameterType.GetConstructor(Type.EmptyTypes)
                    );
                    methodGenerator.Emit(OpCodes.Stloc, local);
                    methodGenerator.Emit(OpCodes.Ldloc, local);
                    methodGenerator.Emit(OpCodes.Ldarg, i);
                    methodGenerator.Emit(OpCodes.Ldobj, originalParameterType);
                    methodGenerator.Emit(OpCodes.Stfld, parameterType.GetField("Value"));
                    methodGenerator.Emit(OpCodes.Ldloc, local);
                }
                else
                {
                    methodGenerator.Emit(OpCodes.Ldarg, i);
                }
            }
            else
            {
                methodGenerator.Emit(OpCodes.Ldarg_0);
            }
        }
    }

    private static void _GeneratePosCall(
        FluentMethodBuilder method,
        Type[] originalParameterTypes,
        ILGenerator methodGenerator,
        int count,
        List<LocalBuilder> locals,
        Type[] parameterTypesForDelegate
    )
    {
        int localIndex = -1;
        for (int i = 0; i < count; i++)
        {
            int realIndex;
            if (method.IsStatic)
            {
                realIndex = i;
            }
            else
            {
                realIndex = i - 1;
            }

            if (realIndex >= 0)
            {
                var originalParameterType = originalParameterTypes[realIndex];
                var parameterType = parameterTypesForDelegate[realIndex];
                if (
                    originalParameterType != parameterType
                    && parameterType.IsGenericType
                    && parameterType.GetGenericTypeDefinition() == typeof(FluentMethodBuilder.Box<>)
                )
                {
                    localIndex++;
                    var local = locals[localIndex];
                    methodGenerator.Emit(OpCodes.Ldarg, i);

                    methodGenerator.Emit(OpCodes.Ldloc, local);
                    methodGenerator.Emit(OpCodes.Ldfld, parameterType.GetField("Value"));

                    methodGenerator.Emit(OpCodes.Stobj, originalParameterType);
                }
            }
        }
    }

    internal readonly Dictionary<
        KeyValuePair<FieldInfo, object>,
        KeyValuePair<MemberExpression, string>
    > _fields =
        new Dictionary<KeyValuePair<FieldInfo, object>, KeyValuePair<MemberExpression, string>>();

    /// <summary>
    /// Adds a new field to this type. You should give an expression that access a local
    /// variable, as this will be marked as the local variable that represents the field
    /// in all other calls.
    /// </summary>
    public void AddField<T>(string fieldName, Expression<Func<T>> expression)
    {
        if (fieldName == null)
        {
            throw new ArgumentNullException("fieldName");
        }

        if (expression == null)
        {
            throw new ArgumentNullException("expression");
        }

        var body = expression.Body;
        if (body.NodeType != ExpressionType.MemberAccess)
        {
            throw new ArgumentException(
                "A call to AddField must access a local variable of your code to work properly."
            );
        }

        _CheckUncompiled();

        MemberExpression memberExpression = (MemberExpression)body;
        MemberInfo member = memberExpression.Member;
        if (member.MemberType != MemberTypes.Field)
        {
            throw new ArgumentException(
                "A call to AddField must access a local variable of your code to work properly."
            );
        }

        object source = null;
        Expression untypedSource = memberExpression.Expression;
        if (untypedSource != null)
        {
            if (untypedSource.NodeType != ExpressionType.Constant)
            {
                throw new ArgumentException(
                    "A call to AddField must access a local variable of your code to work properly."
                );
            }

            var sourceExpression = (ConstantExpression)untypedSource;
            source = sourceExpression.Value;
        }

        var field = (FieldInfo)member;
        object fieldValue = field.GetValue(source);
        if (!EqualityComparer<T>.Default.Equals((T)fieldValue, default(T)))
        {
            throw new ArgumentException(
                "The local variable used to declare a field must have its default value at this moment."
            );
        }

        _type.DefineField(fieldName, field.FieldType, FieldAttributes.Private);
        _fields.Add(
            new KeyValuePair<FieldInfo, object>(field, source),
            new KeyValuePair<MemberExpression, string>(null, fieldName)
        );
    }

    private Dictionary<string, FluentPropertyBuilder> _properties =
        new Dictionary<string, FluentPropertyBuilder>();

    /// <summary>
    /// Adds a new property to the type.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="valueParameterExpression">
    /// An expression that access a local variable of the calling method that
    /// will be seen as the "value" variable passed to the set method.
    /// </param>
    /// <param name="isStatic">Will the generated method be static?</param>
    /// <param name="respectVisibility">When the respectVisibility is true the generated code avoids a virtual call, but it will be unable to access internal or private members.</param>
    public FluentPropertyBuilder AddProperty<T>(
        string name,
        Expression<Func<T>> valueParameterExpression,
        bool isStatic = false,
        bool respectVisibility = false
    )
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }

        if (valueParameterExpression == null)
        {
            throw new ArgumentNullException("valueParameterExpression");
        }

        _CheckUncompiled();

        var result = new FluentPropertyBuilder(this, name, typeof(T), isStatic, respectVisibility);
        result.SetMethod.AddParameter(valueParameterExpression);
        result.GetMethod.MakeParametersReadOnly();
        result.SetMethod.MakeParametersReadOnly();
        _properties.Add(name, result);
        return result;
    }

    /// <summary>
    /// Creates a property of the given type that has direct get and set implementations.
    /// </summary>
    public void AddPropertyWithDefaultImplementation(string name, Type type)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }

        if (type == null)
        {
            throw new ArgumentNullException("type");
        }

        var method = _addPropertyWithDefaultImplementation.MakeGenericMethod(type);
        method.Invoke(this, new object[] { name });
    }

    private static readonly MethodInfo _addPropertyWithDefaultImplementation =
        typeof(FluentTypeBuilder<TBase>).GetMethod(
            "AddPropertyWithDefaultImplementation",
            new Type[] { typeof(string) }
        );

    /// <summary>
    /// Creates a property of type T that has direct get and set implementations.
    /// </summary>
    public void AddPropertyWithDefaultImplementation<T>(string name)
    {
        T value = default(T);
        var property = AddProperty<T>(name, () => value);

        T field = default(T);
        AddField("<backingField>." + name, () => field);

        property.GetMethod.Body.Return(() => field).EndBody();

        property.SetMethod.Body.Assign(() => field, () => value).EndBody();
    }

    private void _CheckThread()
    {
        if (Thread.CurrentThread.ManagedThreadId != _creatorThreadId)
        {
            throw new InvalidOperationException(
                "Only the thread that created this type builder can invoke this method."
            );
        }
    }

    private void _CheckUncompiled()
    {
        _CheckThread();

        if (_compiledType != null)
        {
            throw new InvalidOperationException(
                "The actual type was already compiled and can't be changed anymore."
            );
        }
    }

    private Dictionary<string, FluentEventBuilder> _events =
        new Dictionary<string, FluentEventBuilder>();

    /// <summary>
    /// Adds an event to the type.
    /// </summary>
    /// <param name="name">The name of the generated event.</param>
    /// <param name="valueParameterExpression">
    /// An expression that access a local variable of the calling method that
    /// will be seen as the "value" variable passed to the add and remove methods.
    /// </param>
    /// <param name="isStatic">Will the generated event be static?</param>
    /// <param name="respectVisibility">When the respectVisibility is true the generated code avoids a virtual call, but it will be unable to access internal or private members.</param>
    public FluentEventBuilder AddEvent<T>(
        string name,
        Expression<Func<T>> valueParameterExpression,
        bool isStatic = false,
        bool respectVisibility = false
    )
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }

        if (valueParameterExpression == null)
        {
            throw new ArgumentNullException("valueParameterExpression");
        }

        _CheckUncompiled();

        var result = new FluentEventBuilder(this, name, typeof(T), isStatic, respectVisibility);
        result.AddMethod.AddParameter(valueParameterExpression);
        result.RemoveMethod.AddParameter(valueParameterExpression);
        result.AddMethod.MakeParametersReadOnly();
        result.RemoveMethod.MakeParametersReadOnly();
        _events.Add(name, result);
        return result;
    }

    private static readonly MethodInfo _delegateCombineMethod = typeof(Delegate).GetMethod(
        "Combine",
        BindingFlags.Public | BindingFlags.Static,
        null,
        new Type[] { typeof(Delegate), typeof(Delegate) },
        null
    );

    private static readonly MethodInfo _delegateRemoveMethod = typeof(Delegate).GetMethod(
        "Remove",
        BindingFlags.Public | BindingFlags.Static,
        null,
        new Type[] { typeof(Delegate), typeof(Delegate) },
        null
    );

    /// <summary>
    /// Adds a new event that will have the default add/remove behavior.
    /// </summary>
    /// <param name="name">The name of the generated event.</param>
    /// <param name="handlerFieldExpression">
    /// An expression that access a local variable of the calling method.
    /// Such variable should be used when generating the expressions that
    /// will invoke the event.
    /// </param>
    /// <param name="isStatic">Will the generated method be static?</param>
    public void AddEventWithDefaultImplementation<T>(
        string name,
        Expression<Func<T>> handlerFieldExpression,
        bool isStatic = false
    ) where T : class
    {
        T valueParameter = null;
        Expression<Func<T>> valueExpression = () => valueParameter;
        var eventBuilder = AddEvent<T>(name, valueExpression, isStatic);
        eventBuilder.AddMethod.Body
            .Assign(
                handlerFieldExpression,
                Expression.Lambda<Func<T>>(
                    Expression.Convert(
                        Expression.Call(
                            _delegateCombineMethod,
                            handlerFieldExpression.Body,
                            valueExpression.Body
                        ),
                        typeof(T)
                    )
                )
            )
            .EndBody();

        eventBuilder.RemoveMethod.Body
            .Assign(
                handlerFieldExpression,
                Expression.Lambda<Func<T>>(
                    Expression.Convert(
                        Expression.Call(
                            _delegateRemoveMethod,
                            handlerFieldExpression.Body,
                            valueExpression.Body
                        ),
                        typeof(T)
                    )
                )
            )
            .EndBody();
    }

    Dictionary<
        KeyValuePair<FieldInfo, object>,
        KeyValuePair<MemberExpression, string>
    > _IFluentTypeBuilder.Fields
    {
        get { return _fields; }
    }

    Expression _IFluentTypeBuilder.ThisExpression
    {
        get { return ThisExpression.Body; }
    }

    Expression _IFluentTypeBuilder.DelegatesExpression
    {
        get { return _delegatesExpression.Body; }
    }

    bool _IFluentTypeBuilder.IsCollectible
    {
        get { return _isCollectible; }
    }
}
