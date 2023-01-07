using System.Linq.Expressions;

namespace Pfz.TypeBuilding.Blocks;

/// <summary>
/// This class is returned when calling For() on a FluentMethodBuilder.Body or
/// on any of its sub-blocks. Instances of this class can be used to add the actions
/// that you want to be executed inside the for loop.
/// </summary>
public sealed class FluentForBuilder<TParentBuilder>
    : FluentBlockBuilder<TParentBuilder, FluentForBuilder<TParentBuilder>>,
      _ILoop
{
    private readonly Expression _initialization;
    private readonly Expression _condition;
    private readonly Expression _increment;

    internal FluentForBuilder(
        TParentBuilder parent,
        FluentMethodBuilder method,
        Expression initialization,
        Expression condition,
        Expression increment
    ) : base(parent, method)
    {
        _initialization = initialization;
        _condition = condition;
        _increment = increment;
        ContinueTarget = Expression.Label();
        BreakTarget = Expression.Label();
    }

    /// <summary>
    /// Ends the actual for block and returns its containing block.
    /// </summary>
    public TParentBuilder EndFor()
    {
        return _EndBlock();
    }

    internal override Expression _CompileToExpression()
    {
        var loop = Expression.Label();

        var innerBody = base._CompileToExpression();
        var result = Expression.Block(
            _initialization,
            Expression.Label(loop),
            Expression.IfThen(Expression.Not(_condition), Expression.Goto(BreakTarget)),
            innerBody,
            Expression.Label(ContinueTarget),
            _increment,
            Expression.Goto(loop),
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
