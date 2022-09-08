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
    private const string WAD_FOLDER_NAME = "wad";

    public TestEnvironmentDetails? PreparedEnvDetails => _environment;

    public TestEnvironment(string testBaseDirectory)
    {
        this._testBaseDirectory = testBaseDirectory;
    }

    public async Task<TestEnvironmentDetails> PrepareAsync()
    {
        var nodeDownloadUrl = await GetNodeDownloadUrlAsync().ConfigureAwait(false);

        var (nodeAlreadyExtracted, nodePath) = await DownloadAndExtractAsync(nodeDownloadUrl, "node")
            .ConfigureAwait(false);

        var npmPath = Path.Combine(nodePath, "npm.cmd");

        if (!nodeAlreadyExtracted)
        {
            await ExecuteAsync(npmPath, "install", "-g", "appium").ConfigureAwait(false);
        }

        var (_, wadPath) = await DownloadAndExtractAsync(
                "https://github.com/Gitii/Winstrumenta/raw/main/Artifacts/Windows%20Application%20Driver.zip",
                WAD_FOLDER_NAME
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

    private async Task<string> GetNodeDownloadUrlAsync()
    {
        using var client = new HttpClient();
        var hashes = await client
            .GetStringAsync("https://nodejs.org/download/release/latest-v16.x/SHASUMS256.txt")
            .ConfigureAwait(false);

        var lines = hashes.Split(
            new string[] { "\n", "\n\r" },
            StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries
        );

        var nodeX64ZipLine = lines.First((x) => x.Contains("node-") && x.Contains("win-x64.zip"));

        var fileName = nodeX64ZipLine.Split(
            " ",
            StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries
        )[^1];

        return $"https://nodejs.org/download/release/latest-v16.x/{fileName}";
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
            await ExecuteAsync(
                    details.Value.WadPath,
                    false,
                    "--urls",
                    WAD_URL,
                    "--basepath",
                    "/wd/hub",
                    "--logpath",
                    "logs"
                )
                .ConfigureAwait(false);
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

    public static void KillApp() { }

    public static void KillWAD(string directory)
    {
        var outputDirectory = Path.Combine(directory, WAD_FOLDER_NAME);

        foreach (var process in Process.GetProcesses())
        {
            if (
                (process.MainModule?.FileName ?? "").Equals(
                    outputDirectory,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                process.Kill();
            }

            process.Dispose();
        }
    }
}
