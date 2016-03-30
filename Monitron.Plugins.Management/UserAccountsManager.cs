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

		private AdminClient r_Admin;
		private IMongoDatabase m_MongoDB;
		private IDictionary<string, XMPPMessengerClient> m_UsersDictionary;

		private const string k_AdminHost = "monitron-test";
		private const string k_AdminUsername = "monitron_admin";
		private const string k_AdminPassword = "xxx";
		private const string k_NewUserDefaultPassword = "12356";
		private const int k_MaxWorkersAllowed = 10;
		private const string k_UsersDBCollection = "Users";

		public UserAccountsManager(IMongoDatabase i_Database)
		{
			m_MongoDB = i_Database;
			m_UsersDictionary = loadUsersFromDB().Result;
			XmppClient adminClient = new XmppClient(k_AdminHost, k_AdminUsername, k_AdminPassword);
			adminClient.Connect();
			r_Admin = new AdminClient(adminClient);
		}

		async private Task<Dictionary<string, XMPPMessengerClient>> loadUsersFromDB()
		{
			Dictionary <string, XMPPMessengerClient> usersDictionary = new Dictionary<string, XMPPMessengerClient>();

			var collection = m_MongoDB.GetCollection<BsonDocument>(k_UsersDBCollection);
			var filter = new BsonDocument();
			using (var cursor = await collection.FindAsync(filter))
			{
				while (await cursor.MoveNextAsync())
				{
					var batch = cursor.Current;
					foreach (var document in batch)
					{
						string userName = document["UserName"].AsString;
						string password = document["password"].AsString;
						usersDictionary.Add(userName, new XMPPMessengerClient(new Account(userName, password, k_AdminHost)));
					}
				}
			}

			return usersDictionary;
		}

		private BsonDocument createNewUserBsonDocument(Identity i_Identity)
		{
			var document = new BsonDocument 
			{ 
				{ "Info", new BsonDocument 
					{
						{ "UserName", i_Identity.UserName },
						{ "Password", k_NewUserDefaultPassword }
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
			BsonDocument newUserBsonDocument = createNewUserBsonDocument(i_Identity);

			var collection = m_MongoDB.GetCollection<BsonDocument>(k_UsersDBCollection);
			await collection.InsertOneAsync(newUserBsonDocument);
		}

		public bool AddUser(Identity i_NewUser, Identity i_AssignBuddyTo)
		{
			bool succeededToAdd = false;
			//checking if user already exists
			if (!m_UsersDictionary.ContainsKey(i_NewUser.ToString()))
			{
				try
				{
					r_Admin.AddUser(i_NewUser.ToJid() , k_NewUserDefaultPassword);
					//creating new XMPP Client. adding the caller to it's buddy and adding it to the XMPPMessangerClient list
					XMPPMessengerClient newXmppMessangerClient = new XMPPMessengerClient(new Account(i_NewUser.UserName, k_NewUserDefaultPassword, i_NewUser.Domain));
					newXmppMessangerClient.AddBuddy(i_AssignBuddyTo, new string[] {"Monitron"});
					m_UsersDictionary.Add(i_NewUser.ToString(), newXmppMessangerClient);

					addUserToDB(i_NewUser);
					succeededToAdd = true;
				}
				catch (AdminClientException)
				{	
				}	
			}

			return succeededToAdd;
		}

		public bool DeleteUser(Identity i_UserToDelete)
		{
			bool succeededToRemove = false;

			try
			{
				//delete only if user exists
				if (m_UsersDictionary.ContainsKey(i_UserToDelete.ToString()))
				{
					XMPPMessengerClient xmppClientToDelete = null;

					m_UsersDictionary.TryGetValue(i_UserToDelete.ToString(), out xmppClientToDelete);
					xmppClientToDelete.Dispose();
					m_UsersDictionary.Remove(i_UserToDelete.ToString());
					r_Admin.DeleteUser(i_UserToDelete.ToJid());
				}
			}
			catch (AdminClientException)
			{
			}

			return succeededToRemove;
		}
	}
}

