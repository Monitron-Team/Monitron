using System;

namespace Monitron.Common
{
    public interface IMessengerClient
    {
        event EventHandler<MessageArrivedEventArgs> MessageArrived;
        event EventHandler<BuddySignedInEventArgs> BuddySignedIn;
        event EventHandler<BuddySignedOutEventArgs> BuddySignedOut;

        IBuddyList BuddyList { get; }

        Identity Identity { get; }

        void sendMessage(Identity i_Buddy, string i_Message);
    }
}

