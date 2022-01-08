namespace Community.Archives.Ar;

public static class HeaderExtensions
{
    public static bool IsValid(this in Header header)
    {
        return header.Magic.AsString() == "!<arch>\n";
    }
}
