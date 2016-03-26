using System.Globalization;
using Monitron.Common;

namespace Monitron.ImRpc.Tests
{
    public class TestClass
    {
        [RemoteCommand(MethodName="echo")]
        public string Echo(
            Identity i_Buddy,
            [Opt("text", "t|text=", "{TEXT} to print")] string i_text)
        {
            return i_text;
        }

        [RemoteCommand(MethodName="convert_echo")]
        public string Echo(
            Identity i_Buddy,
            [Opt("convertable", "c|conv=", "{CONV} to convert")] ConvertableType i_Convertable)
        {
            return string.Format("{0};{1}", i_Convertable.String, i_Convertable.Number);
        }

        [RemoteCommand(MethodName="complex_cmd")]
        public string ComplexCmd(
            Identity i_Buddy,
            [Opt("a", "a", "A")] string i_A,
            [Opt("b", "b", "B")] int i_B,
            [Opt("c", "c", "C")] double i_C,
            [Opt("d", "d", "d")] string i_D)
        {
            return string.Format("{0} {1} {2} \"{3}\"", i_A, i_B, i_C, i_D);
        }
    }
}