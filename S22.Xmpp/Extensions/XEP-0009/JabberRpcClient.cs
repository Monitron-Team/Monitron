using System;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;

using S22.Xmpp.Im;
using S22.Xmpp.Core;

namespace S22.Xmpp
{
    public abstract class JabberRpcClient
    {
        private readonly XmppIm im;
        private readonly Jid target;

        public JabberRpcClient(XmppIm im, Jid target)
        {
            this.im = im;
            this.target = target;
        }

        protected T callMethod<T>(MethodCall methodCall)
        {
            XmlElement response = sendRequest(
                Xml.Element("query", "jabber:iq:rpc").Child(methodCall.ToXmlElement())
            );

            return new MethodResponse(response)
                .ParamsList
                .GetResultForReturnType<T>();
        }

        protected void castMethod(MethodCall methodCall)
        {
            sendRequest(
                Xml.Element("query", "jabber:iq:rpc").Child(methodCall.ToXmlElement())
            );
        }

        private XmlElement sendRequest(XmlElement data)
        {
            Iq response = im.IqRequest(
                type: IqType.Set,
                from: im.Jid,
                to: target,
                data: data
            );

            if (response.Type == IqType.Error)
            {
                throw Util.ExceptionFromError(response, "Could execute rpc command.");
            }

            return response.Data["query"].FirstChild as XmlElement;
        }
    }
}

