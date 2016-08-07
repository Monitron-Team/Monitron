using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using Monitron.Common;
using Monitron.ImRpc;

namespace Monitron.Plugins.Kodi
{
    public class KodiPlugin : INodePlugin
    {
        private readonly IMessengerClient r_Client;
        private readonly RpcAdapter r_Adapter;
        private IPluginDataStore m_DataStore;

        private int volume;
        private 

        public IMessengerClient MessangerClient
        {
            get 
            {
                return r_Client;
            }
        }

        public KodiPlugin(IMessengerClient i_MessangerClient, IPluginDataStore i_DataStore)
        {
            r_Client = i_MessangerClient;
            m_DataStore = i_DataStore;
            r_Adapter = new RpcAdapter(this, r_Client);
            r_Client.ConnectionStateChanged += r_Client_ConnectionStateChanged;
            //try
            //{
            //    connectMPDServer();
            //}
            //catch
            //{
            //}
            r_Client_ConnectionStateChanged(r_Client, new ConnectionStateChangedEventArgs(r_Client.IsConnected));
        }

        private void r_Client_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            if (e.IsConnected)
            {
                r_Client.SetNickname("Kodi Bot");
                //r_Client.SetAvatar(Assembly.GetExecutingAssembly().GetManifestResourceStream("Monitron.Plugins.MPD.Boombox.png"));
                foreach (var buddy in r_Client.Buddies)
                {
                    string welcomeMessage = "\nHi, I am a Kodi bot";
                    r_Client.SendMessage(buddy.Identity, welcomeMessage);
                }
            }
        }

        [RemoteCommand(MethodName = "play_pause")]
        public string KodiPlayPause(Identity i_Buddy)
        {
            //return m_Mpc.Stats().ToString();
        }

        [RemoteCommand(MethodName = "volume_up")]
        public string KodiVolumeUp(Identity i_Buddy)
        {
            //return m_Mpc.Stats().ToString();
        }

        [RemoteCommand(MethodName = "volume_down")]
        public string KodiVolumeDown(Identity i_Buddy)
        {

        }

        //[RemoteCommand(MethodName = "volume_up")]
        //public string KodiVolumeUp(Identity i_Buddy)
        //{

        //}

        /*
        class Program
        {
            static void Main(string[] args)
            {
                string playPause = "{\"jsonrpc\": \"2.0\", \"method\": \"Player.PlayPause\", \"params\": { \"playerid\": 1 }, \"id\": 1}";
                string volume50 = "{\"jsonrpc\": \"2.0\", \"method\": \"Application.SetVolume\", \"params\": { \"volume\": 50 }, \"id\": 1}";
                string volume100 = "{\"jsonrpc\": \"2.0\", \"method\": \"Application.SetVolume\", \"params\": { \"volume\": 100 }, \"id\": 1}";
                string speedUp = "{\"jsonrpc\": \"2.0\", \"method\": \"Player.SetSpeed\", \"params\": { \"playerid\": 1, \"speed\": 16 }, \"id\": 1}";
                string getVolume = "{\"jsonrpc\": \"2.0\", \"method\": \"Player.GetProperties\", \"params\": { \"playerid\": 1, \"properties\": [\"volume\"] }, \"id\": 1}";
                //string speedDown = "{\"jsonrpc\": \"2.0\", \"method\": \"Player.SetSpeed\", \"params\": { \"playerid\": 1, \"speed\": -16 }, \"id\": 1}";
                string speedNormal = "{\"jsonrpc\": \"2.0\", \"method\": \"Player.SetSpeed\", \"params\": { \"playerid\": 1, \"speed\": 0 }, \"id\": 1}";
                using (var webClient = new WebClient())
                {
                    var response = webClient.UploadString("http://192.168.1.19:8080/jsonrpc", "POST", getVolume);
                    webClient.UploadString("http://192.168.1.19:8080/jsonrpc", "POST", playPause);
                    System.Threading.Thread.Sleep(1500);
                    webClient.UploadString("http://192.168.1.19:8080/jsonrpc", "POST", playPause);
                    webClient.UploadString("http://192.168.1.19:8080/jsonrpc", "POST", volume50);
                    System.Threading.Thread.Sleep(1000);
                    webClient.UploadString("http://192.168.1.19:8080/jsonrpc", "POST", speedUp);
                    System.Threading.Thread.Sleep(2000);
                    webClient.UploadString("http://192.168.1.19:8080/jsonrpc", "POST", volume100);
                    webClient.UploadString("http://192.168.1.19:8080/jsonrpc", "POST", speedNormal);
                    System.Threading.Thread.Sleep(5000);

                    Console.WriteLine(response.ToString());
                }
            }
        }*/
    }
}
