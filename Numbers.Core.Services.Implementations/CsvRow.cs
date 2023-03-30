namespace Numbers.Core.Services.Implementations;

public class CsvRow : ICsvRow
{
    public CsvRow(ICsvRowCells cells)
    {
        Cells = cells;
    }

    public ICsvRowCells Cells { get; }
}
