using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.IO;

namespace MisavaChecker;

public sealed record SecurityItem(
    string Name,
    string Status,
    bool IsEnabled);

public static class SecurityCollector
{
    public static List<SecurityItem> Collect()
    {
        return new List<SecurityItem>
        {
            Item("Вирт. CPU", GetVirtualization()),
            Item("Гипервизор", GetHypervisor()),
            Item("Hyper-V", SystemFeaturesService.IsHyperVEnabled()),
            Item("Blocklist", GetBlocklist()),
            Item("HVCI", SystemFeaturesService.IsHvciEnabled()),
            Item("Безоп. загрузка", GetSecureBoot()),
            Item("VBS", SystemFeaturesService.IsVbsEnabled()),
            Item("DMA", GetDma()),
            Item("UAC", GetDword(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA") == 1),
            Item("TPM", GetTpm()),
            Item("Cred.Guard", GetDword(@"SYSTEM\CurrentControlSet\Control\Lsa", "LsaCfgFlags") > 0),
            Item("Meltdown", GetMitigation("FeatureSettingsOverride")),
            Item("Spectre", GetMitigation("FeatureSettingsOverrideMask")),
            Item("BitLocker", GetBitLocker()),
            Item("Hello PIN", GetHelloPin()),
            Item("Тест. подпись", GetTestSigning()),
            Item("VMP", GetVmp()),
            Item("Defender", GetDefender()),
            Item("WMI", GetWmi()),
            Item("Репутация", GetReputation())
        };
    }

    private static SecurityItem Item(string name, string status)
    {
        return new SecurityItem(name, status, status == "Включено");
    }

    private static SecurityItem Item(string name, bool enabled)
    {
        return new SecurityItem(
            name,
            enabled ? "Включено" : "Отключено",
            enabled);
    }

    private static string GetVirtualization()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT VirtualizationFirmwareEnabled, VMMonitorModeExtensions FROM Win32_Processor");

            var cpu = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            var value = Convert.ToBoolean(cpu?["VirtualizationFirmwareEnabled"] ?? false)
                        || Convert.ToBoolean(cpu?["VMMonitorModeExtensions"] ?? false);

            return value ? "Включено" : "Отключено";
        }
        catch
        {
            return "Недоступно";
        }
    }

    private static string GetHypervisor()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT VMMonitorModeExtensions FROM Win32_Processor");

            var cpu = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
            return Convert.ToBoolean(cpu?["VMMonitorModeExtensions"] ?? false)
                ? "Включено"
                : "Отключено";
        }
        catch
        {
            return "Недоступно";
        }
    }

    private static string GetSecureBoot()
    {
        var value = GetDword(
            @"SYSTEM\CurrentControlSet\Control\SecureBoot\State",
            "UEFISecureBootEnabled");

        return value == 1 ? "Включено" : "Отключено";
    }

    private static string GetDma()
    {
        var value = GetDword(
            @"SYSTEM\CurrentControlSet\Control\DmaGuard",
            "DeviceEnumerationPolicy");

        return value == 0 ? "Включено" : "Отключено";
    }

    private static string GetBlocklist()
    {
        var value = GetDword(
            @"SYSTEM\CurrentControlSet\Control\CI\Config",
            "VulnerableDriverBlocklistEnable");

        return value == 1 ? "Включено" : "Отключено";
    }

    private static string GetTpm()
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
                : "Отключено";
        }
        catch
        {
            return "Недоступно";
        }
    }

    private static string GetMitigation(string valueName)
    {
        var value = GetDword(
            @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
            valueName);

        return value == 0 ? "Включено" : "Отключено";
    }

    private static string GetBitLocker()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\CIMV2\Security\MicrosoftVolumeEncryption",
                "SELECT ProtectionStatus FROM Win32_EncryptableVolume");

            return searcher.Get().Count > 0
                ? "Включено"
                : "Отключено";
        }
        catch
        {
            return "Недоступно";
        }
    }

    private static string GetHelloPin()
    {
        return Directory.Exists(
            Environment.ExpandEnvironmentVariables(
                @"%LOCALAPPDATA%\Microsoft\IdentityCRL"))
            ? "Включено"
            : "Отключено";
    }

    private static string GetTestSigning()
    {
        var value = GetDword(
            @"SYSTEM\CurrentControlSet\Control",
            "DisableIntegrityChecks");

        return value == 1 ? "Включено" : "Отключено";
    }

    private static string GetVmp()
    {
        return SystemFeaturesService.IsHyperVEnabled()
            ? "Включено"
            : "Отключено";
    }

    private static string GetDefender()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\SecurityCenter2",
                "SELECT displayName FROM AntiVirusProduct");

            return searcher.Get().Count > 0
                ? "Включено"
                : "Отключено";
        }
        catch
        {
            return "Недоступно";
        }
    }

    private static string GetWmi()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name FROM Win32_Process");

            return searcher.Get().Count > 0
                ? "Включено"
                : "Отключено";
        }
        catch
        {
            return "Недоступно";
        }
    }

    private static string GetReputation()
    {
        var value = GetDword(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer",
            "SmartScreenEnabled");

        return value > 0 ? "Включено" : "Отключено";
    }

    private static int GetDword(string path, string name)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(path);
            return Convert.ToInt32(key?.GetValue(name) ?? 0);
        }
        catch
        {
            return 0;
        }
    }
}
