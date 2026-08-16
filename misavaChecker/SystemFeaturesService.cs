using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace MisavaChecker;

public static class SystemFeaturesService
{
    private const string DeviceGuardPath =
        @"SYSTEM\CurrentControlSet\Control\DeviceGuard";

    private const string HvciPath =
        @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity";

    public static bool IsHyperVEnabled()
    {
        var result = RunPowerShell(
            "(Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All).State");

        return result.Trim().Equals(
            "Enabled",
            StringComparison.OrdinalIgnoreCase);
    }

    public static void ToggleHyperV(bool enable)
    {
        var command = enable
            ? "Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All -All -NoRestart"
            : "Disable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All -NoRestart";

        RunPowerShellAsAdministrator(command);
    }

    public static bool IsVbsEnabled()
    {
        return GetDword(DeviceGuardPath, "EnableVirtualizationBasedSecurity") == 1;
    }

    public static void ToggleVbs(bool enable)
    {
        SetDwordAsAdministrator(
            DeviceGuardPath,
            "EnableVirtualizationBasedSecurity",
            enable ? 1 : 0);
    }

    public static bool IsHvciEnabled()
    {
        return GetDword(HvciPath, "Enabled") == 1;
    }

    public static void ToggleHvci(bool enable)
    {
        SetDwordAsAdministrator(
            HvciPath,
            "Enabled",
            enable ? 1 : 0);
    }

    private static int GetDword(
        string path,
        string valueName)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(path);
            var value = key?.GetValue(valueName);

            return value is int integer
                ? integer
                : Convert.ToInt32(value ?? 0);
        }
        catch
        {
            return 0;
        }
    }

    private static void SetDwordAsAdministrator(
        string path,
        string valueName,
        int value)
    {
        var command =
            $"reg.exe ADD HKLM\\{path} /v {valueName} /t REG_DWORD /d {value} /f";

        RunCommandAsAdministrator(
            "cmd.exe",
            "/c " + command);
    }

    private static string RunPowerShell(string command)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments =
                    $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{command}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return output;
    }

    private static void RunPowerShellAsAdministrator(
        string command)
    {
        RunCommandAsAdministrator(
            "powershell.exe",
            $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"");
    }

    private static void RunCommandAsAdministrator(
        string fileName,
        string arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                "Windows не смогло применить изменение.");
        }
    }
}
