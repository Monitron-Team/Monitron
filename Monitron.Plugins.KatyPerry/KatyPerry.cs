using System;
using System.Linq;
using System.Reflection;

using Monitron.Common;
using Monitron.ImRpc;

namespace Monitron.Plugins.KatyPerry
{
    public class KatyPerry : INodePlugin
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

        public KatyPerry(IMessengerClient i_MessangerClient)
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
                sr_Log.Debug("Setting up avatar");
                r_Client.SetAvatar(Assembly.GetExecutingAssembly().GetManifestResourceStream("Monitron.Plugins.KatyPerry.KatyPerryAvatar.png"));
                sr_Log.Debug("Notifying buddies how I feel about them");
                foreach (var buddy in r_Client.Buddies)
                {
                    r_Client.SendMessage(buddy.Identity, "You make me feel like I'm living a teenage dream");
                }
            }
        }

        [RemoteCommand(MethodName="sing")]
        public string Sing()
        {
            return @"You think I'm pretty without any make-up on
You think I'm funny when I tell the punch line wrong
I know you get me, so I let my walls come down, down";
        }
    }
}

