namespace Pfz.TypeBuilding.Blocks;

/// <summary>
/// This class is returned when calling Finally() on a FluentTryBuilder instance.
/// Instances of this class can be used to add the actions that you want to be
/// executed after the try block ends, independently if there are exceptions or not.
/// </summary>
public sealed class FluentFinallyBuilder<TParentBuilder>
    : FluentBlockBuilder<TParentBuilder, FluentFinallyBuilder<TParentBuilder>>
{
    internal FluentFinallyBuilder(FluentTryBuilder<TParentBuilder> tryBuilder)
        : base(tryBuilder._parent, tryBuilder._method) { }

    /// <summary>
    /// Ends this finally block and returns its containing block.
    /// </summary>
    public TParentBuilder EndTry()
    {
        return _EndBlock();
    }
}
