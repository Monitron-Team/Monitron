using System;

namespace Monitron.Common
{
    public sealed class BuddySignedOutEventArgs : EventArgs
    {
        public Identity Buddy { get; private set; }

        public BuddySignedOutEventArgs(Identity i_Buddy) : base()
        {
            Buddy = i_Buddy;
        }
    }
}

