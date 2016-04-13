using System;
using System.Threading.Tasks;

namespace S22.Xmpp.Tests
{
    public class RpcImplementation  : IRpcTestInterface
    {
        public string echo(string argument)
        {
            return argument;
        }

        public string concat(params string[] args)
        {
            return string.Concat(args);
        }

        public int sum(int a, int b)
        {
            return a + b;
        }

        public bool isTrue(bool value)
        {
            return value;
        }

        public double div(double a, double b)
        {
            return a / b;
        }

        public TestStruct echoStruct(TestStruct input)
        {
            return input;
        }

        public void doNothing()
        {
        }
    }
}

