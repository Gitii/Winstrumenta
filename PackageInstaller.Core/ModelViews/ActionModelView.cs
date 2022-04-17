using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace PackageInstaller.Core.ModelViews;

public class ActionModelView : ReactiveObject
{
    public ActionModelView(Func<Task> action, string title, string? warningText = null)
    {
        Title = title;
        WarningText = warningText;
        ActionCommand = ReactiveCommand.CreateFromTask(action);
    }

    public ActionModelView(Action action, string title, string? warningText = null)
    {
        Title = title;
        WarningText = warningText;
        ActionCommand = ReactiveCommand.CreateFromTask(
            () =>
            {
                action();
                return Task.CompletedTask;
            }
        );
    }

    public string Title { get; }

    public string? WarningText { get; }

    public ReactiveCommand<Unit, Unit> ActionCommand { get; }
}
