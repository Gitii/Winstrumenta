namespace Pfz.TypeBuilding.Blocks;

/// <summary>
/// Instances of this class are returned by the FluentMethodBuilder.Body
/// property and can be used to "build" the method.
/// </summary>
public sealed class FluentBodyBuilder : FluentBlockBuilder<FluentMethodBuilder, FluentBodyBuilder>
{
    internal FluentBodyBuilder(FluentMethodBuilder method) : base(method, method) { }

    /// <summary>
    /// Finishes the body of the method.
    /// </summary>
    public FluentMethodBuilder EndBody()
    {
        return _EndBlock();
    }
}
