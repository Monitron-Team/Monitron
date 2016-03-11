using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Monitron.ImRpc
{
    internal class Command
    {
        public string Name { get; set; }
        public IList<string> Args { get; set; }

        public static Command Parse(string i_Message)
        {
            Command result = null;
            List<string> clean = i_Message.Split(null).ToList();
            if (clean.Count > 0)
            {
                result = new Command { Name = clean.First() };
                clean.RemoveAt(0);  //remove the the first (it is not an arg)
                clean.RemoveAll(i_I => i_I == "");
                result.Args = clean;
            }

            return result;
        }

        public Command()
        {
            Args = new List<string>();
        }
    }
}
