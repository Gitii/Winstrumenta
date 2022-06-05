using System.Collections.Immutable;
using PackageInstaller.Core.Services;
using ReactiveUI;

namespace PackageInstaller.Core.ModelViews;

public class OperationProgressModelView : ReactiveObject
{
    string _operationTitle = String.Empty;

    public string OperationTitle
    {
        get { return _operationTitle; }
        set { this.RaiseAndSetIfChanged(ref _operationTitle, value); }
    }

    bool _isIndeterminate = false;

    public bool IsIndeterminate
    {
        get { return _isIndeterminate; }
        set { this.RaiseAndSetIfChanged(ref _isIndeterminate, value); }
    }

    int _currentProgress = 0;

    public int CurrentProgress
    {
        get { return _currentProgress; }
        set { this.RaiseAndSetIfChanged(ref _currentProgress, value); }
    }

    int _maximumProgress = 100;

    public int MaximumProgress
    {
        get { return _maximumProgress; }
        set { this.RaiseAndSetIfChanged(ref _maximumProgress, value); }
    }

    string _statusText = String.Empty;

    public string StatusText
    {
        get { return _statusText; }
        set { this.RaiseAndSetIfChanged(ref _statusText, value); }
    }

    bool _isFinished;

    public bool IsFinished
    {
        get { return _isFinished; }
        set { this.RaiseAndSetIfChanged(ref _isFinished, value); }
    }

    bool _hasFailed;

    public bool HasFailed
    {
        get { return _hasFailed; }
        set { this.RaiseAndSetIfChanged(ref _hasFailed, value); }
    }

    ImmutableList<RecoveryAction> _recoveryActions = ImmutableList<RecoveryAction>.Empty;

    public ImmutableList<RecoveryAction> RecoveryActions
    {
        get { return _recoveryActions; }
        set { this.RaiseAndSetIfChanged(ref _recoveryActions, value); }
    }
}
