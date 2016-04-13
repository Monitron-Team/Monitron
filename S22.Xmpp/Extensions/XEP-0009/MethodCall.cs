using System;
using System.Linq;
using System.Xml;
using System.Reflection;

namespace S22.Xmpp
{
    public class MethodCall
    {
        private readonly XmlElement element;

        private readonly ParamsList paramsList;

        public ParamsList ParamsList
        {
            get
            {
                return paramsList;
            }
        }

        private string getTextField(string name)
        {
            if (element[name] != null)
            {
                return element[name].InnerText;
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

        /// <summary>
        /// Gets or sets the name of the method to call.
        /// </summary>
        /// <value>The name of the method.</value>
        public string MethodName {
            get
            {
                return getTextField("methodName");
            }
            set
            {
                setTextField("methodName", value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="S22.Xmpp.MethodCall"/> class.
        /// </summary>
        /// <param name="element">Element.</param>
        public MethodCall(XmlElement element)
        {
            element.ThrowIfNull("element");
            this.element = element;
            paramsList = new ParamsList(element["params"]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="S22.Xmpp.MethodCall"/> class.
        /// </summary>
        public MethodCall()
        {
            element = Xml.Element("methodCall", null);
            element.Child(Xml.Element("params"));
            paramsList = new ParamsList(element["params"]);
        }

        /// <summary>
        /// Returns class a an xml element.
        /// </summary>
        /// <returns>The xml element.</returns>
        public XmlElement ToXmlElement() {
            return element;
        }

        /// <summary>
        /// Adds a parameter to the request.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        public void AddParam(object value)
        {
            paramsList.AddParam(value);
        }
    }
}

