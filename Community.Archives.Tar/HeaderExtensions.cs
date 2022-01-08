using System.Text;

namespace Community.Archives.Tar;

public static class HeaderExtensions
{
    public const int FILE_NAME_LENGTH = 100;

    public static unsafe string GetName(this in Header header)
    {
        fixed (byte* fileNamePtr = header.FileName)
        {
            return Encoding.ASCII.GetString(new ReadOnlySpan<byte>(fileNamePtr, FILE_NAME_LENGTH)).TrimEnd('\0');
        }
    }
}