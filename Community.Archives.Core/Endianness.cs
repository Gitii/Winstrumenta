namespace Community.Archives.Core;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
public class Endianness : Attribute
{
    public Endianness(ByteOrder byteOrder)
    {
        ByteOrder = byteOrder;
    }

    public ByteOrder ByteOrder { get; }
}