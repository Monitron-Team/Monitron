using System.Globalization;

namespace Monitron.ImRpc.Tests
{
    public class TestClass
    {
        [RemoteCommand(MethodName="echo")]
        public string Echo(string i_text)
        {
            return i_text;
        }

        [RemoteCommand(MethodName="convert_echo")]
        public string Echo(ConvertableType i_Convertable)
        {
            return string.Format("{0};{1}", i_Convertable.String, i_Convertable.Number);
        }

        [RemoteCommand(MethodName="complex_cmd")]
        public string ComplexCmd(string i_A, int i_B, double i_C, string i_D)
        {
            return string.Format("{0} {1} {2} \"{3}\"", i_A, i_B, i_C, i_D);
        }
    }
}