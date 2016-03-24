using System;
using System.Linq;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace S22.Xmpp.Extensions
{
    public class VCard
    {
        private readonly XmlElement element;

        private string getTextField(string name)
        {
            if (element[name] != null)
            {
                return element.InnerText;
            }
            else
            {
                return null;
            }
        }

        private void setTextField(string name, string value)
        {
            var e = element[name];
            if (e != null) {
                if (value == null)
                    element.RemoveChild(e);
                else
                    e.InnerText = value;
            } else {
                if (value != null)
                    element.Child(Xml.Element(name).Text(value));
            }
        }

        public string FullName {
            get
            {
                return getTextField("FN");
            }
            set
            {
                setTextField("FN", value);
            }
        }

        public string Nickname {
            get
            {
                return getTextField("NICKNAME");
            }
            set
            {
                setTextField("NICKNAME", value);
            }
        }

        public string Url {
            get
            {
                return getTextField("URL");
            }
            set
            {
                setTextField("URL", value);
            }
        }

        public Jid JubbleId
        {
            get
            {
                return new Jid(getTextField("URL"));
            }
            set
            {
                setTextField("URL", value.GetBareJid().ToString());
            }
        }

        public Image Photo
        {
            set
            {
                string base64Data;
                using (var ms = new MemoryStream()) {
                    value.Save(ms, value.RawFormat);
                    byte[] data = ms.ToArray();
                    base64Data = Convert.ToBase64String(data);
                }

                var e = element["PHOTO"];
                if (e == null)
                {
                    element.Child(Xml.Element("PHOTO"));
                    e = element["PHOTO"];
                }

                if (e["TYPE"] != null)
                {
                    e.RemoveChild(e["TYPE"]);
                }

                if (e["BINVAL"] != null)
                {
                    e.RemoveChild(e["BINVAL"]);
                }

                e.Child(Xml.Element("TYPE").Text(GetMimeType(value)));
                e.Child(Xml.Element("BINVAL").Text(base64Data));
            }
        }

        private string GetMimeType(Image image) {
            image.ThrowIfNull("image");
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.FormatID == image.RawFormat.Guid)
                    return codec.MimeType;
            }
            throw new ArgumentException("The mime-type could not be determined.");
        }

        public VCard(XmlElement element)
        {
            element.ThrowIfNull("element");
            this.element = element;
        }

        public VCard()
        {
            element = Xml.Element("vCard", "vcard-temp");
        }

        public XmlElement ToXmlElement() {
            return element;
        }
    }
}

