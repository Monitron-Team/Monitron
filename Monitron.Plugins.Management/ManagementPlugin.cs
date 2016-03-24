using System;
using System.Linq;
using System.Reflection;

using MongoDB.Driver;
using MongoDB.Driver.GridFS;

using Monitron.Common;
using Monitron.ImRpc;

namespace Monitron.Plugins.Management
{
    public class ManagementPlugin : INodePlugin
    {
        private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly IMessengerClient r_Client;
        
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
            sr_Log.Debug("Setting up RPC");
            r_Adapter = new RpcAdapter(this, r_Client);
            r_Client_ConnectionStateChanged(this, null);
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

        [RemoteCommand(MethodName="echo")]
        public string Echo(string i_Text)
        {
            return i_Text;
        }
    }
}

