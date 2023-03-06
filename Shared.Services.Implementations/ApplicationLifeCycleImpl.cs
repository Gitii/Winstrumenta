namespace Shared.Services;

public class ApplicationLifeCycleImpl : IApplicationLifeCycle
{
    private readonly IDisposableFiles _disposableFiles;

    public ApplicationLifeCycleImpl(IDisposableFiles disposableFiles)
    {
        _disposableFiles = disposableFiles;
    }

    public void Exit(int exitCode)
    {
        try
        {
            _disposableFiles.DeleteFiles();
        }
        finally
        {
            Kill(exitCode);
        }
    }

    public Task ExitAsync(int exitCode)
    {
        try
        {
            _disposableFiles.DeleteFiles();
        }
        finally
        {
            Kill(exitCode);
        }

        return Task.CompletedTask;
    }

    public void Kill(int exitCode)
    {
        Environment.Exit(exitCode);
    }
}
