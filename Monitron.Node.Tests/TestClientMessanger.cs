using System;
using Monitron.Common;
using System.IO;

namespace Monitron.Node.Tests
{
    public class TestClientMessanger : IMessengerClient
	{
        public void SetAvatar(Stream i_stream)
        {
            // Do nothing
        }

		private readonly Account r_Account;

		public TestClientMessanger(Account i_Account)
		{
			this.r_Account = i_Account;
		}

		public Identity Identity
		{
			get
			{
				return r_Account.Identity;
			}
		}
			
		public event EventHandler<MessageArrivedEventArgs> MessageArrived;

		public event EventHandler<BuddySignedInEventArgs> BuddySignedIn;

		public event EventHandler<BuddySignedOutEventArgs> BuddySignedOut;

		public event EventHandler<BuddyListChangedEventArgs> BuddyListChanged;

        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        public bool IsConnected
        {
            get
            {
                return true;
            }
        }

		public void SendMessage(Identity i_Buddy, string i_Message)
		{
			throw new NotImplementedException();
		}

		public void AddBuddy(Identity i_Identity, params string[] i_Groups)
		{
			throw new NotImplementedException();
		}

		public void RemoveBuddy(Identity i_Identity)
		{
			throw new NotImplementedException();
		}

		public System.Collections.Generic.IEnumerable<BuddyListItem> Buddies {
			get {
				throw new NotImplementedException();
			}
		}
	}
}

