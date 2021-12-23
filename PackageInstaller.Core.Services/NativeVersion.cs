using System.Text;
using System.Text.RegularExpressions;

namespace PackageInstaller.Core.Services;

/// <summary>
/// Represents a Debian package version, with native Python comparison
///
/// Based on https://salsa.debian.org/python-debian-team/python-debian/-/blob/master/lib/debian/debian_support.py
/// </summary>
public class NativeVersion : BaseVersion
{
    public Regex re_all_digits_or_not = new Regex(@"\d+|\D+");

    public Regex re_digits = new Regex(@"\d+");

    public Regex re_digit = new Regex(@"\d");

    public Regex re_alpha = new Regex("[A-Za-z]");

    public override int CompareTo(BaseVersion? other)
    {
        if (other == null)
        {
            return 1;
        }
        
        // Convert other into an instance of BaseVersion if it's not already.
        // (All we need is epoch, upstream_version, and debian_revision
        // attributes, which BaseVersion gives us.) Requires other's string
        // representation to be the raw version.
        // If other is not defined, then the current version is bigger
        var lepoch = ParseString(Epoch, "0");
        var repoch = ParseString(other.Epoch, "0");
        if (lepoch < repoch)
        {
            return -1;
        }
        if (lepoch > repoch)
        {
            return 1;
        }
        var res = _version_cmp_part(
            FirstNotEmpty(UpstreamVersion, "0"),
            FirstNotEmpty(other.UpstreamVersion, "0")
        );

        if (res != 0)
        {
            return res;
        }

        return _version_cmp_part(
            FirstNotEmpty(DebianRevision, "0"),
            FirstNotEmpty(other.DebianRevision, "0")
        );
    }

    // Return an integer value for character x
    public int _order(char x)
    {
        if (x == '~')
        {
            return -1;
        }
        if (char.IsNumber(x))
        {
            return Convert.ToInt32(x) + 1;
        }
        if (re_alpha.IsMatch(x.ToString()))
        {
            return Ord(x);
        }
        return Ord(x) + 256;

        int Ord(char character)
        {
            return Encoding.Unicode.GetBytes(character.ToString()).Sum((b) => b);
        }
    }

    public int _version_cmp_string(string va, string vb)
    {
        var la = (from x in va select _order(x)).ToList();
        var lb = (from x in vb select _order(x)).ToList();

        for (int i = 0; i < Math.Max(la.Count, lb.Count); i++)
        {
            var a = i < la.Count ? la[i] : 0;
            var b = i < lb.Count ? lb[i] : 0;

            if (a < b)
            {
                return -1;
            }

            if (a > b)
            {
                return 1;
            }
        }

        return 0;
    }

    public int _version_cmp_part(string va, string vb)
    {
        var la = re_all_digits_or_not.Matches(va);
        var lb = re_all_digits_or_not.Matches(vb);

        for (int i = 0; i < Math.Max(la.Count, lb.Count); i++)
        {
            var a = i < la.Count ? la[i].Value : "0";
            var b = i < lb.Count ? lb[i].Value : "0";

            if (re_digits.IsMatch(a) && re_digits.IsMatch(b))
            {
                var aval = Convert.ToInt32(a);
                var bval = Convert.ToInt32(b);
                if (aval < bval)
                {
                    return -1;
                }

                if (aval > bval)
                {
                    return 1;
                }
            }
            else
            {
                var res = _version_cmp_string(a, b);
                if (res != 0)
                {
                    return res;
                }
            }
        }

        return 0;
    }

    public NativeVersion(string version) : base(version) { }

    private static string FirstNotEmpty(string strValue, string alternative)
    {
        if (strValue.Length == 0)
        {
            return alternative;
        }

        return strValue;
    }

    private static int ParseString(string strValue, string alternative)
    {
        return Convert.ToInt32(FirstNotEmpty(strValue, alternative));
    }
}
