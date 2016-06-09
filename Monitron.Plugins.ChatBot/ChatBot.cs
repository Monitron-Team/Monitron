using System;

using Monitron.Common;
using Monitron.AI;
using System.Reflection;

namespace Monitron.Plugins.ChatBot
{
    public class ChatBot : INodePlugin
    {
        private readonly AI.AI r_Ai;
        private readonly IMessengerClient r_Client;

        public IMessengerClient MessangerClient
        {
            get
            {
                return r_Client;
            }
        }
        
        public ChatBot(IMessengerClient i_Client, IPluginDataStore i_DataStore)
        {
            r_Client = i_Client;
            r_Client.ConnectionStateChanged += r_Client_ConnectionStateChanged;
            r_Ai = new AI.AI(this, i_Client, true);
            r_Client_ConnectionStateChanged(r_Client, new ConnectionStateChangedEventArgs(r_Client.IsConnected));
        }

        void r_Client_ConnectionStateChanged (object sender, ConnectionStateChangedEventArgs e)
        {
            if (r_Client.IsConnected)
            {
                r_Ai.GlobalSettings.updateSetting("name", "Chat Bot");
                r_Ai.GlobalSettings.updateSetting("location", "Cloud");
                r_Client.SetNickname("Chat Bot");
                r_Client.SetAvatar(Assembly.GetExecutingAssembly().GetManifestResourceStream("Monitron.Plugins.ChatBot.Avatar.jpg"));
            }
        }

    }
}

