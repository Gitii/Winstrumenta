using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Data;

namespace Pfz.TypeBuilding.Blocks;

/// <summary>
/// This is the base class used by "block builders".
/// For example, the Body of a FluentMethodBuilder is the main block, while
/// Ifs, Loops and the like are sub-blocks.
/// </summary>
public class FluentBlockBuilder<TParentBuilder, TThis> : _IBlockBuilder, _ICompilable
{
    private readonly TThis _this;
    internal readonly List<_ICompilable> _statements = new List<_ICompilable>();
    internal readonly TParentBuilder _parent;
    internal FluentMethodBuilder _method;
    private bool _readOnly;

    internal FluentBlockBuilder(TParentBuilder parent, FluentMethodBuilder method)
    {
        _this = (TThis)((object)this);
        _parent = parent;
        _method = method;
    }

    internal TParentBuilder _EndBlock()
    {
        _CheckModifiable();

        _readOnly = true;
        return _parent;
    }

    private void _CheckThread()
    {
        _method._CheckThread();
    }

    internal void _CheckModifiable()
    {
        _CheckThread();

        if (_readOnly)
        {
            throw new ReadOnlyException(
                "The actual block builder is read-only. This happens when you end the block or when the method that uses this block is compiled."
            );
        }
    }

    /// <summary>
    /// Creates an If block that will be executed if the condition evaluates to true.
    /// </summary>
    public FluentIfBuilder<TThis> If(Expression<Func<bool>> conditionExpression)
    {
        if (conditionExpression == null)
        {
            throw new ArgumentNullException("conditionExpression");
        }

        _CheckModifiable();
        var result = new FluentIfBuilder<TThis>(_this, _method, conditionExpression.Body);
        _statements.Add(result);
        return result;
    }

    /// <summary>
    /// Creates an If block that will be executed if the condition evaluates to true.
    /// </summary>
    public FluentIfBuilder<TThis> If(FluentExpression conditionExpression)
    {
        if (conditionExpression == null)
        {
            throw new ArgumentNullException("conditionExpression");
        }

        _CheckModifiable();
        var result = new FluentIfBuilder<TThis>(_this, _method, conditionExpression.Expression);
        _statements.Add(result);
        return result;
    }

    /// <summary>
    /// Creates a Using block (that is, a block that will dispose the disposable
    /// object at the end, independent if there is an exception or not).
    /// </summary>
    /// <param name="usingVariable">
    /// An expression that represents the variable that will be set during
    /// the initialization and finalization of the block.
    /// </param>
    /// <param name="usingValue">The value that will be set to such variable.</param>
    public FluentUsingBuilder<TThis> Using<T1, T2>(
        Expression<Func<T1>> usingVariable,
        Expression<Func<T2>> usingValue
    )
        where T1 : IDisposable
        where T2 : T1
    {
        if (usingVariable == null)
        {
            throw new ArgumentNullException("usingVariable");
        }

        if (usingVariable == null)
        {
            throw new ArgumentNullException("usingValue");
        }

        _CheckModifiable();

        var result = new FluentUsingBuilder<TThis>(
            _this,
            _method,
            usingVariable.Body,
            usingValue.Body
        );
        _statements.Add(result);
        return result;
    }

    /// <summary>
    /// Adds a Return statement to the method. The method should be void
    /// returning to use this overload.
    /// </summary>
    public TThis Return()
    {
        _CheckModifiable();

        var returnExpression = Expression.Return(_method._returnTarget);
        _statements.Add(new _Expression(returnExpression));
        return _this;
    }

    /// <summary>
    /// Adds a Return statement to the method, which will return the
    /// value that will result from the given expression.
    /// </summary>
    public TThis Return<T>(Expression<Func<T>> expression)
    {
        _CheckModifiable();

        var returnExpression = Expression.Return(_method._returnTarget, expression.Body);
        _statements.Add(new _Expression(returnExpression));
        return _this;
    }

    /// <summary>
    /// Simply adds the given expression to the body of the method.
    /// </summary>
    public TThis Do(Expression<Action> expression)
    {
        if (expression == null)
        {
            throw new ArgumentNullException("expression");
        }

        _CheckModifiable();

        _statements.Add(new _Expression(expression.Body));
        return _this;
    }

    /// <summary>
    /// Adds an assignment. The C# compiler does not let things like: x = y inside a expression,
    /// so you should call this method and give two expressions, one representing the variable
    /// that will be set and the other representing the value.
    /// </summary>
    public TThis Assign<T1, T2>(
        Expression<Func<T1>> leftExpression,
        Expression<Func<T2>> rightExpression
    ) where T2 : T1
    {
        if (leftExpression == null)
        {
            throw new ArgumentNullException("leftExpression");
        }

        if (rightExpression == null)
        {
            throw new ArgumentNullException("rightExpression");
        }

        _CheckModifiable();

        var assignExpression = Expression.Assign(leftExpression.Body, rightExpression.Body);
        _statements.Add(new _Expression(assignExpression));
        return _this;
    }

