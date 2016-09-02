using System;
using System.Xml;

namespace Monitron.Common
{
    public interface IPubSub
    {
        void Subscribe(string node, Action<Identity, XmlElement> action);

        void Unsubscribe(string node);

        void Publish(string node, string itemid = null, params XmlElement[] data);
    }
}

