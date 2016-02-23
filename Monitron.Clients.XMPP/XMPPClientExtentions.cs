using System;

using S22.Xmpp;

using Monitron.Common;

namespace Monitron.Clients.XMPP
{
    public static class XMPPClientExtentions
    {
        public static Jid ToJid(this Identity i_Identity)
        {
            return new Jid(i_Identity.UserName + "@" + i_Identity.Host);
        }

        public static Identity ToIdentity(this Jid i_Jid)
        {
            return new Identity {
                UserName = i_Jid.Node,
                Host = i_Jid.Domain
            };
        }
    }
}

