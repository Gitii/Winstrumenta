namespace Numbers.Core.Services;

public interface ICsvTable
{
    IReadOnlyList<string> HeaderNames { get; }
    IReadOnlyList<string> FieldNames { get; }
    IReadOnlyList<ICsvRow> Rows { get; }

    Type RecordType { get; }

    IDictionary<string, string> RecordColumnMapping { get; }
}
