using System;
using System.Linq;
using System.Xml;
using System.Reflection;

namespace S22.Xmpp
{
    public class ParamsList
    {
        private readonly XmlElement element;

        public ParamsList(XmlElement element)
        {
            this.element = element;
        }

        public ParamsList()
        {
            element = Xml.Element("params");
        }

        /// <summary>
        /// Adds a parameter to the request.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        public void AddParam(object value)
        {
            AddParamElement(createValueElement(value));
        }

        private void AddParamElement(XmlElement value)
        {
            element.Child(Xml.Element("param").Child(Xml.Element("value").Child(value)));
        }

        private static XmlElement createValueElement(object value)
        {
            Type valueType = value.GetType();
            if (valueType == typeof(string))
            {
                return Xml.Element("string").Text(value.ToString());
            }

            if (valueType == typeof(int))
            {
                return Xml.Element("int").Text(value.ToString());
            }

            if (valueType == typeof(double))
            {
                return Xml.Element("double").Text(value.ToString());
            }

            if (valueType == typeof(bool))
            {
                return Xml.Element("boolean").Text(value.ToString());
            }

            if (valueType == typeof(DateTime))
            {
                return Xml.Element("dateTime.iso8601").Text(
                    ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz")
                );
            }

            if (valueType.IsArray)
            {
                var arr = (Array) value;
                XmlElement arrayElement = Xml.Element("array").Child(Xml.Element("data"));
                XmlElement dataElement = arrayElement["data"];
                foreach (object item in arr)
                {
                    dataElement.Child(Xml.Element("value").Child(createValueElement(item)));
                }

                return arrayElement;
            }

            XmlElement structElement = Xml.Element("struct").Child(Xml.Element("members"));
            XmlElement membersElement = structElement["members"];
            foreach (var fieldInfo in value.GetType().GetFields())
            {
                membersElement.Child(
                    Xml.Element("member").Child(
                        Xml.Element("name").Text(fieldInfo.Name)
                    ).Child(
                        Xml.Element("value").Child(createValueElement(fieldInfo.GetValue(value)))
                    )
                );
            }

            return structElement;
        }


        private static object deserializeParameter(XmlElement valueElement, Type expectedType)
        {
            if (valueElement["int"] != null)
            {
                return int.Parse(valueElement["int"].InnerText);
            }

            if (valueElement["i4"] != null)
            {
                return int.Parse(valueElement["i4"].InnerText);
            }

            if (valueElement["string"] != null)
            {
                return valueElement["string"].InnerText;
            }

            if (valueElement["double"] != null)
            {
                return double.Parse(valueElement["double"].InnerText);
            }

            if (valueElement["boolean"] != null)
            {
                return bool.Parse(valueElement["boolean"].InnerText);
            }

            if (valueElement["dateTime.iso8601"] != null)
            {
                //TODO
                return DateTime.Parse(valueElement["dateTime.iso8601"].InnerText);
            }

            if (valueElement["struct"] != null)
            {
                object obj = Activator.CreateInstance(expectedType);
                foreach (var member in valueElement["struct"]["members"].ChildNodes.Cast<XmlElement>())
                {
                    var fieldInfo = expectedType.GetField(member["name"].InnerText);
                    fieldInfo.SetValue(obj, deserializeParameter(member["value"], fieldInfo.FieldType));
                }

                return obj;
            }

            if (valueElement["array"] != null)
            {
                Array array = Array.CreateInstance(
                    expectedType.GetElementType(),
                    valueElement["array"]["data"].ChildNodes.Count
                );

                for (int i = 0; i < array.Length; i++)
                {
                    array.SetValue(deserializeParameter(
                        (XmlElement) valueElement["array"]["data"].ChildNodes[i],
                        expectedType.GetElementType()),
                        i
                    );
                }

                return array;
            }

            return null;
        }

        public object[] GetArgsForMethod(MethodInfo methodInfo)
        {
            Type[] parameterTypes = methodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
            object[] parameters = new object[parameterTypes.Length];
            int i = 0;
            foreach (var param in element.ChildNodes.Cast<XmlElement>())
            {
                parameters[i] = deserializeParameter(param["value"], parameterTypes[i]);
                i++;
            }

            return parameters;
        }

        public T GetResultForReturnType<T>()
        {
            return (T) deserializeParameter(element.FirstChild["value"], typeof(T));
        }
    }
}

