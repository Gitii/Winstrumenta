using System;
using System.Linq.Expressions;

namespace Pfz.TypeBuilding.Blocks;

/// <summary>
/// This class is returned when calling Catch() on a FluentTryBuilder.
/// Instances of this class can be used to add the actions
/// that you want to be executed when there is an specific kind of exception.
/// </summary>
public sealed class FluentCatchBuilder<TParentBuilder>
    : FluentBlockBuilder<TParentBuilder, FluentCatchBuilder<TParentBuilder>>
{
    private readonly FluentTryBuilder<TParentBuilder> _tryBuilder;
    private readonly Expression _catchVariable;

    internal FluentCatchBuilder(
        FluentTryBuilder<TParentBuilder> tryBuilder,
        Expression catchVariable
    ) : base(tryBuilder._parent, tryBuilder._method)
    {
        _tryBuilder = tryBuilder;
        _catchVariable = catchVariable;
    }

    /// <summary>
    /// Finishes the entire Try block and returns its containing block.
    /// </summary>
    public TParentBuilder EndTry()
    {
        return _EndBlock();
    }

    /// <summary>
    /// Finishes this catch block and adds another one.
    /// </summary>
    public FluentCatchBuilder<TParentBuilder> Catch<T>(Expression<Func<T>> expression)
        where T : Exception
    {
        return _tryBuilder.Catch(expression);
    }

    /// <summary>
    /// Finishes this catch block and adds a finally one.
    /// </summary>
    public FluentFinallyBuilder<TParentBuilder> Finally()
    {
        return _tryBuilder.Finally();
    }

    internal new CatchBlock _CompileToExpression()
    {
        var catchCondition = _method._visitor.Visit(_catchVariable);
        var catchBody = base._CompileToExpression();
        var result = Expression.Catch((ParameterExpression)catchCondition, catchBody);
        return result;
    }
}
