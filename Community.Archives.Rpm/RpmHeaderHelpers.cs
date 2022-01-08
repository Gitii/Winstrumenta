namespace Community.Archives.Rpm;

public static class RpmHeaderHelpers
{
    public static readonly byte[] MAGIC_RPM_HEADER = { 0x8e, 0xad, 0xe8 };

    public const int MAGIC_RPM_HEADER_SIZE = 3;

    public static unsafe bool IsValid(this RpmHeader lead)
    {
        if (new IntPtr(lead.magic) == IntPtr.Zero)
        {
            return false;
        }

        for (var i = 0; i < MAGIC_RPM_HEADER_SIZE; i++)
        {
            if (lead.magic[i] != MAGIC_RPM_HEADER[i])
            {
                return false;
            }
        }

        return true;
    }
}