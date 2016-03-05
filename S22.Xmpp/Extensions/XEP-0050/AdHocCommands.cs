using System;
using System.Collections.Generic;
using System.Xml;

using S22.Xmpp.Core;
using S22.Xmpp.Im;

namespace S22.Xmpp.Extensions
{
    internal class AdHocCommands : XmppExtension
    {
        public AdHocCommands(XmppIm im): base (im)
        {
        }
        
        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] {
                    "http://jabber.org/protocol/commands"
                };
            }
        }

        public override Extension Xep
        {
            get
            {
                return Extension.AdHocCommands;
            }
        }

        public AdHocCommandSession ExecuteCommand(string node)
        {
            var xml = Xml.Element("command", "http://jabber.org/protocol/commands")
                .Attr("node", node)
                .Attr("action", "execute");
            Iq iq = im.IqRequest(
                type: IqType.Set,
                to: im.Jid.Domain,
                data: xml
            );

            if (iq.Type == IqType.Error)
            {
                throw Util.ExceptionFromError(iq, "Could not execute ad-hoc command.");
            }

            AdHocCommand command = new AdHocCommand(iq.Data.FirstChild as XmlElement);
            AdHocCommandSession session = new AdHocCommandSession(im, command);

            return session;
        }
    }
}


