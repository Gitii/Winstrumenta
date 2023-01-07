using System.Linq.Expressions;

namespace Pfz.TypeBuilding.Blocks;

/// <summary>
/// This class is returned when calling Using() on a FluentMethodBuilder.Body or
/// on any of its sub-blocks. Instances of this class can be used to add the actions
/// that you want to be executed inside the loop.
/// </summary>
public sealed class FluentLoopBuilder<TParentBuilder>
    : FluentBlockBuilder<TParentBuilder, FluentLoopBuilder<TParentBuilder>>,
      _ILoop
{
    internal FluentLoopBuilder(TParentBuilder parent, FluentMethodBuilder method)
        : base(parent, method)
    {
        ContinueTarget = Expression.Label();
        BreakTarget = Expression.Label();
    }

    /// <summary>
    /// Ends the actual loop and returns the containing block so you can add new
    /// actions to it.
    /// </summary>
    public TParentBuilder EndLoop()
    {
        return _EndBlock();
    }

    internal override Expression _CompileToExpression()
    {
        var innerBody = base._CompileToExpression();
        var result = Expression.Block(
            Expression.Label(ContinueTarget),
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
