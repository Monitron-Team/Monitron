using System;
using System.Xml;

namespace S22.Xmpp.Extensions
{
    public class CommandNote
    {
        public NoteType Type { get; private set; }

        public string Content { get; private set; }

        public CommandNote(XmlElement element)
        {
            switch (element.GetAttribute("type"))
            {
                case "info":
                    Type = NoteType.Info;
                    break;
                case "warn":
                    Type = NoteType.Warn;
                    break;
                case "error":
                    Type = NoteType.Error;
                    break;
            }

            Content = element.InnerText;
        }
    }
}

