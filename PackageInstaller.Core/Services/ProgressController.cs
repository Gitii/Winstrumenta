using System.Collections.Immutable;
using System.Reactive.Concurrency;
using DynamicData;
using PackageInstaller.Core.ModelViews;

namespace PackageInstaller.Core.Services;

public class ProgressController : IProgressController
{
    private readonly ISourceList<OperationProgressModelView> _list;
    private readonly IScheduler _scheduler;

    public ProgressController(ISourceList<OperationProgressModelView> list, IScheduler scheduler)
    {
        _list = list;
        _scheduler = scheduler;
    }

    public void StartNew(string operationTitle, bool isIndeterminate = false)
    {
        Schedule(
            () =>
            {
                var lastOp = _list.Items.LastOrDefault();
                if (lastOp != null)
                {
                    lastOp.IsFinished = true;
                }

                _list.Edit(
                    (l) =>
                        l.Add(
                            new OperationProgressModelView()
                            {
                                CurrentProgress = 0,
                                IsIndeterminate = isIndeterminate,
                                MaximumProgress = 100,
                                OperationTitle = operationTitle,
                                StatusText = String.Empty
                            }
                        )
                );
            }
        );
    }

    public void SetProgress(int currentProgress, int? maxProgress = null)
    {
        var lastOp = _list.Items.LastOrDefault();
        if (lastOp != null)
        {
            Schedule(
                () =>
                {
                    lastOp.CurrentProgress = currentProgress;

                    if (lastOp.MaximumProgress != (maxProgress ?? lastOp.MaximumProgress))
                    {
                        lastOp.MaximumProgress = maxProgress ?? lastOp.MaximumProgress;
                    }

                    if (lastOp.IsIndeterminate)
                    {
                        lastOp.IsIndeterminate = false;
                    }
                }
            );
        }
    }

    public void SetStatusText(string? statusText = null)
    {
        var lastOp = _list.Items.LastOrDefault();
        if (lastOp != null)
        {
            Schedule(
                () =>
                {
                    lastOp.StatusText = statusText ?? String.Empty;
                }
            );
        }
    }

    public void Fail(string message, params RecoveryAction[] actions)
    {
        var lastOp = _list.Items.LastOrDefault();
        if (lastOp != null)
        {
            Schedule(
                () =>
                {
                    lastOp.IsFinished = true;
                    lastOp.HasFailed = true;
                    lastOp.RecoveryActions = ImmutableList.Create(actions);
                }
            );
        }
    }

    public IProgress<string> CreateStatusReporter()
    {
        return new Progress<string>(SetStatusText);
    }

    private void Schedule(Action action)
    {
        _scheduler.Schedule(action);
    }
}
