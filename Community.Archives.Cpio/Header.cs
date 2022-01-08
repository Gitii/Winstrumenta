using System.Runtime.InteropServices;
using Community.Archives.Core;

namespace Community.Archives.Cpio;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public unsafe struct Header
{
    public fixed byte c_magic[6];
    public FixedString8 c_ino;
    public FixedString8 c_mode;
    public FixedString8 c_uid;
    public FixedString8 c_gid;
    public FixedString8 c_nlink;
    public FixedString8 c_mtime;
    public FixedString8 c_filesize;
    public FixedString8 c_devmajor;
    public FixedString8 c_devminor;
    public FixedString8 c_rdevmajor;
    public FixedString8 c_rdevminor;
    public FixedString8 c_namesize;
    public FixedString8 c_checksum;
};