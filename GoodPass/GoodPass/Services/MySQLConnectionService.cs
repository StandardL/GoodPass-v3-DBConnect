﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodPass.Dialogs;
using GoodPass.Models;
using MySql.Data.MySqlClient;

namespace GoodPass.Services;
public class MySQLConnectionService
{
    public bool Connected  // A flag shows the connection state of MySQL Server.
    {
        get; set;
    }
    public bool isEnabled  // A flag shows whether enable MySQL Connection service.
    {
        get; set;
    }

    /// <summary>
    /// 连接信息初始化
    /// </summary>
    /// <parm name=connectionStringBuilder>数据库连接配置</parm>
    /// <parm name=_tableName>数据库中表格名称</parm>
    private readonly MySqlConnectionStringBuilder connectionStringBuilder;
    private MySqlConnection mySqlConnection;
    private static readonly string _tableName = "goodpassdata";
    public MySQLConnectionService()
    {
        Connected = false;
        isEnabled = false;  // not used yet.

        connectionStringBuilder = new MySqlConnectionStringBuilder();
        /*connectionStringBuilder.Server = "localhost";                            // IP地址
        connectionStringBuilder.Port = 3306;                                     // 端口号
        connectionStringBuilder.Database = "your_database_name";                 // 数据库名
        connectionStringBuilder.UserID = "your_database_login_user";             // 登录用户名
        connectionStringBuilder.Password = "your_database_login_password";       // 登录密码
        connectionStringBuilder.Database = "password_data";                 // 数据库名
        connectionStringBuilder.UserID = "root";             // 登录用户名
        connectionStringBuilder.Password = "13944tty";       // 登录密码*/

        Helpers.MySQLConfigHelper mySQLConfigHelper = new();
        var config = mySQLConfigHelper.GetConfig();
        if (config.IP != null && config.Port != null && config.dbName != null && config.Username != null && config.Password != null)
        {
            connectionStringBuilder.Clear();
            connectionStringBuilder.Server = config.IP;
            connectionStringBuilder.Port = uint.Parse(config.Port);
            connectionStringBuilder.Database = config.dbName;
            connectionStringBuilder.UserID = config.Username;
            connectionStringBuilder.Password = config.Password;
        }

        mySqlConnection = new();


        // 传入连接数据并启动连接
        // mySqlConnection = new(connectionStringBuilder.ConnectionString);
    }

    #region SQL连接相关函数
    /// <summary>
    /// 检测MySQL服务器连接情况的方法
    /// </summary>
    /// <returns>MySQL服务器连接情况</returns>
    public bool ConnectionState()
    {
        bool isConnected = false;
        try
        {
            mySqlConnection.Open();
            isConnected = true;
            Connected = true;
        }
        catch (Exception ex)
        {
            Connected = false;
            Debug.WriteLine(ex.Message);
            throw new Exception(ex.Message);
        }
        finally
        {
            mySqlConnection.Close();
        }
        return isConnected;
    }

    // 更新连接信息
    public void UpdateConnection()
    {
        Helpers.MySQLConfigHelper mySQLConfigHelper = new();
        var config = mySQLConfigHelper.GetConfig();

        if (config.IP != null && config.Port != null && config.dbName != null && config.Username != null && config.Password != null)
        {
            connectionStringBuilder.Clear();
            connectionStringBuilder.Server = config.IP;
            connectionStringBuilder.Port = uint.Parse(config.Port);
            connectionStringBuilder.Database = config.dbName;
            connectionStringBuilder.UserID = config.Username;
            connectionStringBuilder.Password = config.Password;

            mySqlConnection.ConnectionString = connectionStringBuilder.ConnectionString;
        }
        try
        {
            _ = this.ConnectionState();
        }
        catch(Exception ex) 
        {
            throw new Exception(ex.Message);
        }
    }

    #endregion

