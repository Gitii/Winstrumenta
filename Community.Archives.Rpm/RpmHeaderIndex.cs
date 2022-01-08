using System.Runtime.InteropServices;
using Community.Archives.Core;

namespace Community.Archives.Rpm;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Endianness(ByteOrder.BigEndian)]
public struct RpmHeaderIndex
{
    public int tag;
    public int type;
    public int offset;
    public int count;
};