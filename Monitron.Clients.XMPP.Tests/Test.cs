using System;
using System.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

using NUnit.Framework;
using S22.Xmpp.Client;

using Monitron;
using Monitron.Common;
using Monitron.Clients.XMPP;
using Monitron.Management.AdminClient;
using S22.Xmpp;

namespace Monitron.Clients.XMPP.Tests
{
    [TestFixture()]
    public class Test
    {
        private readonly List<Account> r_testAccounts = new List<Account>();
        
        private AdminClient getAdminClient()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string username = appSettings["admin_username"];
            string password = appSettings["admin_password"];
            string host = appSettings["admin_host"];
            XmppClient client = new XmppClient(host, username, password);
            client.Connect();
            return new AdminClient(client);
        }

        [TearDown()]
        public void TearDown()
        {
            AdminClient client = getAdminClient();
            client.DeleteUser(r_testAccounts.Select(account => account.Identity.ToJid()).ToArray());
            r_testAccounts.Clear();
        }

        private Jid getUniqueJid()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string host = appSettings["admin_host"];
            return new Jid(string.Format(
                "test_{0}@{1}",
                DateTime.Now.ToBinary(),
                host
            ));
        }

        private Account createTestAccount()
        {
            var adminClient = getAdminClient();

            Jid jid = getUniqueJid();
            string password = "123456";
            adminClient.AddUser(jid, password);
            Account account = new Account(jid.Node, password, jid.Domain);
            r_testAccounts.Add(account);
            return account;
        }

        private XMPPMessengerClient getTestClient()
        {
            return new XMPPMessengerClient(
                createTestAccount()
            );
        }

        [Test()]
        public void TestConnect()
        {
            // This will implicitly connect
            getTestClient();
        }

        [Test()]
        public void SendMessageTest()
        {
            TimeSpan testTimeout = TimeSpan.FromSeconds(30);
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            XMPPMessengerClient client1 = getTestClient();
            XMPPMessengerClient client2 = getTestClient();

            client2.MessageArrived += delegate(object sender, MessageArrivedEventArgs e)
            {
                    waitHandle.Set();
            };
            
            client1.SendMessage(client2.Identity, "Hello");

            Assert.IsTrue(waitHandle.WaitOne(testTimeout));
        }

        [Test()]
        public void BuddyListModificationTest()
        {
            TimeSpan buddyListUpdateSleepTimeSpan = TimeSpan.FromSeconds(1);
            TimeSpan testTimeout = TimeSpan.FromSeconds(30);
            XMPPMessengerClient client1 = getTestClient();
            Account account2 = createTestAccount();

            clearBuddyList(client1);
            client1.AddBuddy(account2.Identity);
            // It takes a while for the buddy to be added in the server
            // This loop will wait until it happens or we time out
            DateTime start = DateTime.Now;
            while (DateTime.Now - start < testTimeout)
            {
                if (client1.Buddies.Count() != 0)
                {
                    break;
                }

                Thread.Sleep(buddyListUpdateSleepTimeSpan);
            }
            Assert.AreEqual(1, client1.Buddies.Count());
            Assert.AreEqual(account2.Identity, client1.Buddies.First().Identity);
        }

        private void clearBuddyList(IMessengerClient i_Client)
        {
            foreach (BuddyListItem item in i_Client.Buddies.ToList())
            {
                i_Client.RemoveBuddy(item.Identity);
            }
        }
    }
}
