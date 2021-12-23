namespace PackageInstaller.Core.Services;

/// <summary>
/// An exception which is used to signal a parse failure.
/// </summary>
public class ParseError : Exception
{
    public ParseError(string message = "Failed to parse") : base(message) { }
}

