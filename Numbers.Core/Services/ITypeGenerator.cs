using System.Text.RegularExpressions;

namespace Numbers.Core.Services;

public interface ITypeGenerator
{
    public (Type type, string[] fieldNames, IDictionary<string, string> map) BuildRowTypeFromHeaders(string[] headers);

    public static string[] SanitizeHeaders(string[] headers)
    {
        var whiteSpaceMatch = new Regex("\\s", RegexOptions.None, TimeSpan.FromMilliseconds(100));
        var notLetterOrNumberMatch = new Regex(
            "[^\\w\\d]",
            RegexOptions.None,
            TimeSpan.FromMilliseconds(100)
        );

        var sanitizedHeaders = headers.Select(Sanitize).ToArray();

        for (var i = 0; i < sanitizedHeaders.Length; i++)
        {
            var sanitizedHeader = sanitizedHeaders[0];
            var numberOfDuplicates = sanitizedHeaders.Count(
                (otherHeader) => otherHeader == sanitizedHeader
            );
            if (numberOfDuplicates > 1)
            {
                var suffixCounter = 1;

                for (int j = 0; j < sanitizedHeaders.Length; j++)
                {
                    if (i != j && sanitizedHeaders[j] == sanitizedHeader)
                    {
                        sanitizedHeaders[j] += suffixCounter++;
                    }
                }
            }
        }

        return sanitizedHeaders;

        string Sanitize(string identifier)
        {
            var cleanId = notLetterOrNumberMatch.Replace(
                whiteSpaceMatch.Replace(identifier.Trim(), "_"),
                ""
            );

            if (cleanId.Length == 0)
            {
                cleanId = "Empty";
            }
            else if (!char.IsLetter(cleanId[0]))
            {
                cleanId = "_" + cleanId;
            }

            return cleanId;
        }
    }
}
