using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Community.Archives.Core;
using Community.Archives.Rpm;

namespace PackageInstaller.Core.Services
{
    public interface IRpm : IPlatformDependentPackageManager
    {
    }

    public class Rpm : IRpm
    {
        private readonly IWslCommands _wslCommands;

        public Rpm(IWslCommands wslCommands)
        {
            _wslCommands = wslCommands;
        }

        public async Task<bool> IsPackageInstalled(string distroName, string packageName)
        {
            var result = await _wslCommands.ExecuteCommandAsync(
                distroName,
                "rpm",
                new[] { "-q", packageName },
                true
            );

            if (result == $"package {packageName} is not installed")
            {
                return false;
            }

            return true;
        }

        public async Task<IPlatformDependentPackageManager.PackageInfo> GetInstalledPackageInfo(
            string distroName,
            string packageName
        )
        {
            var result = await _wslCommands.ExecuteCommandAsync(
                distroName,
                "rpm",
                new[] { "-q", packageName, "--queryformat", "%{version}" },
                true
            );

            if (result == $"package {packageName} is not installed")
            {
                throw new Exception($"Package '{packageName}' is not installed!");
            }

            return new IPlatformDependentPackageManager.PackageInfo()
            {
                Name = packageName,
                Version = result
            };
        }

        public async Task<IPlatformDependentPackageManager.PackageMetaData> ExtractPackageMetaData(
            FileSystemPath filePath
        )
        {
            var reader = new RpmArchiveReader();
            await using var stream = File.OpenRead(filePath.WindowsPath);
            var md = await reader.GetMetaData(stream);

            return new IPlatformDependentPackageManager.PackageMetaData()
            {
                Package = md.Package,
                Architecture = md.Architecture,
                Description = md.Description,
                Version = md.Version,
                AllFields = md.AllFields,
                IconName = null,
            };
        }

        public async Task<(bool isSupported, string? reason)> IsPackageSupported(FileSystemPath filePath)
        {
            try
            {
                var reader = new RpmArchiveReader();
                await using var stream = File.OpenRead(filePath.WindowsPath);
                _ = await reader.GetMetaData(stream);

                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }

        public IPlatformDependentPackageManager.PackageInstallationStatus CompareVersions(
            string baseVersion,
            string otherVersion
        )
        {
            throw new NotImplementedException();
        }

        public Task<(bool success, string logs)> Install(string distroName, FileSystemPath filePath)
        {
            throw new NotImplementedException();
        }

        public Task<(bool success, string logs)> Uninstall(string distroName, string packageName)
        {
            throw new NotImplementedException();
        }

        public Task<(bool success, string logs)> Upgrade(string distroName, FileSystemPath filePath)
        {
            throw new NotImplementedException();
        }

        public Task<(bool success, string logs)> Downgrade(
            string distroName,
            FileSystemPath filePath
        )
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsSupportedByDistribution(string distroName)
        {
            var pathToRpm = await _wslCommands.ExecuteCommandAsync(
                distroName,
                "which",
                new[] { "rpm" },
                ignoreExitCode: true
            );

            return pathToRpm.Trim().Length > 0;
        }
    }
}