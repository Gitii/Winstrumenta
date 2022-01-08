using System.Runtime.InteropServices;
using Community.Archives.Core;

namespace Community.Archives.Rpm;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Endianness(ByteOrder.BigEndian)]
public unsafe struct RpmHeader
{
    public fixed byte magic[4];
    public fixed byte reserved[4];
    public int nindex;
    public int hsize;
};
