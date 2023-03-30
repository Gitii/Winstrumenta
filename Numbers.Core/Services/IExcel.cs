namespace Numbers.Core.Services;

public interface IExcel
{
    public Task OpenAsync(string filePath);
}
