using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;

using Monitron.Management.AdminClient;
using Monitron.Common;
using Monitron.Clients.XMPP;

using S22.Xmpp;
using S22.Xmpp.Client;
using System.Threading.Tasks;

namespace Monitron.Plugins.Management
{
	public class UserAccountsManager
	{
		private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
			(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly XmppClient r_Client;
        private readonly AdminClient r_Admin;
        private readonly IMongoDatabase m_MongoDB;

		private const string k_NewUserDefaultPassword = "12356";
		private const int k_MaxWorkersAllowed = 10;
		private const string k_UsersDBCollection = "Users";

        public UserAccountsManager(IMongoDatabase i_Database, string i_AdminHost, string i_AdminUsername, string i_AdminPassword)
		{
			m_MongoDB = i_Database;
			r_Client = new XmppClient(i_AdminHost, i_AdminUsername, i_AdminPassword);
            r_Client.Connect();
            r_Admin = new AdminClient(r_Client);
		}

		private BsonDocument serializeIdentity(Identity i_Identity)
		{
			var document = new BsonDocument 
			{ 
				{ "Info", new BsonDocument 
                        {
                            { "username", i_Identity.UserName },
                            { "host", r_Client.Jid.Domain }
					}
				}, 
				{ "Workers", new BsonDocument 
					{
						{ "Active", 0 },
						{ "MaxWorkersAllowed", k_MaxWorkersAllowed }
					}
				}
			};

			return document;
		}

		async void addUserToDB(Identity i_Identity)
		{
			BsonDocument newUserBsonDocument = serializeIdentity(i_Identity);

			var collection = m_MongoDB.GetCollection<BsonDocument>(k_UsersDBCollection);
			await collection.InsertOneAsync(newUserBsonDocument);
		}

		public void AddUser(Identity i_NewUser, Identity i_AssignBuddyTo)
        {
            r_Admin.AddUser(i_NewUser.ToJid(), k_NewUserDefaultPassword);
            Jid newAccount = new Jid(i_NewUser.Domain, i_NewUser.UserName);
            r_Admin.AddRosterItem(newAccount, r_Client.Jid, "Monitron");
        }

		public void DeleteUser(Identity i_UserToDelete)
        {
            r_Admin.DeleteUser(i_UserToDelete.ToJid());
        }

        public void AddRosterItem(Identity i_User, Identity i_Buddy, params string[] i_Groups)
        {
            r_Admin.AddRosterItem(i_User.ToJid(), i_Buddy.ToJid(), i_Groups);
        }
	}
}

