using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace MisavaChecker;

public partial class Window1 : Window
{
    private readonly string snapshotFile;

    public ObservableCollection<HwidRow> Rows { get; } = new();

    public Window1()
    {
        InitializeComponent();

        DataContext = this;

        var directory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "MisavaChecker",
            "Snapshots");

        Directory.CreateDirectory(directory);

        snapshotFile = Path.Combine(
            directory,
            "baseline.json");

        LoadCurrentInformation();
    }

    private void LoadCurrentInformation()
    {
        Rows.Clear();

        var values = CollectAllInformation();
        var alternate = false;

        foreach (var item in values)
        {
            if (item.Key.StartsWith("[CATEGORY]"))
            {
                Rows.Add(new HwidRow
                {
                    IsCategory = true,
                    Category = item.Value
                });

                alternate = false;
                continue;
            }

            Rows.Add(new HwidRow
            {
                Name = item.Key,
                CurrentValue = item.Value,
                IsAlternate = alternate
            });

            alternate = !alternate;
        }
    }

    private Dictionary<string, string> CollectAllInformation()
    {
        var values = new Dictionary<string, string>();

        CollectNetwork(values);
        CollectStorage(values);
        CollectSystem(values);
        CollectRegistry(values);
        CollectGpu(values);
        CollectMemory(values);
        CollectDevices(values);
        CollectTpm(values);

        return values;
    }

    private static void CollectNetwork(
        Dictionary<string, string> values)
    {
        AddCategory(values, "Сеть");

        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            var mac = adapter.GetPhysicalAddress().ToString();

            if (string.IsNullOrWhiteSpace(mac))
                continue;

            AddValue(
                values,
                $"MAC-адрес ({adapter.Description})",
                FormatMac(mac));

            AddValue(
                values,
                $"GUID адаптера ({adapter.Description})",
                adapter.Id);

            AddValue(
                values,
                $"Статус ({adapter.Name})",
                adapter.OperationalStatus.ToString());
        }

        AddWmi(
            values,
            "Сетевые адаптеры",
            "Win32_NetworkAdapter",
            new[]
            {
                "Name",
                "MACAddress",
                "GUID",
                "PNPDeviceID",
                "Manufacturer"
            });
    }

    private static void CollectStorage(
        Dictionary<string, string> values)
    {
        AddCategory(values, "Накопители");

        AddWmi(
            values,
            "Диски",
            "Win32_DiskDrive",
            new[]
            {
                "Model",
                "SerialNumber",
                "FirmwareRevision",
                "InterfaceType",
                "MediaType",
                "PNPDeviceID",
                "Size"
            });

        AddWmi(
            values,
            "Тома",
            "Win32_LogicalDisk",
            new[]
            {
                "DeviceID",
                "VolumeName",
                "VolumeSerialNumber",
                "FileSystem",
                "Size",
                "FreeSpace"
            });
    }

    private static void CollectSystem(
        Dictionary<string, string> values)
    {
        AddCategory(values, "Система");

        AddValue(
            values,
            "Имя ПК",
            Environment.MachineName);

        AddValue(
            values,
            "Пользователь",
            Environment.UserName);

        AddValue(
            values,
            "ОС",
            Environment.OSVersion.ToString());

        AddValue(
            values,
            "Архитектура",
            Environment.Is64BitOperatingSystem
                ? "x64"
                : "x86");

        AddValue(
            values,
            "CPU-потоки",
            Environment.ProcessorCount.ToString());

        AddWmi(
            values,
            "Процессор",
            "Win32_Processor",
            new[]
            {
                "Name",
                "Manufacturer",
                "ProcessorId",
                "NumberOfCores",
                "NumberOfLogicalProcessors",
                "MaxClockSpeed"
            });

        AddWmi(
            values,
            "Материнская плата",
            "Win32_BaseBoard",
            new[]
            {
                "Manufacturer",
                "Product",
                "Version",
                "SerialNumber"
            });

        AddWmi(
            values,
            "BIOS",
            "Win32_BIOS",
            new[]
            {
                "Manufacturer",
                "SMBIOSBIOSVersion",
                "SerialNumber",
                "Version",
                "ReleaseDate"
            });

        AddWmi(
            values,
            "Компьютер",
            "Win32_ComputerSystemProduct",
            new[]
            {
                "Vendor",
                "Name",
                "Version",
                "UUID"
            });
    }

    private static void CollectRegistry(
        Dictionary<string, string> values)
    {
        AddCategory(values, "Реестр");

        AddRegistry(
            values,
            "MachineGuid",
            @"SOFTWARE\Microsoft\Cryptography",
            "MachineGuid");

        AddRegistry(
            values,
            "ProductId",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            "ProductId");

        AddRegistry(
            values,
            "ProductName",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            "ProductName");

        AddRegistry(
            values,
            "CurrentBuild",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            "CurrentBuild");

        AddRegistry(
            values,
            "BuildLabEx",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            "BuildLabEx");

        AddRegistry(
            values,
            "HardwareProfileGuid",
            @"SYSTEM\CurrentControlSet\Control\IDConfigDB\Hardware Profiles\0001",
            "HwProfileGuid");
    }

    private static void CollectGpu(
        Dictionary<string, string> values)
    {
        AddCategory(values, "Видеокарта");

        AddWmi(
            values,
            "GPU",
            "Win32_VideoController",
            new[]
            {
                "Name",
                "PNPDeviceID",
                "AdapterCompatibility",
                "AdapterRAM",
                "DriverVersion",
                "VideoProcessor",
                "VideoModeDescription",
                "CurrentHorizontalResolution",
                "CurrentVerticalResolution",
                "CurrentRefreshRate"
            });
    }

    private static void CollectMemory(
        Dictionary<string, string> values)
    {
        AddCategory(values, "Память");

        AddWmi(
            values,
            "RAM",
            "Win32_PhysicalMemory",
            new[]
            {
                "Manufacturer",
                "PartNumber",
                "SerialNumber",
                "Capacity",
                "Speed",
                "ConfiguredClockSpeed",
                "SMBIOSMemoryType",
                "DeviceLocator",
                "BankLabel"
            });
    }

    private static void CollectDevices(
        Dictionary<string, string> values)
    {
        AddCategory(values, "USB / Устройства");

        AddWmi(
            values,
            "USB",
            "Win32_PnPEntity",
            new[]
            {
                "Name",
                "PNPDeviceID",
                "Manufacturer",
                "Status"
            },
            "PNPDeviceID LIKE 'USB%'");

        AddCategory(values, "HID");

        AddWmi(
            values,
            "HID",
            "Win32_PnPEntity",
            new[]
            {
                "Name",
                "PNPDeviceID",
                "Manufacturer",
                "Status"
            },
            "PNPDeviceID LIKE 'HID%'");

        AddCategory(values, "Audio");

        AddWmi(
            values,
            "Audio",
            "Win32_SoundDevice",
            new[]
            {
                "Name",
                "DeviceID",
                "PNPDeviceID",
                "Manufacturer",
                "Status"
            });

        AddCategory(values, "Bluetooth");

        AddWmi(
            values,
            "Bluetooth",
            "Win32_PnPEntity",
            new[]
            {
                "Name",
                "PNPDeviceID",
                "Manufacturer",
                "Status"
            },
            "PNPClass = 'Bluetooth'");
    }

    private static void CollectTpm(
        Dictionary<string, string> values)
    {
        AddCategory(values, "TPM / Безопасность");

        try
        {
            var scope = new ManagementScope(
                @"\\.\root\CIMV2\Security\MicrosoftTpm");

            scope.Connect();

            using var searcher =
                new ManagementObjectSearcher(
                    scope,
                    new ObjectQuery(
                        "SELECT * FROM Win32_Tpm"));

            var found = false;

            foreach (ManagementObject tpm in searcher.Get())
            {
                found = true;

                AddValue(
                    values,
                    "TPM / ManufacturerId",
                    GetProperty(tpm, "ManufacturerId"));

                AddValue(
                    values,
                    "TPM / ManufacturerVersion",
                    GetProperty(tpm, "ManufacturerVersion"));

                AddValue(
                    values,
                    "TPM / IsEnabled",
                    GetProperty(tpm, "IsEnabled_InitialValue"));

                AddValue(
                    values,
                    "TPM / IsActivated",
                    GetProperty(tpm, "IsActivated_InitialValue"));

                AddValue(
                    values,
                    "TPM / IsOwned",
                    GetProperty(tpm, "IsOwned_InitialValue"));
            }

            if (!found)
            {
                AddValue(values, "TPM", "TPM не найден");
            }
        }
        catch (Exception error)
        {
            AddValue(
                values,
                "TPM",
                $"Недоступен: {error.Message}");
        }
    }

    private static void AddWmi(
        Dictionary<string, string> values,
        string groupName,
        string className,
        string[] properties,
        string? condition = null)
    {
        try
        {
            var query = $"SELECT * FROM {className}";

            if (!string.IsNullOrWhiteSpace(condition))
            {
                query += $" WHERE {condition}";
            }

            using var searcher =
                new ManagementObjectSearcher(query);

            var unique = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            var index = 0;

            foreach (ManagementObject item in searcher.Get())
            {
                var name = GetProperty(item, "Name");
                var pnp = GetProperty(item, "PNPDeviceID");
                var device = GetProperty(item, "DeviceID");

                var uniqueKey =
                    $"{name}|{pnp}|{device}";

                if (!unique.Add(uniqueKey))
                    continue;

                if (ShouldIgnoreDevice(name))
                    continue;

                var prefix = index == 0
                    ? groupName
                    : $"{groupName} {index}";

                foreach (var property in properties)
                {
                    var value = GetProperty(item, property);

                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    AddValue(
                        values,
                        $"{prefix} / {property}",
                        value);
                }

                index++;
            }

            if (index == 0)
            {
                AddValue(values, groupName, "Не найдено");
            }
        }
        catch (Exception error)
        {
            AddValue(
                values,
                groupName,
                $"Недоступно: {error.Message}");
        }
    }

    private static bool ShouldIgnoreDevice(
        string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return true;

        var value = name.ToLowerInvariant();

        return value.Contains("generic usb hub")
               || value.Contains("usb root hub")
               || value.Contains("usb composite device")
               || value.Contains("usb input device")
               || value.Contains("hid-compliant consumer control")
               || value.Contains("hid-compliant system controller");
    }

    private static string GetProperty(
        ManagementObject item,
        string property)
    {
        try
        {
            var value = item[property];

            if (value is Array array)
            {
                return string.Join(
                    " | ",
                    array.Cast<object>());
            }

            return value?.ToString()?.Trim()
                   ?? "";
        }
        catch
        {
            return "";
        }
    }

    private static void AddCategory(
        Dictionary<string, string> values,
        string category)
    {
        values[$"[CATEGORY]{values.Count:D8}"] =
            category;
    }

    private static void AddValue(
        Dictionary<string, string> values,
        string name,
        string? value)
    {
        values[name] =
            string.IsNullOrWhiteSpace(value)
                ? "Не найдено"
                : value.Trim();
    }

    private static void AddRegistry(
        Dictionary<string, string> values,
        string name,
        string path,
        string valueName)
    {
        try
        {
            using var key =
                Registry.LocalMachine.OpenSubKey(path);

            AddValue(
                values,
                name,
                key?.GetValue(valueName)?.ToString());
        }
        catch
        {
            AddValue(values, name, "Не найдено");
        }
    }

    private static string FormatMac(string mac)
    {
        if (mac.Length != 12)
            return mac;

        return string.Join(
            ":",
            Enumerable.Range(0, 6)
                .Select(i => mac.Substring(i * 2, 2)));
    }

    private static void SaveText(
        string path,
        string text)
    {
        File.WriteAllText(path, text);
    }

    private void SaveSnapshotButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var snapshot = Rows
            .Where(row => !row.IsCategory)
            .ToDictionary(
                row => row.Name,
                row => row.CurrentValue);

        var json = JsonSerializer.Serialize(
            snapshot,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        SaveText(snapshotFile, json);

        MessageBox.Show(
            "Снимок сохранён.",
            "Misava Checker",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void CompareButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!File.Exists(snapshotFile))
        {
            MessageBox.Show(
                "Сначала сохрани базовый снимок.",
                "Misava Checker",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        var oldValues =
            JsonSerializer.Deserialize<
                Dictionary<string, string>>(
                File.ReadAllText(snapshotFile))
            ?? new Dictionary<string, string>();

        foreach (var row in Rows)
        {
            if (row.IsCategory)
                continue;

            if (!oldValues.TryGetValue(
                    row.Name,
                    out var oldValue))
            {
                row.IsChanged = true;
                row.PreviousValue = "отсутствовало";
                continue;
            }

            if (!string.Equals(
                    oldValue,
                    row.CurrentValue,
                    StringComparison.Ordinal))
            {
                row.IsChanged = true;
                row.PreviousValue = oldValue;
            }
        }

        HwidList.Items.Refresh();

        var count = Rows.Count(
            row => row.IsChanged);

        MessageBox.Show(
            $"Сравнение завершено.\nИзменено строк: {count}",
            "Misava Checker",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void RefreshButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        LoadCurrentInformation();
    }

    private void CopyButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var text = string.Join(
            Environment.NewLine,
            Rows.Select(row =>
                row.IsCategory
                    ? $"\n[{row.Category}]"
                    : $"{row.Name}: {row.DisplayValue}"));

        Clipboard.SetText(text);

        MessageBox.Show(
            "Информация скопирована.",
            "Misava Checker",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}

public sealed class HwidRow
{
    public string Category { get; set; } = "";

    public string Name { get; set; } = "";

    public string CurrentValue { get; set; } = "";

    public string PreviousValue { get; set; } = "";

    public bool IsCategory { get; set; }

    public bool IsAlternate { get; set; }

    public bool IsChanged { get; set; }

    public string PreviousText =>
        IsChanged
            ? $"было: {PreviousValue}"
            : "";

    public Visibility PreviousVisibility =>
        IsChanged
            ? Visibility.Visible
            : Visibility.Collapsed;

    public Brush ValueBrush =>
        IsChanged
            ? new SolidColorBrush(
                Color.FromRgb(255, 209, 102))
            : new SolidColorBrush(
                Color.FromRgb(221, 225, 234));

    public Brush RowBackground =>
        IsAlternate
            ? new SolidColorBrush(
                Color.FromRgb(29, 32, 40))
            : new SolidColorBrush(
                Color.FromRgb(21, 23, 30));

    public string DisplayValue =>
        IsChanged
            ? $"{CurrentValue}   было: {PreviousValue}"
            : CurrentValue;
}