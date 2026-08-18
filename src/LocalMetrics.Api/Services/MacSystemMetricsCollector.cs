using LocalMetrics.Api.Models;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LocalMetrics.Api.Services;

public sealed class MacSystemMetricsCollector : ISystemMetricsCollector
{
    private const int CommandTimeoutMilliseconds = 5_000;
    private static readonly Regex CpuUsagePattern = new(
        @"(\d+\.\d+)% user, (\d+\.\d+)% sys",
        RegexOptions.CultureInvariant);

    public SystemMetrics GetMetrics()
    {
        return new SystemMetrics
        {
            CpuUsage = GetCpuUsage(),
            RamUsage = GetRamUsage(),
            DiskUsage = GetDiskUsage(),
            Timestamp = DateTime.UtcNow
        };
    }

    private static float GetCpuUsage()
    {
        var output = ExecuteCommand("/usr/bin/top", "-l", "1");
        var cpuLine = output.Split('\n').FirstOrDefault(line =>
            line.Contains("CPU usage", StringComparison.Ordinal));

        var match = cpuLine is null ? Match.Empty : CpuUsagePattern.Match(cpuLine);
        if (!match.Success)
        {
            throw new InvalidOperationException("Unable to parse CPU usage from top output.");
        }

        var user = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var system = float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        return Math.Clamp(user + system, 0, 100);
    }

    private static float GetRamUsage()
    {
        var output = ExecuteCommand("/usr/bin/vm_stat");
        long free = 0;
        long active = 0;
        long inactive = 0;
        long speculative = 0;
        long wired = 0;

        foreach (var line in output.Split('\n'))
        {
            if (line.StartsWith("Pages free:", StringComparison.Ordinal)) free = ParseVmStat(line);
            else if (line.StartsWith("Pages active:", StringComparison.Ordinal)) active = ParseVmStat(line);
            else if (line.StartsWith("Pages inactive:", StringComparison.Ordinal)) inactive = ParseVmStat(line);
            else if (line.StartsWith("Pages speculative:", StringComparison.Ordinal)) speculative = ParseVmStat(line);
            else if (line.StartsWith("Pages wired down:", StringComparison.Ordinal)) wired = ParseVmStat(line);
        }

        var total = free + active + inactive + speculative + wired;
        if (total == 0)
        {
            throw new InvalidOperationException("Unable to parse memory usage from vm_stat output.");
        }

        var used = active + inactive + speculative + wired;
        return Math.Clamp((float)used / total * 100, 0, 100);
    }

    private static long ParseVmStat(string line)
    {
        var parts = line.Split(':', 2);
        return parts.Length < 2
            ? 0
            : long.Parse(parts[1].Trim().Trim('.'), CultureInfo.InvariantCulture);
    }

    private static float GetDiskUsage()
    {
        var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.Name == "/");
        if (drive is null || drive.TotalSize == 0)
        {
            throw new InvalidOperationException("Unable to identify the macOS root volume.");
        }

        var used = drive.TotalSize - drive.TotalFreeSpace;
        return Math.Clamp((float)used / drive.TotalSize * 100, 0, 100);
    }

    private static string ExecuteCommand(string fileName, params string[] arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        if (!process.Start())
        {
            throw new InvalidOperationException($"Unable to start {fileName}.");
        }

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(CommandTimeoutMilliseconds))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
                // The process exited between the timeout and the kill attempt.
            }

            process.WaitForExit();
            throw new TimeoutException($"{fileName} did not finish within five seconds.");
        }

        var output = outputTask.GetAwaiter().GetResult();
        var error = errorTask.GetAwaiter().GetResult();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"{fileName} exited with code {process.ExitCode}: {error.Trim()}");
        }

        return output;
    }
}
