using System.Linq.Expressions;

namespace Pfz.TypeBuilding.Blocks;

/// <summary>
/// This class is the result of the While() method of the FluentMethodBuilder.Body
/// or any inner block. Use this instance to add the actions that will be executed
/// inside the while that will be generated.
/// </summary>
public sealed class FluentWhileBuilder<TParentBuilder>
    : FluentBlockBuilder<TParentBuilder, FluentWhileBuilder<TParentBuilder>>,
      _ILoop
{
    private readonly Expression _condition;

    internal FluentWhileBuilder(
        TParentBuilder parent,
        FluentMethodBuilder method,
        Expression condition
    ) : base(parent, method)
    {
        _condition = condition;
        ContinueTarget = Expression.Label();
        BreakTarget = Expression.Label();
    }

    /// <summary>
    /// Ends this While block and return to its containing block.
    /// </summary>
    public TParentBuilder EndWhile()
    {
        return _EndBlock();
    }

    internal override Expression _CompileToExpression()
    {
        var innerBody = base._CompileToExpression();
        var result = Expression.Block(
            Expression.Label(ContinueTarget),
            Expression.IfThen(Expression.Not(_condition), Expression.Goto(BreakTarget)),
            innerBody,
            Expression.Goto(ContinueTarget),
            Expression.Label(BreakTarget)
        );

        return result;
    }

    internal LabelTarget ContinueTarget { get; private set; }
    internal LabelTarget BreakTarget { get; private set; }

    LabelTarget _ILoop.ContinueTarget
    {
        get { return ContinueTarget; }
    }

    LabelTarget _ILoop.BreakTarget
    {
        get { return BreakTarget; }
    }
}
