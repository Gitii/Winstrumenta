using System.Collections.Immutable;
using System.ComponentModel;

namespace Numbers.Core.Services;

public interface IWindowInterface : INotifyPropertyChanged
{
    public string? WindowTitle { get; set; }
    public IImmutableList<CommandDescription>? Commands { get; set; }
}
