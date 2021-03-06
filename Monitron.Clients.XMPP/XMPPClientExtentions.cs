﻿using System;

using S22.Xmpp;

using Monitron.Common;

namespace Monitron.Clients.XMPP
{
    public static class XMPPClientExtentions
    {
        public static Jid ToJid(this Identity i_Identity)
        {
            return new Jid(i_Identity.ToString());
        }

        public static Identity ToIdentity(this Jid i_Jid)
        {
            return new Identity {
                UserName = i_Jid.Node,
                Domain = i_Jid.Domain,
                Resource = i_Jid.Resource
            };
        }
    }
}

