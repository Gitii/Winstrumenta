namespace PackageInstaller.Core.Services;

public readonly struct WslDistribution
{
    public string Name { get; init; }
    public WslDistributionVersion Version { get; init; }
    public WslDistributionType Type { get; init; }
    public bool IsRunning { get; init; }

    public bool IsDefault { get; init; }
}
