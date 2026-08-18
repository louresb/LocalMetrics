using LocalMetrics.Api.Models;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace LocalMetrics.Api.Services;

[SupportedOSPlatform("windows")]
public class WindowsSystemMetricsCollector : ISystemMetricsCollector
{
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
        var firstSample = ReadCpuTimes();
        Thread.Sleep(500);
        var secondSample = ReadCpuTimes();

        var idleTicks = secondSample.Idle - firstSample.Idle;
        var totalTicks = secondSample.Total - firstSample.Total;

        if (totalTicks == 0)
        {
            return 0;
        }

        var usage = (double)(totalTicks - idleTicks) / totalTicks * 100;
        return (float)Math.Clamp(usage, 0, 100);
    }

    private static float GetRamUsage()
    {
        var memoryStatus = new MemoryStatusEx();
        if (!GlobalMemoryStatusEx(memoryStatus))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        if (memoryStatus.TotalPhysicalMemory == 0)
        {
            return 0;
        }

        var usedMemory = memoryStatus.TotalPhysicalMemory - memoryStatus.AvailablePhysicalMemory;
        var usage = (double)usedMemory / memoryStatus.TotalPhysicalMemory * 100;
        return (float)Math.Clamp(usage, 0, 100);
    }

    private static float GetDiskUsage()
    {
        var systemDriveRoot = Path.GetPathRoot(Environment.SystemDirectory);
        var systemDrive = DriveInfo.GetDrives().FirstOrDefault(d =>
            d.IsReady &&
            d.DriveType == DriveType.Fixed &&
            string.Equals(d.Name, systemDriveRoot, StringComparison.OrdinalIgnoreCase));

        if (systemDrive is null || systemDrive.TotalSize == 0)
        {
            return 0;
        }

        var usedSpace = systemDrive.TotalSize - systemDrive.TotalFreeSpace;
        var usage = (double)usedSpace / systemDrive.TotalSize * 100;
        return (float)Math.Clamp(usage, 0, 100);
    }

    private static CpuTimes ReadCpuTimes()
    {
        if (!GetSystemTimes(out var idleTime, out var kernelTime, out var userTime))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        var idle = idleTime.ToUInt64();
        var kernel = kernelTime.ToUInt64();
        var user = userTime.ToUInt64();

        return new CpuTimes(idle, kernel + user);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemTimes(
        out FileTime idleTime,
        out FileTime kernelTime,
        out FileTime userTime);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx buffer);

    private readonly record struct CpuTimes(ulong Idle, ulong Total);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct FileTime
    {
        private readonly uint _lowDateTime;
        private readonly uint _highDateTime;

        public ulong ToUInt64()
        {
            return ((ulong)_highDateTime << 32) | _lowDateTime;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MemoryStatusEx
    {
        private readonly uint _length = (uint)Marshal.SizeOf<MemoryStatusEx>();
        private readonly uint _memoryLoad;
        public ulong TotalPhysicalMemory;
        public ulong AvailablePhysicalMemory;
        private readonly ulong _totalPageFile;
        private readonly ulong _availablePageFile;
        private readonly ulong _totalVirtualMemory;
        private readonly ulong _availableVirtualMemory;
        private readonly ulong _availableExtendedVirtualMemory;
    }
}
