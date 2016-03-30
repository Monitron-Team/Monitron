﻿using System;
using System.Linq;
using System.Reflection;

using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ICSharpCode.SharpZipLib.Zip;

using Monitron.Common;
using Monitron.ImRpc;
using Monitron.Management.AdminClient;
using System.IO;
using System.Xml.Serialization;

namespace Monitron.Plugins.Management
{
    public class ManagementPlugin : INodePlugin
    {
        private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly IMessengerClient r_Client;

        private readonly IPluginDataStore r_DataStore;

        private readonly StoredPluginsManager r_PluginsManager;

		private readonly IMongoDatabase m_MongoDB;

		private UserAccountsManager m_UserManager;
        
        public IMessengerClient MessangerClient
        {
            get
            {
                return r_Client;
            }
        }

        private readonly RpcAdapter r_Adapter;

        public ManagementPlugin(IMessengerClient i_MessangerClient, IPluginDataStore i_DataStore)
        {
            i_MessangerClient.ConnectionStateChanged += r_Client_ConnectionStateChanged;
            sr_Log.Info("Management Plugin starting");
            r_Client = i_MessangerClient;
            r_DataStore = i_DataStore;
            sr_Log.Debug("Setting up RPC");
            r_Adapter = new RpcAdapter(this, r_Client);
			sr_Log.Debug("Setting up Mongo DB");
			m_MongoDB = createMongoDatabase();
			sr_Log.Debug("Setting up User Manager");
			m_UserManager = new UserAccountsManager(m_MongoDB);
            r_Client_ConnectionStateChanged(this, null);
			sr_Log.Debug("Setting up Stored Plugin Manager");
			r_PluginsManager = new StoredPluginsManager(m_MongoDB);
        }

		private IMongoDatabase createMongoDatabase()
		{
			return new MongoClient(
				new MongoClientSettings
				{
					Server = MongoServerAddress.Parse(r_DataStore.Read<string>("mongo_server")),
					Credentials = new MongoCredential[] {
						MongoCredential.CreateCredential(
							r_DataStore.Read<string>("mongo_database"),
							r_DataStore.Read<string>("mongo_user"),
							r_DataStore.Read<string>("mongo_password")
						)
					},
				}
			).GetDatabase(r_DataStore.Read<string>("mongo_database"));
		}

        void r_Client_ConnectionStateChanged (object sender, ConnectionStateChangedEventArgs e)
        {
            if (r_Client.IsConnected)
            {
                sr_Log.Debug("Setting up nickname");
                r_Client.SetNickname("The Management");
                sr_Log.Debug("Setting up avatar");
                r_Client.SetAvatar(Assembly.GetExecutingAssembly().GetManifestResourceStream("Monitron.Plugins.Management.MonitronAvatar.png"));
                r_Client.AddBuddy(
                    new Identity { Domain = this.r_Client.Identity.Domain, UserName = "admin" },
                    "admin"
                );

                sr_Log.Debug("Notifying masters I'm up");
                foreach (var buddy in r_Client.Buddies.Where(item => item.Groups.Contains("admin")))
                {
                    r_Client.SendMessage(buddy.Identity, "Hello master");
                }
            }
        }

        [RemoteCommand(MethodName="upload_plugin")]
        public string UploadPlugin(
            Identity buddy,
            [Opt("path", "p|path=", "{PATH} for the plugin zip")] string i_Path)
        {
            r_PluginsManager.UploadPluginAsync(i_Path)
                .ContinueWith((task) =>
                {
                        if (task.IsFaulted)
                        {
                            r_Client.SendMessage(buddy, "Error uploading plugin: " + task.Exception.Message);
                        }
                        else
                        {
                            r_Client.SendMessage(buddy, string.Format("Plugin '{0}' uploaded successfully", task.Result.Name));
                        }
                }
                );
            return "Uploading...";
        }

        [RemoteCommand(MethodName="list_plugins")]
        public string ListPlugins()
        {
            string result = "";
            foreach (PluginManifest manifest in r_PluginsManager.ListPlugins())
            {
                result += string.Format("{0} [{1}:{2}]\n", manifest.Name, manifest.Id, manifest.Version);
            }

            return result;
        }

        [RemoteCommand(MethodName="echo")]
        public string Echo(
            Identity buddy,
            [Opt("text", "t|text=", "{TEXT} to echo back")] string i_Text)
        {
            return i_Text;
        }

		[RemoteCommand(MethodName="add_user")]
		public string AddUser(Identity identity, string i_User)
		{
			string rtnString = "Could not add user " + i_User;
			Identity newIdentity = new Identity();
			newIdentity.UserName = i_User;
			newIdentity.Domain = r_Client.Identity.Domain;

			bool succeeded = m_UserManager.AddUser(newIdentity, r_Client.Identity);

			if(succeeded)
			{
				r_Client.AddBuddy(newIdentity, new string[] { "Customers" });
				rtnString = string.Format("Added user {0} successfully", i_User);
			}

			return rtnString;
		}

		[RemoteCommand(MethodName="delete_user")]
		public string DeleteUser(Identity identity, string i_User)
		{
			string rtnString = "Could not delete user " + i_User;
			Identity identityToDelete = new Identity();
			identityToDelete.UserName = i_User;
			identityToDelete.Domain = r_Client.Identity.Domain;

			bool succeeded = m_UserManager.DeleteUser(identityToDelete);

			if(succeeded)
			{
				r_Client.RemoveBuddy(identityToDelete);
				rtnString = string.Format("Deleted user {0} successfully", i_User);
			}

			return rtnString;
		}
    }
}

