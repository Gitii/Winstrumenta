using System.Text.RegularExpressions;

namespace PackageInstaller.Core.Services;

/// <summary>
/// Base class for classes representing Debian versions
/// 
/// It doesn't implement any comparison, but it does check for valid versions
/// according to Section 5.6.12 in the Debian Policy Manual.  Since splitting
/// the version into epoch, upstream_version, and debian_revision components is
/// pretty much free with the validation, it sets those fields as properties of
/// the object, and sets the raw version to the full_version property.  A
/// missing epoch or debian_revision results in the respective property set to
/// None.  Setting any of the properties results in the full_version being
/// recomputed and the rest of the properties set from that.
///
/// Based on https://salsa.debian.org/python-debian-team/python-debian/-/blob/master/lib/debian/debian_support.py
/// </summary>
public abstract class BaseVersion : IComparable<BaseVersion>, IComparable
{
    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return 1;
        }

        if (ReferenceEquals(this, obj))
        {
            return 0;
        }

        return obj is BaseVersion other
          ? CompareTo(other)
          : throw new ArgumentException(
                $"Object must be of type {nameof(BaseVersion)}",
                nameof(obj)
            );
    }

    public static bool operator <(BaseVersion? left, BaseVersion? right)
    {
        return Comparer<BaseVersion>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(BaseVersion? left, BaseVersion? right)
    {
        return Comparer<BaseVersion>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(BaseVersion? left, BaseVersion? right)
    {
        return Comparer<BaseVersion>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(BaseVersion? left, BaseVersion? right)
    {
        return Comparer<BaseVersion>.Default.Compare(left, right) >= 0;
    }

    public readonly Regex re_valid_version = new Regex(
        @"^((?<epoch>\d+):)?(?<upstream_version>[A-Za-z0-9.+:~-]+?)(-(?<debian_revision>[A-Za-z0-9+.~]+))?$",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(1)
    );

    protected BaseVersion(string version)
    {
        var m = re_valid_version.Match(version);
        if (!m.Success)
        {
            throw new ArgumentException($"Invalid version string {version}", nameof(version));
        }

        // If there no epoch ("1:..."), then the upstream version can not
        // contain a :.
        if (
            m.Groups["epoch"].Value == string.Empty
            && m.Groups["upstream_version"].Value.Contains(":", StringComparison.InvariantCulture)
        )
        {
            throw new ParseError($"Invalid version string {version}");
        }

        FullVersion = version;
        Epoch = m.Groups["epoch"].Value;
        UpstreamVersion = m.Groups["upstream_version"].Value;
        DebianRevision = m.Groups["debian_revision"].Value;
    }

    public string FullVersion { get; set; }

    public string Epoch { get; set; }

    public string UpstreamVersion { get; set; }

    public string DebianRevision { get; set; }

    public override string ToString()
    {
        return FullVersion;
    }

    public abstract int CompareTo(BaseVersion? other);
}
