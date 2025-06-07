using LocalMetrics.Api.Models;

namespace LocalMetrics.Api.Services;

public class LinuxSystemMetricsCollector : ISystemMetricsCollector
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
        var cpuTimes1 = ReadCpuTimes();
        Thread.Sleep(500);
        var cpuTimes2 = ReadCpuTimes();

        var idle1 = cpuTimes1.idle + cpuTimes1.iowait;
        var idle2 = cpuTimes2.idle + cpuTimes2.iowait;

        var total1 = cpuTimes1.user + cpuTimes1.nice + cpuTimes1.system + cpuTimes1.idle +
                     cpuTimes1.iowait + cpuTimes1.irq + cpuTimes1.softirq + cpuTimes1.steal;

        var total2 = cpuTimes2.user + cpuTimes2.nice + cpuTimes2.system + cpuTimes2.idle +
                     cpuTimes2.iowait + cpuTimes2.irq + cpuTimes2.softirq + cpuTimes2.steal;

        var totalDiff = total2 - total1;
        var idleDiff = idle2 - idle1;

        return totalDiff == 0 ? 0 : (float)(totalDiff - idleDiff) / totalDiff * 100;
    }

    private (ulong user, ulong nice, ulong system, ulong idle, ulong iowait, ulong irq, ulong softirq, ulong steal) ReadCpuTimes()
    {
        var cpuLine = File.ReadLines("/proc/stat").FirstOrDefault(l => l.StartsWith("cpu "));
        var parts = cpuLine?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts == null || parts.Length < 8)
            return (0, 0, 0, 0, 0, 0, 0, 0);

        return (
            ulong.Parse(parts[1]),
            ulong.Parse(parts[2]),
            ulong.Parse(parts[3]),
            ulong.Parse(parts[4]),
            ulong.Parse(parts[5]),
            ulong.Parse(parts[6]),
            ulong.Parse(parts[7]),
            parts.Length > 8 ? ulong.Parse(parts[8]) : 0
        );
    }

    private float GetRamUsage()
    {
        string[] lines = File.ReadAllLines("/proc/meminfo");
        float total = 0, free = 0;

        foreach (var line in lines)
        {
            if (line.StartsWith("MemTotal:"))
                total = float.Parse(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);
            else if (line.StartsWith("MemAvailable:"))
                free = float.Parse(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);

            if (total > 0 && free > 0) break;
        }

        return total == 0 ? 0 : 100f - ((free / total) * 100);
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
}
