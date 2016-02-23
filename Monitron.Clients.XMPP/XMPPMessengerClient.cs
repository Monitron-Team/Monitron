using System;

using S22.Xmpp;
using S22.Xmpp.Client;
using S22.Xmpp.Im;

using Monitron.Common;

namespace Monitron.Clients.XMPP
{
    public sealed class XMPPMessengerClient : IMessengerClient
    {
        private static string k_DefaultResource = "Node";

        public event EventHandler<MessageArrivedEventArgs> MessageArrived;

        public event EventHandler<BuddySignedInEventArgs> BuddySignedIn;

        public event EventHandler<BuddySignedOutEventArgs> BuddySignedOut;

        public IBuddyList BuddyList { get; private set; }

        public Identity Identity
        {
            get
            {
                return r_Account.Identity;
            }
        }

        private readonly Account r_Account;

        private XmppClient m_client;

        public XMPPMessengerClient(Account i_Account)
        {
            this.r_Account = i_Account;

            m_client = new XmppClient(
                hostname: i_Account.Identity.Host,
                username: i_Account.Identity.UserName,
                password: i_Account.Password
            );
            BuddyList = new XMPPBuddyList(m_client);
            m_client.Connect(k_DefaultResource);
            m_client.Message += onClientMessageArrived;
        }

        private void onClientMessageArrived(object i_Sender, MessageEventArgs i_Args)
        {
            this.OnMessageArrivedEventArgs(new MessageArrivedEventArgs(
                i_Args.Jid.ToIdentity(),
                i_Args.Message.Body
            ));
        }

        public void OnMessageArrivedEventArgs(MessageArrivedEventArgs i_EventArgs)
        {
            this.MessageArrived?.Invoke(this, i_EventArgs);
        }
        
        public void sendMessage(Identity i_Buddy, string i_Message)
        {
            m_client.SendMessage(new Message(i_Buddy.ToJid(), i_Message));
        }
    }
}

