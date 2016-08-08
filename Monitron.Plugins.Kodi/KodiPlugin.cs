using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using Monitron.Common;
using Monitron.ImRpc;
using Newtonsoft.Json.Linq;

namespace Monitron.Plugins.Kodi
{
    public class KodiPlugin : INodePlugin
    {
        private readonly IMessengerClient r_Client;
        private readonly RpcAdapter r_Adapter;
        private IPluginDataStore m_DataStore;
        private const string k_ErrorMessage = "Sorry, something is wrong..";
        private const string k_SuccessMessage = "Done!";
        private const int k_VolumeChange = 10;
        // Kodi parameters
        private readonly string r_Url;
        private int m_Volume;

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

            string ip = m_DataStore.Read<string>("ip");
            string port = m_DataStore.Read<string>("port");
            r_Url = "http://" + ip + ":" + port + "/jsonrpc";

            r_Adapter = new RpcAdapter(this, r_Client);
            r_Client.ConnectionStateChanged += r_Client_ConnectionStateChanged;
            r_Client_ConnectionStateChanged(r_Client, new ConnectionStateChangedEventArgs(r_Client.IsConnected));

            initKodiParams();
        }

        private void r_Client_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            // Is Kodi up?
            // Test Connection

            if (e.IsConnected)
            {
                r_Client.SetNickname("Kodi Bot");
                r_Client.SetAvatar(Assembly.GetExecutingAssembly().GetManifestResourceStream("Monitron.Plugins.Kodi.Kodi.png"));
                foreach (var buddy in r_Client.Buddies)
                {
                    string welcomeMessage = "\nHi, I am a Kodi bot";
                    r_Client.SendMessage(buddy.Identity, welcomeMessage);
                }
            }
        }

        private void initKodiParams()
        {
            string getVolume = "{\"jsonrpc\": \"2.0\", \"method\": \"Application.GetProperties\", \"params\": { \"properties\": [\"volume\"] }, \"id\": 1}";
            string volResponse = sendRequest(getVolume);
            JObject volJson;
            if (!string.IsNullOrEmpty(volResponse))
            {
                volJson = JObject.Parse(volResponse);
                m_Volume = (int)volJson["result"]["volume"];
            }
            else
            {
                // could not get current volume
            }
        }

        private string sendUserRequest(string req)
        {
            string msg = k_SuccessMessage;
            string response = sendRequest(req);

            if (string.IsNullOrEmpty(response) || response.Contains("error"))
            {
                msg = k_ErrorMessage;
            }

            return msg;
        }

        private string sendRequest(string req)
        {
            string response = string.Empty;

            try
            {
                using (var webClient = new WebClient())
                {
                    response = webClient.UploadString(r_Url, "POST", req);
                }
            }
            catch
            {

            }

            return response;
        }

        [RemoteCommand(MethodName = "play_pause")]
        public string KodiPlayPause(Identity i_Buddy)
        {
            string request = "{\"jsonrpc\": \"2.0\", \"method\": \"Player.PlayPause\", \"params\": { \"playerid\": 1 }, \"id\": 1}";
            return sendUserRequest(request);
        }

        [RemoteCommand(MethodName = "vol_up")]
        public string KodiVolumeUp(Identity i_Buddy)
        {
            string vol = (m_Volume + k_VolumeChange).ToString();
            string request = "{\"jsonrpc\": \"2.0\", \"method\": \"Application.SetVolume\", \"params\": { \"volume\": " + vol + " }, \"id\": 1}";
            string result = sendUserRequest(request);
            if (request.Equals(k_SuccessMessage))
            {
                Int32.TryParse(vol, out m_Volume);
            }
            return result;
        }

        [RemoteCommand(MethodName = "vol_down")]
        public string KodiVolumeDown(Identity i_Buddy)
        {
            string vol = (m_Volume - k_VolumeChange).ToString();
            string request = "{\"jsonrpc\": \"2.0\", \"method\": \"Application.SetVolume\", \"params\": { \"volume\": " + vol + " }, \"id\": 1}";
            string result = sendUserRequest(request);
            if (request.Equals(k_SuccessMessage))
            {
                Int32.TryParse(vol, out m_Volume);
            }
            return result;
        }

        [RemoteCommand(MethodName = "play")]
        public string KodiPlay(Identity i_Buddy)
        {
            //string request = "{\"jsonrpc\": \"2.0\", \"method\": \"Player.Open\", \"params\": { \"item\":{\"file\":\"C:/Users/Public/Videos/Sample Videos/Wildlife.wmv\"}}, \"id\": 1}";
            string request = "{\"jsonrpc\": \"2.0\", \"method\": \"Player.Open\", \"params\": { \"item\":{\"file\":\"C:/Users/tzafi/Downloads/Alt-J  - Taro.mp4\"}}, \"id\": 1}";
            return sendUserRequest(request);
        }
    }
}
