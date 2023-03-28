using System.Globalization;
using Microsoft.Win32;
using Shared.Services;

namespace Numbers.Core.Services.Implementations;

public class ExcelImpl : IExcel
{
    private readonly ILauncher _launcher;

    public ExcelImpl(ILauncher launcher)
    {
        _launcher = launcher;
    }

#pragma warning disable MA0051
    private string? FindExcelPath()
#pragma warning restore MA0051
    {
        RegistryKey? excelKey = null;

        try
        {
            // First, try to find the Excel path for Office 2019 and later versions.
            excelKey = Registry.ClassesRoot.OpenSubKey("Excel.Application\\CurVer", false);
            if (excelKey != null)
            {
                var version = excelKey
                    .GetValue("")
                    ?.ToString()
                    ?.Replace(".", "_", StringComparison.OrdinalIgnoreCase);
                excelKey = Registry.ClassesRoot.OpenSubKey(
                    $"Excel.Application\\{version}\\InstallRoot",
                    false
                );

                if (TryValidateExcelPath(excelKey?.GetValue("Path"), out var path))
                {
                    return path;
                }
            }

            // Next, try for office 365
            excelKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Office");
            if (excelKey != null)
            {
                var officeVersionKeys = excelKey.GetSubKeyNames().Where((key) => float.TryParse(key, NumberStyles.Float, CultureInfo.InvariantCulture, out _)).ToArray();

                foreach (string officeSubKey in officeVersionKeys)
                {
                    var officeKey = excelKey.OpenSubKey(officeSubKey, false)!;

                    foreach (string subKeyName in officeKey.GetSubKeyNames())
                    {
                        if (subKeyName.StartsWith("Excel", StringComparison.OrdinalIgnoreCase))
                        {
                            var subKey = officeKey.OpenSubKey($"{subKeyName}\\InstallRoot", false);

                            if (TryValidateExcelPath(subKey?.GetValue("Path"), out var path))
                            {
                                return path;
                            }
                        }
                    }
                }
            }

            // Next, try to find the Excel path for Office 2016 and earlier versions.
            excelKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Office");
            if (excelKey != null)
            {
                foreach (string subKeyName in excelKey.GetSubKeyNames())
                {
                    if (subKeyName.StartsWith("Excel"))
                    {
                        var subKey = excelKey.OpenSubKey($"{subKeyName}\\InstallRoot", false);

                        if (TryValidateExcelPath(subKey?.GetValue("Path"), out var path))
                        {
                            return path;
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            excelKey?.Close();
        }

        return null;

        bool TryValidateExcelPath(object? officePath, out string fullExcelPath)
        {
            fullExcelPath = String.Empty;

            if (officePath is not string strOfficePath || string.IsNullOrWhiteSpace(strOfficePath))
            {
                return false;
            }

            fullExcelPath = Path.Combine(strOfficePath, "Excel.exe");

            return File.Exists(fullExcelPath);
        }
    }

    public async Task OpenAsync(string filePath)
    {
        var excelPath = FindExcelPath();
        if (excelPath != null)
        {
            await _launcher.LaunchWithAssociatedAppAsync(filePath, excelPath).ConfigureAwait(false);
        }
    }
}
