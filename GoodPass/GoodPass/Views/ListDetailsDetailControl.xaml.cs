﻿using GoodPass.Dialogs;
using GoodPass.Helpers;
using GoodPass.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace GoodPass.Views;

public sealed partial class ListDetailsDetailControl : UserControl
{
    #region Properties
    public GPData? ListDetailsMenuItem
    {
        get => GetValue(ListDetailsMenuItemProperty) as GPData;
        set => SetValue(ListDetailsMenuItemProperty, value);
    }
    public static readonly DependencyProperty ListDetailsMenuItemProperty = DependencyProperty.Register("ListDetailsMenuItem", typeof(GPData), typeof(ListDetailsDetailControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));
    #endregion

    #region Constructor and Basic Handler
    /// <summary>
    /// 初始化控件并加载多语言资源
    /// </summary>
    public ListDetailsDetailControl()
    {
        InitializeComponent();
        ListDetailsDetailControl_AccountNameTitle.Text = App.UIStrings.ListDetailsDetailControl_AccountNameTitleText;
        ListDetailsDetailControl_LastmodifiedTitle.Text = App.UIStrings.ListDetailsDetailControl_LastmodifiedTitleText;
        ListDetailsDetailControl_PasswordTitle.Text = App.UIStrings.ListDetailsDetailControl_PasswordTitleText;
        ListDetailsDetailControl_PlatformUrlTitle.Text = App.UIStrings.ListDetailsDetailControl_PlatformUrlTitleText;
        EditButtonTip.Content = App.UIStrings.EditButtonTipText;
        DeleteButtonTip.Content = App.UIStrings.DeleteButtonTipText;
        PasswordCopyButtonTip.Content = App.UIStrings.PasswordCopyButtonTipText;
        PasswordRevealButtonTip.Content = App.UIStrings.PasswordRevealButtonTipText;
        AccountNameCopyButtonTip.Content = App.UIStrings.AccountNameCopyButtonTipText;
        PlatformUrlButtonTip.Content = App.UIStrings.PlatformUrlButtonTipText;
    }

