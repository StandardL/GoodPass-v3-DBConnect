﻿using GoodPass.Dialogs;
using GoodPass.Helpers;
using GoodPass.Services;
using GoodPass.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Security.Credentials;

namespace GoodPass.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
        if (App.App_IsLock())
        {
            MicrosoftPassportButton.IsEnabled = false;
            DataInsertButton.IsEnabled = false;
            AESButton.IsEnabled = false;
        }
        switch (SecurityStatusHelper.GetMSPassportStatusAsync().Result)
        {
            case true:
                MicrosoftPassportButton.IsChecked = true;
                MicrosoftPassportSituationIcon.Glyph = "\xE73E";
                MicrosoftPassportSituationText.Text = App.UIStrings.MicrosoftPassportSituatoinText1;
                break;
            case false:
                MicrosoftPassportButton.IsChecked = false;
                MicrosoftPassportSituationIcon.Glyph = "\xE711";
                MicrosoftPassportSituationText.Text = App.UIStrings.MicrosoftPassportSituatoinText2;
                break;
        }
        switch (SecurityStatusHelper.GetDataInsetStatusAsync().Result)
        {
            case true:
                DataInsertButton.IsChecked = true;
                DataInsertSituationIcon.Glyph = "\xE73E";
                DataInsertSituationText.Text = App.UIStrings.DataInsertSituationText1;
                break;
            case false:
                DataInsertButton.IsChecked = false;
                DataInsertSituationIcon.Glyph = "\xE711";
                DataInsertSituationText.Text = App.UIStrings.DataInsertSituationText2;
                break;
        }
        switch (SecurityStatusHelper.GetAESStatusAsync().Result)
        {
            case true:
                AESButton.IsChecked = true;
                AESSituationIcon.Glyph = "\xE73E";
                AESSituationText.Text = App.UIStrings.AESSituationText1;
                break;
            case false:
                AESButton.IsChecked = false;
                AESSituationIcon.Glyph = "\xE711";
                AESSituationText.Text = App.UIStrings.AESSituationText2;
                break;
        }
    }

    private async void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Check Microsoft Passport is setup and available on this machine
        if (await MicrosoftPassportHelper.MicrosoftPassportAvailableCheckAsync())
        {
            //TestButton.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 98, 255, 223));
            var openKeyResult = await KeyCredentialManager.OpenAsync("2572593789@qq.com");

            if (openKeyResult.Status == KeyCredentialStatus.Success)
            {
                var userKey = openKeyResult.Credential;
                var publicKey = userKey.RetrievePublicKey();
                var signResult = await KeyCredentialManager.OpenAsync("2572593789@qq.com");

                if (signResult.Status == KeyCredentialStatus.Success)
                {
                    var dialog = new GPDialog2()
                    {
                        XamlRoot = this.XamlRoot,
                        Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
                    };
                    dialog.Content = "验证成功";
                    dialog.Title = "Test";
                    _ = await dialog.ShowAsync();
                }
                else if (signResult.Status == KeyCredentialStatus.UserPrefersPassword)
                {

                }
            }
        }
        else
        {
            //TestButton.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 255, 0, 0));
        }
    }

    private async void MicrosoftPassportButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb)
        {
            //TODO: 在完成功能后删除
            tb.ContextFlyout.ShowAt(tb);
            tb.IsChecked = false;
            return;
            switch (tb.IsChecked)
            {
                case true:
                    //TODO: 取消Microsoft Passport关联
                    MicrosoftPassportSituationIcon.Glyph = "\xE73E";
                    MicrosoftPassportSituationText.Text = App.UIStrings.MicrosoftPassportSituatoinText1;
                    _ = await SecurityStatusHelper.SetMSPassportStatusAsync(false);
                    break;
                case false:
                    //TODO: 关联Microsoft Passport
                    MicrosoftPassportSituationIcon.Glyph = "\xE711";
                    MicrosoftPassportSituationText.Text = App.UIStrings.MicrosoftPassportSituatoinText2;
                    _ = await SecurityStatusHelper.SetMSPassportStatusAsync(true);
                    break;
            }
        }
    }

    private void DataInsertButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb)
        {
            //TODO: 在完成功能后删除
            tb.ContextFlyout.ShowAt(tb);
            tb.IsChecked = false;
            return;
            //TODO: 启用/关闭数据内嵌功能
        }
    }

    private async void AESButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb)
        {
            /*TODO: 在完成功能后删除
            tb.ContextFlyout.ShowAt(tb);
            tb.IsChecked = false;
            return;*/
            //TODO: 启用/关闭AES加密
            switch (tb.IsChecked)
            {
                case true:
                    //TODO: 取消Microsoft Passport关联
                    AESSituationIcon.Glyph = "\xE73E";
                    AESSituationText.Text = App.UIStrings.AESSituationText1;
                    _ = await SecurityStatusHelper.SetAESStatusAsync(true);
                    App.DataManager.EncryptAllDatas();
                    break;
                case false:
                    //TODO: 关联Microsoft Passport
                    AESSituationIcon.Glyph = "\xE711";
                    AESSituationText.Text = App.UIStrings.AESSituationText2;
                    _ = await SecurityStatusHelper.SetAESStatusAsync(false);
                    App.DataManager.EncryptAllDatas();
                    break;
            }
        }
    }
}
