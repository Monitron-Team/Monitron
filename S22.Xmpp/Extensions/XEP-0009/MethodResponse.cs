using System;
using System.Xml;

namespace S22.Xmpp
{
    /// <summary>
    /// Method response.
    /// </summary>
    public class MethodResponse
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


        /// <summary>
        /// Initializes a new instance of the <see cref="S22.Xmpp.MethodResponse"/> class.
        /// </summary>
        /// <param name="element">Element.</param>
        public MethodResponse(XmlElement element)
        {
            element.ThrowIfNull("element");
            this.element = element;
            paramsList = new ParamsList(element["params"]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="S22.Xmpp.MethodResponse"/> class.
        /// </summary>
        public MethodResponse()
        {
            element = Xml.Element("methodResponse", null);
            element.Child(Xml.Element("params"));
            paramsList = new ParamsList(element["params"]);
        }

        /// <summary>
        /// Adds a parameter to the request.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        public void AddParam(object value)
        {
            paramsList.AddParam(value);
        }

        /// <summary>
        /// Returns class a an xml element.
        /// </summary>
        /// <returns>The xml element.</returns>
        public XmlElement ToXmlElement() {
            return element;
        }
    }
}

