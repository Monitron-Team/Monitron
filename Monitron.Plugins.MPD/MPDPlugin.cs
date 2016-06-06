using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;

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

			connectMPDServer();
			new Thread(PopulatePlayList).Start();
		}

		private void connectMPDServer()
		{
			IPAddress[] address = Dns.GetHostAddresses("10.0.0.8");
			IPEndPoint ie = new IPEndPoint(address[0], 6600);
			m_Mpc = new Mpc();
			m_Mpc.Connection = new MpcConnection(ie);
		}

		private void r_Client_ConnectionStateChanged (object sender, ConnectionStateChangedEventArgs e)
		{
			if(e.IsConnected) 
			{
				foreach(var buddy in r_Client.Buddies) 
				{
					string welcomeMessange = "\nHi, I am a MPD bot";
					r_Client.SendMessage(buddy.Identity, welcomeMessange);
				}
			}
		}

		[RemoteCommand(MethodName="get_details")]
		public string GetMPDServerDetails(Identity i_Buddy)
		{
			return m_Mpc.Stats().ToString();
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
			m_Mpc.Stop();
			return "stop playing " + m_Mpc.CurrentSong().Title;
		}

		[RemoteCommand(MethodName="play")]
		public string PlaySong(Identity i_Buddy)
		{
			m_Mpc.Play();
			return "start playing " + m_Mpc.CurrentSong().Title;
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