    #region SQL语句执行
    /// <summary>
    /// 执行SQL语句函数
    /// </summary>
    /// <param name="sql">传入的SQL语句</param>
    /// <returns>是否成功执行SQL语句</returns>
    private bool ExecuteSQLCommand(string sql)
    {
        // 确保服务器连接已成功连上
        if (mySqlConnection.State == System.Data.ConnectionState.Closed)
        {
            try
            {
                mySqlConnection.Open();
                Connected = true;
            }
            catch(Exception ex) 
            {
                Debug.WriteLine("数据库链接错误：");
                Debug.WriteLine(ex.Message);
                Connected = false;
                return false; 
            }
        }

        try
        {
            // 创建用于实现MySQL语句的对象
            MySqlCommand mySqlCommand = new MySqlCommand(sql, mySqlConnection);
            // 执行MySQL语句进行插入
            var res = mySqlCommand.ExecuteNonQuery();
            Debug.WriteLine("成功执行插入，数据库中受影响的行数为：" + res);
        }
        catch(Exception ex) 
        {
            Debug.WriteLine("SQL语句执行错误：");
            Debug.WriteLine(ex.Message);
            return false; 
        }   

        return true;
    }
    private List<GPData> ExecuteSQLCommandForSearch(string sql)
    {
        // 确保服务器连接已成功连上
        if (mySqlConnection.State == System.Data.ConnectionState.Closed)
        {
            try
            {
                mySqlConnection.Open();
                Connected = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("数据库链接错误：");
                Debug.WriteLine(ex.Message);
                Connected = false;
                return null;
            }
        }

        List<GPData> search_result = new List<GPData>();
        try
        {
            // 创建用于实现MySQL语句的对象
            MySqlCommand mySqlCommand = new MySqlCommand(sql, mySqlConnection);
            // 执行MySQL语句进行查询
            var reader = mySqlCommand.ExecuteReader();

            while (reader.Read())
            {
                var PlatformName = reader.GetString("PlatformName");
                var PlatformUrl = reader.GetString("PlatformUrl");
                var AccountName = reader.GetString("AccountName");
                var EncPassword = reader.GetString("EncPassword");
                var LatestUpdateTime = reader.GetString("LatestUpdateTime");
                var LatestUpdateTime_DateTime = Convert.ToDateTime(LatestUpdateTime);

                GPData temp = new GPData(PlatformName, PlatformUrl, AccountName, EncPassword, LatestUpdateTime_DateTime);
                temp.DataDecrypt();
                search_result.Add(temp);
            }

            reader.Close();
            Debug.WriteLine("成功执行查询");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("SQL语句执行错误：");
            Debug.WriteLine(ex.Message);
            return null;
        }
        finally
        {
            
        }

        return search_result;
    }
    #endregion

    

    /// <summary>
    /// 在初次连接时，创建对应的表的预留接口
    /// </summary>
    public void FirstConnection()
    {
    }

    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="newData"></param>
    public bool AddData(GPData newData)
    {
        var PlatformName = newData.PlatformName;
        var PlatformUrl = newData.PlatformUrl;
        var AccountName = newData.AccountName;
        var EncPassword = newData.EncPassword;
        var LastestUpdateTime = newData.LatestUpdateTime;

        var sql_AddData = "INSERT INTO " + _tableName + " VALUES ('" + PlatformName + "' ,'" + 
                            PlatformUrl + "', '" + AccountName + "', '" + EncPassword + "','" + 
                            LastestUpdateTime.ToString("y-M-d h:m:s") + "')";

        return ExecuteSQLCommand(sql_AddData);
        
    }

    /// <summary>
    /// 删除数据
    /// </summary>
    public bool DeleteData(string tarPlatform, string tarAccountName)
    {
        var sql_delete = "DELETE FROM " + _tableName + " WHERE PlatformName = '" + 
                         tarPlatform + "' AND AccountName = '" + tarAccountName + "';";

        return ExecuteSQLCommand(sql_delete);
    }