    /// <summary>
    /// Adds a Throw clause into this block.
    /// </summary>
    public TThis Throw(Expression<Func<Exception>> expression)
    {
        if (expression == null)
        {
            throw new ArgumentNullException("expression");
        }

        _CheckModifiable();

        var throwExpression = Expression.Throw(expression.Body);
        var statement = new _Expression(throwExpression);
        _statements.Add(statement);
        return _this;
    }

    /// <summary>
    /// Adds a Rethrow clause into this block. This should only be used if the
    /// actual block is a catch or is inside one.
    /// </summary>
    public TThis Rethrow()
    {
        _CheckModifiable();

        var rethrowExpression = Expression.Rethrow();
        var statement = new _Expression(rethrowExpression);
        _statements.Add(statement);
        return _this;
    }

    /// <summary>
    /// Initiates a Try/Catch/Finally operation.
    /// </summary>
    public FluentTryBuilder<TThis> Try()
    {
        _CheckModifiable();

        var result = new FluentTryBuilder<TThis>(_this, _method);
        _statements.Add(result);
        return result;
    }

    /// <summary>
    /// Starts a Loop block.
    /// </summary>
    public FluentLoopBuilder<TThis> Loop()
    {
        _CheckModifiable();

        var result = new FluentLoopBuilder<TThis>(_this, _method);
        _statements.Add(result);
        return result;
    }

    /// <summary>
    /// Starts a While block that will execute while the result of
    /// the conditionExpression is true.
    /// </summary>
    public FluentWhileBuilder<TThis> While(Expression<Func<bool>> conditionExpression)
    {
        if (conditionExpression == null)
        {
            throw new ArgumentNullException("conditionExpression");
        }

        _CheckModifiable();

        var result = new FluentWhileBuilder<TThis>(_this, _method, conditionExpression.Body);
        _statements.Add(result);
        return result;
    }

    /// <summary>
    /// Starts a While block that will execute while the result of
    /// the conditionExpression is true.
    /// </summary>
    public FluentWhileBuilder<TThis> While(FluentExpression conditionExpression)
    {
        if (conditionExpression == null)
        {
            throw new ArgumentNullException("conditionExpression");
        }

        _CheckModifiable();

        var result = new FluentWhileBuilder<TThis>(_this, _method, conditionExpression);
        _statements.Add(result);
        return result;
    }

    /// <summary>
    /// Starts a For loop.
    /// </summary>
    public FluentForBuilder<TThis> For<T>(
        Expression<Func<T>> initialization,
        Expression<Func<bool>> condition,
        Expression<Func<T>> increment
    )
    {
        return _For(initialization.Body, condition.Body, increment.Body);
    }

    /// <summary>
    /// Starts a For loop.
    /// </summary>
    public FluentForBuilder<TThis> For<T>(
        FluentExpression initialization,
        Expression<Func<bool>> condition,
        Expression<Func<T>> increment
    )
    {
        return _For(initialization, condition.Body, increment.Body);
    }

    /// <summary>
    /// Starts a For loop.
    /// </summary>
    public FluentForBuilder<TThis> For<T>(
        Expression<Func<T>> initialization,
        FluentExpression condition,
        Expression<Func<T>> increment
    )
    {
        return _For(initialization.Body, condition, increment.Body);
    }

    /// <summary>
    /// Starts a For loop.
    /// </summary>
    public FluentForBuilder<TThis> For<T>(
        Expression<Func<T>> initialization,
        Expression<Func<bool>> condition,
        FluentExpression increment
    )
    {
        return _For(initialization.Body, condition.Body, increment);
    }

    /// <summary>
    /// Starts a For loop.
    /// </summary>
    public FluentForBuilder<TThis> For<T>(
        Expression<Func<T>> initialization,
        FluentExpression condition,
        FluentExpression increment
    )
    {
        return _For(initialization.Body, condition, increment);
    }

    /// <summary>
    /// Starts a For loop.
    /// </summary>
    public FluentForBuilder<TThis> For<T>(
        FluentExpression initialization,
        Expression<Func<bool>> condition,
        FluentExpression increment
    )
    {
        return _For(initialization, condition.Body, increment);
    }

    /// <summary>
    /// Starts a For loop.
    /// </summary>
    public FluentForBuilder<TThis> For<T>(
        FluentExpression initialization,
        FluentExpression condition,
        Expression<Func<T>> increment
    )
    {
        return _For(initialization, condition, increment.Body);
    }

