using System.Runtime.InteropServices;
using Community.Archives.Core;

namespace Community.Archives.Tar;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public unsafe struct Header
{
    public fixed byte FileName[HeaderExtensions.FILE_NAME_LENGTH];
    public FixedString8 FileMode;
    public FixedString8 OwnerUserId;
    public FixedString8 OwnerGroupId;
    public FixedString12 FileSize;
    public FixedString12 LastModDate;
    public FixedString8 CheckSum;
    public byte FileType;
    public fixed byte LinkTarget[HeaderExtensions.FILE_NAME_LENGTH];

    public fixed byte Padding[255];

    public bool IsEmpty()
    {
        for (int i = 0; i < HeaderExtensions.FILE_NAME_LENGTH; i++)
        {
            if (FileName[i] != '\0')
            {
                return false;
            }
        }

        for (int i = 0; i < HeaderExtensions.FILE_NAME_LENGTH; i++)
        {
            if (LinkTarget[i] != '\0')
            {
                return false;
            }
        }

        return true;
    }

    public bool IsFile()
    {
        return FileType == '0' || FileType == '\0';
    }
};