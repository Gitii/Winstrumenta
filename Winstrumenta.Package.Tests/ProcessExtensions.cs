using System.Diagnostics;
using System.Management;

namespace Winstrumenta.Package.Tests;

public static class ProcessExtensions
{
    public static IList<Process> GetChildProcesses(this Process process)
    {
        return new ManagementObjectSearcher(
            $"Select * From Win32_Process Where ParentProcessID={process.Id}"
        )
            .Get()
            .Cast<ManagementObject>()
            .Select(mo => Process.GetProcessById(Convert.ToInt32(mo["ProcessID"])))
            .ToList();
    }
}
