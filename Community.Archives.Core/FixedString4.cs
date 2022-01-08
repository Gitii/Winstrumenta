using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Community.Archives.Core;

/// <summary>
/// A 4-byte buffer which is encoded as ascii string.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct FixedString4
{
    public byte Data1;
    public byte Data2;
    public byte Data3;
    public byte Data4;

    public override string ToString()
    {
        return AsString();
    }

    public byte[] AsByteArray()
    {
        return new[] { Data1, Data2, Data3, Data4 };
    }

    public string AsString()
    {
        return Encoding.ASCII.GetString(AsByteArray());
    }

    public long DecodeStringAsInteger(bool isHexString = false)
    {
        if (isHexString)
        {
            return Int32.Parse(ToString(), NumberStyles.HexNumber);
        }
        else
        {
            return Int32.Parse(ToString());
        }
    }
};
