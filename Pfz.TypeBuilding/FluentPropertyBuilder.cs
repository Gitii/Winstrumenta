using System;

namespace Pfz.TypeBuilding;

/// <summary>
/// Instances of this class are returned by a call to the FluentTypeBuilder.AddProperty.
/// You should use this class to build the Get and Set methods.
/// </summary>
public sealed class FluentPropertyBuilder
{
    internal FluentPropertyBuilder(
        _IFluentTypeBuilder typeBuilder,
        string name,
        Type type,
        bool isStatic,
        bool respectVisibility
    )
    {
        GetMethod = typeBuilder.AddMethod(type, "get_" + name, isStatic, respectVisibility);
        SetMethod = typeBuilder.AddMethod(typeof(void), "set_" + name, isStatic, respectVisibility);

        Name = name;
        PropertyType = type;
    }

    /// <summary>
    /// Gets the name of this property.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the type of this property.
    /// </summary>
    public Type PropertyType { get; private set; }

    /// <summary>
    /// Gets the method builder that defines the property get.
    /// </summary>
    public FluentMethodBuilder GetMethod { get; private set; }

    /// <summary>
    /// Gets the method builder that defines the property set.
    /// </summary>
    public FluentMethodBuilder SetMethod { get; private set; }
}
