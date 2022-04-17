using Community.Wsl.Sdk;
using Community.Wsl.Sdk.Strategies.Commands;

namespace PackageInstaller.Core.Services;

public class ManagedWslCommands : IWslCommands
{
    public ICommand CreateCommand(
        string distroName,
        string command,
        string[] arguments,
        CommandExecutionOptions options,
        bool asRoot = false,
        bool shellExecute = false
    )
    {
        return new Command(distroName, command, arguments, options, asRoot, shellExecute);
    }

    public async Task<string> ExecuteCommandAsync(
        string distroName,
        string command,
        string[] arguments,
        bool asRoot = false,
        bool shellExecute = false,
        bool ignoreExitCode = false,
        bool includeStandardError = false
    )
    {
        var cmd = CreateCommand(
            distroName,
            command,
            arguments,
            new CommandExecutionOptions()
            {
                FailOnNegativeExitCode = !ignoreExitCode,
                StdoutDataProcessingMode = DataProcessingMode.String,
                StdErrDataProcessingMode = includeStandardError
                    ? DataProcessingMode.String
                    : DataProcessingMode.Drop
            }
        );

        var result = await cmd.StartAndGetResultsAsync().ConfigureAwait(false);

        string stdOut = result.Stdout ?? string.Empty;
        string stdErr = result.Stderr ?? String.Empty;
        return (stdOut + stdErr).Trim();
    }

    public async Task<bool> CheckCommandExistsAsync(string distroName, string command)
    {
        if (await CheckCommandExistsUsingWhichAsync(distroName, "which").ConfigureAwait(false))
        {
            // distro supports which
            return await CheckCommandExistsUsingWhichAsync(distroName, command)
                .ConfigureAwait(false);
        }

        if (await CheckCommandExistsUsingWhereisAsync(distroName, "whereis").ConfigureAwait(false))
        {
            // distro supports whereis
            return await CheckCommandExistsUsingWhereisAsync(distroName, command)
                .ConfigureAwait(false);
        }

        throw new Exception(
            $"{distroName} doesn't support which and whereis. Cannot determine if {command} is installed."
        );
    }

    public async Task<bool> CheckCommandExistsUsingWhichAsync(string distroName, string command)
    {
        var pathToDpkg = await ExecuteCommandAsync(
                distroName,
                "which",
                new string[] { command },
                ignoreExitCode: true
            )
            .ConfigureAwait(false);

        return pathToDpkg.Trim().Length > 0;
    }

    public async Task<bool> CheckCommandExistsUsingWhereisAsync(string distroName, string command)
    {
        var whereisOutput = await ExecuteCommandAsync(
                distroName,
                "whereis",
                new string[] { "-b", command },
                ignoreExitCode: true
            )
            .ConfigureAwait(false);

        if (
            whereisOutput.Length > 0
            && !whereisOutput.StartsWith(command + ":", StringComparison.Ordinal)
        )
        {
            throw new Exception("Unexpected return value from whereis: " + whereisOutput);
        }

        return whereisOutput.Contains('/', StringComparison.Ordinal); // assume that any path to a binary starts with /
    }
}
