using System;
using System.Collections.Generic;

namespace Monitron.Common
{
    public interface IBuddyList
    {
        IReadOnlyCollection<BuddyListItem> Buddies { get; }

        void AddBuddy(Identity i_Account, params string [] i_Groups);
        void RemoveBuddy(Identity i_Account);
    }
}

