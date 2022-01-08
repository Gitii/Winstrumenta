using System.Text;

namespace Community.Archives.Cpio;

public static class HeaderExtensions
{
    public static readonly byte[] MAGIC = Encoding.ASCII.GetBytes("070701");

    public static unsafe bool IsValid(this Header header)
    {
        if (new IntPtr(header.c_magic) == IntPtr.Zero)
        {
            return false;
        }

        for (var i = 0; i < MAGIC.Length; i++)
        {
            if (header.c_magic[i] != MAGIC[i])
            {
                return false;
            }
        }

        return true;
    }
}