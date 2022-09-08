using Community.Wsl.Sdk;

namespace Winstrumenta.Package.Tests.HostEnvironment;

class WslHostEnvironment : BaseHostEnvironment
{
    private readonly IWslApi _wslApi;

    protected override async Task<(string stdOut, string stdErr)> ExecuteShellAsync(string command)
    {
        var d = _wslApi.GetDefaultDistribution()!;

        var cmd = new Community.Wsl.Sdk.Command(
            d.Value.DistroName,
            command,
            Array.Empty<string>(),
            new CommandExecutionOptions()
            {
                FailOnNegativeExitCode = false,
                StdoutDataProcessingMode = DataProcessingMode.String,
                StdErrDataProcessingMode = DataProcessingMode.String
            },
            asRoot: true,
            shellExecute: true
        );

        var results = await cmd.WaitAndGetResultsAsync();

        if (results.ExitCode != 0)
        {
            Console.WriteLine($"ExecuteShellAsync failed: {command}");
            Console.WriteLine(results.Stdout ?? "<no stdout>");
            Console.WriteLine(results.Stderr ?? "<no stderr>");

            throw new Exception($"Command {command} failed");
        }

        return (results.Stdout ?? "", results.Stderr ?? "");
    }

    protected override async Task<(string stdOut, string stdErr)> ExecuteAsync(
        string command,
        params string[] arguments
    )
    {
        var d = _wslApi.GetDefaultDistribution()!;

        var cmd = new Community.Wsl.Sdk.Command(
            d.Value.DistroName,
            command,
            arguments,
            new CommandExecutionOptions()
            {
                FailOnNegativeExitCode = false,
                StdoutDataProcessingMode = DataProcessingMode.String,
                StdErrDataProcessingMode = DataProcessingMode.String
            },
            asRoot: true
        );

        var results = await cmd.WaitAndGetResultsAsync();

        if (results.ExitCode != 0)
        {
            Console.WriteLine($"ExecuteAsync failed: {command} {string.Join(" ", arguments)}");
            Console.WriteLine(results.Stdout ?? "<no stdout>");
            Console.WriteLine(results.Stderr ?? "<no stderr>");

            throw new Exception($"Command {command} failed");
        }

        return (results.Stdout ?? "", results.Stderr ?? "");
    }

    public WslHostEnvironment(IWslApi? wslApi = null)
    {
        _wslApi = wslApi ?? new WslApi();
    }
}
