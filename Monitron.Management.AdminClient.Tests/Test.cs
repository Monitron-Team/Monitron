using System;

using NUnit.Framework;
using S22.Xmpp;
using S22.Xmpp.Client;

namespace Monitron.Management.AdminClient.Tests
{
    [TestFixture()]
    public class Test
    {
        [Test()]
        public void TestCase()
        {
            Jid account = new Jid(string.Format("test_{0}@prosody.test", DateTime.Now.ToBinary()));
            XmppClient client = new XmppClient("prosody.test", "test_admin", "123456");
            client.Connect();
            AdminClient adminClient = new AdminClient(client);
            adminClient.AddUser(account, "123456");
            adminClient.DeleteUser(account);
        }
    }
}

