using System.Text.RegularExpressions;

namespace Community.Archives.Core
{
    public static class RegexExtensions
    {
        public static bool IsMatch(this Regex[] regexList, string value)
        {
            return regexList.Any((r) => r.IsMatch(value));
        }

        public static bool IsMatch(this string[] strRegexList, string value)
        {
            return strRegexList.Any((r) => Regex.IsMatch(value, r));
        }
    }
}