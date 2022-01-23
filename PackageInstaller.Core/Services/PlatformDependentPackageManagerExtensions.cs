namespace PackageInstaller.Core.Services;

public static class PlatformDependentPackageManagerExtensions
{
    public static async Task<IPlatformDependentPackageManager> GetSupportedManagerAsync(
        this IEnumerable<IPlatformDependentPackageManager> managers,
        FileSystemPath packageFilePath
    )
    {
        IList<string> rejectionReasons = new List<string>();

        foreach (var manager in managers)
        {
            (var isSupported, string? reason) = await manager
                .IsPackageSupportedAsync(packageFilePath)
                .ConfigureAwait(false);

            if (isSupported)
            {
                return manager;
            }
            else
            {
                rejectionReasons.Add($"{manager.GetType().Name}: {reason ?? "Unknown reason"}");
            }
        }

        var message =
            $"No package manager supported this package:{Environment.NewLine}{String.Join(Environment.NewLine, rejectionReasons)}";
        throw new Exception(message);
    }

    public static async Task<IPlatformDependentPackageManager> GetSupportedManagerAsync(
        this IEnumerable<IPlatformDependentPackageManager> managers,
        FileSystemPath packageFilePath,
        string distroName
    )
    {
        IList<string> rejectionReasons = new List<string>();

        foreach (var manager in managers)
        {
            if (!await manager.IsSupportedByDistributionAsync(distroName).ConfigureAwait(false))
            {
                continue;
            }

            (var isSupported, string? reason) = await manager
                .IsPackageSupportedAsync(packageFilePath)
                .ConfigureAwait(false);

            if (isSupported)
            {
                return manager;
            }
            else
            {
                rejectionReasons.Add($"{manager.GetType().Name}: {reason ?? "Unknown reason"}");
            }
        }

        var message =
            $"No package manager supported this package:{Environment.NewLine}{String.Join(Environment.NewLine, rejectionReasons)}";
        throw new Exception(message);
    }
}
