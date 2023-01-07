using System;

namespace Pfz.TypeBuilding;

/// <summary>
/// This class is the result of calls to the FluentTypeBuilder.AddEvent method and
/// should be used to build the body of the Add and Remove methods of the event.
/// </summary>
public sealed class FluentEventBuilder
{
    internal FluentEventBuilder(
        _IFluentTypeBuilder typeBuilder,
        string name,
        Type type,
        bool isStatic,
        bool respectVisibility
    )
    {
        TypeBuilder = typeBuilder;
        Name = name;
        Type = type;
        IsStatic = isStatic;

        AddMethod = typeBuilder.AddMethod(typeof(void), "add_" + name, isStatic, respectVisibility);
        RemoveMethod = typeBuilder.AddMethod(
            typeof(void),
            "remove_" + name,
            isStatic,
            respectVisibility
        );
    }

    internal _IFluentTypeBuilder TypeBuilder { get; private set; }

    /// <summary>
    /// Gets the name of the event.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the type of the handlers of the event.
    /// </summary>
    public Type Type { get; private set; }

    /// <summary>
    /// Gets a value indicating if this event is static.
    /// </summary>
    public bool IsStatic { get; private set; }

    /// <summary>
    /// Gets the method builder that you must use to implement the
    /// event add.
    /// </summary>
    public FluentMethodBuilder AddMethod { get; private set; }

    /// <summary>
    /// Gets the method builder that you must use to implement the
    /// event remove.
    /// </summary>
    public FluentMethodBuilder RemoveMethod { get; private set; }
}
