using System;

namespace Monitron.Common
{
    public sealed class BuddySignedInEventArgs : EventArgs
    {
        public Identity Buddy { get; private set; }

        public BuddySignedInEventArgs(Identity i_Buddy) : base()
        {
            Buddy = i_Buddy;
        }
    }
}

