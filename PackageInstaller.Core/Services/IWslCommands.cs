using Community.Wsl.Sdk;
using Community.Wsl.Sdk.Strategies.Command;

namespace PackageInstaller.Core.Services;

public interface IWslCommands
{
    public ICommand CreateCommand(
        string distroName,
        string command,
        string[] arguments,
        CommandExecutionOptions options,
        bool asRoot = false,
        bool shellExecute = false
    );

    public Task<string> ExecuteCommandAsync(
        string distroName,
        string command,
        string[] arguments,
        bool asRoot = false,
        bool shellExecute = false,
        bool ignoreExitCode = false,
        bool includeStandardError = false
    );

    /// <summary>
    /// Checks if the specified command (program) exists.
    /// </summary>
    /// <param name="distroName">The name of the distribution.</param>
    /// <param name="command">The command to check.</param>
    /// <returns><c>true</c> if the command exists, <c>false</c> is the command doesn't exist.</returns>
    public Task<bool> CheckCommandExistsAsync(string distroName, string command);
}
