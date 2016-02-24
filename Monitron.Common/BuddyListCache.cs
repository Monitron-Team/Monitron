using System;
using System.Collections.Generic;

namespace Monitron.Common
{
    public class BuddyListCache
    {
        private readonly IMessengerClient r_Client;

        private readonly Dictionary<Identity, ISet<string>> r_Buddies;

        public IDictionary<Identity, ISet<string>> Buddies
        {
            get
            {
                return r_Buddies;
            }
        }

        public BuddyListCache(IMessengerClient i_Client)
        {
            r_Buddies = new Dictionary<Identity, ISet<string>>();
            r_Client = i_Client;
            ReloadCache();
            r_Client.BuddyListChanged += r_Client_BuddyListChanged;
        }

        public void ReloadCache()
        {
            r_Buddies.Clear();
            foreach (BuddyListItem item in r_Client.Buddies)
            {
                HashSet<string> groupSet = new HashSet<string>();
                foreach (string group in item.Groups)
                {
                    groupSet.Add(group);
                }

                r_Buddies.Add(item.Identity, groupSet);
            }
        }

        private void r_Client_BuddyListChanged(object sender, BuddyListChangedEventArgs e)
        {
            r_Buddies.Remove(e.Item.Identity);
            if (!e.Removed)
            {
                HashSet<string> groupSet = new HashSet<string>();
                foreach (string group in e.Item.Groups)
                {
                    groupSet.Add(group);
                }

                r_Buddies.Add(e.Item.Identity, groupSet);
            }
        }
    }
}

