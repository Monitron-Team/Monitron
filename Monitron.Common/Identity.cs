using System;

namespace Monitron.Common
{
    public struct Identity
    {
        public string UserName { get; set; }
        public string Host { get; set; }

        public override bool Equals(object i_Obj)
        {
            Identity other = (Identity)i_Obj;
            return this.UserName == other.UserName &&
                this.Host == other.Host;
        }

        public override int GetHashCode()
        {
            return (UserName + "@" + Host).GetHashCode();
        }
    }
}

