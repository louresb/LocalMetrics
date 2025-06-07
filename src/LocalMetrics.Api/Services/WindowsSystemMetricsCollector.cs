using LocalMetrics.Api.Models;
using System.Diagnostics;

namespace LocalMetrics.Api.Services;

public class WindowsSystemMetricsCollector : ISystemMetricsCollector
{
    public SystemMetrics GetMetrics()
    {
        float cpuUsage = GetCpuUsage();
        float ramUsage = GetRamUsage();
        float diskUsage = GetDiskUsage();

        return new SystemMetrics
        {
            CpuUsage = cpuUsage,
            RamUsage = ramUsage,
            DiskUsage = diskUsage,
            Timestamp = DateTime.UtcNow
        };
    }

    private float GetCpuUsage()
    {
        using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        cpuCounter.NextValue();
        Thread.Sleep(500);
        return cpuCounter.NextValue();
    }

    private float GetRamUsage()
    {
        var output = ExecuteCommand("wmic OS get FreePhysicalMemory,TotalVisibleMemorySize /Value");

        var lines = output.Split('\n');
        float total = 0, free = 0;

        foreach (var line in lines)
        {
            if (line.StartsWith("FreePhysicalMemory"))
                free = float.Parse(line.Split('=')[1].Trim());
            else if (line.StartsWith("TotalVisibleMemorySize"))
                total = float.Parse(line.Split('=')[1].Trim());
        }

        if (total == 0) return 0;
        return 100f - ((free / total) * 100);
    }

    private float GetDiskUsage()
    {
        var drives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed);
        var systemDrive = drives.FirstOrDefault(d => d.Name == Path.GetPathRoot(Environment.SystemDirectory));
        if (systemDrive != null)
        {
            var total = systemDrive.TotalSize;
            var used = total - systemDrive.TotalFreeSpace;
            return (float)used / total * 100;
        }
        return 0;
    }

    private string ExecuteCommand(string command)
    {
        try
        {
            var escapedArgs = command.Replace("\"", "\\\"");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {escapedArgs}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }
        catch (Exception ex)
        {
            return $"Error executing command: {ex.Message}";
        }
    }
}
