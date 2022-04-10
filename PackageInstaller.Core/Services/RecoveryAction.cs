using System.Windows.Input;
using ReactiveUI;

namespace PackageInstaller.Core.Services;

public readonly struct RecoveryAction
{
    public string InstructionText { get; init; }

    public string InstructionDetails { get; init; }

    public bool OpensExternalProgram { get; init; }

    public Func<Task> Action { get; init; }

    public ICommand Command
    {
        get { return ReactiveCommand.CreateFromTask(Action); }
    }
}
