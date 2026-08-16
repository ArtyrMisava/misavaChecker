using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MisavaChecker;

public partial class MainWindow : Window
{
    public ObservableCollection<SecurityCardViewModel> SecurityItems { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        UpdateFunctionStatuses();
        UpdateDashboard();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Opacity = 0;

        var animation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(350)
        };

        BeginAnimation(OpacityProperty, animation);
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void HwidButton_Click(object sender, RoutedEventArgs e)
    {
        var window = new Window1
        {
            Owner = this
        };

        window.ShowDialog();
    }

    private void UpdateDashboard()
    {
        try
        {
            var snapshot = DashboardCollector.Collect();

            SecurityItems.Clear();
            foreach (var item in SecurityCollector.Collect())
            {
                SecurityItems.Add(new SecurityCardViewModel(item));
            }

            OperatingSystemText.Text = snapshot.OperatingSystem;
            CpuText.Text = snapshot.Cpu;
            GpuText.Text = snapshot.Gpu;
            MemoryText.Text = snapshot.Memory;

        }
        catch (Exception error)
        {
            MessageBox.Show(
                error.Message,
                "Ошибка сканирования системы",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void UpdateFunctionStatuses()
    {
        SetHyperVStatus();
        SetVbsStatus();
        SetHvciStatus();

        DmaButton.Content = "DMA";
        UacButton.Content = "UAC";
        BlocklistButton.Content = "Blocklist";
        DefenderButton.Content = "Defender";
        AntiCheatButton.Content = "Античиты";
        DebuggerButton.Content = "Отладчики";
    }

    private void SetHyperVStatus()
    {
        try
        {
            HyperVButton.Content = SystemFeaturesService.IsHyperVEnabled()
                ? "HV включен"
                : "HV выключен";
        }
        catch
        {
            HyperVButton.Content = "HV неизвестно";
        }
    }

    private void SetVbsStatus()
    {
        VbsButton.Content = SystemFeaturesService.IsVbsEnabled()
            ? "VBS включен"
            : "VBS выключен";
    }

    private void SetHvciStatus()
    {
        HvciButton.Content = SystemFeaturesService.IsHvciEnabled()
            ? "HVCI включен"
            : "HVCI выключен";
    }

    private void HyperVButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var enabled = SystemFeaturesService.IsHyperVEnabled();
            var action = enabled ? "выключить" : "включить";

            var result = MessageBox.Show(
                $"Hyper-V сейчас {(enabled ? "включен" : "выключен")}.\n\nВы хотите {action} Hyper-V?",
                "Hyper-V",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            SystemFeaturesService.ToggleHyperV(!enabled);
            UpdateDashboard();
            SetHyperVStatus();
            ShowRebootMessage();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            ShowAdminCancelledMessage();
        }
        catch (Exception error)
        {
            ShowError("Ошибка Hyper-V", error);
        }
    }

    private void VbsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var enabled = SystemFeaturesService.IsVbsEnabled();
            var action = enabled ? "выключить" : "включить";

            var result = MessageBox.Show(
                $"VBS сейчас {(enabled ? "включен" : "выключен")}.\n\nВы хотите {action} VBS?",
                "VBS",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            SystemFeaturesService.ToggleVbs(!enabled);
            UpdateDashboard();
            SetVbsStatus();
            ShowRebootMessage();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            ShowAdminCancelledMessage();
        }
        catch (Exception error)
        {
            ShowError("Ошибка VBS", error);
        }
    }

    private void HvciButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var enabled = SystemFeaturesService.IsHvciEnabled();
            var action = enabled ? "выключить" : "включить";

            var result = MessageBox.Show(
                $"HVCI сейчас {(enabled ? "включен" : "выключен")}.\n\nВы хотите {action} HVCI?",
                "HVCI",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            SystemFeaturesService.ToggleHvci(!enabled);
            UpdateDashboard();
            SetHvciStatus();
            ShowRebootMessage();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            ShowAdminCancelledMessage();
        }
        catch (Exception error)
        {
            ShowError("Ошибка HVCI", error);
        }
    }

    private void DmaButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("DMA", "Проверка DMA уже отображается на главном экране.");
    }

    private void UacButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("UAC", "Статус UAC отображается на главном экране.");
    }

    private void BlocklistButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("Blocklist", "Проверку Blocklist добавим следующим этапом.");
    }

    private void DefenderButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("Defender", "Статус Defender отображается на главном экране.");
    }

    private void AntiCheatButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("Античиты", "Сканер античитов добавим следующим этапом.");
    }

    private void DebuggerButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("Отладчики", "Сканер отладчиков добавим следующим этапом.");
    }

    private static void ShowRebootMessage()
    {
        MessageBox.Show(
            "Изменение применено. Для завершения требуется перезагрузка Windows.",
            "Misava Checker",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private static void ShowAdminCancelledMessage()
    {
        MessageBox.Show(
            "Операция отменена или не были предоставлены права администратора.",
            "Misava Checker",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    private static void ShowError(string title, Exception error)
    {
        MessageBox.Show(
            error.Message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private static void ShowFunctionMessage(string title, string message)
    {
        MessageBox.Show(
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}

public sealed class SecurityCardViewModel
{
    public string Name { get; }
    public string Status { get; }
    public System.Windows.Media.Brush Brush { get; }

    public SecurityCardViewModel(SecurityItem item)
    {
        Name = item.Name;
        Status = item.Status;
        Brush = item.Status switch
        {
            "Включено" => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(240, 82, 82)),
            "Отключено" => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(34, 201, 129)),
            _ => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(232, 168, 62))
        };
    }
}