    /// <summary>
    /// Starts a For loop.
    /// </summary>
    public FluentForBuilder<TThis> For<T>(
        FluentExpression initialization,
        FluentExpression condition,
        FluentExpression increment
    )
    {
        return _For(initialization, condition, increment);
    }

    internal FluentForBuilder<TThis> _For(
        Expression initialization,
        Expression condition,
        Expression increment
    )
    {
        if (initialization == null)
        {
            throw new ArgumentNullException("initialization");
        }

        if (condition == null)
        {
            throw new ArgumentNullException("condition");
        }

        if (increment == null)
        {
            throw new ArgumentNullException("increment");
        }

        _CheckModifiable();

        var result = new FluentForBuilder<TThis>(
            _this,
            _method,
            initialization,
            condition,
            increment
        );
        _statements.Add(result);
        return result;
    }

    /// <summary>
    /// Adds a Continue statement to this block, making it go to the next iteration of the
    /// loop without finishing the rest of the statements in the loop. This should only be
    /// used in loop blocks (Loop, While and For) or in sub-blocks of such blocks.
    /// </summary>
    public TThis Continue()
    {
        _CheckModifiable();

        _IBlockBuilder instance = this;
        while (instance != null)
        {
            _ILoop loop = instance as _ILoop;
            if (loop != null)
            {
                var gotoExpression = Expression.Goto(loop.ContinueTarget);
                var statement = new _Expression(gotoExpression);
                _statements.Add(statement);
                return _this;
            }

            instance = instance.Parent as _IBlockBuilder;
        }

        throw new InvalidOperationException("You are not inside a continuable loop block.");
    }

    /// <summary>
    /// Adds a Break statement to this block, making it exit the actual loop.
    /// This should only be used in loop blocks (Loop, While and For) or in sub-blocks of such blocks.
    /// </summary>
    public TThis Break()
    {
        _CheckModifiable();

        _IBlockBuilder instance = this;
        while (instance != null)
        {
            _ILoop loop = instance as _ILoop;
            if (loop != null)
            {
                var gotoExpression = Expression.Goto(loop.BreakTarget);
                var statement = new _Expression(gotoExpression);
                _statements.Add(statement);
                return _this;
            }

            instance = instance.Parent as _IBlockBuilder;
        }

        throw new InvalidOperationException("You are not inside a breakable block.");
    }

    private static void _DoNothing() { }

    private static readonly Expression<Action> _doNothing = () => _DoNothing();

    internal virtual Expression _CompileToExpression()
    {
        _readOnly = true;

        if (_statements.Count == 0)
        {
            return _doNothing.Body;
        }

        Expression result;
        if (object.ReferenceEquals(this, _method.Body))
        {
            result = Expression.Block(_method._locals.Values, _EnumerateExpressions());
        }
        else
        {
            result = Expression.Block(_EnumerateExpressions());
        }

        return result;
    }

    private IEnumerable<Expression> _EnumerateExpressions()
    {
        foreach (var statement in _statements)
        {
            var expression = statement._CompileToExpression();
            yield return expression;
        }

        if (object.ReferenceEquals(this, _method.Body))
        {
            if (_method.ReturnType == typeof(void))
            {
                yield return Expression.Label(_method._returnTarget);
            }
            else if (_method.ReturnType.IsValueType)
            {
                object value = Activator.CreateInstance(_method.ReturnType);
                yield return Expression.Label(_method._returnTarget, Expression.Constant(value));
            }
            else
            {
                yield return Expression.Label(
                    _method._returnTarget,
                    Expression.Constant(null, _method.ReturnType)
                );
            }
        }
    }

    /// <summary>
    /// Adds the body of another FluentMethodBuilder inside this one.
    /// </summary>
    public TThis Inline(FluentMethodBuilder action)
    {
        _CheckModifiable();
        action._CheckThread();

        if (action._parameters.Count > 0)
        {
            throw new ArgumentException(
                "An action method should not have any parameters.",
                "actionMethod"
            );
        }

        action._modifiable = false;

        foreach (var pair in action._locals)
        {
            if (!_method._locals.ContainsKey(pair.Key))
            {
                _method._locals.Add(pair.Key, pair.Value);
            }
        }

        foreach (var pair in action._sharedVariables)
        {
            if (!_method._sharedVariables.Contains(pair))
            {
                _method._sharedVariables.Add(pair);
            }
        }

        _statements.Add(action.Body);
        return _this;
    }

    object _IBlockBuilder.Parent
    {
        get { return _parent; }
    }

    Expression _ICompilable._CompileToExpression()
    {
        return _CompileToExpression();
    }
}
