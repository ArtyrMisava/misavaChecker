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
        HyperVButton.Content = "HV Ч статус";
        VbsButton.Content = "VBS Ч статус";
        HvciButton.Content = "HVCI Ч статус";
        DmaButton.Content = "DMA Ч статус";
        UacButton.Content = "UAC Ч статус";
        BlocklistButton.Content = "Blocklist Ч статус";
        DefenderButton.Content = "Defender Ч статус";
        AntiCheatButton.Content = "јнтичиты Ч статус";
        DebuggerButton.Content = "ќтладчики Ч статус";
    }

    private void HyperVButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage(
            "Hyper-V",
            "”правление Hyper-V будет подключено следующим этапом.");
    }

    private void VbsButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage(
            "VBS",
            "”правление VBS будет подключено следующим этапом.");
    }

    private void HvciButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage(
            "HVCI",
            "”правление изол€цией €дра будет подключено следующим этапом.");
    }

    private void DmaButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage(
            "DMA",
            "ѕроверка защиты от DMA-атак будет подключена следующим этапом.");
    }

    private void UacButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage(
            "UAC",
            "”правление UAC будет подключено следующим этапом.");
    }

    private void BlocklistButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage(
            "Blocklist",
            "ѕроверка блокировки у€звимых драйверов будет подключена следующим этапом.");
    }

    private void DefenderButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage(
            "Defender",
            "ѕроверка Microsoft Defender будет подключена следующим этапом.");
    }

    private void AntiCheatButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage(
            "јнтичиты",
            "ѕроверка служб античитов будет подключена следующим этапом.");
    }

    private void DebuggerButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFunctionMessage(
            "ќтладчики",
            "ѕроверка процессов и служб отладчиков будет подключена следующим этапом.");
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
