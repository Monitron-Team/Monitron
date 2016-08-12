using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using Monitron.Common;
using Monitron.ImRpc;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Web.Script.Serialization;

namespace Monitron.Plugins.Kodi
{
    public class KodiPlugin : INodePlugin
    {
        private readonly IMessengerClient r_Client;
        private readonly RpcAdapter r_Adapter;
        private IPluginDataStore m_DataStore;
        private const string k_ErrorMessage = "Sorry, something is wrong..";
        private const string k_SuccessMessage = "Done!";
        private const string k_MaxVolMessage = "The volume is on maximum level";
        private const string k_MinVolMessage = "The volume is on minimum level";
        private const int k_VolumeChange = 10;
        private const int k_MaxVolume = 100;
        private const int k_MinVolume = 0;

        // Kodi parameters
        private readonly string r_Url;
        private int m_Volume;
        private bool m_IsPlaying;
        private MovieDetails[] m_Movies;
        private string m_VideoListMsg;

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
            m_Movies = getVideoList();
            getVolume();
            getPlayingStatus();
        }

        private void getVolume()
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

        private void getPlayingStatus()
        {
            // TBD
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

        private MovieDetails[] getVideoList()
        {
            string request = "{\"jsonrpc\": \"2.0\", \"method\": \"VideoLibrary.GetMovies\", \"params\": { \"limits\": { \"start\" : 0, \"end\": 75 }, \"properties\" : [\"file\"] }, \"id\": 1}";
            var response = sendRequest(request);
            m_VideoListMsg = "Video List:\n";

            JObject responseJson;
            responseJson = JObject.Parse(response);
            MovieDetails[] movies = responseJson["result"]["movies"].ToObject<MovieDetails[]>();

            foreach (MovieDetails movie in movies)
            {
                movie.file = movie.file.Replace(@"\", @"/");
                m_VideoListMsg += (movie.movieid.ToString() + " - " + movie.label + "\n");
            }

            return movies;
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

        private string getMoviePath(int id)
        {
            string path = string.Empty;
            foreach (MovieDetails movie in m_Movies)
            {
                if(movie.movieid == id)
                {
                    path = movie.file;
                    break;
                }
            }

            return path;
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
            string msg = string.Empty;

            if (m_Volume == k_MaxVolume)
            {
                msg = k_MaxVolMessage;
            }
            else
            {
                string vol = (m_Volume + k_VolumeChange).ToString();
                string request = "{\"jsonrpc\": \"2.0\", \"method\": \"Application.SetVolume\", \"params\": { \"volume\": " + vol + " }, \"id\": 1}";
                msg = sendUserRequest(request);
                if (msg.Equals(k_SuccessMessage))
                {
                    Int32.TryParse(vol, out m_Volume);
                }
            }

            return msg;
        }

        [RemoteCommand(MethodName = "vol_down")]
        public string KodiVolumeDown(Identity i_Buddy)
        {
            string msg = string.Empty;

            if (m_Volume == k_MinVolume)
            {
                msg = k_MinVolMessage;
            }
            else
            {
                string vol = (m_Volume - k_VolumeChange).ToString();
                string request = "{\"jsonrpc\": \"2.0\", \"method\": \"Application.SetVolume\", \"params\": { \"volume\": " + vol + " }, \"id\": 1}";
                msg = sendUserRequest(request);
                if (msg.Equals(k_SuccessMessage))
                {
                    Int32.TryParse(vol, out m_Volume);
                }
            }

            return msg;
        }

        [RemoteCommand(MethodName = "mute")]
        public string KodiMute(Identity i_Buddy)
        {
            string vol = "0";
            string request = "{\"jsonrpc\": \"2.0\", \"method\": \"Application.SetVolume\", \"params\": { \"volume\": " + vol + " }, \"id\": 1}";
            string msg = sendUserRequest(request);
            if (msg.Equals(k_SuccessMessage))
            {
                Int32.TryParse(vol, out m_Volume);
            }

            return msg;
        }

        [RemoteCommand(MethodName = "play")]
        public string KodiPlay(Identity i_Buddy, int i_Video)
        {
            string videoPath = getMoviePath(i_Video);
            string request = "{\"jsonrpc\": \"2.0\", \"method\": \"Player.Open\", \"params\": { \"item\":{\"file\":\"" + videoPath + "\"}}, \"id\": 1}";
            return sendUserRequest(request);
        }

        [RemoteCommand(MethodName = "list")]
        public string KodiList(Identity i_Buddy)
        {
            return m_VideoListMsg;
        }

        private class MovieDetails
        {
            public string file;
            public string label;
            public int movieid;

            public MovieDetails(string file, string label, int movieid)
            {
                this.file = file;
                this.label = label;
                this.movieid = movieid;
            }
        }
    }
}
