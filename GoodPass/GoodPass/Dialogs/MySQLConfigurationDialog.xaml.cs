using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using GoodPass.Helpers;
using Microsoft.Windows.System.Power;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GoodPass.Dialogs;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MySQLConfigurationDialog : ContentDialog
{
    public bool Savestatus { get; set; }

    public MySQLConfigurationDialog()
    {
        this.InitializeComponent();

        MySQLConfigHelper mySQLConfigHelper = new();
        MySQLConfig config = mySQLConfigHelper.GetConfig();

        if (config.IP != null)
            MySQLConfig_IPAddress.Text = config.IP;
        if (config.Port != null)
            MySQLConfig_Port.Text = config.Port;
        if (config.dbName != null)
            MySQLConfig_DBName.Text = config.dbName;
        if (config.Username != null)
            MySQLConfig_Username.Text = config.Username;
        if (config.Password != null)
            MySQLConfig_Password.Password = config.Password;

    }

    private void MySQLConfigDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        MySQLConfigHelper mySQLConfigHelper = new();

        var IP = MySQLConfig_IPAddress.Text;
        var port = MySQLConfig_Port.Text;
        var dbName = MySQLConfig_DBName.Text;
        var username = MySQLConfig_Username.Text;
        var password = MySQLConfig_Password.Password;

        if (mySQLConfigHelper.SaveConfig(IP, port, dbName, username, password))
            Savestatus = true;
        else
            Savestatus = false;

        if (Savestatus)
        {
            try
            {
                App.SQLManager.UpdateConnection();
            }
            catch(Exception ex)
            {

            }
        }
    }
}
