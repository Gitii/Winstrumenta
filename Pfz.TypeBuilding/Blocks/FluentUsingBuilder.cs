using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Pfz.TypeBuilding.Blocks;

/// <summary>
/// This class is returned when calling Using() on a FluentMethodBuilder.Body or
/// on any of its sub-blocks. Instances of this class can be used to add the actions
/// that you want to be executed inside the using clause.
/// </summary>
public sealed class FluentUsingBuilder<TParentBuilder>
    : FluentBlockBuilder<TParentBuilder, FluentUsingBuilder<TParentBuilder>>
{
    private readonly Expression _usingVariable;
    private readonly Expression _usingValue;

    internal FluentUsingBuilder(
        TParentBuilder parent,
        FluentMethodBuilder method,
        Expression usingVariable,
        Expression usingValue
    ) : base(parent, method)
    {
        _usingVariable = usingVariable;
        _usingValue = usingValue;
    }

    /// <summary>
    /// Finishes the using clause and returns its containing block.
    /// </summary>
    public TParentBuilder EndUsing()
    {
        return _EndBlock();
    }

    private static readonly MethodInfo _disposeMethod = typeof(IDisposable).GetMethod("Dispose");

    internal override Expression _CompileToExpression()
    {
        var otherVariable = Expression.Variable(_usingVariable.Type);

        var result = Expression.Block(
            new ParameterExpression[] { otherVariable },
            Expression.Assign(otherVariable, _usingValue),
            Expression.Assign(_usingVariable, otherVariable),
            Expression.TryFinally(
                base._CompileToExpression(),
                Expression.IfThen(
                    Expression.NotEqual(otherVariable, Expression.Constant(null)),
                    Expression.Block(
                        Expression.Assign(
                            _usingVariable,
                            Expression.Constant(null, _usingVariable.Type)
                        ),
                        Expression.Call(otherVariable, _disposeMethod)
                    )
                )
            )
        );

        return result;
    }
}
