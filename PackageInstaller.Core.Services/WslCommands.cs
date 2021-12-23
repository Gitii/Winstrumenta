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
            throw new NotImplementedException();
        }

        public async Task<string> ExecuteCommandAsync(
            string distroName,
            string command,
            string[] arguments,
            bool asRoot = false,
            bool shellExecute = false
        )
        {
            var cmd = CreateCommand(
                distroName,
                command,
                arguments,
                new CommandExecutionOptions()
                {
                    FailOnNegativeExitCode = true,
                    StdoutDataProcessingMode = DataProcessingMode.String
                }
            );

            var result = await cmd.StartAndGetResultsAsync().ConfigureAwait(false);

            return result.Stdout ?? string.Empty;
        }
    }
}
