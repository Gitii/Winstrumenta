using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Community.Archives.Core;

/// <summary>
/// A 16-byte buffer which is encoded as ascii string.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct FixedString16
{
    public FixedString8 Data1;
    public FixedString8 Data2;

    public override string ToString()
    {
        return AsString();
    }

    public byte[] AsByteArray()
    {
        return Data1.AsByteArray().Concat(Data2.AsByteArray()).ToArray();
    }

    public string AsString()
    {
        return Encoding.ASCII.GetString(AsByteArray());
    }

    public long DecodeStringAsLong(bool isHexString = false)
    {
        return Data1.DecodeStringAsLong(isHexString);
    }
};