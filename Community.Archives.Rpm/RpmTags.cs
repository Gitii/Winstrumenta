namespace Community.Archives.Rpm;

public struct RpmTags
{
    //[RpmTag(62, IndexType.RPM_BIN_TYPE, 16, false)]
    //public byte[] HeaderSignatures;

    //[RpmTag(63, IndexType.RPM_BIN_TYPE, 16, false)]
    //public byte[] HeaderImmutable;

    //[RpmTag(100, IndexType.RPM_I18NSTRING_TYPE, 0, false)]
    //public string[] HeaderI18nTable;

    [RpmTag(1000, IndexType.RPM_INT32_TYPE, 1, true)]
    public int SignatureTagSize;

    [RpmTag(1007, IndexType.RPM_INT32_TYPE, 1, false)]
    public int SignatureTagPayloadSize;

    //[RpmTag(269, IndexType.RPM_STRING_TYPE, 1, false)]
    //public string SignatureTagSha1;

    //[RpmTag(1004, IndexType.RPM_BIN_TYPE, 16, false)]
    //public byte[] SignatureTagMd5;

    //[RpmTag(267, IndexType.RPM_BIN_TYPE, 65, false)]
    //public byte[] SignatureTagDsa;

    //[RpmTag(268, IndexType.RPM_BIN_TYPE, 1, false)]
    //public byte SignatureTagRsa;

    //[RpmTag(1002, IndexType.RPM_BIN_TYPE, 1, false)]
    //public byte SignatureTagPgp;

    //[RpmTag(1005, IndexType.RPM_BIN_TYPE, 65, false)]
    //public byte SignatureTagGpg;

    [RpmTag(1000, IndexType.RPM_STRING_TYPE, 1, true)]
    public string Name;

    [RpmTag(1001, IndexType.RPM_STRING_TYPE, 1, true)]
    public string Version;

    [RpmTag(1002, IndexType.RPM_STRING_TYPE, 1, true)]
    public string Release;

    [RpmTag(1004, IndexType.RPM_I18NSTRING_TYPE, 1, true)]
    public string Summary;

    [RpmTag(1005, IndexType.RPM_I18NSTRING_TYPE, 1, true)]
    public string Description;

    [RpmTag(1009, IndexType.RPM_INT32_TYPE, 1, true)]
    public int Size;

    [RpmTag(1010, IndexType.RPM_STRING_TYPE, 1, false)]
    public string Distribution;

    [RpmTag(1011, IndexType.RPM_STRING_TYPE, 1, false)]
    public string Vendor;

    [RpmTag(1014, IndexType.RPM_STRING_TYPE, 1, true)]
    public string License;

    [RpmTag(1015, IndexType.RPM_STRING_TYPE, 1, false)]
    public string Packager;

    [RpmTag(1016, IndexType.RPM_I18NSTRING_TYPE, 1, true)]
    public string Group;

    [RpmTag(1020, IndexType.RPM_STRING_TYPE, 1, false)]
    public string Url;

    [RpmTag(1021, IndexType.RPM_STRING_TYPE, 1, false)]
    public string Os;

    [RpmTag(1022, IndexType.RPM_STRING_TYPE, 1, true)]
    public string Architecture;

    [RpmTag(1044, IndexType.RPM_STRING_TYPE, 1, false)]
    public string SourceRpm;

    [RpmTag(1046, IndexType.RPM_INT32_TYPE, 1, false)]
    public int ArchiveSize;

    [RpmTag(1064, IndexType.RPM_STRING_TYPE, 1, false)]
    public string RpmVersion;

    [RpmTag(1094, IndexType.RPM_STRING_TYPE, 1, false)]
    public string Cookie;

    [RpmTag(1123, IndexType.RPM_STRING_TYPE, 1, false)]
    public string DistUrl;

    [RpmTag(1124, IndexType.RPM_STRING_TYPE, 1, true)]
    public string PayloadFormat;

    [RpmTag(1125, IndexType.RPM_STRING_TYPE, 1, true)]
    public string PayloadCompressor;

    [RpmTag(1126, IndexType.RPM_STRING_TYPE, 1, true)]
    public string PayloadFlags;

    public IReadOnlyDictionary<string, string> GetFields()
    {
        var thisRef = __makeref(this);
        var fieldDict = new Dictionary<string, string>();

        foreach (var fieldInfo in GetType().GetFields())
        {
            var value = fieldInfo.GetValueDirect(thisRef);
            if (value != null)
            {
                fieldDict.Add(fieldInfo.Name, value.ToString() ?? string.Empty);
            }
        }

        return fieldDict;
    }
}