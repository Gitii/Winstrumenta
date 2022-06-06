using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;

namespace Winstrumenta.Package.Tests;

class TestEnvironment
{
    private TestEnvironmentDetails? _environment;
    private string _testBaseDirectory;

    public const string WAD_URL = "127.0.0.1:4723";

    public TestEnvironmentDetails? PreparedEnvDetails => _environment;

    public TestEnvironment(string testBaseDirectory)
    {
        this._testBaseDirectory = testBaseDirectory;
    }

    public async Task<TestEnvironmentDetails> PrepareAsync()
    {
        var (nodeAlreayExtracted, nodePath) = await DownloadAndExtractAsync(
                @"https://nodejs.org/dist/latest-v16.x/node-v16.15.1-win-x64.zip",
                "node"
            )
            .ConfigureAwait(false);

        var npmPath = Path.Combine(nodePath, "npm.cmd");

        if (!nodeAlreayExtracted)
        {
            await ExecuteAsync(npmPath, "install", "-g", "appium").ConfigureAwait(false);
        }

        var (_, wadPath) = await DownloadAndExtractAsync(
                @"https://github.com/licanhua/YWinAppDriver/releases/download/v0.2.88.0/WinAppDriver.zip",
                "wad"
            )
            .ConfigureAwait(false);

        var wadExecPath = Path.Combine(wadPath, "WinAppDriver.exe");

        var d = new TestEnvironmentDetails()
        {
            NodePath = Path.Combine(nodePath, "node.exe"),
            AppiumPath = Path.Combine(nodePath, @"node_modules\appium\build\lib\main.js"),
            WadPath = wadExecPath,
        };

        _environment = d;

        return d;
    }

    public async Task StartAsync()
    {
        var details = PreparedEnvDetails;
        if (!details.HasValue)
        {
            throw new Exception("Test environment is not prepared!");
        }

        if (await IsPortOpenAsync(WAD_URL).ConfigureAwait(false) is false)
        {
            await ExecuteAsync(details.Value.WadPath, waitForExit: false).ConfigureAwait(false);
        }
    }

    private async Task<bool> IsPortOpenAsync(string url)
    {
        var tcp = new TcpClient();

        var triesLeft = 5;
        while (triesLeft > 0)
        {
            try
            {
                await tcp.ConnectAsync(IPEndPoint.Parse(url)).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                triesLeft--;
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }

        return false;
    }

    private Task ExecuteAsync(string executablePath, params string[] arguments)
    {
        return ExecuteAsync(executablePath, true, arguments);
    }

    private async Task ExecuteAsync(
        string executablePath,
        bool waitForExit,
        params string[] arguments
    )
    {
        var si = new ProcessStartInfo(executablePath);
        foreach (var argument in arguments)
        {
            si.ArgumentList.Add(argument);
        }

        si.WorkingDirectory = Path.GetDirectoryName(executablePath);

        var process = Process.Start(si);
        if (process == null)
        {
            throw new Exception("Command failed to start!");
        }

        if (waitForExit)
        {
            await process.WaitForExitAsync().ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                throw new Exception("Command failed!");
            }

            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

        if (process.HasExited)
        {
            throw new Exception("Command started and should still be running but failed!");
        }
    }

    public async Task TeardownAsync() { }

    private async Task<(bool alreadyExists, string fullPath)> DownloadAndExtractAsync(
        string downloadUrl,
        string folderName
    )
    {
        var outputDirectory = Path.Combine(_testBaseDirectory, folderName);

        if (ContainsFilesOrFolders(outputDirectory))
        {
            return (true, outputDirectory);
        }

        string tempFile = Path.GetTempFileName();
        try
        {
            var http = new HttpClient();
            var request = await http.GetStreamAsync(downloadUrl).ConfigureAwait(false);
            await using var _ = request.ConfigureAwait(false);
            var file = File.OpenWrite(tempFile);
            await using var __ = file.ConfigureAwait(false);
            await request.CopyToAsync(file).ConfigureAwait(false);
            file.Close();

            ZipFile.ExtractToDirectory(tempFile, outputDirectory, true);

            PullUpChildWhenRootDirectoryIsEmpty(outputDirectory);

            return (false, outputDirectory);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private bool ContainsFilesOrFolders(string directory)
    {
        return Directory.Exists(directory) && Directory.GetFileSystemEntries(directory).Length > 0;
    }

    private void PullUpChildWhenRootDirectoryIsEmpty(string directory)
    {
        var files = Directory.GetFiles(directory);
        var folders = Directory.GetDirectories(directory);
        if (files.Length == 0 && folders.Length == 1)
        {
            MoveContentOfDirectory(folders[0], directory);
        }
    }

    private void MoveContentOfDirectory(string sourceDirectory, string targetDirectory)
    {
        var files = Directory.GetFiles(sourceDirectory);
        var folders = Directory.GetDirectories(sourceDirectory);

        foreach (var folder in folders)
        {
            Directory.Move(folder, Path.Combine(targetDirectory, Path.GetFileName(folder)!));
        }

        foreach (var file in files)
        {
            File.Move(file, Path.Combine(targetDirectory, Path.GetFileName(file)!));
        }
    }
}
