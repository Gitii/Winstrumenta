namespace Numbers.Core.Services;

class CsvTable : ICsvTable
{
    public IReadOnlyList<string> HeaderNames { get; init; } = new List<string>(0);
    public IReadOnlyList<string> FieldNames { get; init; } = new List<string>(0);
    public IReadOnlyList<ICsvRow> Rows { get; init; } = new List<ICsvRow>(0);
    public Type RecordType { get; init; } = typeof(object);
    public IDictionary<string, string> RecordColumnMapping { get; set; } =
        new Dictionary<string, string>();
}
