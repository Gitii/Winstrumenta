using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PackageInstaller.Core.Services;

public readonly record struct DebianVersion : IComparable<DebianVersion>, IComparable
{
    public uint Epoch { get; }

    public string UpstreamVersion { get; }

    public string? DebianRevision { get; }

    public DebianVersion()
    {
        Epoch = 0;
        UpstreamVersion = String.Empty;
        DebianRevision = null;
    }

    public DebianVersion(string versionString)
    {
        var firstColon = versionString.IndexOf(':');

        if (firstColon >= 0)
        {
            if (
                !uint.TryParse(
                    versionString.AsSpan().Slice(0, firstColon),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var epoch
                )
            )
            {
                throw new Exception($"Version string '{versionString}' is malformed.");
            }

            Epoch = epoch;

            versionString = versionString.Substring(firstColon + 1);
        }
        else
        {
            Epoch = 0;
        }

        var lastHyphen = versionString.LastIndexOf('-');
        if (lastHyphen >= 0)
        {
            UpstreamVersion = versionString.Substring(0, lastHyphen);
            DebianRevision = versionString.Substring(lastHyphen + 1);
        }
        else
        {
            UpstreamVersion = versionString;
            DebianRevision = null;
        }

        CheckFormat(UpstreamVersion, "^[A-Za-z0-9.+:~-]+$");
        CheckFormat(DebianRevision, "^[A-Za-z0-9.+~]+$");
    }

    private void CheckFormat(string? strValue, string format)
    {
        if (strValue != null)
        {
            Regex reg = new Regex(format, RegexOptions.None, TimeSpan.FromSeconds(1));

            if (!reg.IsMatch(strValue))
            {
                throw new Exception($"String '{strValue}' does not match format {format}");
            }
        }
    }

    public override string ToString()
    {
        if (Epoch > 0 && !string.IsNullOrEmpty(DebianRevision))
        {
            return $"{Epoch}:{UpstreamVersion}-{DebianRevision}";
        }

        if (Epoch > 0 && string.IsNullOrEmpty(DebianRevision))
        {
            return $"{Epoch}:{UpstreamVersion}";
        }

        if (Epoch == 0 && !string.IsNullOrEmpty(DebianRevision))
        {
            return $"{UpstreamVersion}-{DebianRevision}";
        }

        return UpstreamVersion;
    }

    public int CompareTo(object? obj)
    {
        if (obj is DebianVersion)
        {
            return this.CompareTo((DebianVersion)obj);
        }

        return -1;
    }

    public int CompareTo(DebianVersion other)
    {
        if (Epoch < other.Epoch)
        {
            return -1;
        }

        if (Epoch > other.Epoch)
        {
            return 1;
        }

        var upstreamCmp = CompareVersion(UpstreamVersion, other.UpstreamVersion);
        if (upstreamCmp != 0)
        {
            return upstreamCmp;
        }

        var revisionCmp = CompareVersion(DebianRevision ?? "", other.DebianRevision ?? "");
        if (revisionCmp != 0)
        {
            return revisionCmp;
        }

        return 0;
    }

    private int CompareVersion(string left, string right)
    {
        do
        {
            var leftAlphaNumericPart = GetAlphaNumericPart(ref left);
            var rightAlphaNumericPart = GetAlphaNumericPart(ref right);

            var alphaNumericCmp = LexicalCompare(leftAlphaNumericPart, rightAlphaNumericPart);

            if (alphaNumericCmp != 0)
            {
                return alphaNumericCmp;
            }

            var leftNumericPart = GetNumericPart(ref left);
            var rightNumericPart = GetNumericPart(ref right);

            var numericCmp = leftNumericPart.CompareTo(rightNumericPart);

            if (numericCmp != 0)
            {
                return numericCmp;
            }
        } while (left.Length > 0 || right.Length > 0);

        return 0;
    }

    private int LexicalCompare(string left, string right)
    {
        for (int i = 0; i < Math.Max(left.Length, right.Length); i++)
        {
            char leftChar = i < left.Length ? left[i] : '0';
            char rightChar = i < right.Length ? right[i] : '0';

            var leftCharValue = GetOrderOfChar(leftChar);
            var rightCharValue = GetOrderOfChar(rightChar);

            if (leftCharValue != rightCharValue)
            {
                return leftCharValue.CompareTo(rightCharValue);
            }
        }

        return 0;
    }

    public int GetOrderOfChar(char c)
    {
        if (c == '~')
        {
            return -1; // must always be first
        }

        if (char.IsNumber(c))
        {
            return Convert.ToInt32(c) + 1; // avoid '0' == 0
        }

        if (Char.IsAscii(c) && Char.IsLetter(c))
        {
            return OrdinalValue(c);
        }

        return OrdinalValue(c) + 256; // always last

        int OrdinalValue(char character)
        {
            return Encoding.Unicode.GetBytes(character.ToString()).Sum((b) => b);
        }
    }

    private uint GetNumericPart(ref string value)
    {
        Regex reg = new Regex("^([0-9]+)", RegexOptions.None, TimeSpan.FromSeconds(1));

        var match = reg.Match(value);
        if (match.Success)
        {
            var start = match.Groups[1].Value;
            value = value.Substring(start.Length);
            return uint.Parse(start, NumberStyles.None, CultureInfo.InvariantCulture);
        }

        return 0;
    }

    private string GetAlphaNumericPart(ref string value)
    {
        Regex reg = new Regex("^([A-Za-z.+:~-]+)", RegexOptions.None, TimeSpan.FromSeconds(1));

        var match = reg.Match(value);
        if (match.Success)
        {
            var start = match.Groups[1].Value;
            value = value.Substring(start.Length);
            return start;
        }

        return String.Empty;
    }

    public static bool operator <(DebianVersion left, DebianVersion right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(DebianVersion left, DebianVersion right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(DebianVersion left, DebianVersion right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(DebianVersion left, DebianVersion right)
    {
        return left.CompareTo(right) >= 0;
    }
}
