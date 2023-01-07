using System.Linq.Expressions;

namespace Pfz.TypeBuilding.Blocks;

/// <summary>
/// This class is returned when calling If() on a FluentMethodBuilder.Body or
/// on any of its sub-blocks. Instances of this class can be used to add the actions
/// that you want to be executed inside the if clause and to create an else clause.
/// </summary>
public sealed class FluentIfBuilder<TParentBuilder>
    : FluentBlockBuilder<TParentBuilder, FluentIfBuilder<TParentBuilder>>
{
    private readonly Expression _condition;
    private FluentElseBuilder<TParentBuilder>? _else;

    internal FluentIfBuilder(
        TParentBuilder parent,
        FluentMethodBuilder method,
        Expression condition
    ) : base(parent, method)
    {
        _condition = condition;
    }

    /// <summary>
    /// Ends the actual If and starts an else block.
    /// </summary>
    public FluentElseBuilder<TParentBuilder> Else()
    {
        _CheckModifiable();

        if (_else == null)
        {
            _else = new FluentElseBuilder<TParentBuilder>(this);
        }

        return _else;
    }

    /// <summary>
    /// Ends the actual If and returns to the containing block.
    /// </summary>
    public TParentBuilder EndIf()
    {
        return _EndBlock();
    }

    internal override Expression _CompileToExpression()
    {
        Expression ifTrue = base._CompileToExpression();

        Expression result;
        if (_else != null)
        {
            result = Expression.IfThenElse(_condition, ifTrue, _else._CompileToExpression());
        }
        else
        {
            result = Expression.IfThen(_condition, ifTrue);
        }

        return result;
    }
}
