namespace Community.Archives.Rpm;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class RpmTagAttribute : Attribute
{
    public RpmTagAttribute(int tagValue, IndexType type, int count, bool isRequired)
    {
        TagValue = tagValue;
        Count = count;
        IsRequired = isRequired;
        Type = type;
    }

    public int TagValue { get; }

    public IndexType Type { get; }

    public int Count { get; }

    public bool IsRequired { get; }
}