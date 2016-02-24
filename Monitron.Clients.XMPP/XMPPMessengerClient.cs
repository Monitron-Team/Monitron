using System;

using S22.Xmpp;
using S22.Xmpp.Client;
using S22.Xmpp.Im;

using Monitron.Common;
using System.Collections.Generic;

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
                hostname: i_Account.Identity.Host,
                username: i_Account.Identity.UserName,
                password: i_Account.Password
            );
            m_Client.Connect(k_DefaultResource);
            m_Client.Message += onClientMessageArrived;
            m_Client.RosterUpdated += onClientRosterUpdated;
        }

        private void onClientMessageArrived(object i_Sender, MessageEventArgs i_Args)
        {
            this.OnMessageArrived(new MessageArrivedEventArgs(
                i_Args.Jid.ToIdentity(),
                i_Args.Message.Body
            ));
        }

        public void onClientRosterUpdated(object sender, RosterUpdatedEventArgs e)
        {
            OnBuddyListChanged(new BuddyListChangedEventArgs(
                new BuddyListItem(e.Item.Jid.ToIdentity(), e.Item.Groups),
                e.Removed
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
        
        public void sendMessage(Identity i_Buddy, string i_Message)
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
    }
}

