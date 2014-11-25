using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AzureConfigurationReplaceTool
{
    public partial class Form1 : Form
    {
        private const string SettingFileName = "configuration.xml";
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                var fullPath = AppDomain.CurrentDomain.BaseDirectory + SettingFileName;
                if(File.Exists(fullPath))
                {
                    string content = File.ReadAllText(fullPath);
                    ConfigurationEntity entity = XmlConverter.FromXmlTo<ConfigurationEntity>(content);

                    if(entity != null)
                    {
                        ShowEntityOnUI(entity);
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                ConfigurationEntity entity = GetUIValues();
                string body = XmlConverter.ToXml(entity);

                WriteFile(SettingFileName, body);

                var fullPath = AppDomain.CurrentDomain.BaseDirectory + SettingFileName;
                this.txtOutput.Text = fullPath;

                MessageBox.Show("UI screen data has been save to " + fullPath);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            try
            {
                string folder = this.txtFolder.Text.Trim();
                if (String.IsNullOrWhiteSpace(folder))
                {
                    MessageBox.Show("Please select a folder that includes *.cscfg files.");
                    return;
                }

                if (!Directory.Exists(folder))
                {
                    MessageBox.Show("The folder doesn't exist, please check the folder path.");
                    return;
                }

                ConfigurationEntity entity = GetUIValues();
                bool valid = ValidateAllData(entity);

                if (!valid)
                {
                    MessageBox.Show("There exists invalid value, Please check all values are available.");
                    return;
                }

                // find all *.cscfg files
                //var files = System.IO.Directory.GetFiles(folder,);
                List<string> files = new List<string>();
                foreach (string file in Directory.EnumerateFiles(folder, "*.cscfg", SearchOption.AllDirectories))
                {
                    files.Add(file);
                }

                if (files.Count == 0)
                {
                    MessageBox.Show("The folder doesn't include any *.cscfg files.");
                    return;
                }

                this.txtOutput.Clear();
                StringBuilder sbOutput = new StringBuilder();
                var suffix = DateTime.Now.ToString(".yyyyMMddHHmmss");

                foreach (var file in files)
                {
                    //replace all key with real value
                    string content = File.ReadAllText(file);

                    content = content.Replace("{thumbprint}", entity.Thumbprint);
                    content = content.Replace("{internal-accesskey}", entity.SubSystemAccessKey);
                    content = content.Replace("{redis-server}", entity.RedisServer);
                    content = content.Replace("{redis-key}", entity.RedisKey);
                    content = content.Replace("{AccessTokenSigningKey}", entity.AccessTokenSignKey);
                    content = content.Replace("{ResourceServerEncryptionKey}", entity.ResourceServerEncryptKey);

                    content = content.Replace("{admin-baseurl}", entity.BaseURL_Admin);
                    content = content.Replace("{auth-baseurl}", entity.BaseURL_Auth);
                    content = content.Replace("{developer-baseurl}", entity.BaseURL_Dev);
                    content = content.Replace("{user-baseurl}", entity.BaseURL_User);
                    content = content.Replace("{apis-baseurl}", entity.BaseURL_API);

                    content = content.Replace("{database-server-admin}", entity.DB_Server_Admin);
                    content = content.Replace("{database-server-dev}", entity.DB_Server_Dev);
                    content = content.Replace("{database-server-user}", entity.DB_Server_User);
                    content = content.Replace("{database-server-apis}", entity.DB_Server_API);
                    content = content.Replace("{database-server-log}", entity.DB_Server_Log);

                    content = content.Replace("{admin-database}", entity.DB_Admin);
                    content = content.Replace("{dev-database}", entity.DB_Dev);
                    content = content.Replace("{user-database}", entity.DB_User);
                    content = content.Replace("{apis-database}", entity.DB_API);
                    content = content.Replace("{log-database}", entity.DB_Log);

                    content = content.Replace("{dbUserID-admin}", entity.DB_Admin_UserID);
                    content = content.Replace("{dbUserID-dev}", entity.DB_Dev_UserID);
                    content = content.Replace("{dbUserID-user}", entity.DB_User_UserID);
                    content = content.Replace("{dbUserID-apis}", entity.DB_API_UserID);
                    content = content.Replace("{dbUserID-log}", entity.DB_Log_UserID);

                    content = content.Replace("{dbPassword-admin}", entity.DB_Admin_Password);
                    content = content.Replace("{dbPassword-dev}", entity.DB_Dev_Password);
                    content = content.Replace("{dbPassword-user}", entity.DB_User_Password);
                    content = content.Replace("{dbPassword-apis}", entity.DB_API_Password);
                    content = content.Replace("{dbPassword-log}", entity.DB_Log_Password);

                    content = content.Replace("{AdminStorageAccount}", entity.Storage_Admin_Account);
                    content = content.Replace("{DeveloperStorageAccount}", entity.Storage_Dev_Account);
                    content = content.Replace("{UserStorageAccount}", entity.Storage_User_Account);
                    content = content.Replace("{ApisStorageAccount}", entity.Storage_API_Account);
                    content = content.Replace("{logStorageAccount}", entity.Storage_Log_Account);

                    content = content.Replace("{AccountKeyofAdminStorageAccount}", entity.Storage_Admin_AccountKey);
                    content = content.Replace("{AccountKeyofDeveloperStorageAccount}", entity.Storage_Dev_AccountKey);
                    content = content.Replace("{AccountKeyofUserStorageAccount}", entity.Storage_User_AccountKey);
                    content = content.Replace("{AccountKeyofApisStorageAccount}", entity.Storage_API_AccountKey);
                    content = content.Replace("{AccountKeyoflogStorageAccount}", entity.Storage_Log_AccountKey);

                    content = content.Replace("{facebookAppID}", entity.FacebookAppId);
                    content = content.Replace("{facebookAppSecret}", entity.FacebookAppSecret);
                    content = content.Replace("{twitterConsumerKey}", entity.TwitterAppId);
                    content = content.Replace("{twitterConsumerSecret}", entity.TwitterAppSecret);

                    // create a new file .cscfg
                    var filePath = file + suffix;
                    WriteFile(filePath, content);

                    sbOutput.AppendLine(filePath);
                }

                this.txtOutput.Text = sbOutput.ToString();
                MessageBox.Show("All *.cscfg files have been replaced successfully and save as *.cscfg" + suffix + ". Please see the file list in output.");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static void WriteFile(string filePath, string fileContent)
        {
            try
            {
                Stream stream = File.Open(filePath, FileMode.Create);
                StreamWriter sw = new StreamWriter(stream);

                try
                {
                    sw.Write(fileContent);
                }
                catch
                {
                }
                finally
                {
                    if (sw != null)
                        sw.Close();
                }
            }
            catch
            { }
        }
        private ConfigurationEntity GetUIValues()
        {
            ConfigurationEntity entity = new ConfigurationEntity();

            entity.Thumbprint = this.txtThumbprint.Text.Trim();
            entity.SubSystemAccessKey = this.txtInternalAccessKey.Text.Trim();

            entity.RedisServer = this.txtRedisServer.Text.Trim();
            entity.RedisKey = txtRedisKey.Text.Trim();
            entity.AccessTokenSignKey = txtAccessTokenSigningKey.Text.Trim();
            entity.ResourceServerEncryptKey = txtResourceServerEncryptionKey.Text.Trim();

            entity.BaseURL_Admin = this.txtBaseURL_Admin.Text.Trim();
            entity.BaseURL_API = txtBaseURL_API.Text.Trim();
            entity.BaseURL_Auth = txtBaseURL_Auth.Text.Trim();
            entity.BaseURL_Dev = txtBaseURL_Developer.Text.Trim();
            entity.BaseURL_User = txtBaseURL_User.Text.Trim();

            entity.DB_Server_Admin = this.txtDBServer_Admin.Text.Trim();
            entity.DB_Server_Dev = this.txtDBServer_Dev.Text.Trim();
            entity.DB_Server_Log = this.txtDBServer_Log.Text.Trim();
            entity.DB_Server_User = this.txtDBServer_User.Text.Trim();
            entity.DB_Server_API = this.txtDBServer_API.Text.Trim();

            entity.DB_Admin = this.txtDBName_Admin.Text.Trim();
            entity.DB_API = txtDBName_API.Text.Trim();
            entity.DB_Dev = txtDBName_Dev.Text.Trim();
            entity.DB_User = txtDBName_User.Text.Trim();
            entity.DB_Log = txtDBName_Log.Text.Trim();

            entity.DB_Admin_UserID = this.txtDB_Admin_UserID.Text.Trim();
            entity.DB_Dev_UserID = this.txtDB_Dev_UserId.Text.Trim();
            entity.DB_User_UserID = this.txtDB_User_UserId.Text.Trim();
            entity.DB_API_UserID = this.txtDB_API_UserID.Text.Trim();
            entity.DB_Log_UserID = this.txtDB_Log_UserId.Text.Trim();

            entity.DB_Admin_Password = this.txtDB_Admin_Password.Text.Trim();
            entity.DB_Dev_Password = this.txtDB_Dev_Password.Text.Trim();
            entity.DB_User_Password = this.txtDB_User_Password.Text.Trim();
            entity.DB_API_Password = this.txtDB_API_Password.Text.Trim();
            entity.DB_Log_Password = this.txtDB_Log_Password.Text.Trim();


            entity.Storage_Admin_Account = this.txtStorageAccount_Admin.Text.Trim();
            entity.Storage_API_Account = txtStorageAccount_API.Text.Trim();
            entity.Storage_Dev_Account = txtStorageAccount_Dev.Text.Trim();
            entity.Storage_Log_Account = this.txtStorageAccount_Log.Text.Trim();
            entity.Storage_User_Account = txtStorageAccount_User.Text.Trim();

            entity.Storage_Admin_AccountKey = this.txtStorageAccountKey_Admin.Text.Trim();
            entity.Storage_API_AccountKey = txtStorageAccountKey_API.Text.Trim();
            entity.Storage_Dev_AccountKey = txtStorageAccountKey_Dev.Text.Trim();
            entity.Storage_Log_AccountKey = this.txtStorageAccountKey_Log.Text.Trim();
            entity.Storage_User_AccountKey = txtStorageAccountKey_User.Text.Trim();

            entity.FacebookAppId = this.txtFBAppId.Text.Trim();
            entity.FacebookAppSecret = this.txtFBAppSecret.Text.Trim();

            entity.TwitterAppId = this.txtTwitterKey.Text.Trim();
            entity.TwitterAppSecret = this.txtTwitterSecret.Text.Trim();

            return entity;
        }

        private void ShowEntityOnUI(ConfigurationEntity entity)
        {
            if (entity == null)
                return;

             txtThumbprint.Text = entity.Thumbprint;
             txtInternalAccessKey.Text = entity.SubSystemAccessKey;

             txtRedisServer.Text = entity.RedisServer;
             txtRedisKey.Text = entity.RedisKey;
             txtAccessTokenSigningKey.Text = entity.AccessTokenSignKey;
             txtResourceServerEncryptionKey.Text = entity.ResourceServerEncryptKey;

             txtBaseURL_Admin.Text = entity.BaseURL_Admin;
             txtBaseURL_API.Text = entity.BaseURL_API;
             txtBaseURL_Auth.Text = entity.BaseURL_Auth;
             txtBaseURL_Developer.Text = entity.BaseURL_Dev;
             txtBaseURL_User.Text = entity.BaseURL_User;

             txtDBServer_Admin.Text = entity.DB_Server_Admin;
             txtDBServer_Dev.Text = entity.DB_Server_Dev;
             txtDBServer_Log.Text = entity.DB_Server_Log;
             txtDBServer_User.Text = entity.DB_Server_User;
             txtDBServer_API.Text = entity.DB_Server_API;

             txtDBName_Admin.Text = entity.DB_Admin;
             txtDBName_API.Text = entity.DB_API;
             txtDBName_Dev.Text = entity.DB_Dev;
             txtDBName_User.Text = entity.DB_User;
             txtDBName_Log.Text = entity.DB_Log;

             txtDB_Admin_UserID.Text = entity.DB_Admin_UserID;
             txtDB_Dev_UserId.Text = entity.DB_Dev_UserID;
             txtDB_User_UserId.Text = entity.DB_User_UserID;
             txtDB_API_UserID.Text = entity.DB_API_UserID;
             txtDB_Log_UserId.Text = entity.DB_Log_UserID;

             txtDB_Admin_Password.Text = entity.DB_Admin_Password;
             txtDB_Dev_Password.Text = entity.DB_Dev_Password;
             txtDB_User_Password.Text = entity.DB_User_Password;
             txtDB_API_Password.Text = entity.DB_API_Password;
             txtDB_Log_Password.Text = entity.DB_Log_Password;


             txtStorageAccount_Admin.Text = entity.Storage_Admin_Account;
             txtStorageAccount_API.Text = entity.Storage_API_Account;
             txtStorageAccount_Dev.Text = entity.Storage_Dev_Account;
             txtStorageAccount_Log.Text = entity.Storage_Log_Account;
             txtStorageAccount_User.Text = entity.Storage_User_Account;

             txtStorageAccountKey_Admin.Text = entity.Storage_Admin_AccountKey;
             txtStorageAccountKey_API.Text = entity.Storage_API_AccountKey;
             txtStorageAccountKey_Dev.Text = entity.Storage_Dev_AccountKey;
             txtStorageAccountKey_Log.Text = entity.Storage_Log_AccountKey;
             txtStorageAccountKey_User.Text = entity.Storage_User_AccountKey;

             txtFBAppId.Text = entity.FacebookAppId;
             txtFBAppSecret.Text = entity.FacebookAppSecret;

             txtTwitterKey.Text = entity.TwitterAppId;
             txtTwitterSecret.Text = entity.TwitterAppSecret;
        }

        private void btnBrowser_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select a folder";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    this.txtFolder.Text = dlg.SelectedPath;
                }
            }
        }

        private bool ValidateAllData(ConfigurationEntity entity)
        {

            bool invliad = String.IsNullOrWhiteSpace(entity.Thumbprint)
                || String.IsNullOrWhiteSpace(entity.SubSystemAccessKey)
                || String.IsNullOrWhiteSpace(entity.RedisServer)
                || String.IsNullOrWhiteSpace(entity.RedisKey)
                || String.IsNullOrWhiteSpace(entity.AccessTokenSignKey)
                || String.IsNullOrWhiteSpace(entity.ResourceServerEncryptKey)
                || String.IsNullOrWhiteSpace(entity.BaseURL_Admin)
                || String.IsNullOrWhiteSpace(entity.BaseURL_API)
                || String.IsNullOrWhiteSpace(entity.BaseURL_Auth)
                || String.IsNullOrWhiteSpace(entity.BaseURL_Dev)
                || String.IsNullOrWhiteSpace(entity.BaseURL_User)
                || String.IsNullOrWhiteSpace(entity.DB_Admin)
                || String.IsNullOrWhiteSpace(entity.DB_Admin_Password)
                || String.IsNullOrWhiteSpace(entity.DB_Admin_UserID)
                || String.IsNullOrWhiteSpace(entity.DB_API)
                || String.IsNullOrWhiteSpace(entity.DB_API_Password)
                || String.IsNullOrWhiteSpace(entity.DB_API_UserID)
                || String.IsNullOrWhiteSpace(entity.DB_Dev)
                || String.IsNullOrWhiteSpace(entity.DB_Dev_Password)
                || String.IsNullOrWhiteSpace(entity.DB_Dev_UserID)
                || String.IsNullOrWhiteSpace(entity.DB_Log)
                || String.IsNullOrWhiteSpace(entity.DB_Log_Password)
                || String.IsNullOrWhiteSpace(entity.DB_Log_UserID)
                || String.IsNullOrWhiteSpace(entity.DB_Server_Admin)
                || String.IsNullOrWhiteSpace(entity.DB_Server_API)
                || String.IsNullOrWhiteSpace(entity.DB_Server_Dev)
                || String.IsNullOrWhiteSpace(entity.DB_Server_Log)
                || String.IsNullOrWhiteSpace(entity.DB_Server_User)
                || String.IsNullOrWhiteSpace(entity.DB_User)
                || String.IsNullOrWhiteSpace(entity.DB_User_Password)
                || String.IsNullOrWhiteSpace(entity.DB_User_UserID)
                || String.IsNullOrWhiteSpace(entity.Storage_Admin_Account)
                || String.IsNullOrWhiteSpace(entity.Storage_Admin_AccountKey)
                || String.IsNullOrWhiteSpace(entity.Storage_API_Account)
                || String.IsNullOrWhiteSpace(entity.Storage_API_AccountKey)
                || String.IsNullOrWhiteSpace(entity.Storage_Dev_Account)
                || String.IsNullOrWhiteSpace(entity.Storage_Dev_AccountKey)
                || String.IsNullOrWhiteSpace(entity.Storage_Log_Account)
                || String.IsNullOrWhiteSpace(entity.Storage_Log_AccountKey)
                || String.IsNullOrWhiteSpace(entity.Storage_User_Account)
                || String.IsNullOrWhiteSpace(entity.Storage_User_AccountKey)

                || entity.Thumbprint.StartsWith("{")
                || entity.SubSystemAccessKey.StartsWith("{")
                || entity.RedisServer.StartsWith("{")
                || entity.RedisKey.StartsWith("{")
                || entity.AccessTokenSignKey.StartsWith("{")
                || entity.ResourceServerEncryptKey.StartsWith("{")
                || entity.BaseURL_Admin.StartsWith("{")
                || entity.BaseURL_API.StartsWith("{")
                || entity.BaseURL_Auth.StartsWith("{")
                || entity.BaseURL_Dev.StartsWith("{")
                || entity.BaseURL_User.StartsWith("{")
                || entity.DB_Admin.StartsWith("{")
                || entity.DB_Admin_Password.StartsWith("{")
                || entity.DB_Admin_UserID.StartsWith("{")
                || entity.DB_API.StartsWith("{")
                || entity.DB_API_Password.StartsWith("{")
                || entity.DB_API_UserID.StartsWith("{")
                || entity.DB_Dev.StartsWith("{")
                || entity.DB_Dev_Password.StartsWith("{")
                || entity.DB_Dev_UserID.StartsWith("{")
                || entity.DB_Log.StartsWith("{")
                || entity.DB_Log_Password.StartsWith("{")
                || entity.DB_Log_UserID.StartsWith("{")
                || entity.DB_Server_Admin.StartsWith("{")
                || entity.DB_Server_API.StartsWith("{")
                || entity.DB_Server_Dev.StartsWith("{")
                || entity.DB_Server_Log.StartsWith("{")
                || entity.DB_Server_User.StartsWith("{")
                || entity.DB_User.StartsWith("{")
                || entity.DB_User_Password.StartsWith("{")
                || entity.DB_User_UserID.StartsWith("{")
                || entity.Storage_Admin_Account.StartsWith("{")
                || entity.Storage_Admin_AccountKey.StartsWith("{")
                || entity.Storage_API_Account.StartsWith("{")
                || entity.Storage_API_AccountKey.StartsWith("{")
                || entity.Storage_Dev_Account.StartsWith("{")
                || entity.Storage_Dev_AccountKey.StartsWith("{")
                || entity.Storage_Log_Account.StartsWith("{")
                || entity.Storage_Log_AccountKey.StartsWith("{")
                || entity.Storage_User_Account.StartsWith("{")
                || entity.Storage_User_AccountKey.StartsWith("{");

            return !invliad;
        }

        private void ClearText(TextBox ctrTextBox)
        {
            if (ctrTextBox.Text.StartsWith("{"))
            {
                ctrTextBox.Clear();
            }
        }

        private void SetDefaultValue(TextBox ctrTextBox, string defaultValue)
        {
            if (String.IsNullOrWhiteSpace(ctrTextBox.Text))
            {
                ctrTextBox.Text = defaultValue;
            }
        }

        private void txtThumbprint_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtThumbprint);
        }

        private void txtInternalAccessKey_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtInternalAccessKey);
        }

        private void txtRedisServer_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtRedisServer);
        }

        private void txtRedisKey_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtRedisKey);
        }

        private void txtAccessTokenSigningKey_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtAccessTokenSigningKey);
        }

        private void txtResourceServerEncryptionKey_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtResourceServerEncryptionKey);
        }

        private void txtBaseURL_API_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtBaseURL_API);
        }

        private void txtBaseURL_Admin_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtBaseURL_Admin);
        }

        private void txtBaseURL_User_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtBaseURL_User);
        }

        private void txtBaseURL_Auth_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtBaseURL_Auth);
        }

        private void txtBaseURL_Developer_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtBaseURL_Developer);
        }

        private void txtDBServer_API_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDBServer_API);
        }

        private void txtDBServer_Admin_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDBServer_Admin);
        }

        private void txtDBServer_User_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDBServer_User);
        }

        private void txtDBServer_Dev_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDBServer_Dev);
        }

        private void txtDBServer_Log_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDBServer_Log);
        }

        private void txtDBName_API_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDBName_API);
        }

        private void txtDBName_Admin_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDBName_Admin);
        }

        private void txtDBName_User_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDBName_User);
        }

        private void txtDBName_Dev_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDBName_Dev);
        }

        private void txtDBName_Log_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDBName_Log);
        }

        private void txtDB_API_UserID_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDB_API_UserID);
        }

        private void txtDB_Admin_UserID_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDB_Admin_UserID);
        }

        private void txtDB_User_UserId_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDB_User_UserId);
        }

        private void txtDB_Dev_UserId_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDB_Dev_UserId);
        }

        private void txtDB_Log_UserId_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDB_Log_UserId);
        }

        private void txtDB_API_Password_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDB_API_Password);
        }

        private void txtDB_Admin_Password_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDB_Admin_Password);
        }

        private void txtDB_User_Password_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDB_User_Password);
        }

        private void txtDB_Dev_Password_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDB_Dev_Password);
        }

        private void txtDB_Log_Password_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtDB_Log_Password);
        }

        private void txtStorageAccount_API_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtStorageAccount_API);
        }

        private void txtStorageAccount_Admin_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtStorageAccount_Admin);
        }

        private void txtStorageAccount_User_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtStorageAccount_User);
        }

        private void txtStorageAccount_Dev_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtStorageAccount_Dev);
        }

        private void txtStorageAccount_Log_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtStorageAccount_Log);
        }

        private void txtStorageAccountKey_API_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtStorageAccountKey_API);
        }

        private void txtStorageAccountKey_Admin_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtStorageAccountKey_Admin);
        }

        private void txtStorageAccountKey_User_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtStorageAccountKey_User);
        }

        private void txtStorageAccountKey_Dev_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtStorageAccountKey_Dev);
        }

        private void txtStorageAccountKey_Log_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtStorageAccountKey_Log);
        }

        private void txtFBAppId_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtFBAppId);
        }

        private void txtFBAppSecret_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtFBAppSecret);
        }

        private void txtTwitterKey_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtTwitterKey);
        }

        private void txtTwitterSecret_MouseClick(object sender, MouseEventArgs e)
        {
            ClearText(txtTwitterSecret);
        }


        private void txtInternalAccessKey_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtInternalAccessKey, "{internal-accesskey}");
        }

        private void txtThumbprint_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtThumbprint, "{thumbprint}");
        }

        private void txtRedisServer_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtRedisServer, "{redis-server}");
        }

        private void txtRedisKey_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtRedisKey, "{redis-key}");
        }

        private void txtAccessTokenSigningKey_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtAccessTokenSigningKey, "{AccessTokenSigningKey}");
        }

        private void txtResourceServerEncryptionKey_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtResourceServerEncryptionKey, "{ResourceServerEncryptionKey}");
        }

        private void txtBaseURL_API_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtBaseURL_API, "{apis-baseurl}");
        }

        private void txtBaseURL_Admin_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtBaseURL_Admin, "{admin-baseurl}");
        }

        private void txtBaseURL_User_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtBaseURL_User, "{user-baseurl}");
        }

        private void txtBaseURL_Developer_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtBaseURL_Developer, "{developer-baseurl}");
        }

        private void txtBaseURL_Auth_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtBaseURL_Auth, "{auth-baseurl}");
        }

        private void txtDBServer_API_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDBServer_API, "{database-server-apis}");
        }

        private void txtDBServer_Admin_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDBServer_Admin, "{database-server-admin}");
        }

        private void txtDBServer_User_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDBServer_User, "{database-server-user}");
        }

        private void txtDBServer_Dev_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDBServer_Dev, "{database-server-dev}");
        }

        private void txtDBServer_Log_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDBServer_Log, "{database-server-log}");
        }

        private void txtDBName_API_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDBName_API, "{apis-database}");
        }

        private void txtDBName_Admin_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDBName_Admin, "{admin-database}");
        }

        private void txtDBName_User_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDBName_User, "{user-database}");
        }

        private void txtDBName_Dev_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDBName_Dev, "{dev-database}");
        }

        private void txtDBName_Log_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDBName_Log, "{log-database}");
        }

        private void txtDB_API_UserID_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDB_API_UserID, "{dbUserID-apis}");
        }

        private void txtDB_Admin_UserID_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDB_Admin_UserID, "{dbUserID-admin}");
        }

        private void txtDB_User_UserId_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDB_User_UserId, "{dbUserID-user}");
        }

        private void txtDB_Dev_UserId_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDB_Dev_UserId, "{dbUserID-dev}");
        }

        private void txtDB_Log_UserId_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDB_Log_UserId, "{dbUserID-log}");
        }

        private void txtDB_API_Password_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDB_API_Password, "{dbPassword-apis}");
        }

        private void txtDB_Admin_Password_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDB_Admin_Password, "{dbPassword-admin}");
        }

        private void txtDB_User_Password_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDB_User_Password, "{dbPassword-user}");
        }

        private void txtDB_Dev_Password_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDB_Dev_Password, "{dbPassword-dev}");
        }

        private void txtDB_Log_Password_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtDB_Log_Password, "{dbPassword-log}");
        }

        private void txtStorageAccount_API_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtStorageAccount_API, "{ApisStorageAccount}");
        }

        private void txtStorageAccount_Admin_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtStorageAccount_Admin, "{AdminStorageAccount}");
        }

        private void txtStorageAccount_User_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtStorageAccount_User, "{UserStorageAccount}");
        }

        private void txtStorageAccount_Dev_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtStorageAccount_Dev, "{DeveloperStorageAccount}");
        }

        private void txtStorageAccount_Log_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtStorageAccount_Log, "{logStorageAccount}");
        }

        private void txtStorageAccountKey_API_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtStorageAccountKey_API, "{AccountKeyofApisStorageAccount}");
        }

        private void txtStorageAccountKey_Admin_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtStorageAccountKey_Admin, "{AccountKeyofAdminStorageAccount}");
        }

        private void txtStorageAccountKey_User_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtStorageAccountKey_User, "{AccountKeyofUserStorageAccount}");
        }

        private void txtStorageAccountKey_Dev_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtStorageAccountKey_Dev, "{AccountKeyofDeveloperStorageAccount}");
        }

        private void txtStorageAccountKey_Log_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtStorageAccountKey_Log, "{AccountKeyoflogStorageAccount}");
        }

        private void txtFBAppId_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtFBAppId, "{facebookAppID}");
        }

        private void txtFBAppSecret_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtFBAppSecret, "{facebookAppSecret}");
        }

        private void txtTwitterKey_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtTwitterKey, "{twitterConsumerKey}");
        }

        private void txtTwitterSecret_Leave(object sender, EventArgs e)
        {
            SetDefaultValue(txtTwitterSecret, "{twitterConsumerSecret}");
        }


    }
}
