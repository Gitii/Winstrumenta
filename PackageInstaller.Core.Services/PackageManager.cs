namespace PackageInstaller.Core.Services
{
    public class PackageManager : IPackageManager
    {
        private IDpkg _dpkg;

        public PackageManager(IDpkg dpkg)
        {
            _dpkg = dpkg;
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

                    var versionComparison = _dpkg.CompareVersions(packageVersion, installedPackage.Version);

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

        private IPackageManager.PackageInstallationStatus GetInstallationStatusFromComparison(int versionComparison)
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
