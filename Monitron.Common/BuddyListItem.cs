using System;
using System.Collections.Generic;

namespace Monitron.Common
{
    public class BuddyListItem
    {
        public IEnumerable<string> Groups { get; private set; }

        public Identity Identity { get; set; }
        
        public BuddyListItem(Identity i_Identity, IEnumerable<string> i_Groups)
        {
            Groups = i_Groups;
            Identity = i_Identity;
        }
    }
}

