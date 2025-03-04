﻿using System.Security.Cryptography;
using GoodPass.Dialogs;
using GoodPass.Helpers;
using GoodPass.Services;
using GoodPass.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace GoodPass.Views;

public sealed partial class SettingsPage : Page
{
    #region Properties
    public SettingsViewModel ViewModel
    {
        get;
    }

    #endregion

    #region Methods
    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
        if (App.App_IsLock())
        {
            MicrosoftPassportButton.IsEnabled = false;
            MySQLSyncButton.IsEnabled = false;
            MySQLSettingButton.IsEnabled = false;
        }
        else
        {
            MicrosoftPassportButton.IsEnabled = true;
            MySQLSyncButton.IsEnabled = true;
            MySQLSettingButton.IsEnabled = true;
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

        Helpers.MySQLConfigHelper mySQLConfigHelper = new();
        var MySQLres = mySQLConfigHelper.GetMySQLStatusAsync();
        switch(MySQLres.Result)
        {
            case true:
                MySQLSyncButton.IsChecked = true;
                MySQLSyncSituationIcon.Glyph = "\xE73E";
                MySQLSyncSituationText.Text = "已启用MySQL同步";
                break;
            case false:
                MySQLSyncButton.IsChecked = false;
                MySQLSyncSituationText.Text = "已禁用MySQL同步";
                MySQLSyncSituationIcon.Glyph = "\xE711";
                break;
        }
    }

    private async void MicrosoftPassportButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb)
        {
            switch (tb.IsChecked)
            {
                case true:
                    var dialog = new MicrosoftPassportDialog()
                    {
                        XamlRoot = this.XamlRoot,
                        Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
                        Title = "启用Microsoft Passport"
                    };
                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        var masterKey = dialog.MasterKey;
                        var username = Convert.ToBase64String(Aes.Create().IV);
                        var mpResult = await MicrosoftPassportService.SetMicrosoftPassportAsync(username, masterKey);
                        if (mpResult)
                        {
                            _ = SecurityStatusHelper.SetVaultUsername(username);
                        }
                    }
                    else
                    {
                        tb.IsChecked = false;
                        return;
                    }
                    MicrosoftPassportSituationIcon.Glyph = "\xE73E";
                    MicrosoftPassportSituationText.Text = App.UIStrings.MicrosoftPassportSituatoinText1;
                    _ = await SecurityStatusHelper.SetMSPassportStatusAsync(true);
                    break;
                case false:
                    var dialog1 = new MicrosoftPassportDialog()
                    {
                        XamlRoot = this.XamlRoot,
                        Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
                        Title = "禁用Microsoft Passport"
                    };
                    var result1 = await dialog1.ShowAsync();
                    if (result1 == ContentDialogResult.Primary)
                    {
                        var masterKey = dialog1.MasterKey;
                        var username = await SecurityStatusHelper.GetVaultUsername();
                        _ = await MicrosoftPassportService.RemoveMicrosoftPassportAsync(username, masterKey);
                    }
                    else
                    {
                        tb.IsChecked = true;
                        return;
                    }
                    MicrosoftPassportSituationIcon.Glyph = "\xE711";
                    MicrosoftPassportSituationText.Text = App.UIStrings.MicrosoftPassportSituatoinText2;
                    _ = await SecurityStatusHelper.SetMSPassportStatusAsync(false);
                    break;
            }
        }
    }

    #endregion

    private async void MySQLSyncButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton tb)
        {
            Helpers.MySQLConfigHelper mySQLConfigHelper = new();
            switch (tb.IsChecked)
            {
                case true:
                    _ = await mySQLConfigHelper.SetMySQLStatusAsync(true);
                    MySQLSyncSituationIcon.Glyph = "\xE73E";
                    MySQLSyncSituationText.Text = "已启用MySQL同步";
                    App.SQLManager ??= new();
                    MySQLConfigurationDialog configDialog = new MySQLConfigurationDialog()
                    {
                        XamlRoot = this.XamlRoot,
                        Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
                    };
                    _ = await configDialog.ShowAsync();
                    App.SQLManager.isEnabled = true;
                    break;

                case false:
                    _ = await mySQLConfigHelper.SetMySQLStatusAsync(false);
                    MySQLSyncSituationText.Text = "已禁用MySQL同步";
                    MySQLSyncSituationIcon.Glyph = "\xE711";
                    App.SQLManager ??= new();
                    App.SQLManager.isEnabled = false;
                    break;
            }
        }
    }

    private async void MySQLSettingButton_Click(object sender, RoutedEventArgs e)
    {
        MySQLConfigurationDialog configDialog = new MySQLConfigurationDialog()
        {
            XamlRoot = this.XamlRoot,
            Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
        };

        _ = await configDialog.ShowAsync();
    }
}
