using System;

namespace SimpleDebug.Management
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Monitron.Node.Program.setUpLogging();
            Monitron.Node.Program.runNode("NodeConf.xml");
        }
    }
}
