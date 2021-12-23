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

    [Flags]
    public enum PackageTypes
    {
        Unknown = 0x0,
        Deb = 0x00001,
        Rpm = 0x00010
    }

    public enum PackageType
    {
        Deb = 1,
        Rpm = 2
    }

    public readonly struct WslDistribution
    {
        public string Name { get; init; }
        public WslDistributionVersion Version { get; init; }
        public WslDistributionType Type { get; init; }
        public bool IsRunning { get; init; }
        public PackageTypes SupportedPackageTypes { get; init; }

        public bool IsDefault { get; init; }
    }

    public interface IWsl
    {
        void AssertWslIsReady();

        Task<WslDistribution[]> GetAllInstalledDistributions();

        Task<int> ExecuteCommand(
            WslDistribution distribution,
            string command,
            string[] arguments,
            Action<string> onDataReceived
        );
    }
}
