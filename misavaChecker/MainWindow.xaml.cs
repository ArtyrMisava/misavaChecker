using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MisavaChecker;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        UpdateFunctionStatuses();
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
        {
            DragMove();
        }
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

    private void UpdateFunctionStatuses()
    {
        SetHyperVStatus();

        VbsButton.Content = "VBS — статус";
        HvciButton.Content = "HVCI — статус";
        DmaButton.Content = "DMA — статус";
        UacButton.Content = "UAC — статус";
        BlocklistButton.Content = "Blocklist — статус";
        DefenderButton.Content = "Defender — статус";
        AntiCheatButton.Content = "Античиты — статус";
        DebuggerButton.Content = "Отладчики — статус";
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

    private void HyperVButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var enabled = SystemFeaturesService.IsHyperVEnabled();
            var action = enabled ? "выключить" : "включить";
            var state = enabled ? "Hyper-V включен" : "Hyper-V выключен";

            var message = state + "." + Environment.NewLine + Environment.NewLine;
            message += "Вы хотите " + action + " Hyper-V?";

            var result = MessageBox.Show(
                message,
                "Hyper-V",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            SystemFeaturesService.ToggleHyperV(!enabled);
            SetHyperVStatus();

            MessageBox.Show(
                "Изменение применено. Для завершения требуется перезагрузка Windows.",
                "Hyper-V",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            MessageBox.Show(
                "Операция отменена или не были предоставлены права администратора.",
                "Hyper-V",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception error)
        {
            MessageBox.Show(
                error.Message,
                "Ошибка Hyper-V",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void VbsButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("VBS", "Переключатель VBS добавим следующим этапом.");
    }

    private void HvciButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("HVCI", "Переключатель HVCI добавим следующим этапом.");
    }

    private void DmaButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("DMA", "Проверку DMA добавим следующим этапом.");
    }

    private void UacButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("UAC", "Переключатель UAC добавим следующим этапом.");
    }

    private void BlocklistButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("Blocklist", "Проверку Blocklist добавим следующим этапом.");
    }

    private void DefenderButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("Defender", "Проверку Defender добавим следующим этапом.");
    }

    private void AntiCheatButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("Античиты", "Проверку служб античитов добавим следующим этапом.");
    }

    private void DebuggerButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage("Отладчики", "Проверку процессов и служб добавим следующим этапом.");
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
