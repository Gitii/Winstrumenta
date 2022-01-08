using System.Runtime.InteropServices;
using Community.Archives.Core;

namespace Community.Archives.Rpm;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
[Endianness(ByteOrder.BigEndian)]
public unsafe struct RpmLead
{
    public fixed byte magic[RpmLeadHelpers.MAGIC_RPM_LEAD_SIZE];
    public byte major;
    public byte minor;
    public short type;
    public short archnum;
    public fixed byte name[RpmLeadHelpers.NAME_SIZE];
    public short osnum;
    public short signature_type;
    public fixed byte reserved[16];
};
