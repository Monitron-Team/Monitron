using System;
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
using System.Collections.Generic;

namespace Monitron.Plugins.Management
{
    public class ManagementPlugin : INodePlugin
    {
        private const long k_MaxPluginSize = 20 * (1<<20); //20M

        private readonly string k_WorkerUserName = "local_monitor";

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

        private readonly SortedSet<string> r_AvailableWorkers = new SortedSet<string>();

        public ManagementPlugin(IMessengerClient i_MessangerClient, IPluginDataStore i_DataStore)
        {
            i_MessangerClient.ConnectionStateChanged += r_Client_ConnectionStateChanged;
            i_MessangerClient.FileTransferRequest = r_Client_FileTransferRequest;
            i_MessangerClient.FileTransferProgress += i_MessangerClient_FileTransferProgress;
            i_MessangerClient.FileTransferAborted += i_MessangerClient_FileTransferAborted;
            i_MessangerClient.BuddySignedIn += (sender, e) => {
                if (e.Buddy.UserName == k_WorkerUserName)
                {
                    r_AvailableWorkers.Add(e.Buddy.Resource);
                }
            };
            i_MessangerClient.BuddySignedOut += (sender, e) => {
                if (e.Buddy.UserName == k_WorkerUserName)
                {
                    r_AvailableWorkers.Remove(e.Buddy.Resource);
                }
            };
            sr_Log.Info("Management Plugin starting");
            r_Client = i_MessangerClient;
            r_DataStore = i_DataStore;
            sr_Log.Debug("Setting up RPC");
            r_Adapter = new RpcAdapter(this, r_Client);
			sr_Log.Debug("Setting up Mongo DB");
			m_MongoDB = createMongoDatabase();
			sr_Log.Debug("Setting up Stored Plugin Manager");
			r_PluginsManager = new StoredPluginsManager(m_MongoDB);
        }
        private void i_MessangerClient_FileTransferAborted (object sender, FileTransferAbortedEventArgs e)
        {
            string path = getFileTransferTmpPath(e.FileTransfer);
            File.Delete(path);
        }

        private void i_MessangerClient_FileTransferProgress (object sender, FileTransferProgressEventArgs e)
        {
            bool transferFinished = e.FileTransfer.Transferred == e.FileTransfer.Size;
            Identity buddy = e.FileTransfer.From;
            if (transferFinished)
            {
                string path = getFileTransferTmpPath(e.FileTransfer);
                r_PluginsManager.UploadPluginAsync(path)
                    .ContinueWith((task) =>
                        {
                            if (task.IsFaulted)
                            {
                                r_Client.SendMessage(buddy, "Error uploading plugin: " + task.Exception.InnerException.Message);
                            }
                            else
                            {
                                r_Client.SendMessage(buddy, string.Format("Plugin '{0}' uploaded successfully", task.Result.Name));
                            }

                            File.Delete(path);
                        }
                    );
            }
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

        private string r_Client_FileTransferRequest(IFileTransfer i_FileTransfer) {
            if (i_FileTransfer.Size > k_MaxPluginSize)
            {
                this.r_Client.SendMessage(i_FileTransfer.From, "Refusing upload, file to large");
                return null;
            }
                
            //this.r_Client.SendMessage(i_FileTransfer.From,
            //    string.Format("File transfer for {} initiated", i_FileTransfer.Name));
            return getFileTransferTmpPath(i_FileTransfer);
        }

        private static string getFileTransferTmpPath(IFileTransfer i_FileTransfer)
        {
            return string.Join(Path.DirectorySeparatorChar.ToString(), Path.GetTempPath(), i_FileTransfer.Id);
        }

        private void r_Client_ConnectionStateChanged (object sender, ConnectionStateChangedEventArgs e)
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

                sr_Log.Debug("Starting accounts manager");
                m_UserManager = new UserAccountsManager(
                    i_Database: m_MongoDB,
                    i_AdminHost: r_Client.Identity.Domain,
                    i_AdminUsername: r_DataStore.Read<string>("admin_user"),
                    i_AdminPassword: r_DataStore.Read<string>("admin_password")
                );
                Identity adminIdent = new Identity { Domain = this.r_Client.Identity.Domain, UserName = "admin" };
                Identity localMonitorIdent = new Identity { Domain = this.r_Client.Identity.Domain, UserName = "local_monitor" };
                m_UserManager.AddRosterItem(adminIdent, localMonitorIdent, "Monitron");
                m_UserManager.AddRosterItem(localMonitorIdent, adminIdent, "admin");
                m_UserManager.AddRosterItem(r_Client.Identity, localMonitorIdent, "Workers");
                m_UserManager.AddRosterItem(localMonitorIdent, r_Client.Identity, "Management");
            }
        }

