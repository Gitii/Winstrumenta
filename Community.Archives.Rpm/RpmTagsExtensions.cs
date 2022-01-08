using System.Reflection;
using Community.Archives.Core;

namespace Community.Archives.Rpm;

public static class RpmTagsExtensions
{
    public static RpmTags Parse(RpmHeaderIndex[] indices, byte[] data)
    {
        var tags = new RpmTags();

        foreach (var fieldInfo in typeof(RpmTags).GetFields())
        {
            var tagAttr = fieldInfo.GetCustomAttribute<RpmTagAttribute>();

            if (tagAttr == null)
            {
                continue;
            }

            var index = indices.FirstOrNullable((i) => i.tag == tagAttr.TagValue);

            if (!index.HasValue)
            {
                continue;
            }

            if (tagAttr.Type != IndexType.RPM_STRING_TYPE)
            {
                continue;
            }

            var value = index.Value.GetValue(tagAttr.Type, tagAttr.Count, data);

            if (value != null)
            {
                fieldInfo.SetValueDirect(__makeref(tags), value);
            }
        }

        return tags;
    }
}
