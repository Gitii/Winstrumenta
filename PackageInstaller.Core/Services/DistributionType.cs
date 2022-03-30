namespace PackageInstaller.Core.Services;

public enum DistributionType
{
    Android,
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
