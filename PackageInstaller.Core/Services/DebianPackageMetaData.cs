namespace PackageInstaller.Core.Services;

public readonly struct DebianPackageMetaData
{
    public string Package { get; init; }
    public string Version { get; init; }
    public string Architecture { get; init; }
    public string Description { get; init; }
    public string? IconName { get; init; }

    public IReadOnlyDictionary<string, string> AllFields { get; init; }
}
