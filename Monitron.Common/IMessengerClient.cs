using System;
using System.Collections.Generic;
using System.IO;

namespace Monitron.Common
{
    public interface IMessengerClient
    {
        event EventHandler<MessageArrivedEventArgs> MessageArrived;
        event EventHandler<BuddySignedInEventArgs> BuddySignedIn;
        event EventHandler<BuddySignedOutEventArgs> BuddySignedOut;
        event EventHandler<BuddyListChangedEventArgs> BuddyListChanged;
        event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;
        event EventHandler<FileTransferProgressEventArgs> FileTransferProgress;
        event EventHandler<FileTransferAbortedEventArgs> FileTransferAborted;

        IEnumerable<BuddyListItem> Buddies { get; }

        Identity Identity { get; }

        bool IsConnected { get; }

        FileTransferRequest FileTransferRequest {get; set;}

        void SendMessage(Identity i_Buddy, string i_Message);

        void AddBuddy(Identity i_Identity, params string [] i_Groups);

        void RemoveBuddy(Identity i_Identity);

        void SetAvatar(Stream i_Stream);

        void SetNickname(string i_Nickname);
    }
}

