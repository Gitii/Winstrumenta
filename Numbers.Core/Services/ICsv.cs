namespace Numbers.Core.Services;

public interface ICsv
{
    Task<(ICsvTable, FileEncoding)> ParseAsync(Stream reader);
    Task SaveAsAsync(ICsvTable data, FileEncoding encoding, string filePath);
    Task SaveAsAsync(IGlobalContext context);
}
