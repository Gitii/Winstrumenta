using System.Runtime.InteropServices;
using Community.Archives.Core;

namespace Community.Archives.Ar;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public unsafe struct Header
{
    public FixedString8 Magic;
    public FixedString16 FileIdentifier;
    public fixed byte LastModDate[12];
    public fixed byte OwnerUserId[6];
    public fixed byte OwnerGroupId[6];
    public ulong FileMode;
    public fixed byte FileSize[10];
    public fixed byte EndChar[2];
    public FixedString4 Version;
};