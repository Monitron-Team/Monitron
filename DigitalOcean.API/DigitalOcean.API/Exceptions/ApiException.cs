using System;
using System.Collections.Generic;
using System.Net;

using RestSharp;

namespace DigitalOcean.API.Exceptions {
    public class ApiException : Exception {
        private string status;

        private string message;

        public HttpStatusCode StatusCode { get; private set; }

        public string Status {
            get { return status; }
        }

        public override string Message {
            get { return message; }
        }

        public ApiException(HttpStatusCode statusCode, IRestResponse response) {
            StatusCode = statusCode;
            status = response.StatusDescription;
            Dictionary<string, string> content = SimpleJson.DeserializeObject<Dictionary<string, string>>(response.Content);
            if (content.ContainsKey("message"))
            {
                message = string.Format("{0}: {1}", status, content["message"]);
            }
            else
            {
                message = status;
            }
        }
    }
}