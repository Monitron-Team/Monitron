using System;
using System.Collections.Generic;

namespace Monitron.Common
{
	public class BuddyListChangedEventArgs : EventArgs
	{
        public BuddyListItem Item { get; private set; }
        public bool Removed { get; private set; }

        public BuddyListChangedEventArgs(BuddyListItem i_Item, bool i_Removed) : base()
        {
            Item = i_Item;
            Removed = i_Removed;
        }

	}
}