    #region MySQL修改数据
    /// <summary>
    /// 更新网站链接数据
    /// </summary>
    /// <param name="oldPlatformName">检索的主键</param>
    /// <param name="oldAccountName">检索的主键</param>
    /// <param name="newUrl">新的平台网址</param>
    /// <param name="LatestUpdatetime">最后更新时间</param>
    public bool UpdateData_Url(string oldPlatformName, string oldAccountName, string newUrl, DateTime LatestUpdatetime)
    {
        var sql_update = "UPDATE " + _tableName + " SET PlatformUrl = '" + newUrl +
                         "' ,LatestUpdateTime = '" + LatestUpdatetime.ToString("y-M-d h:m:s") +
                         "' where (PlatformName = '" + oldPlatformName + "' " +
                         "AND AccountName = '" + oldAccountName + "');"; 

        return ExecuteSQLCommand(sql_update);
    }

    /// <summary>
    /// 更新网站密码 
    /// </summary>
    /// <param name="oldPlatformName">检索的主键</param>
    /// <param name="oldAccountName">检索的主键</param>
    /// <param name="newPassword">新密码</param>
    /// <param name="LatestUpdatetime">最后更新时间</param>
    public bool UpdateData_Password(string oldPlatformName, string oldAccountName, string newPassword, DateTime LatestUpdatetime)
    {
        var sql_update = "UPDATE " + _tableName + " SET EncPassword = '" + newPassword +
                         "' ,LatestUpdateTime = '" + LatestUpdatetime.ToString("y-M-d h:m:s") +
                         "' where (PlatformName = '" + oldPlatformName + "' " +
                         "AND AccountName = '" + oldAccountName + "');";

        return ExecuteSQLCommand(sql_update);
    }

    /// <summary>
    /// 更新网站用户名
    /// </summary>
    /// <param name="oldPlatformName">检索的主键</param>
    /// <param name="oldAccountName">检索的主键</param>
    /// <param name="newAccountName">新用户名</param>
    /// <param name="LatestUpdatetime">最后更新时间</param>
    public bool UpdateData_AccountName(string oldPlatformName, string oldAccountName, string newAccountName, DateTime LatestUpdatetime)
    {
        var sql_update = "UPDATE " + _tableName + " SET AccountName = '" + newAccountName +
                         "' ,LatestUpdateTime = '" + LatestUpdatetime.ToString("y-M-d h:m:s") +
                         "' where (PlatformName = '" + oldPlatformName + "' " +
                         "AND AccountName = '" + oldAccountName + "');";

        return ExecuteSQLCommand(sql_update);
    }

    /// <summary>
    /// 更新网站名
    /// </summary>
    /// <param name="oldPlatformName">检索的主键</param>
    /// <param name="oldAccountName">检索的主键</param>
    /// <param name="newPlatformName">新网站名</param>
    /// <param name="LatestUpdatetime">最后更新时间</param>
    public bool UpdateData_PlatformName(string oldPlatformName, string oldAccountName, string newPlatformName, DateTime LatestUpdatetime)
    {
        var sql_update = "UPDATE " + _tableName + " SET PlatformName = '" + newPlatformName +
                             "' ,LatestUpdateTime = '" + LatestUpdatetime.ToString("y-M-d h:m:s") +
                             "' where (PlatformName = '" + oldPlatformName + "' " +
                             "AND AccountName = '" + oldAccountName + "');";

        return ExecuteSQLCommand(sql_update);
    }
    #endregion

    /// <summary>
    /// 查询数据
    /// </summary>
    public List<GPData> SearchData(string text)
    {
        var sql_search = "SELECT * FROM " + _tableName + 
                         " WHERE (PlatformName LIKE '%" + text + "%' OR AccountName LIKE '%" + text + "%');";

        return ExecuteSQLCommandForSearch(sql_search);
    }

    public List<GPData> getAllData()
    {
        var sql_search = "SELECT * FROM " + _tableName;    

        return ExecuteSQLCommandForSearch(sql_search);
    }
}
