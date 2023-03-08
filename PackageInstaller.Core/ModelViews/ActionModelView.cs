using System.Reactive;
using ReactiveUI;

namespace PackageInstaller.Core.ModelViews;

public class ActionModelView : ReactiveObject
{
    public ActionModelView(
        Func<ActionModelView, Task> action,
        string title,
        string? warningText = null,
        string? tooltip = null
    )
    {
        Title = title;
        WarningText = warningText;
        ActionCommand = ReactiveCommand.CreateFromTask(action);
        ToolTip = tooltip;
    }

    public ActionModelView(
        Action<ActionModelView> action,
        string title,
        string? warningText = null,
        string? tooltip = null
    )
    {
        Title = title;
        WarningText = warningText;
        ActionCommand = ReactiveCommand.CreateFromTask(
            (ActionModelView vm) =>
            {
                action(vm);
                return Task.CompletedTask;
            }
        );
        ToolTip = tooltip;
    }

    public string Title { get; }

    public string? WarningText { get; }

    public string? ToolTip { get; }

    public ReactiveCommand<ActionModelView, Unit> ActionCommand { get; }
}
