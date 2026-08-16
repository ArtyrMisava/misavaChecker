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

public partial class HwidWindow : Window
{
    private readonly string snapshotDirectory;
    private readonly string snapshotFile;

    public ObservableCollection<HwidRow> Rows { get; } = new();

    public HwidWindow()
    {
        InitializeComponent();

        DataContext = this;

        snapshotDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "MisavaChecker",
            "Snapshots");

        snapshotFile = Path.Combine(
            snapshotDirectory,
            "baseline.json");

        LoadCurrentInformation();
    }

    private void LoadCurrentInformation()
    {
        Rows.Clear();

        var data = CollectAllInformation();
        var alternate = false;

        foreach (var item in data)
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
        var data = new Dictionary<string, string>();

        CollectNetwork(data);
        CollectStorage(data);
        CollectSystem(data);
        CollectRegistry(data);
        CollectGraphics(data);
        CollectMemory(data);
        CollectDevices(data);
        CollectTpm(data);

        return data;
    }

    private static void CollectNetwork(
        Dictionary<string, string> data)
    {
        AddCategory(data, "Сеть");

        foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
        {
            var mac = adapter.GetPhysicalAddress().ToString();

            if (string.IsNullOrWhiteSpace(mac))
                continue;

            var prefix = adapter.Description;

            AddValue(
                data,
                $"MAC-адрес ({prefix})",
                FormatMac(mac));

            AddValue(
                data,
                $"GUID адаптера ({prefix})",
                adapter.Id);

            AddValue(
                data,
                $"Имя адаптера ({prefix})",
                adapter.Name);

            AddValue(
                data,
                $"Статус ({prefix})",
                adapter.OperationalStatus.ToString());

            AddValue(
                data,
                $"Тип ({prefix})",
                adapter.NetworkInterfaceType.ToString());
        }

        AddWmiCollection(
            data,
            "Сетевые устройства",
            "Win32_NetworkAdapter",
            new[]
            {
                "Name",
                "MACAddress",
                "GUID",
                "PNPDeviceID",
                "Manufacturer",
                "NetConnectionStatus"
            });
    }

    private static void CollectStorage(
        Dictionary<string, string> data)
    {
        AddCategory(data, "Накопители");

        AddWmiCollection(
            data,
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

        AddWmiCollection(
            data,
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
        Dictionary<string, string> data)
    {
        AddCategory(data, "Система");

        AddValue(
            data,
            "Имя ПК",
            Environment.MachineName);

        AddValue(
            data,
            "Пользователь",
            Environment.UserName);

        AddValue(
            data,
            "ОС",
            Environment.OSVersion.ToString());

        AddValue(
            data,
            "Архитектура",
            Environment.Is64BitOperatingSystem
                ? "x64"
                : "x86");

        AddValue(
            data,
            "Количество CPU-потоков",
            Environment.ProcessorCount.ToString());

        AddWmiCollection(
            data,
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

        AddWmiCollection(
            data,
            "Материнская плата",
            "Win32_BaseBoard",
            new[]
            {
                "Manufacturer",
                "Product",
                "Version",
                "SerialNumber"
            });

        AddWmiCollection(
            data,
            "BIOS",
            "Win32_BIOS",
            new[]
            {
                "Manufacturer",
                "Name",
                "SMBIOSBIOSVersion",
                "SerialNumber",
                "Version",
                "ReleaseDate"
            });

        AddWmiCollection(
            data,
            "Компьютер",
            "Win32_ComputerSystemProduct",
            new[]
            {
                "Vendor",
                "Name",
                "Version",
                "IdentifyingNumber",
                "UUID"
            });

        AddWmiCollection(
            data,
            "Операционная система",
            "Win32_OperatingSystem",
            new[]
            {
                "Caption",
                "Version",
                "BuildNumber",
                "SerialNumber",
                "OSArchitecture"
            });
    }

    private static void CollectRegistry(
        Dictionary<string, string> data)
    {
        AddCategory(data, "Реестр");

        AddRegistryValue(
            data,
            "MachineGuid",
            @"SOFTWARE\Microsoft\Cryptography",
            "MachineGuid");

        AddRegistryValue(
            data,
            "ProductId",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            "ProductId");

        AddRegistryValue(
            data,
            "ProductName",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            "ProductName");

        AddRegistryValue(
            data,
            "CurrentBuild",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            "CurrentBuild");

        AddRegistryValue(
            data,
            "BuildLabEx",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            "BuildLabEx");

        AddRegistryValue(
            data,
            "InstallDate",
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion",
            "InstallDate");

        AddRegistryValue(
            data,
            "HardwareConfig",
            @"SYSTEM\CurrentControlSet\Control\IDConfigDB\Hardware Profiles\0001",
            "HwProfileGuid");
    }

    private static void CollectGraphics(
        Dictionary<string, string> data)
    {
        AddCategory(data, "Видеокарта");

        AddWmiCollection(
            data,
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
                "CurrentRefreshRate",
                "Status"
            });
    }

    private static void CollectMemory(
        Dictionary<string, string> data)
    {
        AddCategory(data, "Память");

        AddWmiCollection(
            data,
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
                "MemoryType",
                "DeviceLocator",
                "BankLabel"
            });
    }

    private static void CollectDevices(
        Dictionary<string, string> data)
    {
        AddCategory(data, "USB / Устройства");

        AddWmiCollection(
            data,
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

        AddCategory(data, "HID");

        AddWmiCollection(
            data,
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

        AddCategory(data, "Audio");

        AddWmiCollection(
            data,
            "Аудиоустройства",
            "Win32_SoundDevice",
            new[]
            {
                "Name",
                "DeviceID",
                "PNPDeviceID",
                "Manufacturer",
                "Status"
            });

        AddCategory(data, "Bluetooth");

        AddWmiCollection(
            data,
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
        Dictionary<string, string> data)
    {
        AddCategory(data, "TPM / Безопасность");

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
                    data,
                    "TPM / ManufacturerId",
                    GetProperty(tpm, "ManufacturerId"));

                AddValue(
                    data,
                    "TPM / ManufacturerVersion",
                    GetProperty(tpm, "ManufacturerVersion"));

                AddValue(
                    data,
                    "TPM / IsEnabled",
                    GetProperty(tpm, "IsEnabled_InitialValue"));

                AddValue(
                    data,
                    "TPM / IsActivated",
                    GetProperty(tpm, "IsActivated_InitialValue"));

                AddValue(
                    data,
                    "TPM / IsOwned",
                    GetProperty(tpm, "IsOwned_InitialValue"));
            }

            if (!found)
            {
                AddValue(
                    data,
                    "TPM",
                    "TPM не найден");
            }
        }
        catch (Exception error)
        {
            AddValue(
                data,
                "TPM",
                $"Недоступен: {error.Message}");
        }

        AddRegistryValue(
            data,
            "TPM / Registry",
            @"SYSTEM\CurrentControlSet\Services\TPM",
            "Start");
    }

    private static void AddWmiCollection(
        Dictionary<string, string> data,
        string groupName,
        string className,
        string[] properties,
        string? condition = null)
    {
        try
        {
            var query =
                $"SELECT * FROM {className}";

            if (!string.IsNullOrWhiteSpace(condition))
            {
                query += $" WHERE {condition}";
            }

            using var searcher =
                new ManagementObjectSearcher(query);

            var uniqueDevices =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            var index = 0;

            foreach (ManagementObject item in searcher.Get())
            {
                var name = GetProperty(item, "Name");
                var pnpId = GetProperty(item, "PNPDeviceID");
                var deviceId = GetProperty(item, "DeviceID");

                if (string.IsNullOrWhiteSpace(name) &&
                    string.IsNullOrWhiteSpace(pnpId) &&
                    string.IsNullOrWhiteSpace(deviceId))
                {
                    continue;
                }

                var uniqueKey =
                    $"{name.Trim()}|{pnpId.Trim()}|{deviceId.Trim()}";

                if (!uniqueDevices.Add(uniqueKey))
                {
                    continue;
                }

                if (ShouldIgnoreDevice(name))
                {
                    continue;
                }

                var itemName = index == 0
                    ? groupName
                    : $"{groupName} {index}";

                foreach (var property in properties)
                {
                    var value = GetProperty(
                        item,
                        property);

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    AddValue(
                        data,
                        $"{itemName} / {property}",
                        value);
                }

                index++;
            }

            if (index == 0)
            {
                AddValue(
                    data,
                    groupName,
                    "Не найдено");
            }
        }
        catch (Exception error)
        {
            AddValue(
                data,
                groupName,
                $"Недоступно: {error.Message}");
        }
    }

    private static bool ShouldIgnoreDevice(
        string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return true;
        }

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
        string propertyName)
    {
        try
        {
            var value = item[propertyName];

            if (value is Array array)
            {
                return string.Join(
                    " | ",
                    array.Cast<object>());
            }

            return value?.ToString()?.Trim()
                   ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void AddCategory(
        Dictionary<string, string> data,
        string category)
    {
        data[$"[CATEGORY]{data.Count:D8}"] = category;
    }

    private static void AddValue(
        Dictionary<string, string> data,
        string name,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            value = "Не найдено";
        }

        data[name] = value.Trim();
    }

    private static void AddRegistryValue(
        Dictionary<string, string> data,
        string name,
        string path,
        string valueName)
    {
        try
        {
            using var key =
                Registry.LocalMachine.OpenSubKey(path);

            var value =
                key?.GetValue(valueName)?.ToString();

            AddValue(data, name, value);
        }
        catch
        {
            AddValue(data, name, "Не найдено");
        }
    }

    private static string FormatMac(string mac)
    {
        if (mac.Length != 12)
        {
            return mac;
        }

        return string.Join(
            ":",
            Enumerable.Range(0, 6)
                .Select(index =>
                    mac.Substring(index * 2, 2)));
    }

    private static string FormatBytes(
        string? value)
    {
        return long.TryParse(
            value,
            out var bytes)
            ? FormatBytes(bytes)
            : "Не найдено";
    }

    private static string FormatBytes(long bytes)
    {
        string[] units =
        {
            "B",
            "KB",
            "MB",
            "GB",
            "TB"
        };

        double value = bytes;
        var unit = 0;

        while (value >= 1024 &&
               unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.##} {units[unit]}";
    }

    private void SaveSnapshotButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Directory.CreateDirectory(
            snapshotDirectory);

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

        File.WriteAllText(
            snapshotFile,
            json);

        MessageBox.Show(
            "Текущий снимок сохранён.",
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
                "Сначала нажми «Сохранить снимок».",
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

        var currentValues =
            CollectAllInformation();

        foreach (var row in Rows)
        {
            if (row.IsCategory)
            {
                continue;
            }

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

        foreach (var oldValue in oldValues)
        {
            if (currentValues.ContainsKey(
                    oldValue.Key))
            {
                continue;
            }

            Rows.Add(new HwidRow
            {
                Name = oldValue.Key,
                CurrentValue = "значение отсутствует",
                PreviousValue = oldValue.Value,
                IsChanged = true
            });
        }

        HwidList.Items.Refresh();

        var changedCount =
            Rows.Count(row => row.IsChanged);

        MessageBox.Show(
            $"Сравнение завершено.\nИзменено строк: {changedCount}",
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