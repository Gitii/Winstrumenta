using System.Runtime.InteropServices;

namespace Community.Archives.Rpm;

public static class RpmLeadHelpers
{
    public static readonly byte[] MAGIC_RPM_LEAD = { 0xed, 0xab, 0xee, 0xdb };

    public const int MAGIC_RPM_LEAD_SIZE = 4;

    public const int NAME_SIZE = 66;

    public static unsafe bool IsValid(this RpmLead lead)
    {
        if (new IntPtr(lead.magic) == IntPtr.Zero)
        {
            return false;
        }

        for (var i = 0; i < MAGIC_RPM_LEAD_SIZE; i++)
        {
            if (lead.magic[i] != MAGIC_RPM_LEAD[i])
            {
                return false;
            }
        }

        return true;
    }

    public static unsafe string GetName(this RpmLead lead)
    {
        if (new IntPtr(lead.name) == IntPtr.Zero)
        {
            throw new Exception("Invalid struct");
        }

        return Marshal.PtrToStringAnsi((IntPtr)lead.name, NAME_SIZE).TrimEnd('\0');
    }
}