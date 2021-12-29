namespace PackageInstaller.Core.Services
{
    public enum WslDistributionType
    {
        Ubuntu,
        Debian,
        Fedora,
        Arch,
        Kali,
        Alpine,
        OpenSUSE,
        SUSE,
        GeneralDebBased = 90,
        GeneralRpmBased = 91,
        Unknown = 99
    }

    public enum WslDistributionVersion
    {
        One,
        Two
    }

    public readonly struct WslDistribution
    {
        public string Name { get; init; }
        public WslDistributionVersion Version { get; init; }
        public WslDistributionType Type { get; init; }
        public bool IsRunning { get; init; }

        public bool IsDefault { get; init; }
    }

    public interface IWsl
    {
        Task<WslDistribution[]> GetAllInstalledDistributions();
    }
}
