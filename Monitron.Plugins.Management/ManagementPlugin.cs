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
using System.Collections.Generic;
using Monitron.Plugins.LocalMonitorPlugin.Common;
using System.Text;
using System.Security;
using System.Xml;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using System.Threading;

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

        private IInstanceAllocationStrategy m_IstanceAllocationStrategy;

        public IMessengerClient MessangerClient
        {
            get
            {
                return r_Client;
            }
        }

        private readonly RpcAdapter r_Adapter;

        private readonly Dictionary<string, IWorkerManager> r_AvailableWorkers = new Dictionary<string, IWorkerManager>();

        Timer m_reloadTimer;

        public ManagementPlugin(IMessengerClient i_MessangerClient, IPluginDataStore i_DataStore)
        {
            m_IstanceAllocationStrategy = new BalancedInstanceAllocationStrategy();
            i_MessangerClient.ConnectionStateChanged += r_Client_ConnectionStateChanged;
            i_MessangerClient.FileTransferRequest = r_Client_FileTransferRequest;
            i_MessangerClient.FileTransferProgress += i_MessangerClient_FileTransferProgress;
            i_MessangerClient.FileTransferAborted += i_MessangerClient_FileTransferAborted;
            i_MessangerClient.BuddySignedIn += (sender, e) => {
                if (e.Buddy.UserName == k_WorkerUserName)
                {
                    IMessengerRpc rpc = (r_Client as IMessengerRpc);
                    IWorkerManager wm = rpc.CreateRpcClient<IWorkerManager>(e.Buddy);
                    r_AvailableWorkers.Add(e.Buddy.Resource, wm);
                    reloadInstances();
                }
            };
            i_MessangerClient.BuddySignedOut += (sender, e) => {
                if (e.Buddy.UserName == k_WorkerUserName)
                {
                    r_AvailableWorkers.Remove(e.Buddy.Resource);
                    reloadInstances();
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
            m_reloadTimer = new Timer((obj) => reloadInstances(), null, 0, 60 * 1000);

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
                this.r_Client.SendMessage(i_FileTransfer.From, "Refusing upload, file too large");
                return null;
            }
                
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
                sr_Log.DebugFormat("Roster: {0}", string.Join(", ", r_Client.Buddies.Select((i) => i.Identity.ToString()).ToArray()));
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
            }
        }

        [RemoteCommand(
            MethodName="list_plugins",
            Description="List all available plugins"
        )]
        public string ListPlugins(Identity buddy)
        {
            StringBuilder result = new StringBuilder("\n");
            bool pluginsFound = false;
            foreach (PluginManifest manifest in r_PluginsManager.ListPlugins())
            {
                pluginsFound = true;
                result.AppendFormat(
                    "{0} [{1}:{2}]\n\t{3}\n",
                    manifest.Name,
                    manifest.Id,
                    manifest.Version,
                    manifest.Description);
            }

            if (!pluginsFound)
            {
                result.Append("No plugins available");
            }

            return result.ToString();
        }

        [RemoteCommand(
            MethodName="delete_plugin",
            Description="Make a plugin unavailable"
        )]
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
                m_UserManager.AddUser(newIdentity, "123456");
                m_UserManager.AddRosterItem(newIdentity, r_Client.Identity, "Monitron");
                m_UserManager.AddRosterItem(r_Client.Identity, newIdentity, "Users");
            }
            catch (AdminClientException e)
            {
                return e.Message;
            }

            return string.Format("Added user {0} successfully", i_User);
        }

        [RemoteCommand(
            MethodName="list_worker_hosts",
            Description="gets the list of all active worker hosts"
        )]
        public string GetWorkers(Identity identity)
        {
            return "\n" + string.Join("\n", r_AvailableWorkers.Keys);
        }

        private Dictionary<string, IWorkerManager> getInstanceMapping() {
            Dictionary<string, IWorkerManager> instanceLocation = new Dictionary<string, IWorkerManager>();
            foreach (var worker in r_AvailableWorkers.Values)
            {
                foreach (var instance in worker.ListInstances().Statuses)
                {

                    if (!instance.Status.ToLower().StartsWith("up"))
                    {
                        worker.RemoveInstance(instance.Name);
                    }
                    else
                    {
                        try
                        {
                            instanceLocation.Add(instance.Name, worker);
                        }
                        catch (Exception)
                        {
                            worker.RemoveInstance(instance.Name);
                        }
                    }
                }
            }

            return instanceLocation;
        }

        private bool m_isAllocating = false;

        private async Task reloadInstances() {
            Task<List<BsonDocument>> netbots = await m_MongoDB.GetCollection<BsonDocument>("netbots")
                .FindAsync(Builders<BsonDocument>.Filter.Empty)
                .ContinueWith((task) => task.Result.ToListAsync());
            lock (r_AvailableWorkers)
            {
                if (m_isAllocating)
                {
                    return;
                }

                m_isAllocating = true;
            }
            try
            {
                var instanceLocation = getInstanceMapping();

                foreach (var netbot in netbots.Result)
                {
                    string id = netbot.GetValue("_id").AsObjectId.ToString();
                    IWorkerManager worker;
                    if (!instanceLocation.TryGetValue(id, out worker))
                    {
                        await allocateInstance(netbot);
                    }

                    instanceLocation.Remove(id);
                }

                // Remove deleted instances
                foreach (var entry in instanceLocation)
                {
                    entry.Value.RemoveInstance(entry.Key);
                }
            }
            catch (Exception e)
            {
                sr_Log.Error("Error reallocating instances: ", e);
            }
            finally {
                lock (r_AvailableWorkers)
                {
                    m_isAllocating = false;
                }
            }
        }

        private async Task allocateInstance(BsonDocument i_NetBot)
        {
            string pluginId = await r_PluginsManager.GetPluginIdAsync(i_NetBot.GetValue("nodePlugin").AsObjectId);
            Task<BsonDocument> contact = await m_MongoDB.GetCollection<BsonDocument>("contacts")
                .FindAsync(Builders<BsonDocument>.Filter.Eq("_id", i_NetBot.GetValue("contact").AsObjectId))
                .ContinueWith((task) => task.Result.FirstAsync());
            if (contact.IsFaulted)
            {
                return;
            }

            IWorkerManager workerManager = m_IstanceAllocationStrategy.SelectWorkerForNewInstance(r_AvailableWorkers.Values);
            if (workerManager == null)
            {
                sr_Log.Warn("Could not allocate new instance, system at full capacity");
            }

            string[] jid = contact.Result.GetValue("jid").AsString.Split('@');
            Account botIdentity = new Account(jid[0], contact.Result.GetValue("password").AsString, jid[1]);

            CreateInstanceResult result;
            try{
                result = workerManager.CreateInstance(
                    i_NetBot.GetValue("_id").AsObjectId.ToString(),
                    pluginId,
                    generateConfiguration(botIdentity, r_PluginsManager.GetManifest(pluginId)));
            }
            catch (Exception e)
            {
                result = new CreateInstanceResult
                    {
                        Success = false,
                        Error = e.Message,
                    };
            }
        }

        [RemoteCommand(
            MethodName="reload_netbots",
            Description="reload netbots from DB"
        )]
        public string ReloadNetBot(Identity Identity) {
            try
            {
                reloadInstances().Wait();
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }

        [RemoteCommand(
            MethodName="create_instance",
            Description="create a new instance of a plugin"
        )]
        public string CreateInstance(
            Identity identity,
            [Opt("name", "n|name=", "{NAME} of the instance")] string i_Name,
            [Opt("plugin_id", "p|plugin-id=", "{PLUGIN_ID} of the instance")] string i_PluginId)
        {
            IWorkerManager workerManager = m_IstanceAllocationStrategy.SelectWorkerForNewInstance(r_AvailableWorkers.Values);
            if (workerManager == null)
            {
                return string.Format("Could not allocate new instance, system at full capacity");
            }

            Identity botIdentity = getBotIdentity(identity, i_Name);
            Account buddy = createBotBuddy(botIdentity);
            CreateInstanceResult result;
            try{
                result = workerManager.CreateInstance(
                    botIdentity.UserName,
                    i_PluginId,
                    generateConfiguration(buddy, r_PluginsManager.GetManifest(i_PluginId)));
            }
            catch (Exception e)
            {
                result = new CreateInstanceResult
                {
                    Success = false,
                    Error = e.Message,
                };
            }

            if (!result.Success)
            {
                m_UserManager.DeleteUser(buddy.Identity);
                return result.Error;
            }

            m_UserManager.AddRosterItem(buddy.Identity, identity, "Admins");
            m_UserManager.AddRosterItem(identity, buddy.Identity, "Bots");

            return "Instance created successfully";
        }

        string generateConfiguration(Account i_Buddy, PluginManifest i_Manifest)
        {
            return string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<NodeConfiguration>
    <Account>
        <Username>{0}</Username>
        <Domain>{1}</Domain>
        <Password>{2}</Password>
    </Account>
    <MessangerClient>
        <DllPath>.</DllPath>
        <DllName>Monitron.Clients.XMPP.dll</DllName>
        <Type>Monitron.Clients.XMPP.XMPPMessengerClient</Type>
    </MessangerClient>
    <Plugin>
        <DllPath>{3}</DllPath>
        <DllName>{4}</DllName>
        <Type>{5}</Type>
    </Plugin>
    <DataStore>
        <DllPath>.</DllPath>
        <DllName>Monitron.PluginDataStore.Cloud.dll</DllName>
        <Type>Monitron.PluginDataStore.Cloud.CloudPluginDataStore</Type>
        <Location>mongodb://monitron_mgmt:HellIsForHeroes%40!@boss.monitron.test/monitron</Location>
    </DataStore>
 </NodeConfiguration>
",
            SecurityElement.Escape(i_Buddy.Identity.UserName),
            SecurityElement.Escape(i_Buddy.Identity.Domain),
            SecurityElement.Escape(i_Buddy.Password),
            SecurityElement.Escape(i_Manifest.DllPath),
            SecurityElement.Escape(i_Manifest.DllName),
            SecurityElement.Escape(i_Manifest.Type)
        );
        }

        private static string generatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        private Account createBotBuddy(Identity i_BotIdentity)
        {
            Account buddy = new Account(
                i_UserName: i_BotIdentity.UserName,
                i_Host: i_BotIdentity.Domain,
                i_Password: generatePassword(16));
            m_UserManager.AddUser(buddy.Identity, buddy.Password);
            return buddy;
        }

        private Identity getBotIdentity(Identity i_Owner, string i_Name)
        {
            return new Identity
            {
                Domain = r_Client.Identity.Domain,
                UserName = string.Format("{0}-{1}", i_Owner.UserName, i_Name),
            };
        }

        private IWorkerManager FindInstance(Identity i_BotIdentity)
        {
            string name = i_BotIdentity.UserName;
            foreach (var worker in r_AvailableWorkers.Values)
            {
                if (worker.ListInstances().Statuses.Any((status) => status.Name == name))
                    return worker;
            }

            return null;
        }

        [RemoteCommand(
            MethodName="pause_instance",
            Description="pause an active instance of a plugin"
        )]
        public string PauseInstance(
            Identity identity,
            [Opt("name", "n|name=", "{NAME} of the instance")] string i_Name)
        {
            Identity botIdentity = getBotIdentity(identity, i_Name);

            IWorkerManager workerManager = FindInstance(botIdentity);
            if (workerManager == null)
            {
                return string.Format("Instance not found");
            }
            
            var result = workerManager.PauseInstance(botIdentity.UserName);
            if (!result.Success)
            {
                return result.Error;
            }

            return "Instance paused successfully";
        }

        [RemoteCommand(
            MethodName="unpause_instance",
            Description="unpause an active instance of a plugin"
        )]
        public string UnpauseInstance(
            Identity identity,
            [Opt("name", "n|name=", "{NAME} of the instance")] string i_Name)
        {
            Identity botIdentity = getBotIdentity(identity, i_Name);

            IWorkerManager workerManager = FindInstance(botIdentity);
            if (workerManager == null)
            {
                return string.Format("Instance not found");
            }
            
            var result = workerManager.UnpauseInstance(botIdentity.UserName);
            if (!result.Success)
            {
                return result.Error;
            }

            return "Instance paused successfully";
        }

        [RemoteCommand(
            MethodName="get_log",
            Description="get log on an instance of a plugin"
        )]
        public string GetLog(
            Identity identity,
            [Opt("name", "n|name=", "{NAME} of the instance")] string i_Name)
        {
            Identity botIdentity = getBotIdentity(identity, i_Name);

            IWorkerManager workerManager = FindInstance(botIdentity);
            if (workerManager == null)
            {
                return string.Format("Instance not found");
            }

            var result = workerManager.GetLog(botIdentity.UserName);
            if (!result.Success)
            {
                return result.Error;
            }

            return "log:\n" + result.Log;
        }


        [RemoteCommand(
            MethodName="remove_instance",
            Description="remove an active instance of a plugin"
        )]
        public string RemoveInstance(
            Identity identity,
            [Opt("name", "n|name=", "{NAME} of the instance")] string i_Name)
        {
            Identity botIdentity = getBotIdentity(identity, i_Name);

            IWorkerManager workerManager = FindInstance(botIdentity);
            if (workerManager == null)
            {
                return string.Format("Instance not found");
            }

            var result = workerManager.RemoveInstance(botIdentity.UserName);
            if (!result.Success)
            {
                return result.Error;
            }

            m_UserManager.RemoveRosterItem(identity, botIdentity);
            m_UserManager.DeleteUser(botIdentity);

            return "Instance removed successfully";
        }

        [RemoteCommand(
            MethodName="list_instances",
            Description="lists all currently running instances"
        )]
        public string ListInstances(Identity identity)
        {
            StringBuilder result = new StringBuilder("\n");
            bool instancesFound = false;
            foreach (var worker in r_AvailableWorkers)
            {
                foreach (var instance in worker.Value.ListInstances().Statuses)
                {
                    instancesFound = true;
                    result.Append(worker.Key.PadRight(10));
                    result.Append(instance.Name);
                    result.Append("   ");
                    result.AppendLine(instance.Status);
                }
            }
            if (!instancesFound)
            {
                result.Append("No instances are running");
            }
            return result.ToString();
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


