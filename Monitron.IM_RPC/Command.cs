using System.Collections.Generic;
using System.Linq;

namespace Monitron.ImRpc
{
    public class Command
    {
        public string Name { get; set; }
        public List<string> Args { get; set; }

        //parsing the string to method name and args. Acutally I think it can be a ctor instead of a static func (according to design)
        public static Command Parse(string i_Message)
        {
            Command result = null;
            char[] stringSeparators = new char[] { ' ', '\n', '\t' };
            List<string> clean = (i_Message.Split(stringSeparators)).ToList();
            if (clean.Count > 0)
            {
                result = new Command();
                result.Name = clean.First();
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
