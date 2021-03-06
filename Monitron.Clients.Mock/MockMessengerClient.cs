﻿using System;
using System.Collections.Generic;

using Monitron.Common;
using System.IO;

namespace Monitron.Clients.Mock
{
    public class MockMessengerClient : IMessengerClient
    {
        public event EventHandler<MessageArrivedEventArgs> MessageArrived;

        public event EventHandler<BuddySignedInEventArgs> BuddySignedIn;

        public event EventHandler<BuddySignedOutEventArgs> BuddySignedOut;

        public event EventHandler<BuddyListChangedEventArgs> BuddyListChanged;

        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        public event EventHandler<FileTransferProgressEventArgs> FileTransferProgress;

        public event EventHandler<FileTransferAbortedEventArgs> FileTransferAborted;

        public FileTransferRequest FileTransferRequest
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsConnected
        {
            get
            {
                return true;
            }
        }

        public readonly Dictionary<Identity, string[]> BuddyDictionary = new Dictionary<Identity, string[]>();

        public readonly Queue<Tuple<Identity, string>> SentMessageQueue = new Queue<Tuple<Identity, string>>();

        public void PushMessage(Identity i_Buddy, string i_Message)
        {
            MessageArrived?.Invoke(this, new MessageArrivedEventArgs(i_Buddy, i_Message));
        }
        
        public IEnumerable<BuddyListItem> Buddies
        {
            get
            {
                foreach (var pair in BuddyDictionary)
                {
					yield return new BuddyListItem(pair.Key, null, null);
                }
            }
        }
        
        public Account Account { get; set; }

        public Identity Identity
        {
            get
            {
                return Account.Identity;
            }
        }

        public string Nickname { get; set; }

        public void SetNickname(string i_Nickname)
        {
            Nickname = i_Nickname;
        }

        public MockMessengerClient(Identity i_Identity)
        {
            Account account = new Account(
                i_UserName: i_Identity.UserName,
                i_Host: i_Identity.Domain,
                i_Password: null
            );

            Account = account;
        }

        public MockMessengerClient(Account i_Account)
        {
            Account = i_Account;
        }
        
        public void SendMessage(Identity i_Buddy, string i_Message)
        {
            SentMessageQueue.Enqueue(Tuple.Create(i_Buddy, i_Message));
        }

        public void AddBuddy(Identity i_Identity, params string[] i_Groups)
        {
            if (BuddyDictionary.ContainsKey(i_Identity))
            {
                BuddyDictionary.Remove(i_Identity);
            }

            BuddyDictionary.Add(Account.Identity, i_Groups);
            OnBuddyListChanged(new BuddyListChangedEventArgs(
				new BuddyListItem(i_Identity, i_Groups, null), false)
            );
        }

        public void OnBuddyListChanged(BuddyListChangedEventArgs i_EventArgs)
        {
            this.BuddyListChanged?.Invoke(this, i_EventArgs);
        }

        public void OnBuddySignedIn(BuddySignedInEventArgs i_EventArgs)
        {
            this.BuddySignedIn?.Invoke(this, i_EventArgs);
        }

        public void OnBuddySignedOut(BuddySignedOutEventArgs i_EventArgs)
        {
            this.BuddySignedOut?.Invoke(this, i_EventArgs);
        }

        public void RemoveBuddy(Identity i_Identity)
        {
            string[] groups = BuddyDictionary[i_Identity];
            BuddyDictionary.Remove(i_Identity);
            OnBuddyListChanged(new BuddyListChangedEventArgs(
                new BuddyListItem(i_Identity, groups, null), true)
            );
        }

        public void SetAvatar(Stream stream)
        {
            // Do nothing
        }
    }
}

