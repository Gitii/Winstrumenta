using System;

namespace Pfz.TypeBuilding;

/// <summary>
/// Typed version of the FluentMethodBuilder, which will have no result.
/// </summary>
public sealed class FluentVoidMethodBuilder : FluentMethodBuilder
{
    /// <summary>
    /// Create a new method builder instance.
    /// </summary>
    public FluentVoidMethodBuilder() : base(typeof(void)) { }

    internal FluentVoidMethodBuilder(
        _IFluentTypeBuilder typeBuilder,
        string name,
        bool isStatic,
        int creatorThreadId,
        int delegateIndex,
        bool respectVisibility
    )
        : base(
            typeBuilder,
            name,
            typeof(void),
            isStatic,
            creatorThreadId,
            delegateIndex,
            respectVisibility
        )
    {
    }

    /// <summary>
    /// This method is here only to make it possible to generate expressions that call it.
    /// If you invoke it directly an InvalidOperationException will be thrown.
    /// </summary>
    public void Call(params object[] parameters)
    {
        throw new InvalidOperationException(
            "This method is here only to build expressions that do a call to other methods that are being built."
        );
    }
}
