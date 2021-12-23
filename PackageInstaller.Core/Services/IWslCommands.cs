using Community.Wsl.Sdk;
using Community.Wsl.Sdk.Strategies.Command;

namespace PackageInstaller.Core.Services
{
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
            bool shellExecute = false
        );
    }
}
