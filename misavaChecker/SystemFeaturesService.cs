using System;
using System.Diagnostics;

namespace MisavaChecker;

public static class SystemFeaturesService
{
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

    private static string RunPowerShell(string command)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{command}\"",
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

    private static void RunPowerShellAsAdministrator(string command)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
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
                "Windows не смогло изменить состояние Hyper-V.");
        }
    }
}