    private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ListDetailsDetailControl control)
        {
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
    #endregion

    #region CopyButton Functions
    /// <summary>
    /// 复制密码到Windows剪贴板
    /// </summary>
    private void ListDetailsDetailControl_PasswordCopyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var password = ListDetailsDetailControl_PasswordBox.Password;
        var passwordDatapackage = new DataPackage();
        passwordDatapackage.SetText(password);
        Clipboard.SetContent(passwordDatapackage);
        CopiedTipforPasswordCopyButton.Title = App.UIStrings.ListDetailsDetailControl_CopiedTipforPasswordCopyButtonTitle;
        CopiedTipforPasswordCopyButton.IsOpen = true;
    }

    /// <summary>
    /// 复制账号到Windows剪贴板
    /// </summary>
    private void ListDetailsDetailControl_AcconutNameCopyButton_Click(object sender, RoutedEventArgs e)
    {
        var accountName = ListDetailsDetailControl_AccountNameText.Text;
        var dataPackage = new DataPackage();
        dataPackage.SetText(accountName);
        Clipboard.SetContent(dataPackage);
        CopiedTipforAcconutNameCopyButton.Title = App.UIStrings.ListDetailsDetailControl_CopiedTipforAcconutNameCopyButtonTitle;
        CopiedTipforAcconutNameCopyButton.IsOpen = true;
    }
    #endregion

    #region RevealButton Function
    /// <summary>
    /// 显示PasswordBox中的密码
    /// </summary>
    private void PasswordRevealButton_Click(object sender, RoutedEventArgs e)
    {
        if (PasswordRevealButton.IsChecked == true)
        {
            ListDetailsDetailControl_PasswordBox.PasswordRevealMode = PasswordRevealMode.Visible;
        }
        else
        {
            ListDetailsDetailControl_PasswordBox.PasswordRevealMode = PasswordRevealMode.Hidden;
        }
    }
    #endregion

    #region Data Edit and Delete Functions
    /// <summary>
    /// 进入编辑模式
    /// </summary>
    private async void ListDetailsDetailControl_EditButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new EditDataDialog(ListDetailsDetailControl_AccountNameText.Text, PlatformNameText.Text, ListDetailsDetailControl_PlatformUrlHyperLinkText.Text, ListDetailsDetailControl_PasswordBox.Password)
        {
            XamlRoot = this.XamlRoot,
            Style = App.Current.Resources["DefaultContentDialogStyle"] as Style
        };
        ListDetailsDetailControl_EditButton.IsEnabled = false;
        _ = await dialog.ShowAsync();
        if (dialog.Result == EditDataResult.Nochange)
        {
            /*等待用户反馈是否需要添加*/
            /*var warningdialog = new GPDialog2()
            {
                XamlRoot = this.XamlRoot,
                Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "提示",
                Content = "数据没有任何修改！"
            };
            _ = warningdialog.ShowAsync();*/
        }
        else if (dialog.Result == EditDataResult.Success)
        {
            ListDetailsDetailControl_AccountNameText.Text = dialog.newAccountName;
            ListDetailsDetailControl_PasswordBox.Password = dialog.newPassword;
            ListDetailsDetailControl_PlatformUrlHyperLinkText.Text = dialog.newPlatformUrl;
            PlatformNameText.Text = dialog.newPlatformName;
            ListDetailsDetailControl_LastmodifiedText.Text = dialog.newDateTime.ToString();
            await App.DataManager.SaveToFileAsync($"C:\\Users\\{Environment.UserName}\\AppData\\Local\\GoodPass\\GoodPassData.csv");
        }
        else if (dialog.Result == EditDataResult.Failure)
        {
            var warningdialog = new GPDialog2()
            {
                XamlRoot = this.XamlRoot,
                Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "出错了！",
                Content = "编辑数据过程中出错，请重试或者联系开发者"
            };
            _ = warningdialog.ShowAsync();
        }
        ListDetailsDetailControl_EditButton.IsEnabled = true;
    }

    /// <summary>
    /// 删除数据
    /// </summary>
    private async void ListDetailsDetailControl_DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        //弹窗提示用户确认
        var dialog = new GPDialog2()
        {
            XamlRoot = this.XamlRoot,
            Style = App.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "删除确认",
            Content = "您确定要删除这个数据吗？该操作不可撤销！",
        };
        ListDetailsDetailControl_DeleteButton.IsEnabled = false;
        var result = await dialog.ShowAsync();
        bool sql_result = false;

        if (result == ContentDialogResult.Primary)
        {
            var tarPlatform = ListDetailsMenuItem.PlatformName;
            var tarAccountName = ListDetailsMenuItem.AccountName;
            try
            {
                App.ListDetailsVM.DeleteDataItem(App.DataManager.GetData(tarPlatform, tarAccountName));

                Helpers.MySQLConfigHelper mySQL = new();
                if (mySQL.GetMySQLStatusAsync().Result)
                {
                    sql_result = App.SQLManager.DeleteData(tarPlatform, tarAccountName);  // SQL删除
                }

                var delResult = App.DataManager.DeleteData(tarPlatform, tarAccountName);
                if (delResult == false)
                    throw new GPObjectNotFoundException("Data Not Found!");
                else
                    await App.DataManager.SaveToFileAsync($"C:\\Users\\{Environment.UserName}\\AppData\\Local\\GoodPass\\GoodPassData.csv");
                ListDetailsDetailControl_DeleteButton.IsEnabled = true;    
            }
            catch (System.ArgumentOutOfRangeException)
            {
                var warningDialog = new GPDialog2();
                warningDialog.XamlRoot = XamlRoot;
                warningDialog.Style = App.Current.Resources["DefaultContentDialogStyle"] as Style;
                warningDialog.Title = "出错了！";
                warningDialog.Content = "您试图删除一个不存在的对象";
                _ = await warningDialog.ShowAsync();
            }
            catch (GPObjectNotFoundException)
            {
                var warningDialog = new GPDialog2();
                warningDialog.XamlRoot = XamlRoot;
                warningDialog.Style = App.Current.Resources["DefaultContentDialogStyle"] as Style;
                warningDialog.Title = "出错了！";
                warningDialog.Content = "您试图删除一个不存在的对象";
                _ = await warningDialog.ShowAsync();
            }
        }
        else
        {
            ListDetailsDetailControl_DeleteButton.IsEnabled = true;
        }

        Helpers.MySQLConfigHelper mySQLConfigHelper = new();
        if (mySQLConfigHelper.GetMySQLStatusAsync().Result)
        {
            if (sql_result == false)
            {
                var message = "因为某种原因，向MySQL发起删除请求失败。请检查以下几种情况：\n1.MySQL服务是否能正常连接？可以点击右上角的云朵图标进行检查" +
                              "\n2.是否删除了不存在的内容？\n3.若前面几点已确认无异常，请及时向开发者反馈.";
                MySQLConnectionErrorDialog mySQLConnectionErrorDialog = new MySQLConnectionErrorDialog("MySQL Delete Failed", message)
                {
                    XamlRoot = this.XamlRoot,
                    Style = App.Current.Resources["DefaultContentDialogStyle"] as Style
                };
                _ = await mySQLConnectionErrorDialog.ShowAsync();
            }
        }

        
    }
    #endregion

    #region HyperLink Functions
    /// <summary>
    /// 超链接按钮点击功能
    /// </summary>
    private async void ListDetailsDetailControl_PlatformUrlHyperLink_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ListDetailsDetailControl_PlatformUrlHyperLink.NavigateUri = new Uri(ListDetailsMenuItem.PlatformUrl);
        }
        catch (ArgumentNullException)
        {
            ListDetailsDetailControl_PlatformUrlHyperLink.NavigateUri = null;
            var warningDialog = new GPDialog2();
            warningDialog.XamlRoot = XamlRoot;
            warningDialog.Style = App.Current.Resources["DefaultContentDialogStyle"] as Style;
            warningDialog.Title = "出错了！";
            warningDialog.Content = "链接为空，无法访问！";
            _ = await warningDialog.ShowAsync();
        }
        catch (UriFormatException)
        {
            ListDetailsDetailControl_PlatformUrlHyperLink.NavigateUri = null;
            var warningDialog = new GPDialog2();
            warningDialog.XamlRoot = XamlRoot;
            warningDialog.Style = App.Current.Resources["DefaultContentDialogStyle"] as Style;
            warningDialog.Title = "出错了！";
            warningDialog.Content = "链接为空，无法访问！";
            _ = await warningDialog.ShowAsync();
        }
    }
    #endregion

    #region OOBE Functions
    /// <summary>
    /// 显示OOBE提示
    /// </summary>
    public void ShowOOBETips()
    {
        this.OOBE_DeleteTip.IsOpen = true;
        this.OOBE_EditTip.IsOpen = true;
        this.CopiedTipforPasswordCopyButton.Title = "点击此按钮复制账号名";
        this.CopiedTipforAcconutNameCopyButton.Title = "点击此按钮复制密码";
        this.CopiedTipforAcconutNameCopyButton.IsOpen = true;
        this.CopiedTipforPasswordCopyButton.IsOpen = true;
    }
    #endregion
}
