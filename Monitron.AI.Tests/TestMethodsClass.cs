﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Monitron.Common;

namespace Monitron.AI.Tests
{
    class TestMethodsClass
    {
        [RemoteCommand(MethodName = "echo")]
        public static string Echo(Identity i_Buddy, State i_State)
        {
            string input = i_State.getKey("echo::text") ?? "";
            return input;
        }

        //[RemoteCommand(MethodName = "get_five")]
        public static string GetFive(Identity i_Buddy, State i_State)
        {
            return "5";
        }

        [RemoteCommand(MethodName = "add")]
        public static string Add(Identity i_Buddy, State i_State)
        {
            string i_First = i_State.getKey("add::first") ?? "";
            string i_Seconds = i_State.getKey("add::second") ?? "";
            string result = "";
            int first, second;
            if (int.TryParse(i_First, out first) && int.TryParse(i_Seconds, out second))
            {
                result = (first + second).ToString();
            }

            return result;
        }

        [RemoteCommand(MethodName = "movieDetailsByName")]
        public string MovieDetailsByName(Identity i_Buddy, State i_State)
        {
            string name = i_State.getKey("name");
            string query = "http://www.omdbapi.com/?t=" + name + "&y=&plot=short&r=xml";
            string responseFromServer = sendWebRequest(query);
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



        [RemoteCommand(MethodName = "imageSearch")]
        public string findPic(Identity i_Buddy, State i_State)
        {
            string result = "";
            string subject = i_State.getKey("imagesearch::subject");
            string query = string.Format(
                "https://pixabay.com/en/photos/?q={0}&image_type=&cat=&min_width=&min_height=",
                subject);
            string responseFromServer = sendWebRequest(query);
            //XDocument doc = XDocument.Parse(responseFromServer);
            string regexPattern = "(srcset=\")([/0-9a-zA-Z_.\\-]*)";
            //string regexPattern = "(data\\-url=\")([/ 0-9 a-z A-Z_.\\-]*)";
            Regex rgx = new Regex(regexPattern);
            Match match = rgx.Match(responseFromServer);
            if (match.Success)
            {
                string pictureLink = string.Format("https://pixabay.com/{0}", match.Groups[2].Value);
                result = pictureLink;
            }
            else
            {
                result = string.Format("Couldn't find pictues for {0}", subject);
            }

            return result;
        }



        private string sendWebRequest(string i_Url)
        {
            // Create a request using a URL that can receive a post. 
            WebRequest request = WebRequest.Create(i_Url);
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
            return responseFromServer;
        }
    }
}
