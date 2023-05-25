using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace GoodPass.Helpers;

public struct MySQLConfig
{
    public string IP
    {
        get; set;
    }
    public string Port
    {
        get; set;
    }
    public string dbName
    {
        get; set;
    }
    public string Username
    {
        get; set;
    }
    public string Password
    {
        get; set;
    }
};

internal class MySQLConfigHelper
{
    private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
    private ApplicationDataCompositeValue composite;

    public MySQLConfigHelper()
    {
        composite = new();
    }

    public bool SaveConfig(string IP, string port, string dbName, string Username, string Password)
    {
        var savestaus = false;

        try
        {
            composite["IP"] = IP;
            composite["port"] = port;
            composite["name"] = dbName;
            composite["username"] = Username;
            composite["password"] = Password;
            localSettings.Values["MySQLConfig"] = composite;
            savestaus = true;
        }
        catch
        {
        }
        
        return savestaus;
    }

    public MySQLConfig GetConfig()
    {
        // load a composite setting
        ApplicationDataCompositeValue getComposite = (ApplicationDataCompositeValue)localSettings.Values["MySQLConfig"];

        MySQLConfig mySQLConfig = new();

        if (getComposite != null) 
        {
            mySQLConfig.IP = getComposite["IP"] as string;
            mySQLConfig.Port = getComposite["port"] as string;
            mySQLConfig.dbName = getComposite["name"] as string;
            mySQLConfig.Username = getComposite["username"] as string;
            mySQLConfig.Password = getComposite["password"] as string;

            return mySQLConfig;
        }

        return mySQLConfig;
    }

    public async Task<bool> SetMySQLStatusAsync(bool value)
    {
        if (RuntimeHelper.IsMSIX)
        {
            localSettings.Values["MySQLEnable"] = value.ToString();
            await Task.CompletedTask;
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<bool> GetMySQLStatusAsync()
    {
        if (RuntimeHelper.IsMSIX)
        {
            bool MSPS;
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue("MySQLEnable", out var obj))
            {
                MSPS = (string)obj switch
                {
                    "True" => true,
                    "False" => false,
                    _ => false,
                };
            }
            else
            {
                MSPS = false;
            }
            await Task.CompletedTask;
            return MSPS;
        }
        else
        {
            throw new GPRuntimeException("GetMySQLEnable: Not in MSIX");
        }
    }
}
