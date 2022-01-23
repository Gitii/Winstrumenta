namespace PackageInstaller.Core.Services;

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
