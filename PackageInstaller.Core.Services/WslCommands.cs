using Community.Wsl.Sdk;
using Community.Wsl.Sdk.Strategies.Command;

namespace PackageInstaller.Core.Services
{
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
            return new ManagedCommand(
                distroName,
                command,
                arguments,
                options,
                asRoot,
                shellExecute
            );
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
                    StdErrDataProcessingMode =
                        includeStandardError ? DataProcessingMode.String : DataProcessingMode.Drop
                }
            );

            var result = await cmd.StartAndGetResultsAsync().ConfigureAwait(false);

            string stdOut = result.Stdout ?? string.Empty;
            string stdErr = result.Stderr ?? String.Empty;
            return (stdOut + stdErr).Trim();
        }
    }
}