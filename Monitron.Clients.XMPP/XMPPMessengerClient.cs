using System;

using S22.Xmpp;
using S22.Xmpp.Client;
using S22.Xmpp.Extensions;
using S22.Xmpp.Im;

using Monitron.Common;
using System.Collections.Generic;
using System.IO;

namespace Monitron.Clients.XMPP
{
    public sealed class XMPPMessengerClient : IMessengerClient
    {
        private static string k_DefaultResource = "Node";

        public event EventHandler<MessageArrivedEventArgs> MessageArrived;

        public event EventHandler<BuddySignedInEventArgs> BuddySignedIn;

        public event EventHandler<BuddySignedOutEventArgs> BuddySignedOut;

        public event EventHandler<BuddyListChangedEventArgs> BuddyListChanged;

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

        private readonly Account r_Account;

        private XmppClient m_Client;

        public XMPPMessengerClient(Account i_Account)
        {
            this.r_Account = i_Account;

            m_Client = new XmppClient(
                hostname: i_Account.Identity.Domain,
                username: i_Account.Identity.UserName,
                password: i_Account.Password
            );
            m_Client.Connect(k_DefaultResource);
            m_Client.Message += m_Client_MessageArrived;
            m_Client.RosterUpdated += m_Client_RosterUpdated;
            m_Client.StatusChanged += m_Client_StatusChanged;
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
    }
}

