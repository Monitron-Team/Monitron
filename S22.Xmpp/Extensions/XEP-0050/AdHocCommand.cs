using System;

using System.Xml;

using S22.Xmpp.Extensions.Dataforms;

namespace S22.Xmpp.Extensions
{
    internal class AdHocCommand
    {
        public string SessionId { get; private set; }

        public string Node { get; private set; }

        public CommandStatus Status { get; private set; }

        public CommandAction Actions { get; private set; }

        public RequestForm Form {get; private set; }

        public CommandNote Note {get; private set; }

        private static CommandStatus parseStatus(string status)
        {
            switch (status)
            {
                case "executing":
                    return CommandStatus.Executing;
                    break;
                case "completed":
                    return CommandStatus.Completed;
                    break;
                case "canceled":
                    return CommandStatus.Canceled;
                    break;
                default:
                    // We cancel since this is invalid.
                    return CommandStatus.Canceled;
                    break;
            }
        }

        private static CommandAction parseActionsElement(XmlElement element)
        {
            CommandAction actions = CommandAction.Cancel | CommandAction.Execute;
            foreach (var child in element.ChildNodes)
            {
                var actionElement = child as XmlElement;
                if (actionElement == null)
                {
                    continue;
                }

                switch(actionElement.Name)
                {
                    case "prev":
                        actions |= CommandAction.Previous;
                        break;
                    case "next":
                        actions |= CommandAction.Next;
                        break;
                    case "complete":
                        actions |= CommandAction.Complete;
                        break;
                }
            }

            return actions;
        }
        
        public AdHocCommand(XmlElement data)
        {
            SessionId = data.GetAttribute("sessionid");
            Node = data.GetAttribute("node");
            Status = parseStatus(data.GetAttribute("status"));
            Actions = CommandAction.Cancel | CommandAction.Execute;

            foreach (var child in data.ChildNodes)
            {
                var element = child as XmlElement;
                if (element == null)
                {
                    continue;
                }

                switch(element.Name)
                {
                    case "actions":
                        Actions = parseActionsElement(element);
                        break;
                    case "x":
                        Form = new RequestForm(element);
                        break;
                    case "note":
                        Note = new CommandNote(element);
                        break;
                }
            }
        }
    }
}

