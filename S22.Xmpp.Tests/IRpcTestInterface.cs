using System;
using System.Threading.Tasks;

namespace S22.Xmpp.Tests
{
    public interface IRpcTestInterface
    {
        string echo(string argument);
        string concat(params string[] args);
        int sum(int a, int b);
        bool isTrue(bool value);
        double div(double a, double b);
        TestStruct echoStruct(TestStruct input);
        void doNothing();
    }
}

