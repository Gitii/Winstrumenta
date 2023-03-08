namespace Shared.Services;

public interface IApplicationLifeCycle
{
    /// <summary>
    /// Gracefully exits the application. It is guaranteed that the application will be closed even if internal clean up fails.
    /// That means that the returned task can be awaited but when it's completed or failed, the application will be closed regardless of the outcome.
    /// </summary>
    /// <param name="exitCode">The exit code of the application.</param>
    public Task ExitAsync(int exitCode);

    /// <summary>
    /// Gracefully exits the application. It is guaranteed that the application will be closed even if internal clean up fails.
    /// </summary>
    /// <param name="exitCode">The exit code of the application.</param>
    public void Exit(int exitCode);

    /// <summary>
    /// Kills the application. No cleanup of any kind will be done. Depending on the implementation, this equals <seealso cref="Environment.Exit"/>.
    /// </summary>
    /// <param name="exitCode">The exit code of the application.</param>
    public void Kill(int exitCode);
}
