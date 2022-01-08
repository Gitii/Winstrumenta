using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Community.Archives.Core;

/// <summary>
/// A 12-byte buffer which is encoded as ascii string.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct FixedString12
{
    public byte Data1;
    public byte Data2;
    public byte Data3;
    public byte Data4;
    public byte Data5;
    public byte Data6;
    public byte Data7;
    public byte Data8;
    public byte Data9;
    public byte Data10;
    public byte Data11;
    public byte Data12;

    public override string ToString()
    {
        return AsString();
    }

    public byte[] AsByteArray()
    {
        return new[]
        {
            Data1,
            Data2,
            Data3,
            Data4,
            Data5,
            Data6,
            Data7,
            Data8,
            Data9,
            Data10,
            Data11,
            Data12
        };
    }

    public string AsString()
    {
        return Encoding.ASCII.GetString(AsByteArray());
    }

    public long DecodeStringAsLong(bool isHexString = false)
    {
        if (isHexString)
        {
            return Int64.Parse(AsString(), NumberStyles.HexNumber);
        }
        else
        {
            return Int64.Parse(AsString());
        }
    }

    public long DecodeStringAsOctalLong()
    {
        var strNumber = AsString().Trim('\0').TrimStart('0');

        if (string.IsNullOrEmpty(strNumber))
        {
            return 0;
        }

        return Convert.ToInt64(strNumber, 8);
    }
};