using System;

using S22.Xmpp.Client;
using S22.Xmpp.Im;

using Monitron.Common;
using System.Collections.Generic;

namespace Monitron.Clients.XMPP
{
    public class XMPPBuddyList : IBuddyList
    {       
        private readonly List<BuddyListItem> m_BuddyCache = new List<BuddyListItem>();
        private bool m_IsCacheDirty = true;

        private readonly XmppClient m_Client;
            
        public IReadOnlyCollection<BuddyListItem> Buddies
        {
            get
            {
                return this.m_BuddyCache;
            }
        }
        
        public XMPPBuddyList(XmppClient i_Client)
        {
            m_Client = i_Client;
            m_Client.RosterUpdated += onRosterUpdated;
        }

        private void onRosterUpdated(object i_Sender, RosterUpdatedEventArgs i_Args)
        {
            if (m_IsCacheDirty)
            {
                refreshBuddyCache();
            }
            else
            {
                m_BuddyCache.RemoveAll(item => item.Identity.Equals(i_Args.Item.Jid.ToIdentity()));
                
                if (!i_Args.Removed)
                {
                    m_BuddyCache.Add(new BuddyListItem(
                            i_Args.Item.Jid.ToIdentity(),
                            i_Args.Item.Groups
                        ));
                }
            }
        }

        private void refreshBuddyCache()
        {
            Roster roster = m_Client.GetRoster();
            m_BuddyCache.Clear();
            foreach (RosterItem item in roster)
            {
                m_BuddyCache.Add(new BuddyListItem(
                    item.Jid.ToIdentity(),
                    item.Groups
                ));
            }

            m_IsCacheDirty = false;
        }

        public void AddBuddy(Identity i_Identity, params string[] i_Groups)
        {
            m_Client.AddContact(jid: i_Identity.ToJid(), groups: i_Groups);
        }

        public void RemoveBuddy(Identity i_Identity)
        {
            m_Client.RemoveContact(i_Identity.ToJid());
        }
    }
}

