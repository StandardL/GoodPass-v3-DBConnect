﻿using System.Timers;
using GoodPass.Contracts.Services;
using GoodPass.Dialogs;
using GoodPass.Helpers;
using GoodPass.Services;
using GoodPass.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.System;

namespace GoodPass.Views;

public partial class ShellPage : Page
{
    #region Properties
    public ShellViewModel ViewModel
    {
        get;
    }

    private System.Timers.Timer timer;  // 计时器
    #endregion

    #region Constructor and Basic Handlers
    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        App.UIStrings = App.GetService<MultilingualStringsServices>().Getzh_CN();
        InitializeComponent();
        if (OOBEServices.GetOOBEStatusAsync("ShellOOBE").Result == Models.OOBESituation.EnableOOBE)
        {
            OOBE_GoBackTip.IsOpen = true;
            OOBE_AddDataTip.IsOpen = true;
            OOBE_SettingTip.IsOpen = true;
        }
        if (OOBEServices.GetOOBEStatusAsync("SearchOOBE").Result == Models.OOBESituation.EnableOOBE)
        {
            OOBE_SearchTip.IsOpen = true;
        }
        ViewModel.NavigationService.Frame = NavigationFrame;

        // TODO: Set the title bar icon by updating /Assets/WindowIcon.ico.
        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();

    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));

        ShellMenuBarSettingsButton.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(ShellAnimatedIcon_PointerPressed), true);
        ShellMenuBarSettingsButton.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(ShellAnimatedIcon_PointerReleased), true);
        ShellMenuBarItem_Back.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(ShellAnimatedIcon_PointerPressed), true);
        ShellMenuBarItem_Back.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(ShellAnimatedIcon_PointerReleased), true);
        ShellMenuSearchButton.AddHandler(UIElement.PointerEnteredEvent, new PointerEventHandler(ShellAnimatedIcon_PointerEntered), true);
        ShellMenuSearchButton.AddHandler(UIElement.PointerExitedEvent, new PointerEventHandler(ShellAnimatedIcon_PointerExited), true);
        ShellMenuSearchButton.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(ShellAnimatedIcon_PointerPressed), true);
        ShellMenuSearchButton.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(ShellAnimatedIcon_PointerReleased), true);

        Grid.SetRow(App.infoBar, 3);
        AppTitleBar.Children.Add(App.infoBar);
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ShellMenuBarSettingsButton.RemoveHandler(UIElement.PointerPressedEvent, (PointerEventHandler)ShellAnimatedIcon_PointerPressed);
        ShellMenuBarSettingsButton.RemoveHandler(UIElement.PointerReleasedEvent, (PointerEventHandler)ShellAnimatedIcon_PointerReleased);
        ShellMenuBarItem_Back.RemoveHandler(UIElement.PointerReleasedEvent, (PointerEventHandler)ShellAnimatedIcon_PointerReleased);
        ShellMenuBarItem_Back.RemoveHandler(UIElement.PointerPressedEvent, (PointerEventHandler)ShellAnimatedIcon_PointerPressed);
        ShellMenuSearchButton.RemoveHandler(UIElement.PointerEnteredEvent, (PointerEventHandler)ShellAnimatedIcon_PointerEntered);
        ShellMenuSearchButton.RemoveHandler(UIElement.PointerExitedEvent, (PointerEventHandler)ShellAnimatedIcon_PointerExited);
        ShellMenuSearchButton.RemoveHandler(UIElement.PointerPressedEvent, (PointerEventHandler)ShellAnimatedIcon_PointerPressed);
        ShellMenuSearchButton.RemoveHandler(UIElement.PointerReleasedEvent, (PointerEventHandler)ShellAnimatedIcon_PointerReleased);
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }

    private void ShellAnimatedIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "PointerOver");
    }

    private void ShellAnimatedIcon_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "Pressed");
    }

    private void ShellAnimatedIcon_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "Normal");
    }

    private void ShellAnimatedIcon_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        AnimatedIcon.SetState((UIElement)sender, "Normal");
    }
    #endregion

    #region AddDataButton Functions
    /// <summary>
    /// 添加数据按钮的事件响应
    /// </summary>
    private async void ShellMenuBarAddDataButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.App_IsLock())
        {
            ShellMenuBarAddDataButton.Flyout.ShowAt(ShellMenuBarSettingsButton);
        }
        else if (App.App_IsLock() == false)
        {
            ShellMenuBarAddDataButton.Flyout.Hide();
            AddDataDialog addDataDialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = App.Current.Resources["DefaultContentDialogStyle"] as Style
            };
            _ = await addDataDialog.ShowAsync();
            if (addDataDialog.Result == Models.AddDataResult.Failure_Duplicate)
            {
                GPDialog2 warningdialog = new()
                {
                    XamlRoot = this.XamlRoot,
                    Style = App.Current.Resources["DefaultContentDialogStyle"] as Style
                };
                warningdialog.Title = "出错了！";
                warningdialog.Content = "数据重复，请前往修改已存在的数据";
                _ = await warningdialog.ShowAsync();
            }
            else if (addDataDialog.Result == Models.AddDataResult.Success)
            {
                await App.DataManager.SaveToFileAsync($"C:\\Users\\{Environment.UserName}\\AppData\\Local\\GoodPass\\GoodPassData.csv");
            }

            if (addDataDialog.SQLResult == Models.SQLAddDataResult.Success)
            {
                SQLConnectionSuccess("");
            }
            else if (addDataDialog.SQLResult == Models.SQLAddDataResult.Failure_Duplicate)
            {
                SQLConnectionError("插入到MySQL服务器中失败！");
            }
        }
        else
        {
            ShellMenuBarAddDataButton.Flyout.ShowAt(ShellMenuBarSettingsButton);
        }
    }


    #endregion

    #region SearchButton Functions 
    private void ShellMenuSearchButton_Click(object sender, RoutedEventArgs e)
    {
        if (!App.App_IsLock() && !App.IsInSettingsPage())
        {
            ShellMenuSearchTip.IsOpen = true;
        }
        if (App.SQLManager != null && App.SQLManager.Connected == false)
        {
            SQLConnectionWarning("因为MySQL服务器连接失败，对服务器进行搜索的功能已禁用");
        }
    }

    /// <summary>
    /// Handle text change and present suitable items
    /// </summary>
    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        // Since selecting an item will also change the text,
        // only listen to changes caused by user entering text.
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var suitableItems = new List<string>();
            var text = sender.Text;
            var matchDatas = App.DataManager.SuggestSearch(text);
            List<Models.GPData> matchDatasDB = null;

            if (App.SQLManager.Connected)
               matchDatasDB = App.SQLManager.SearchData(text);

            foreach (var data in matchDatas)
            {
                suitableItems.Add($"{data.PlatformName} - {data.AccountName}");
            }
            if (matchDatasDB != null)
            {
                foreach (var data in matchDatasDB)
                {
                    suitableItems.Add($"{data.PlatformName} - {data.AccountName} (Cloud Database)");
                }
            }
            
            if (suitableItems.Count == 0)
            {
                suitableItems.Add("No results found");
            }
            sender.ItemsSource = suitableItems;
        }
    }

    /// <summary>
    /// 处理用户选择的结果
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        var slectedItem = args.SelectedItem;
        if (slectedItem is not null)
        {
            if (slectedItem.ToString() == "No results found")
            {
            }
            else
            {
                var originText = slectedItem.ToString();
                var appendindex = originText.IndexOf("(Cloud Database)");
                originText = originText.Remove(appendindex - 1);
                var splitText = originText.Split(" - ");
                var platformName = splitText[0];
                var accountName = splitText[1];
                var index = App.DataManager.AccurateSearch(platformName, accountName);
                App.ListDetailsVM.GoToData(index);
            }
        }
        else
        {
            throw new ArgumentNullException("AutoSuggestBox_SuggestionChosen: SelectedItem is null");
        }
    }
    #endregion

    #region ExportButton Functions
    /// <summary>
    /// 导出加密数据
    /// </summary>
    private async void ShellMenuItem_File_ExportCiphertext_Click(object sender, RoutedEventArgs e)
    {
        if (App.App_IsLock() == false)
        {
            GPDialog2 confirmDialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = App.Current.Resources["DefaultContentDialogStyle"] as Style
            };
            confirmDialog.Title = "导出数据？";
            confirmDialog.Content = "点击确定即可导出数据";
            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var saveResult = await App.DataManager.SaveToFileAsync($"C:\\Users\\{Environment.UserName}\\Downloads\\GoodPassData.csv");
                if (saveResult == true)
                {
                    var infoDialog = new GPDialog2
                    {
                        XamlRoot = this.XamlRoot,
                        Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
                        Title = "导出数据",
                        Content = "导出成功，请前往下载文件夹查看"
                    };
                    _ = await infoDialog.ShowAsync();
                }
            }
        }
    }

    /// <summary>
    /// 导出未加密数据
    /// </summary>
    private async void ShellMenuItem_File_ExportPlaintext_Click(object sender, RoutedEventArgs e)
    {
        if (App.App_IsLock() == false)
        {
            GPDialog2 confirmDialog = new()
            {
                XamlRoot = this.XamlRoot,
                Style = App.Current.Resources["DefaultContentDialogStyle"] as Style
            };
            confirmDialog.Title = "导出未加密数据？";
            confirmDialog.Content = "您正在导出未加密数据！请点击确认按钮导出数据";
            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var saveResult = await App.DataManager.SavePlaintextToFile($"C:\\Users\\{Environment.UserName}\\Downloads\\GoodPassData.csv");
                if (saveResult == true)
                {
                    var infoDialog = new GPDialog2
                    {
                        XamlRoot = this.XamlRoot,
                        Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
                        Title = "导出数据",
                        Content = "导出成功，请前往下载文件夹查看"
                    };
                    _ = await infoDialog.ShowAsync();
                }
            }
        }
    }
    #endregion

    #region TeachingTip Functions
    private async void OOBE_AddDataTip_CloseButtonClick(TeachingTip sender, object args)
    {
        OOBE_AddDataTip.IsOpen = false;
        _ = await OOBEServices.SetOOBEStatusAsync("ShellOOBE", Models.OOBESituation.DIsableOOBE);
    }

    private async void OOBE_SearchTip_CloseButtonClick(TeachingTip sender, object args)
    {
        await OOBEServices.SetOOBEStatusAsync("SearchOOBE", Models.OOBESituation.DIsableOOBE);
    }
    #endregion

    #region InfoBar Functions
    public void SQLConnectionWarning(string  message)
    {
        ShellpageInfoBar.Severity = InfoBarSeverity.Warning;
        ShellpageInfoBar.Title = "MySQL Server Connection Fail";
        ShellpageInfoBar.Message = message + "\nPlease check MySQL server state.";
        ShellpageInfoBar.IsOpen = true;
    }
    public void SQLConnectionError(string message)
    {
        ShellpageInfoBar.Severity = InfoBarSeverity.Error;
        ShellpageInfoBar.Title = "MySQL Server Connection Error";
        ShellpageInfoBar.Message = message + "\nPlease check MySQL server state.";
        ShellpageInfoBar.IsOpen = true;
    }
    public void SQLConnectionSuccess()
    {
        ShellpageInfoBar.Severity = InfoBarSeverity.Success;
        ShellpageInfoBar.Title = "MySQL Server Connection Success";
        ShellpageInfoBar.Message = "Your modification is sync to MySQL server.";
        ShellpageInfoBar.IsOpen = true;
    }
    public void SQLConnectionSuccess(string message)
    {
        ShellpageInfoBar.Severity = InfoBarSeverity.Success;
        ShellpageInfoBar.Title = "MySQL Server Connection Success";
        ShellpageInfoBar.Message = message;
        ShellpageInfoBar.IsOpen = true;
    }
    #endregion

    #region Checking MySQL Conneciton Functions (Every 10sec)
    /// <summary>
    /// Timing code are written by ChatGPT (12th, May version)
    /// Debug and Update by @StandardL
    /// </summary>
    private void StartTimer()
    {
        timer = new System.Timers.Timer();
        timer.Interval = 10000; // 10 seconds in milliseconds
        timer.Elapsed += TimerElapsed;
        timer.Start();
    }
    private void StopTimer()
    {
        if (timer != null)
        {
            timer.Stop();
            timer.Elapsed -= TimerElapsed;
            timer.Dispose();
            timer = null;
        }
    }
    private async void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        // Perform MySQL server connection status check here
        bool isConnected = await CheckMySQLServerConnection();

        // Update UI or perform any other actions based on the connection status
        if (isConnected)
        {
            // MySQL server is connected
            // Update UI or perform necessary actions
            SetIconGreen();
        }
        else
        {
            // MySQL server is not connected
            // Update UI or perform necessary actions
            SetIconRed();
        }
    }
    private async Task<bool> CheckMySQLServerConnection()
    {
        // Implement your logic to check MySQL server connection
        // You can use libraries like MySQL Connector/NET to establish a connection
        // Return true if the server is connected, false otherwise

        bool isConnected = true;

        try
        {
            // Establish MySQL server connection
            // Example code:
            // MySqlConnection connection = new MySqlConnection("your_connection_string");
            // await connection.OpenAsync();

            // Check the connection state
            // Example code:
            // isConnected = connection.State == ConnectionState.Open;

            // Close the connection
            // Example code:
            // connection.Close();
        }
        catch (Exception ex)
        {
            SQLConnectionError(ex.Message);
        }
        await Task.CompletedTask;
        return isConnected;
    }
    private void ShellMenuBarDatabaseConnectionSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        ToggleSwitch toggleSwitch = sender as ToggleSwitch;
        if (toggleSwitch != null)
        {
            if (toggleSwitch.IsOn == true)
            {
                StartTimer();            }
            else
            {
                StopTimer();
            }
        }
    }

    private void SetIconGreen()
    {
        SuccessInfoBadge.Visibility = Visibility.Visible;
        FailInfoBadge.Visibility = Visibility.Collapsed;
    }
    private void SetIconRed()
    {
        FailInfoBadge.Visibility = Visibility.Visible;
        SuccessInfoBadge.Visibility = Visibility.Collapsed;
    }
    #endregion

    #region 点击时检测MySQL连接情况
    private void ShellMenuBarDatabaseInfoButton_Click(object sender, RoutedEventArgs e)
    {
        if (CheckMySQLConnection())
        {
            SetIconGreen();
            SQLConnectionSuccess("MySQL服务器连接成功！");
        }
        else
        {
            SQLConnectionWarning("MySQL服务器连接失败，请先解锁后再次尝试，然后检查本机网络连接和MySQL服务是否正常！");
            SetIconRed();
        }
    }

    private bool CheckMySQLConnection()
    {
        // Implement your logic to check MySQL server connection
        // You can use libraries like MySQL Connector/NET to establish a connection
        // Return true if the server is connected, false otherwise

        bool isConnected = false;

        try
        {
            if (App.SQLManager !=  null)
            {
                isConnected = App.SQLManager.ConnectionState();
            }
        }
        catch (Exception ex)
        {
            SQLConnectionError(ex.Message);
        }
        
        return isConnected;
    }
    #endregion


}
