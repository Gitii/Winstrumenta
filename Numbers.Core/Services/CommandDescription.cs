using System.Windows.Input;

namespace Numbers.Core.Services;

public readonly struct CommandDescription
{
    public string Label { get; init; }
    public string? Icon { get; init; }
    public string? KeyboardAcceleratorKey { get; init; }
    public string? KeyboardAcceleratorModifier { get; init; }
    public ICommand Command { get; init; }
}
