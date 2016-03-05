using System;
using System.Linq;
using System.Configuration;

using NUnit.Framework;
using S22.Xmpp;
using S22.Xmpp.Client;

namespace Monitron.Management.AdminClient.Tests
{
    [TestFixture()]
    public class Test
    {
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

        private Jid getUniqueJid()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string host = appSettings["admin_host"];
            return new Jid(string.Format("test_{0}@{1}", DateTime.Now.ToBinary(), host));
        }

        [Test()]
        public void AddAndDeleteUser()
        {
            Jid account = getUniqueJid();
            AdminClient adminClient = getAdminClient();
            adminClient.AddUser(account, "123456");
            adminClient.DeleteUser(account);
        }

        [Test()]
        public void AddRosterItem()
        {
            Jid account = getUniqueJid();
            Jid buddy = getUniqueJid();
            AdminClient adminClient = getAdminClient();
            adminClient.AddUser(account, "123456");
            adminClient.AddUser(buddy, "123456");

            try
            {
                // This connects before adding
                using (XmppClient account_client = new XmppClient(account.Domain, account.Node, "123456"))
                {
                    account_client.Connect();
                    string[] groups = new [] { "group", "another group" };
                    adminClient.AddRosterItem(account, buddy, groups);
                    adminClient.AddRosterItem(buddy, account, groups);
                    Assert.IsTrue(account_client.GetRoster().Any(
                        item => item.Jid == buddy && groups.All(group => item.Groups.Contains(group))));
                    // This connects after adding
                    using (XmppClient buddy_client = new XmppClient(buddy.Domain, buddy.Node, "123456"))
                    {
                        buddy_client.Connect();
                        Assert.IsTrue(buddy_client.GetRoster().Any(
                        item => item.Jid == account && groups.All(group => item.Groups.Contains(group))));
                    }
                }
            }
            finally
            {
                adminClient.DeleteUser(account, buddy);
            }
        }

        [Test()]
        public void DeleteRosterItem()
        {
            Jid account = getUniqueJid();
            Jid buddy = getUniqueJid();
            AdminClient adminClient = getAdminClient();
            adminClient.AddUser(account, "123456");
            adminClient.AddUser(buddy, "123456");

            try
            {
                using (XmppClient account_client = new XmppClient(account.Domain, account.Node, "123456"))
                {
                    // This connects before adding
                    account_client.Connect();
                    string[] groups = new [] { "group", "another group" };
                    adminClient.AddRosterItem(account, buddy, groups);
                    adminClient.DeleteRosterItem(account, buddy);
                    Assert.IsTrue(account_client.GetRoster().All(
                        item => item.Jid != buddy));
                }
            }
            finally
            {
                adminClient.DeleteUser(account, buddy);
            }
        }
    }
}

