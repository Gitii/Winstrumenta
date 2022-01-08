using System.Runtime.InteropServices;
using Community.Archives.Core;

namespace Community.Archives.Ar;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public unsafe struct FileEntry
{
    public FixedString16 FileIdentifier;
    public fixed byte LastModDate[12];
    public fixed byte OwnerUserId[6];
    public fixed byte OwnerGroupId[6];
    public ulong FileMode;
    public FixedString10 FileSize;
    public fixed byte EndChar[2];
};
