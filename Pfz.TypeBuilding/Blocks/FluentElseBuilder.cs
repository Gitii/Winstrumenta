namespace Pfz.TypeBuilding.Blocks;

/// <summary>
/// This class is returned when calling Else() on a FluentIfBuilder.
/// Instances of this class can be used to add the actions that you want
/// to be executed if the condition on the if clause is false.
/// </summary>
public sealed class FluentElseBuilder<TParentBuilder>
    : FluentBlockBuilder<TParentBuilder, FluentElseBuilder<TParentBuilder>>
{
    internal FluentElseBuilder(FluentIfBuilder<TParentBuilder> ifBuilder)
        : base(ifBuilder._parent, ifBuilder._method) { }

    /// <summary>
    /// Ends this else block (or the entire if, if you prefer) and
    /// returns its containing block.
    /// </summary>
    public TParentBuilder EndIf()
    {
        return _EndBlock();
    }
}
