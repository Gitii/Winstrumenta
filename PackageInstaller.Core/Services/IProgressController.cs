namespace PackageInstaller.Core.Services;

public interface IProgressController
{
    void StartNew(string operationTitle, bool isIndeterminate = false);
    void SetProgress(int currentProgress, int? maxProgress = null);
    void SetStatusText(string? statusText = null);
    void Fail(string message, params RecoveryAction[] actions);

    IProgress<string> CreateStatusReporter();
}
