namespace Pfz.TypeBuilding;

/// <summary>
/// Determines how the parameter works (input only or input and output).
/// </summary>
public enum FluentParameterKind
{
    /// <summary>
    /// This is the default value and means the parameter is for input only.
    /// </summary>
    Input,

    /// <summary>
    /// This means the parameter is given as a reference, so it
    /// can work as input and as output.
    /// </summary>
    InputAndOutput
}
