using System.Buffers.Binary;
using System.Text;

namespace Community.Archives.Rpm;

public static class RpmHeaderIndexExtensions
{
    public static IndexType GetIndexType(this in RpmHeaderIndex index)
    {
        var indexType = (IndexType)index.type;

        if (!Enum.IsDefined<IndexType>(indexType))
        {
            throw new Exception($"Invalid index type {index.type}");
        }

        return indexType;
    }

    public static void AssertTypeAndCount(
        this in RpmHeaderIndex index,
        IndexType indexType,
        int count = 1
    )
    {
        if (index.GetIndexType() != indexType)
        {
            throw new Exception(
                $"Expected index type to be {indexType} but it's {index.GetIndexType()}"
            );
        }

        if (index.count != count)
        {
            throw new Exception($"Expected count to be {count} but it's " + index.count);
        }
    }

    public static object? GetValue(
        this in RpmHeaderIndex index,
        IndexType tagAttrType,
        int tagAttrCount,
        byte[] data
    )
    {
        switch (tagAttrType)
        {
            case IndexType.RPM_NULL_TYPE:
                throw new NotImplementedException();
            case IndexType.RPM_CHAR_TYPE:
                return index.GetChar(data);
            case IndexType.RPM_INT8_TYPE:
                return index.GetByte(data);
            case IndexType.RPM_INT16_TYPE:
                return index.GetShort(data);
            case IndexType.RPM_INT32_TYPE:
                return index.GetInteger(data);
            case IndexType.RPM_INT64_TYPE:
                return index.GetLong(data);
            case IndexType.RPM_STRING_TYPE:
                return index.GetString(data);
            case IndexType.RPM_BIN_TYPE:
                return index.GetByte(data);
            case IndexType.RPM_STRING_ARRAY_TYPE:
                return index.GetStrings(data);
            case IndexType.RPM_I18NSTRING_TYPE:
                throw new NotSupportedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(tagAttrType), tagAttrType, null);
        }
    }

    public static byte GetByte(this in RpmHeaderIndex index, byte[] data)
    {
        AssertTypeAndCount(index, IndexType.RPM_BIN_TYPE);

        return data[index.offset];
    }

    public static char GetChar(this in RpmHeaderIndex index, byte[] data)
    {
        AssertTypeAndCount(index, IndexType.RPM_CHAR_TYPE);

        return (char)data[index.offset];
    }

    public static sbyte GetSByte(this in RpmHeaderIndex index, byte[] data)
    {
        AssertTypeAndCount(index, IndexType.RPM_INT8_TYPE);

        return (sbyte)data[index.offset];
    }

    public static short GetShort(this in RpmHeaderIndex index, byte[] data)
    {
        AssertTypeAndCount(index, IndexType.RPM_INT16_TYPE);

        return BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(index.offset, 2));
    }

    public static int GetInteger(this in RpmHeaderIndex index, byte[] data)
    {
        AssertTypeAndCount(index, IndexType.RPM_INT32_TYPE);

        return BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(index.offset, 4));
    }

    public static long GetLong(this in RpmHeaderIndex index, byte[] data)
    {
        AssertTypeAndCount(index, IndexType.RPM_INT64_TYPE);

        return BinaryPrimitives.ReadInt64BigEndian(data.AsSpan(index.offset, 8));
    }

    public static string GetString(this in RpmHeaderIndex index, byte[] data)
    {
        AssertTypeAndCount(index, IndexType.RPM_STRING_TYPE);
        var endOfString = System.Array.IndexOf<byte>(data, 0, index.offset);

        if (endOfString <= 0)
        {
            return String.Empty;
        }

        var size = endOfString - index.offset;

        return Encoding.ASCII.GetString(data.AsSpan(index.offset, size));
    }

    public static string[] GetStrings(this in RpmHeaderIndex index, byte[] data)
    {
        AssertTypeAndCount(index, IndexType.RPM_STRING_ARRAY_TYPE);
        var strings = new List<string>(0);

        int size;
        int offset = index.offset;

        while ((size = System.Array.IndexOf(data, 0, offset)) > 0)
        {
            strings.Add(Encoding.ASCII.GetString(data.AsSpan(offset, size)));

            offset += size + 1;
        }

        return strings.ToArray();
    }
}