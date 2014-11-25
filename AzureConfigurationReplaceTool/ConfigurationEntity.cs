using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureConfigurationReplaceTool
{
    [Serializable]
    public class ConfigurationEntity
    {
        public string Thumbprint;
        public string SubSystemAccessKey;
        public string RedisServer;
        public string RedisKey;
        public string AccessTokenSignKey;
        public string ResourceServerEncryptKey;

        public string BaseURL_API;
        public string BaseURL_User;
        public string BaseURL_Admin;
        public string BaseURL_Dev;
        public string BaseURL_Auth;

        public string DB_Server_API;
        public string DB_API;
        public string DB_API_UserID;
        public string DB_API_Password;

        public string DB_Server_User;
        public string DB_User;
        public string DB_User_UserID;
        public string DB_User_Password;

        public string DB_Server_Dev;
        public string DB_Dev;
        public string DB_Dev_UserID;
        public string DB_Dev_Password;

        public string DB_Server_Admin;
        public string DB_Admin;
        public string DB_Admin_UserID;
        public string DB_Admin_Password;

        public string DB_Server_Log;
        public string DB_Log;
        public string DB_Log_UserID;
        public string DB_Log_Password;

        public string Storage_API_Account;
        public string Storage_API_AccountKey;
        public string Storage_User_Account;
        public string Storage_User_AccountKey;
        public string Storage_Dev_Account;
        public string Storage_Dev_AccountKey;
        public string Storage_Admin_Account;
        public string Storage_Admin_AccountKey;
        public string Storage_Log_Account;
        public string Storage_Log_AccountKey;

        public string FacebookAppId;
        public string FacebookAppSecret;
        public string TwitterAppId;
        public string TwitterAppSecret;
    }
}
