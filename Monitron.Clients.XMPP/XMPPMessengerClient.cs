using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Net;

using S22.Xmpp;
using S22.Xmpp.Client;
using S22.Xmpp.Extensions;
using S22.Xmpp.Im;

using Monitron.Common;

namespace Monitron.Clients.XMPP
{
    public sealed class XMPPMessengerClient : IMessengerClient, IDisposable
    {
        private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string sr_DefaultResource = Dns.GetHostName();

        private static readonly TimeSpan sr_ConnectRetryTimeSpan = TimeSpan.FromSeconds(10);

        private static readonly TimeSpan sr_PingDelayTimeSpan = TimeSpan.FromSeconds(30);

        private Timer m_PingTimer;

        public event EventHandler<MessageArrivedEventArgs> MessageArrived;

        public event EventHandler<BuddySignedInEventArgs> BuddySignedIn;

        public event EventHandler<BuddySignedOutEventArgs> BuddySignedOut;

        public event EventHandler<BuddyListChangedEventArgs> BuddyListChanged;

        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        public IEnumerable<BuddyListItem> Buddies
        {
            get
            {
                foreach (RosterItem item in m_Client.GetRoster())
                {
                    yield return new BuddyListItem(item.Jid.ToIdentity(), item.Groups);
                }
            }
        }

        public Identity Identity
        {
            get
            {
                return r_Account.Identity;
            }
        }

        public bool IsConnected
        {
            get
            {
                return m_Client.Connected;
            }
        }

        private readonly Account r_Account;

        private XmppClient m_Client;

        public XMPPMessengerClient(Account i_Account)
        {
            this.r_Account = i_Account;
            m_Client = new XmppClient(
                hostname: r_Account.Identity.Domain,
                username: r_Account.Identity.UserName,
                password: r_Account.Password
            );
            startConnect();
        }

        private void startConnect()
        {
            Thread t = new Thread(delegate ()
                {
                    while (!m_Client.Connected)
                    {
                        try
                        {
                            if (m_PingTimer != null)
                            {
                                m_PingTimer.Dispose();
                            }

                            m_Client.Dispose();
                        }
                        catch (InvalidOperationException)
                        {
                            // We are not connected
                        }

                        m_Client = new XmppClient(
                            hostname: r_Account.Identity.Domain,
                            username: r_Account.Identity.UserName,
                            password: r_Account.Password
                        );

                        Jid jid = r_Account.Identity.ToJid();
                        sr_Log.InfoFormat("Trying to connect to {0} as {1}",
                            jid.Domain,
                            jid.ToString()
                        );

                        try
                        {
                            m_Client.Connect(sr_DefaultResource);
                        }
                        catch (Exception e)
                        {
                            sr_Log.Warn("Connect failed", e);
                            sr_Log.InfoFormat("Could not connect, retrying in {0} seconds",
                                sr_ConnectRetryTimeSpan.TotalSeconds);
                            Thread.Sleep(sr_ConnectRetryTimeSpan);
                        }
                    }

                    OnConnectionStateChanged(new ConnectionStateChangedEventArgs(m_Client.Connected));
                    m_Client.Message += m_Client_MessageArrived;
                    m_Client.RosterUpdated += m_Client_RosterUpdated;
                    m_Client.StatusChanged += m_Client_StatusChanged;
                    m_Client.Error += m_Client_Error;
                    m_PingTimer = new Timer(delegate
                        {
                            if (IsConnected)
                            {
                                Jid jid = new Jid(r_Account.Identity.Domain, null);
                                m_Client.Ping(jid);
                            }
                        },
                        null,
                        sr_PingDelayTimeSpan,
                        sr_PingDelayTimeSpan
                    );
                });
            t.IsBackground = true;
            t.Start();
        }

        private void m_Client_Error(object i_Sender, S22.Xmpp.Im.ErrorEventArgs i_Args)
        {
            sr_Log.WarnFormat("Got an XMPP error '{0}':{1}", i_Args.Reason, i_Args.Exception);
            if (!m_Client.Connected)
            {
                sr_Log.Info("Got disconnected, reconnecting");
            }

            if (!m_Client.Connected)
            {
                OnConnectionStateChanged(new ConnectionStateChangedEventArgs(m_Client.Connected));
                startConnect();
            }
        }

        private void m_Client_StatusChanged(object i_Sender, StatusEventArgs i_Args)
        {
            switch (i_Args.Status.Availability)
            {
                case Availability.Online:
                    OnBuddySignedIn(new BuddySignedInEventArgs(i_Args.Jid.ToIdentity()));
                    break;
                case Availability.Offline:
                    OnBuddySignedOut(new BuddySignedOutEventArgs(i_Args.Jid.ToIdentity()));
                    break;
            }
        }

        public void OnBuddySignedIn(BuddySignedInEventArgs i_BuddySignedInEventArgs)
        {
            BuddySignedIn?.Invoke(this, i_BuddySignedInEventArgs);
        }

        public void OnBuddySignedOut(BuddySignedOutEventArgs i_BuddySignedOutEventArgs)
        {
            BuddySignedOut?.Invoke(this, i_BuddySignedOutEventArgs);
        }

        public void OnConnectionStateChanged(ConnectionStateChangedEventArgs i_Args)
        {
            ConnectionStateChanged?.Invoke(this, i_Args);
        }

        private void m_Client_MessageArrived(object i_Sender, MessageEventArgs i_Args)
        {
            this.OnMessageArrived(new MessageArrivedEventArgs(
                i_Args.Jid.ToIdentity(),
                i_Args.Message.Body
            ));
        }

        public void m_Client_RosterUpdated(object i_Sender, RosterUpdatedEventArgs i_Args)
        {
            OnBuddyListChanged(new BuddyListChangedEventArgs(
                new BuddyListItem(i_Args.Item.Jid.ToIdentity(), i_Args.Item.Groups),
                i_Args.Removed
            ));
        }

        public void OnBuddyListChanged(BuddyListChangedEventArgs i_EventArgs)
        {
            this.BuddyListChanged?.Invoke(this, i_EventArgs);
        }

        public void OnMessageArrived(MessageArrivedEventArgs i_EventArgs)
        {
            this.MessageArrived?.Invoke(this, i_EventArgs);
        }
        
        public void SendMessage(Identity i_Buddy, string i_Message)
        {
            m_Client.SendMessage(new Message(i_Buddy.ToJid(), i_Message));
        }

        public void AddBuddy(Identity i_Identity, params string[] i_Groups)
        {
            m_Client.AddContact(jid: i_Identity.ToJid(), groups: i_Groups);
        }

        public void RemoveBuddy(Identity i_Identity)
        {
            m_Client.RemoveContact(i_Identity.ToJid());
        }

        public void SetAvatar(Stream i_Stream)
        {
            m_Client.SetAvatar(i_Stream);
        }

        public void Dispose()
        {
            m_PingTimer.Dispose();
        }
    }
}

