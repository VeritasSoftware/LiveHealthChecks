using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using System.Management;

namespace Sample.Api.HealthChecks
{
    public class SystemHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var cpuUsage = GetTotalCpuUsage();
            var (totalRam, availableRam) = GetSystemRamInfo();

            ulong usedRam = totalRam - availableRam;
            float ramUsagePercent = (float)(usedRam / (double)totalRam * 100);

            var healthData = new Dictionary<string, object>
            {
                { "CPU Usage (%)", $"{cpuUsage.ToString("F2")}" },
                { "RAM Usage (%)", $"{ramUsagePercent.ToString("F2")}" }
            };

            // Example health check logic (replace with actual logic)
            if ((cpuUsage > 80) || (availableRam < totalRam * 0.1))
            {
                return HealthCheckResult.Unhealthy("High CPU usage or low available RAM detected.", data: healthData);
            }

            return HealthCheckResult.Healthy("System is performing well.", data: healthData);
        }

        public static float GetTotalCpuUsage()
        {
            // Create a counter for total CPU usage
            using var cpuCounter = new PerformanceCounter(
                "Processor",       // Category name
                "% Processor Time",// Counter name
                "_Total"           // Instance name (all cores)
            );

            // First call returns 0; wait 1 second, then fetch real value
            cpuCounter.NextValue(); // Dummy read
            System.Threading.Thread.Sleep(1000);
            float cpuUsage = cpuCounter.NextValue();

            return cpuUsage; // Percentage (e.g., 85.2 for 85.2% usage)
        }

        public static (ulong TotalRamBytes, ulong AvailableRamBytes) GetSystemRamInfo()
        {
            ulong totalRam = 0;
            ulong availableRam = 0;

            // Query total physical memory (Win32_ComputerSystem)
            using var totalRamSearcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in totalRamSearcher.Get())
            {
                totalRam = (ulong)obj["TotalPhysicalMemory"];
            }

            // Query available free memory (Win32_OperatingSystem, returns KB)
            using var availableRamSearcher = new ManagementObjectSearcher("SELECT FreePhysicalMemory FROM Win32_OperatingSystem");
            foreach (ManagementObject obj in availableRamSearcher.Get())
            {
                availableRam = (ulong)obj["FreePhysicalMemory"] * 1024; // Convert KB to bytes
            }

            return (totalRam, availableRam);
        }
    }
}
