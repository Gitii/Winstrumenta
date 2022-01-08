using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Community.Archives.Core;

/// <summary>
/// A 6-byte buffer which is encoded as ascii string.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct FixedString6
{
    public byte Data1;
    public byte Data2;
    public byte Data3;
    public byte Data4;
    public byte Data5;
    public byte Data6;

    /// <summary>
    /// Returns a string representation of the buffer.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return AsString();
    }

    /// <summary>
    /// Returns the buffer (unchanged).
    /// </summary>
    /// <returns></returns>
    public byte[] AsByteArray()
    {
        return new[] { Data1, Data2, Data3, Data4, Data5, Data6 };
    }

    /// <summary>
    /// Returns a string representation of the buffer.
    /// </summary>
    /// <returns></returns>
    public string AsString()
    {
        return Encoding.ASCII.GetString(AsByteArray());
    }

    /// <summary>
    /// Returns value of <seealso cref="AsString"/> parsed as long.
    /// </summary>
    /// <param name="isHexString">If <c>true</c>, the string is parsed as hex number. Otherwise as decimal number.</param>
    /// <returns></returns>
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