namespace Numbers.Core.Services.Implementations;

public class DefaultTypeGenerator : ITypeGenerator
{
    public (Type type, string[] fieldNames, IDictionary<
        string,
        string
    > map) BuildRowTypeFromHeaders(string[] headers)
    {
        if (headers.Length > 100)
        {
            throw new Exception("Only less than 100 columns are supported");
        }

        var fieldNames = Enumerable.Range(0, 101).Select((index) => $"Column{index}").ToArray();
        return (
            typeof(CsvRow100),
            fieldNames,
            headers.Zip(fieldNames).ToDictionary((tuple => tuple.First), tuple => tuple.Second)
        );
    }
}
