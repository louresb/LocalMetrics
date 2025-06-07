using LocalMetrics.Api.Models;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LocalMetrics.Api.Services;

public class MacSystemMetricsCollector : ISystemMetricsCollector
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
        var output = ExecuteCommand("top -l 1");
        var cpuLine = output.Split('\n').FirstOrDefault(line => line.Contains("CPU usage"));

        if (cpuLine == null)
            return 0f;

        Console.WriteLine("Raw CPU line: " + cpuLine); 

        var match = Regex.Match(cpuLine, @"(\d+\.\d+)% user, (\d+\.\d+)% sys");

        if (match.Success)
        {
            var user = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var sys = float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            return user + sys;
        }

        return 0f;
    }

    private float GetRamUsage()
    {
        var output = ExecuteCommand("vm_stat");

        var pageSizeLine = output.Split('\n').FirstOrDefault(line => line.Contains("page size of"));
        int pageSize = 4096;

        if (pageSizeLine != null && int.TryParse(pageSizeLine.Split("page size of")[1].Trim().Split(' ')[0], out int parsedSize))
        {
            pageSize = parsedSize;
        }

        long free = 0, active = 0, inactive = 0, speculative = 0, wired = 0;

        foreach (var line in output.Split('\n'))
        {
            if (line.StartsWith("Pages free:")) free = ParseVmStat(line);
            else if (line.StartsWith("Pages active:")) active = ParseVmStat(line);
            else if (line.StartsWith("Pages inactive:")) inactive = ParseVmStat(line);
            else if (line.StartsWith("Pages speculative:")) speculative = ParseVmStat(line);
            else if (line.StartsWith("Pages wired down:")) wired = ParseVmStat(line);
        }

        long total = free + active + inactive + speculative + wired;
        long used = active + inactive + speculative + wired;

        return total == 0 ? 0 : (float)used / total * 100;
    }

    private long ParseVmStat(string line)
    {
        var parts = line.Split(':');
        if (parts.Length < 2) return 0;
        return long.Parse(parts[1].Trim().Trim('.'));
    }

    private float GetDiskUsage()
    {
        var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.Name == "/");
        if (drive != null)
        {
            var total = drive.TotalSize;
            var used = total - drive.TotalFreeSpace;
            return (float)used / total * 100;
        }

        return 0;
    }

    private string ExecuteCommand(string command)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
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
