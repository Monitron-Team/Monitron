using System;
using System.Collections.Generic;

using S22.Xmpp.Im;
using S22.Xmpp.Core;

namespace S22.Xmpp.Extensions
{
    internal class VCards : XmppExtension
    {
        public VCards(XmppIm im): base (im)
        {
        }

        public override IEnumerable<string> Namespaces
        {
            get
            {
                return new string[] {
                    "vcard-temp"
                };
            }
        }

        public override Extension Xep
        {
            get
            {
                return Extension.VCards;
            }
        }

        public VCard Retrieve()
        {
            Iq iq = im.IqRequest(
                type: IqType.Get,
                from: im.Jid,
                data: Xml.Element("vCard", "vcard-temp")
            );

            if (iq.Type == IqType.Error)
            {
                if (iq.Data["error"]?["item-not-found"] != null)
                {
                    // vCard is empty
                    return new VCard();
                }

                throw Util.ExceptionFromError(iq, "Could not retrieve vCard.");
            }

            Console.WriteLine(iq.Data["vCard"].ToXmlString());

            return new VCard(iq.Data["vCard"]);
        }

        public void Update(VCard vcard)
        {
            Iq iq = im.IqRequest(
                type: IqType.Set,
                from: im.Jid,
                data: vcard.ToXmlElement()
            );

            if (iq.Type == IqType.Error)
            {
                throw Util.ExceptionFromError(iq, "Could update vCard.");
            }
        }
    }
}

