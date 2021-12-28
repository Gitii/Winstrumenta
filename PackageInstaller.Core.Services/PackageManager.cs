namespace PackageInstaller.Core.Services
{
    public class PackageManager : IPackageManager
    {
        private IDpkg _dpkg;
        private IWslCommands _wslCommands;

        public PackageManager(IDpkg dpkg, IWslCommands wslCommands)
        {
            _dpkg = dpkg;
            _wslCommands = wslCommands;
        }

        public async Task<IPackageManager.PackageStatus> CheckInstallationStatus(
            WslDistribution distribution,
            string packageName,
            string packageVersion
        )
        {
            if (distribution.SupportedPackageTypes.HasFlag(PackageTypes.Deb))
            {
                if (await _dpkg.IsPackageInstalled(distribution.Name, packageName))
                {
                    var installedPackage = await _dpkg.GetPackage(distribution.Name, packageName);

                    var versionComparison = _dpkg.CompareVersions(
                        packageVersion,
                        installedPackage.Version
                    );

                    return new IPackageManager.PackageStatus()
                    {
                        InstalledPackageVersion = installedPackage.Version,
                        Status = GetInstallationStatusFromComparison(versionComparison)
                    };
                }
                else
                {
                    return new IPackageManager.PackageStatus()
                    {
                        Status = IPackageManager.PackageInstallationStatus.NotInstalled,
                        InstalledPackageVersion = null
                    };
                }
            }
            else if (distribution.SupportedPackageTypes.HasFlag(PackageTypes.Rpm))
            {
                throw new NotSupportedException("yum/rpm isn't supported yet!");
            }
            else
            {
                throw new NotSupportedException(
                    $"Unsupported package types {distribution.SupportedPackageTypes}"
                );
            }
        }

        public async Task<string> InstallPackage(WslDistribution distribution, string filePath)
        {
            var unixFilePath = await _wslCommands.ExecuteCommandAsync(
                distribution.Name,
                "wslpath",
                new[] { "-u", filePath }
            );

            if (distribution.SupportedPackageTypes.HasFlag(PackageTypes.Deb))
            {
                var (success, logs) = await _dpkg.Install(distribution.Name, unixFilePath);
                var logString = string.Join(Environment.NewLine, logs);

                if (!success)
                {
                    throw new Exception(logString);
                }

                return logString;
            }
            else if (distribution.SupportedPackageTypes.HasFlag(PackageTypes.Rpm))
            {
                throw new NotSupportedException("yum/rpm isn't supported yet!");
            }
            else
            {
                throw new NotSupportedException(
                    $"Unsupported package types {distribution.SupportedPackageTypes}"
                );
            }
        }

        public async Task<string> UpgradePackage(WslDistribution distribution, string filePath)
        {
            var unixFilePath = await _wslCommands.ExecuteCommandAsync(
                distribution.Name,
                "wslpath",
                new[] { "-u", filePath }
            );

            if (distribution.SupportedPackageTypes.HasFlag(PackageTypes.Deb))
            {
                var (success, logs) = await _dpkg.Upgrade(distribution.Name, unixFilePath);
                var logString = string.Join(Environment.NewLine, logs);

                if (!success)
                {
                    throw new Exception(logString);
                }

                return logString;
            }
            else if (distribution.SupportedPackageTypes.HasFlag(PackageTypes.Rpm))
            {
                throw new NotSupportedException("yum/rpm isn't supported yet!");
            }
            else
            {
                throw new NotSupportedException(
                    $"Unsupported package types {distribution.SupportedPackageTypes}"
                );
            }
        }

        public async Task<string> DowngradePackage(WslDistribution distribution, string filePath)
        {
            var unixFilePath = await _wslCommands.ExecuteCommandAsync(
                distribution.Name,
                "wslpath",
                new[] { "-u", filePath }
            );

            if (distribution.SupportedPackageTypes.HasFlag(PackageTypes.Deb))
            {
                var (success, logs) = await _dpkg.Downgrade(distribution.Name, unixFilePath);
                var logString = string.Join(Environment.NewLine, logs);

                if (!success)
                {
                    throw new Exception(logString);
                }

                return logString;
            }
            else if (distribution.SupportedPackageTypes.HasFlag(PackageTypes.Rpm))
            {
                throw new NotSupportedException("yum/rpm isn't supported yet!");
            }
            else
            {
                throw new NotSupportedException(
                    $"Unsupported package types {distribution.SupportedPackageTypes}"
                );
            }
        }

        public async Task<string> UninstallPackage(WslDistribution distribution, string packageName)
        {
            if (distribution.SupportedPackageTypes.HasFlag(PackageTypes.Deb))
            {
                var (success, logs) = await _dpkg.Uninstall(distribution.Name, packageName);
                var logString = string.Join(Environment.NewLine, logs);

                if (!success)
                {
                    throw new Exception(logString);
                }

                return logString;
            }
            else if (distribution.SupportedPackageTypes.HasFlag(PackageTypes.Rpm))
            {
                throw new NotSupportedException("yum/rpm isn't supported yet!");
            }
            else
            {
                throw new NotSupportedException(
                    $"Unsupported package types {distribution.SupportedPackageTypes}"
                );
            }
        }

        private IPackageManager.PackageInstallationStatus GetInstallationStatusFromComparison(
            int versionComparison
        )
        {
            if (versionComparison < 0)
            {
                return IPackageManager.PackageInstallationStatus.InstalledNewerVersion;
            }
            else if (versionComparison > 0)
            {
                return IPackageManager.PackageInstallationStatus.InstalledOlderVersion;
            }
            else
            {
                return IPackageManager.PackageInstallationStatus.InstalledSameVersion;
            }
        }
    }
}