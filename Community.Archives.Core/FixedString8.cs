using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Community.Archives.Core;

/// <summary>
/// A 8-byte buffer which is encoded as ascii string.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct FixedString8
{
    public byte Data1;
    public byte Data2;
    public byte Data3;
    public byte Data4;
    public byte Data5;
    public byte Data6;
    public byte Data7;
    public byte Data8;

    public override string ToString()
    {
        return AsString();
    }

    public byte[] AsByteArray()
    {
        return new[] { Data1, Data2, Data3, Data4, Data5, Data6, Data7, Data8 };
    }

    public string AsString()
    {
        return Encoding.ASCII.GetString(AsByteArray());
    }

    public long DecodeStringAsLong(bool isHexString = false)
    {
        if (isHexString)
        {
            return Int64.Parse(ToString(), NumberStyles.HexNumber);
        }
        else
        {
            return Int64.Parse(ToString());
        }
    }
};