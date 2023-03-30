using System.Collections;
using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Numbers.Core.Services.Implementations;

public class Csv : ICsv
{
    private ITypeGenerator _typeGenerator;

    public Csv(ITypeGenerator typeGenerator)
    {
        _typeGenerator = typeGenerator;
    }

    public async Task<(ICsvTable, FileEncoding)> ParseAsync(Stream inputStream)
    {
        var cachedStream = await ReadAsync(inputStream).ConfigureAwait(false);
        var headers = await ReadColumnHeadersAsync(cachedStream).ConfigureAwait(false);
        var (headerType, fieldNames, map) = _typeGenerator.BuildRowTypeFromHeaders(headers);

        var (rows, encoding) = await ReadRowsAsync(cachedStream, headerType, map)
            .ConfigureAwait(false);

        return (
            new CsvTable()
            {
                HeaderNames = Array.AsReadOnly(headers),
                FieldNames = Array.AsReadOnly(fieldNames),
                Rows = rows,
                RecordType = headerType,
                RecordColumnMapping = map
            },
            encoding
        );
    }

    public async Task SaveAsAsync(ICsvTable data, FileEncoding encoding, string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            LineBreakInQuotedFieldIsBadData = false,
            AllowComments = true,
            Delimiter = encoding.Delimiter,
            Encoding = encoding.Encoding,
            Quote = encoding.QuoteCharacter,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim,
            ShouldQuote = _ => encoding.QuoteCharacter != '\0'
        };

        using var writer = new StreamWriter(
            File.Open(filePath, FileMode.Create),
            encoding.Encoding
        );
        using var csv = new CsvWriter(writer, config);

        RegisterRecord(data.RecordType, data.RecordColumnMapping, csv.Context);

        await csv.WriteRecordsAsync(data.Rows.Select((r) => r.Cells) as IEnumerable)
            .ConfigureAwait(false);

        await csv.FlushAsync().ConfigureAwait(false);
    }

    public Task SaveAsAsync(IGlobalContext context)
    {
        var filePath =
            context.FilePath
            ?? throw new ArgumentException(
                "file path in context must not be null",
                nameof(context)
            );

        var encoding =
            context.FileEncoding
            ?? throw new ArgumentException("encoding in context must not be null", nameof(context));

        var table =
            context.Table
            ?? throw new ArgumentException("csv data in context must not be null", nameof(context));

        return SaveAsAsync(table, encoding, filePath);
    }

    private async Task<(List<ICsvRow>, FileEncoding)> ReadRowsAsync(
        MemoryStream stream,
        Type headerType,
        IDictionary<string, string> columnMap
    )
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            LineBreakInQuotedFieldIsBadData = false,
            AllowComments = true,
            Delimiter = ",",
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim,
        };

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);

        RegisterRecord(headerType, columnMap, csv.Context);

        await csv.ReadAsync().ConfigureAwait(true);
        csv.ReadHeader();

        var records = csv.GetRecords(headerType).ToList();

        return (
            records.Select((cells) => (new CsvRow((ICsvRowCells)cells)) as ICsvRow).ToList(),
            new FileEncoding()
            {
                Delimiter = csv.Parser.Delimiter,
                Encoding = csv.Configuration.Encoding,
                QuoteCharacter = csv.Configuration.Quote
            }
        );
    }

    private static void RegisterRecord(
        Type headerType,
        IDictionary<string, string> columnMap,
        CsvContext context
    )
    {
        context.UnregisterClassMap();
        var typeMap = context.AutoMap(headerType);
        typeMap.MemberMaps.Clear();
        typeMap.ParameterMaps.Clear();
        typeMap.ReferenceMaps.Clear();

        foreach ((string columnName, string fieldName) in columnMap)
        {
            typeMap
                .Map(
                    headerType,
                    headerType.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public)
                )
                .Name(columnName)
                .TypeConverter<StringConverter>();
        }
    }

    private async Task<MemoryStream> ReadAsync(Stream stream)
    {
        var output = TryGetStreamLength(stream, out var length)
          ? new MemoryStream(new byte[length], true)
          : new MemoryStream();

        await stream.CopyToAsync(output).ConfigureAwait(false);

        output.Position = 0;
        return output;
    }

    private bool TryGetStreamLength(Stream stream, out long length)
    {
        try
        {
            length = stream.Length;

            return true;
        }
        catch
        {
            length = 0;
            return false;
        }
    }

    private async Task<string[]> ReadColumnHeadersAsync(MemoryStream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        using var csv = new CsvReader(
            reader,
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                LineBreakInQuotedFieldIsBadData = false,
                AllowComments = true,
                DetectDelimiter = true,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim
            }
        );
        await csv.ReadAsync().ConfigureAwait(false);
        csv.ReadHeader();

        stream.Position = 0;

        return csv.HeaderRecord ?? Array.Empty<string>();
    }
}
