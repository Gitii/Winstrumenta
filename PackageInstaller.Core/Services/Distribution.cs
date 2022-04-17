namespace PackageInstaller.Core.Services;

public readonly struct Distribution
{
    public string Name { get; init; }
    public string Id { get; init; }
    public string Origin { get; init; }
    public Version Version { get; init; }
    public DistributionType Type { get; init; }
    public bool IsRunning { get; init; }
    public bool IsAvailable { get; init; }
}
