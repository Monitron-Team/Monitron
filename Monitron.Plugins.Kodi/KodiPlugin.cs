using Monitron.Common;
using Monitron.ImRpc;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Reflection;
using System.Linq;

namespace Monitron.Plugins.Kodi
{
    public class KodiPlugin : INodePlugin, IMovieBot
    {
		private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
			(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IMessengerClient r_Client;
        private readonly RpcAdapter r_Adapter;
        private IPluginDataStore m_DataStore;
        private const string k_ErrorMessage = "Sorry, something is wrong..";
        private const string k_SuccessMessage = "Done!";
        private const string k_MaxVolMessage = "The volume is on maximum level";
        private const string k_MinVolMessage = "The volume is on minimum level";
        private const string k_VolErrorMessage = "Volume Error";
        private const int k_VolumeChange = 10;
        private const int k_MaxVolume = 100;
        private const int k_MinVolume = 0;
        private const int k_VolError = -1;

        // Kodi parameters
        private readonly string r_Url;
        private int m_Volume;
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
			sr_Log.Info("Starting Kodi Pluggin");
			r_Client = i_MessangerClient;
            m_DataStore = i_DataStore;

            string ip = m_DataStore.Read<string>("ip");
            string port = m_DataStore.Read<string>("port");
            r_Url = "http://" + ip + ":" + port + "/jsonrpc";

            r_Adapter = new RpcAdapter(this, r_Client);
            r_Client.ConnectionStateChanged += r_Client_ConnectionStateChanged;
            r_Client_ConnectionStateChanged(r_Client, new ConnectionStateChangedEventArgs(r_Client.IsConnected));

            registerToJabber(i_MessangerClient);

            initKodiParams();
        }

        private void registerToJabber(IMessengerClient r_Client)
        {
            IMessengerRpc rpc = (r_Client as IMessengerRpc);
            if (rpc != null)
            {
                rpc.RegisterRpcServer<IMovieBot, KodiPlugin>(this);
            }
        }

        private void r_Client_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            if (e.IsConnected)
            {
                r_Client.SetNickname("Kodi Bot");
                r_Client.SetAvatar(Assembly.GetExecutingAssembly().GetManifestResourceStream("Monitron.Plugins.Kodi.Kodi.png"));
                var admins = r_Client.Buddies.Where(item => item.Groups.Contains("admin"));
                foreach (var buddy in admins)
                {
                    string welcomeMessage = "Hi, I am a Kodi bot :)";
                    r_Client.SendMessage(buddy.Identity, welcomeMessage);
                }
            }
        }

        private void initKodiParams()
        {
			sr_Log.Info("Starting init params");
			m_Movies = getVideoList();
            getVolume();
        }

        private void getVolume()
        {
            string getVolume = "{\"jsonrpc\": \"2.0\", \"method\": \"Application.GetProperties\", \"params\": { \"properties\": [\"volume\"] }, \"id\": 1}";
            string volResponse = sendRequest(getVolume);
            JObject volJson;
            if (!string.IsNullOrEmpty(volResponse))
            {
                try
                {
                    volJson = JObject.Parse(volResponse);
                    m_Volume = (int)volJson["result"]["volume"];
                }
                catch
                {
                    m_Volume = k_VolError;
                }
            }
            else
            {
                m_Volume = k_VolError;
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

        private MovieDetails[] getVideoList()
        {
			sr_Log.Info("Getting Video list");
			string request = "{\"jsonrpc\": \"2.0\", \"method\": \"VideoLibrary.GetMovies\", \"params\": { \"limits\": { \"start\" : 0, \"end\": 75 }, \"properties\" : [\"file\"] }, \"id\": 1}";
            var response = sendRequest(request);
            m_VideoListMsg = "Video List:\n";
			sr_Log.Info("reponse: " + response);
            JObject responseJson;
			sr_Log.Info("Try to parse Json response");
            MovieDetails[] movies = new MovieDetails[0];

            try
            {
                responseJson = JObject.Parse(response);
                movies = responseJson["result"]["movies"].ToObject<MovieDetails[]>();

                foreach (MovieDetails movie in movies)
                {
                    movie.file = movie.file.Replace(@"\", @"/");
                    m_VideoListMsg += (movie.movieid.ToString() + " - " + movie.label + "\n");
                }
            }
            catch
            {

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
            else if(m_Volume == k_VolError)
            {
                msg = k_VolErrorMessage;
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
            else if (m_Volume == k_VolError)
            {
                msg = k_VolErrorMessage;
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
			IMessengerRpc rpc = (r_Client as IMessengerRpc);
			foreach(var buddy in r_Client.Buddies)
			{
				string[] resources = buddy.Resources;
				if(resources.Length > 0)
				{
					var buddyIden = buddy.Identity;
					buddyIden.Resource = resources[0];
					string[] implementedInterfaces = rpc.GetRegisterServersList(buddyIden);

					if(implementedInterfaces.Contains("IAudioBot"))
					{
						IMessengerRpc AudioRpc = (r_Client as IMessengerRpc);
						IAudioBot audioBot = AudioRpc.CreateRpcClient<IAudioBot>(buddyIden);
						audioBot.PauseAudio();
					}
				}
			}

			string msg = string.Empty;
            string videoPath = getMoviePath(i_Video);
            if (string.IsNullOrEmpty(videoPath) == false)
            {
                string request = "{\"jsonrpc\": \"2.0\", \"method\": \"Player.Open\", \"params\": { \"item\":{\"file\":\"" + videoPath + "\"}}, \"id\": 1}";
                msg = sendUserRequest(request);
            }

            return msg;
        }

        [RemoteCommand(MethodName = "list")]
        public string KodiList(Identity i_Buddy)
        {
            return m_VideoListMsg;
        }

        [RemoteCommand(MethodName = "stop")]
        public string KodiStop(Identity i_Buddy)
        {
            string request = "{\"jsonrpc\": \"2.0\", \"method\": \"Player.Stop\", \"params\": { \"playerid\": 1 }, \"id\": 1}";
            return sendUserRequest(request);
        }

        public string PauseMovie()
        {
			return KodiStop(new Identity());
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
