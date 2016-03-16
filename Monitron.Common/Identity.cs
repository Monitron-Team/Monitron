using System;

namespace Monitron.Common
{
    public struct Identity
    {
        public string UserName { get; set; }
        public string Domain { get; set; }
        public string Resource { get; set; }

        public override bool Equals(object i_Obj)
        {
            try
            {
                Identity other = (Identity)i_Obj;
                return this.UserName == other.UserName &&
                    this.Domain == other.Domain;
            }
            catch
            {
                return false;
            }

        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}@{1}/{2}", UserName, Domain, Resource);
        }
    }
}

