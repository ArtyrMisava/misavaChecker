using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Windows;

namespace MisavaChecker;

public sealed class DashboardSnapshot
{
    public string OperatingSystem { get; init; } = "Неизвестно";
    public string WindowsVersion { get; init; } = "Неизвестно";
    public string InstallDate { get; init; } = "Неизвестно";
    public string Uptime { get; init; } = "Неизвестно";
    public string Architecture { get; init; } = "Неизвестно";
    public string BiosMode { get; init; } = "Неизвестно";
    public string BiosVersion { get; init; } = "Неизвестно";
    public string Motherboard { get; init; } = "Неизвестно";
    public string Cpu { get; init; } = "Неизвестно";
    public string Gpu { get; init; } = "Неизвестно";
    public string Memory { get; init; } = "Неизвестно";
    public string HyperV { get; init; } = "Неизвестно";
    public string Vbs { get; init; } = "Неизвестно";
    public string Hvci { get; init; } = "Неизвестно";
    public string SecureBoot { get; init; } = "Неизвестно";
    public string Tpm { get; init; } = "Неизвестно";
    public string Defender { get; init; } = "Неизвестно";
    public string Uac { get; init; } = "Неизвестно";
    public string Dma { get; init; } = "Неизвестно";
}

public static class DashboardCollector
{
    public static DashboardSnapshot Collect()
    {
        var os = First("Win32_OperatingSystem");
        var cpu = First("Win32_Processor");
        var board = First("Win32_BaseBoard");
        var bios = First("Win32_BIOS");
        var gpu = First("Win32_VideoController");
        var memory = GetMemory();

        return new DashboardSnapshot
        {
            OperatingSystem = Value(os, "Caption"),
            WindowsVersion = Value(os, "Version") + " (сборка " + Value(os, "BuildNumber") + ")",
            InstallDate = FormatWmiDate(Value(os, "InstallDate")),
            Uptime = GetUptime(Value(os, "LastBootUpTime")),
            Architecture = Value(os, "OSArchitecture"),
            BiosMode = IsUefi() ? "UEFI" : "Legacy",
            BiosVersion = Value(bios, "Manufacturer") + ", " + Value(bios, "SMBIOSBIOSVersion"),
            Motherboard = Value(board, "Manufacturer") + ", " + Value(board, "Product"),
            Cpu = Value(cpu, "Name"),
            Gpu = Value(gpu, "Name"),
            Memory = memory,
            HyperV = SystemFeaturesService.IsHyperVEnabled() ? "Включено" : "Отключено",
            Vbs = SystemFeaturesService.IsVbsEnabled() ? "Включено" : "Отключено",
            Hvci = SystemFeaturesService.IsHvciEnabled() ? "Включено" : "Отключено",
            SecureBoot = GetSecureBootStatus(),
            Tpm = GetTpmStatus(),
            Defender = GetDefenderStatus(),
            Uac = GetDword(
                Registry.LocalMachine,
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                "EnableLUA") == 1 ? "Включено" : "Отключено",
            Dma = GetDmaStatus()
        };
    }

    private static ManagementObject? First(string className)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                $"SELECT * FROM {className}");

            return searcher.Get().Cast<ManagementObject>().FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private static string Value(ManagementObject? item, string property)
    {
        try
        {
            return item?[property]?.ToString()?.Trim() ?? "Неизвестно";
        }
        catch
        {
            return "Неизвестно";
        }
    }

    private static string GetMemory()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Capacity FROM Win32_PhysicalMemory");

            var total = searcher.Get()
                .Cast<ManagementObject>()
                .Sum(item => Convert.ToInt64(item["Capacity"] ?? 0));

            return $"{Math.Round(total / 1024d / 1024d / 1024d)} GB";
        }
        catch
        {
            return "Неизвестно";
        }
    }

    private static string GetUptime(string value)
    {
        if (DateTime.TryParse(FormatWmiDate(value), out var boot))
        {
            var uptime = DateTime.Now - boot;
            return $"{uptime.Days} д {uptime.Hours} ч {uptime.Minutes} мин";
        }

        return "Неизвестно";
    }

    private static string FormatWmiDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 14)
            return "Неизвестно";

        try
        {
            return $"{value[..4]}-{value[4..6]}-{value[6..8]} " +
                   $"{value[8..10]}:{value[10..12]}:{value[12..14]}";
        }
        catch
        {
            return "Неизвестно";
        }
    }

    private static bool IsUefi()
    {
        return Registry.LocalMachine.OpenSubKey(
            @"SYSTEM\CurrentControlSet\Control")?.GetValue("PEFirmwareType") is not null;
    }

    private static string GetSecureBootStatus()
    {
        try
        {
            var value = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\SecureBoot\State")?
                .GetValue("UEFISecureBootEnabled");

            return Convert.ToInt32(value ?? 0) == 1
                ? "Включено"
                : "Отключено";
        }
        catch
        {
            return "Недоступно";
        }
    }

    private static string GetTpmStatus()
    {
        try
        {
            var scope = new ManagementScope(
                @"\\.\root\CIMV2\Security\MicrosoftTpm");

            scope.Connect();

            using var searcher = new ManagementObjectSearcher(
                scope,
                new ObjectQuery("SELECT * FROM Win32_Tpm"));

            return searcher.Get().Count > 0
                ? "Включено"
                : "Не найдено";
        }
        catch
        {
            return "Недоступно";
        }
    }

    private static string GetDefenderStatus()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\SecurityCenter2",
                "SELECT displayName, productState FROM AntiVirusProduct");

            return searcher.Get().Count > 0
                ? "Обнаружен"
                : "Не найдено";
        }
        catch
        {
            return "Недоступно";
        }
    }

    private static string GetDmaStatus()
    {
        var value = GetDword(
            Registry.LocalMachine,
            @"SYSTEM\CurrentControlSet\Control\DmaGuard",
            "DeviceEnumerationPolicy");

        return value switch
        {
            0 => "Включено",
            1 => "Отключено",
            _ => "Неизвестно"
        };
    }

    private static int GetDword(
        RegistryKey root,
        string path,
        string name)
    {
        try
        {
            return Convert.ToInt32(
                root.OpenSubKey(path)?.GetValue(name) ?? 0);
        }
        catch
        {
            return 0;
        }
    }
}
