using System;

using S22.Xmpp;
using S22.Xmpp.Client;
using S22.Xmpp.Extensions;
using S22.Xmpp.Extensions.Dataforms;

namespace Monitron.Management.AdminClient
{
    public class AdminClient
    {
        private static string k_MonitronNodePrefix = "http://monitron.ddns.net/protocol/admin#";
        private readonly XmppClient r_Client;
        
        public AdminClient(XmppClient i_Client)
        {
            r_Client = i_Client;
        }

        public void AddUser(Jid i_UserName, string i_Password)
        {
            var session = r_Client.ExecuteAdHocCommand(k_MonitronNodePrefix + "add-user");
            SubmitForm form = new SubmitForm(
                new JidField("accountjid", i_UserName),
                new TextField("password", i_Password)
            );
            session.Execute(form);
            if (session.Status != CommandStatus.Completed)
            {
                session.Cancel();
                throw new AdminClientException("Operation did not complete as expected");
            }

            if (session.Note != null && session.Note.Type == NoteType.Error)
            {
                throw new AdminClientException("Could not create user: " + session.Note.Content);
            }
        }

        public void DeleteUser(params Jid[] i_UserNames)
        {
            var session = r_Client.ExecuteAdHocCommand(k_MonitronNodePrefix + "delete-user");
            SubmitForm form = new SubmitForm(
                new JidMultiField("accountjids", i_UserNames)
            );
            session.Execute(form);
            if (session.Status != CommandStatus.Completed)
            {
                session.Cancel();
                throw new AdminClientException("Operation did not complete as expected");
            }

            if (session.Note != null && session.Note.Type == NoteType.Error)
            {
                throw new AdminClientException("Could not delete user: " + session.Note.Content);
            }
        }

        public void AddRosterItem(Jid i_UserName, Jid i_Item, params string[] i_Groups)
        {
            var session = r_Client.ExecuteAdHocCommand(k_MonitronNodePrefix + "add-roster-item");
            SubmitForm form = new SubmitForm(
                new JidField("accountjid", i_UserName),
                new JidField("itemjid", i_Item),
                new TextMultiField("groups", i_Groups)
            );
            session.Execute(form);
            if (session.Status != CommandStatus.Completed)
            {
                session.Cancel();
                throw new AdminClientException("Operation did not complete as expected");
            }

            if (session.Note != null && session.Note.Type == NoteType.Error)
            {
                throw new AdminClientException("Could not add roster item: " + session.Note.Content);
            }
        }

        public void DeleteRosterItem(Jid i_UserName, Jid i_Item)
        {
            var session = r_Client.ExecuteAdHocCommand(k_MonitronNodePrefix + "delete-roster-item");
            SubmitForm form = new SubmitForm(
                new JidField("accountjid", i_UserName),
                new JidField("itemjid", i_Item)
            );
            session.Execute(form);
            if (session.Status != CommandStatus.Completed)
            {
                session.Cancel();
                throw new AdminClientException("Operation did not complete as expected");
            }

            if (session.Note != null && session.Note.Type == NoteType.Error)
            {
                throw new AdminClientException("Could not delete roster item: " + session.Note.Content);
            }
        }

    }
}

