﻿using System;
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
	public class MPDPlugin: INodePlugin, IAudioBot
	{
		private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
			(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

		private void vertifyConnection()
		{
            connectMPDServer();
		}

		private void registerToJabber(IMessengerClient r_Client)
		{
			IMessengerRpc rpc = (r_Client as IMessengerRpc);
			if(rpc != null)
			{
				rpc.RegisterRpcServer <IAudioBot, MPDPlugin>(this);
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
            m_Mpc.Update();
		}

		private void r_Client_ConnectionStateChanged (object sender, ConnectionStateChangedEventArgs e)
		{
			if(e.IsConnected) 
            {
                r_Client.SetNickname("BoomBox Bot");
                r_Client.SetAvatar(Assembly.GetExecutingAssembly().GetManifestResourceStream("Monitron.Plugins.MPD.Boombox.png"));
				foreach(var buddy in r_Client.Buddies.Where(item => item.Groups.Contains("admin"))) 
                {
					string welcomeMessage = "\nHi, I am a MPD bot";
                    r_Client.SendMessage(buddy.Identity, welcomeMessage);
				}
			}
		}

		[RemoteCommand(MethodName="get-details")]
		public string GetMPDServerDetails(Identity i_Buddy)
		{
			vertifyConnection();
			if(m_Mpc != null)
			{
				return m_Mpc.Stats().ToString();
			} 
			else
			{
				return "Cannot get details";
			}
		}

        //[RemoteCommand(MethodName="set-host")]
        public string setHost(Identity i_Buddy, string i_Host, int i_Port)
        {
            m_DataStore.Write("host", i_Host);
            m_DataStore.Write("port", i_Port);
            return "Host information updated";
        }

        //[RemoteCommand(MethodName="connect")]
        public string connect(Identity i_Buddy)
        {
            string IP = m_DataStore.Read<string>("host");
            int port = m_DataStore.Read<int>("port");
            r_Client.SendMessage(i_Buddy, 
                string.Format("Connecting to {0}:{1}", IP, port));
            connectMPDServer();
            return "Connected to Boombox, Start playing!";
        }

		[RemoteCommand(MethodName="songs-list")]
		public string GetSongsList(Identity i_Buddy)
		{
			vertifyConnection();
			PopulatePlayList();
			string songString = null;
            List<string> songs = m_Mpc.PlaylistInfo()
                .Select((item) => string.Format("{0} - {1}", item.Artist, item.Title))
                .ToList();

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
			vertifyConnection();
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

        [RemoteCommand(MethodName="set-volume")]
        public string setVolume(Identity i_Buddy, int volume) {
			vertifyConnection();            
			m_Mpc.SetVol(volume);
            return string.Format("Volume set to: {0}", m_Mpc.Status().Volume);
        }

        [RemoteCommand(MethodName="play")]
        public string PlaySong(Identity i_Buddy)
        {
            return PlaySong(i_Buddy, 0);
        }

        [RemoteCommand(MethodName="toggle-random")]
        public string ToggleRandom(Identity i_Buddy) {
            vertifyConnection();
            PopulatePlayList();
            m_Mpc.Random(m_Mpc.Status().Random == false);
            return string.Format("Random is set to {0}", m_Mpc.Status().Random);
        }

		[RemoteCommand(MethodName="play-from")]
        public string PlaySong(Identity i_Buddy, int num)
		{
			vertifyConnection();
			if(m_Mpc != null)
			{
                PopulatePlayList();
                if (num < 1)
                {
                    m_Mpc.Play();
                }
                else
                {
                    m_Mpc.Play(num - 1);
                }
                new Thread(delegate()
                    {
                        IMessengerRpc rpc = (r_Client as IMessengerRpc);
                        foreach (var buddy in r_Client.Buddies)
                        {
                            string[] resources = buddy.Resources;
                            if (resources.Length > 0)
                            {
                                var buddyIden = buddy.Identity;
                                buddyIden.Resource = resources[0];
                                try
                                {
                                    string[] implementedInterfaces = rpc.GetRegisterServersList(buddyIden);
                                    sr_Log.Info("Checking " + buddyIden.ToString() + " for implemented interfaces");
                                    sr_Log.Info("implemented Interfaces: " + String.Join(", ", implementedInterfaces.Select(item => item.ToString())));
                                    if (implementedInterfaces.Contains("IMovieBot"))
                                    {
                                        IMessengerRpc movieRpc = (r_Client as IMessengerRpc);
                                        IMovieBot movieBot = movieRpc.CreateRpcClient<IMovieBot>(buddyIden);
                                        sr_Log.Debug("Pause movie for " + buddy.ToString());
                                        movieBot.PauseMovie();
                                    }
                                }
                                catch (Exception e)
                                {
                                    sr_Log.Info(e);
                                }
                            }
                        }    
                    }).Start();

				return "start playing " + m_Mpc.CurrentSong().Title;
			} 
			else
			{
				return "Cannot start playing";
			}


		}
			
		[RemoteCommand(MethodName="next")]
		public string NextSong(Identity i_Buddy)
		{
			vertifyConnection();
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
            var playlist = new SortedSet<String>(m_Mpc.PlaylistInfo().Select(item => item.File));
            var songsToAdd = m_Mpc.LsInfo().FileList.Where(item => !playlist.Contains(item.File));
            foreach (var song in songsToAdd)
            {
                m_Mpc.Add(song.File);
            }
		}

		public string PauseAudio()
		{
			return StopSong(new Identity());
		}
	}
}


