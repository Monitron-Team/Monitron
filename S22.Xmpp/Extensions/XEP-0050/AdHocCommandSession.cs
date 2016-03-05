using System;
using System.Xml;

using S22.Xmpp.Core;
using S22.Xmpp.Extensions.Dataforms;
using S22.Xmpp.Im;

namespace S22.Xmpp.Extensions
{
    /// <summary>
    /// Ad hoc command session.
    /// </summary>
    public class AdHocCommandSession
    {
        /// <summary>
        /// Gets the possible actions.
        /// </summary>
        /// <value>The possible actions.</value>
        public CommandAction PossibleActions
        {
            get
            {
                return m_currentCommand.Actions;
            }
        }

        /// <summary>
        /// Gets the note.
        /// </summary>
        /// <value>The note.</value>
        public CommandNote Note
        {
            get
            {
                return m_currentCommand.Note;
            }
        }

        /// <summary>
        /// Gets the request form.
        /// </summary>
        /// <value>The request form.</value>
        public RequestForm RequestForm
        {
            get
            {
                return m_currentCommand.Form;
            }
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        public CommandStatus Status
        {
            get
            {
                return m_currentCommand.Status;
            }
        }

        private AdHocCommand m_currentCommand;

        private XmppIm m_Im;

        internal AdHocCommandSession(XmppIm im, AdHocCommand initalResponse)
        {
            m_currentCommand = initalResponse;
            m_Im = im;
        }

        internal void UpdateCurrentCommand(AdHocCommand command)
        {
            m_currentCommand = command;
        }

        /// <summary>
        /// Perform the execute action.
        /// </summary>
        /// <param name="form">Form to submit.</param>
        public void Execute(SubmitForm form)
        {
            var xml = Xml.Element("command", "http://jabber.org/protocol/commands")
                .Attr("node", m_currentCommand.Node)
                .Attr("action", "execute")
                .Attr("sessionid", m_currentCommand.SessionId)
                .Child(form.ToXmlElement());
            Iq iq = m_Im.IqRequest(
                        type: IqType.Set,
                        to: m_Im.Jid.Domain,
                        data: xml
                    );

            if (iq.Type == IqType.Error)
            {
                throw Util.ExceptionFromError(iq, "Could not execute ad-hoc command.");
            }

            m_currentCommand = new AdHocCommand(iq.Data.FirstChild as XmlElement);
        }

        /// <summary>
        /// Cancel the current command session.
        /// </summary>
        public void Cancel()
        {
            var xml = Xml.Element("command", "http://jabber.org/protocol/commands")
                .Attr("node", m_currentCommand.Node)
                .Attr("action", "cancel")
                .Attr("sessionid", m_currentCommand.SessionId);
            
            Iq iq = m_Im.IqRequest(
                type: IqType.Set,
                to: m_Im.Jid.Domain,
                data: xml
            );

            if (iq.Type == IqType.Error)
            {
                throw Util.ExceptionFromError(iq, "Could not cancel ad-hoc command.");
            }

            m_currentCommand = new AdHocCommand(iq.Data.FirstChild as XmlElement);
        }
    }
}

