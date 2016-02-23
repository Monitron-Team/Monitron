using System;

namespace Monitron.Common
{
    public sealed class Account
    {
        public Identity Identity { get; private set; }
        public string Password { get; set; }
        
        public Account(string i_UserName, string i_Password, string i_Host)
        {
            Identity = new Identity {
                UserName = i_UserName,
                Host = i_Host
            };
            Password = i_Password;
        }

        public Account clone()
        {
            return (Account) this.MemberwiseClone();
        }
    }
}