        [RemoteCommand(MethodName="list_plugins")]
        public string ListPlugins(Identity buddy)
        {
            string result = "";
            foreach (PluginManifest manifest in r_PluginsManager.ListPlugins())
            {
                result += string.Format("{0} [{1}:{2}]\n", manifest.Name, manifest.Id, manifest.Version);
            }

            if (result == string.Empty)
            {
                result = "No plugins available";
            }

            return result;
        }

        [RemoteCommand(MethodName="delete_plugin")]
        public string DeletePlugin(
            Identity buddy,
            [Opt("plugin_id", "p|plugin_id=", "{PLUGIN} to delete")] string i_PluginId)
        {
            r_PluginsManager.RemovePluginAsync(i_PluginId).ContinueWith((task) =>
                {
                        if (task.IsFaulted)
                        {
                            r_Client.SendMessage(buddy, "Error deleting plugin: " + task.Exception.Message);
                        }
                        else
                        {
                            r_Client.SendMessage(buddy, string.Format("Plugin '{0}' deleted successfully", i_PluginId));
                        }
                }
            );
            return "Trying to delete plugin";
        }

        [RemoteCommand(MethodName="echo")]
        public string Echo(
            Identity buddy,
            [Opt("text", "t|text=", "{TEXT} to echo back")] string i_Text)
        {
            return i_Text;
        }

		[RemoteCommand(
            MethodName="add_user",
            Description="Adds a new user"
        )]
		public string AddUser(
            Identity identity,
            [Opt("user", "u|user=", "{USER} to add")] string i_User)
        {
            Identity newIdentity = new Identity();
            newIdentity.UserName = i_User;
            newIdentity.Domain = r_Client.Identity.Domain;
            try
            {
                m_UserManager.AddUser(newIdentity, r_Client.Identity);
            }
            catch (AdminClientException e)
            {
                return e.Message;
            }

            return string.Format("Added user {0} successfully", i_User);
        }

        [RemoteCommand(
            MethodName="get_workers",
            Description="gets the list of all active worker hosts"
        )]
        public string GetWorkers(Identity identity)
        {
            return "\n" + string.Join("\n", r_AvailableWorkers);
        }

        [RemoteCommand(
            MethodName="list_instances",
            Description="lists all currently running instances"
        )]
        public string ListInstances(Identity identity)
        {
        }

        [RemoteCommand(
            MethodName="\ud83d\udca9\ud83d\udca9\ud83d\udca9",
            Description=
            @"That's not very nice!"
        )]
        public string Poop(Identity identity)
        {
            return "\ud83d\udca9\ud83d\udca9\ud83d\udca9";
        }

		[RemoteCommand(
            MethodName="delete_user",
            Description=
@"Deletes a user

This action allways succeeds."
        )]
		public string DeleteUser(
            Identity identity,
            [Opt("user", "u|user=", "{USER} to delete")] string i_User)
        {
            Identity identityToDelete = new Identity();
            identityToDelete.UserName = i_User;
            identityToDelete.Domain = r_Client.Identity.Domain;

            try
            {
                m_UserManager.DeleteUser(identityToDelete);
            }
            catch (AdminClientException e)
            {
                return e.Message;
            }

            return string.Format("Deleted user {0} successfully", i_User);
        }
    }
}

