using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Monitron.Common;

namespace Monitron.AI.Tests
{
    class TestMethodsClass
    {
        [RemoteCommand(MethodName = "echo")]
        public static string echo(Identity i_Buddy, State i_state ,string i_input)
        {
            return i_input;
        }

        //[RemoteCommand(MethodName = "get_five")]
        public static string getFive(Identity i_Buddy, State i_state)
        {
            return "5";
        }

        [RemoteCommand(MethodName = "add")]
        public static string add(Identity i_Buddy, State i_state, string i_First, string i_Seconds)
        {
            string result = "";
            int first, second;
            if (int.TryParse(i_First, out first) && int.TryParse(i_Seconds, out second))
            {
                result = (first + second).ToString();
            }

            return result;
        }

        [RemoteCommand(MethodName = "movieDetailsByName")]
        public static string movieDetailsByName(Identity i_Buddy, State i_state)
        {
            string name = i_state.getKey("name");
            string query = "http://www.omdbapi.com/?t=" + name +"&y=&plot=short&r=xml";

            //todo: first check if there is an internet connection.

            // Create a request using a URL that can receive a post. 
            WebRequest request = WebRequest.Create(query);
            // Set the Method property of the request to POST.
            request.Method = "POST";
            // Create POST data and convert it to a byte array.
            string postData = "This is a test that posts this string to a Web server.";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            Console.WriteLine(responseFromServer);
            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();
            XDocument doc = XDocument.Parse(responseFromServer);
            string result = "";
            if (doc.Root.FirstAttribute.Value == "True")
            {
                XmlReader xmlReader = doc.Root.FirstNode.CreateReader();
                while (xmlReader.Read())
                {
                    string title = xmlReader.GetAttribute("title");
                    string year = xmlReader.GetAttribute("year");
                    string director = xmlReader.GetAttribute("director");
                    string genre = xmlReader.GetAttribute("genre");
                    char firstChar = genre.ToLower().First();
                    string aOrAn = (firstChar == 'a' || firstChar == 'o' || firstChar == 'i' || firstChar == 'e')
                                       ? "an"
                                       : "a";
                    result = string.Format("'{0}' is {1} {2} movie from {3}.", title, aOrAn, genre, year);
                }
            }
            else
            {
                result = "didn't find any movie titles with the name";
            }
            return result;
        }
    }
}
