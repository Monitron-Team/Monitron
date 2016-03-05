using System;

namespace Monitron.Management.AdminClient
{
    public class AdminClientException : Exception
    {
        public AdminClientException(string i_Message) : base(i_Message)
        {
        }
    }
}

