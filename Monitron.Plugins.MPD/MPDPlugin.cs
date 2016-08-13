using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using Monitron.Common;
using Monitron.ImRpc;
using Libmpc;


namespace Monitron.Plugins.MPD
{
	public class MPDPlugin: INodePlugin
	{
		private readonly IMessengerClient r_Client;

		private readonly RpcAdapter r_Adapter;

		private IPluginDataStore m_DataStore;

		public IMessengerClient MessangerClient
		{
			get
			{
				return r_Client;
			}
		}

		private Mpc m_Mpc;

		public MPDPlugin(IMessengerClient i_MessangerClient, IPluginDataStore i_DataStore)
		{
			r_Client = i_MessangerClient;
			m_DataStore = i_DataStore;
			r_Adapter = new RpcAdapter(this, r_Client);
			r_Client.ConnectionStateChanged += r_Client_ConnectionStateChanged;
            try
            {
                connectMPDServer();
            }
            catch
            {
            }
			registerToJabber(r_Client);
            r_Client_ConnectionStateChanged(r_Client, new ConnectionStateChangedEventArgs(r_Client.IsConnected));
		}

		private void registerToJabber(IMessengerClient r_Client)
		{
			IMessengerRpc rpc = (r_Client as IMessengerRpc);
			if(rpc != null)
			{
				rpc.RegisterRpcServer <INodePlugin, MPDPlugin>(this);
			}
		}

		private void connectMPDServer()
		{
			string IP = m_DataStore.Read<string>("host");
			int port = m_DataStore.Read<int>("port");

			IPAddress[] addresses = Dns.GetHostAddresses(IP);
            if (addresses.Length == 0)
            {
                throw new Exception("Could not find " + IP);
            }
			IPEndPoint ie = new IPEndPoint(
                addresses.Where((address) => address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First(),
                port);
            m_Mpc = new Mpc();
            m_Mpc.Connection = new MpcConnection(ie);
            new Thread(PopulatePlayList).Start();

		}

		private void r_Client_ConnectionStateChanged (object sender, ConnectionStateChangedEventArgs e)
		{
			if(e.IsConnected) 
            {
                r_Client.SetNickname("BoomBox Bot");
                r_Client.SetAvatar(Assembly.GetExecutingAssembly().GetManifestResourceStream("Monitron.Plugins.MPD.Boombox.png"));
                foreach(var buddy in r_Client.Buddies) 
                {
                    string welcomeMessage = "\nHi, I am a MPD bot";
                    r_Client.SendMessage(buddy.Identity, welcomeMessage);
				}
			}
		}

		[RemoteCommand(MethodName="get_details")]
		public string GetMPDServerDetails(Identity i_Buddy)
		{
			return m_Mpc.Stats().ToString();
		}

        [RemoteCommand(MethodName="set_host")]
        public string setHost(Identity i_Buddy, string i_Host, int i_Port)
        {
            m_DataStore.Write("host", i_Host);
            m_DataStore.Write("port", i_Port);
            return "Host information updated";
        }

        [RemoteCommand(MethodName="connect")]
        public string connect(Identity i_Buddy)
        {
            string IP = m_DataStore.Read<string>("host");
            int port = m_DataStore.Read<int>("port");
            r_Client.SendMessage(i_Buddy, 
                string.Format("Connecting to {0}:{1}", IP, port));
            connectMPDServer();
            return "Connected to Boombox, Start playing!";
        }

		[RemoteCommand(MethodName="songs_list")]
		public string GetSongsList(Identity i_Buddy)
		{
			string songString = null;
			List<string> songs = m_Mpc.List(ScopeSpecifier.Title);

			int index = 1;
			foreach(string song in songs)
			{
				songString += index.ToString() + ". " + song + "\n";
				index++;
			}

			return songString;
		}

		[RemoteCommand(MethodName="stop")]
		public string StopSong(Identity i_Buddy)
		{
			if (m_Mpc != null)
			{
				m_Mpc.Stop();
				return "stop playing " + m_Mpc.CurrentSong().Title;
			} 
			else
			{
				return "Cannot stop playing current song";
			}
		}

		[RemoteCommand(MethodName="play")]
		public string PlaySong(Identity i_Buddy)
		{
			if(m_Mpc != null)
			{
				m_Mpc.Play();
				return "start playing " + m_Mpc.CurrentSong().Title;
			} 
			else
			{
				return "Cannot start playing";
			}


		}

		[RemoteCommand(MethodName="test")]
		public string test(Identity i_Buddy)
		{
			Identity newIdentity = new Identity();
			newIdentity.Domain = i_Buddy.Domain;
			newIdentity.Resource = "Test";
			newIdentity.UserName = i_Buddy.UserName;
			IMessengerRpc rpc = (r_Client as IMessengerRpc);
			string[] rpcServers = rpc.GetRegisterServersList(newIdentity);

			string welcomeMessage = "my friends are: ";
			foreach(var buddy in r_Client.Buddies) 
			{
				//string[] rpcServers = rpc.GetRegisterServersList(buddy.Identity);
			}
			return welcomeMessage;
		}

		[RemoteCommand(MethodName="next")]
		public string NextSong(Identity i_Buddy)
		{
			if (m_Mpc != null)
			{
				m_Mpc.Next();
				return "start playing " + m_Mpc.CurrentSong().Title;
			} 
			else
			{
				return "Cannot move to next song"; 
			}

		}
			
		private void PopulatePlayList()
		{
			List<string> songs = m_Mpc.List(ScopeSpecifier.Filename);

			foreach(string song in songs)
			{
				try
				{
					m_Mpc.Add(song);
				}
				catch(MpdResponseException)
				{
				}
			}
		}
	}
}


