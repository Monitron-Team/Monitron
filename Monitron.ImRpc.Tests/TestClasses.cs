using System.Globalization;

namespace Monitron.ImRpc.Tests
{
    public class TestClass
    {
        int m_Num;
        string m_Text;

        [RemoteCommand(m_MethodName = "print_details")]
        public string Printdetails(string i_FullName)
        {
            return "Age is: " + this.m_Num + "and my address is: " + this.m_Text + "and Input name was: " + i_FullName;
        }


        [RemoteCommand(m_MethodName = "print_Numbers")]
        public string PrintNumbers(string i_FullName, int i_Int, float i_Float, double i_Double)
        {
            return "string: " + i_FullName + "int: " + i_Int.ToString() + "float: " + i_Float.ToString() + "double: " + i_Double.ToString();
        }

        [RemoteCommand(m_MethodName = "print_with_Args_Parser")]
        public string PrintwithArgsParser(string i_FullName, int i_Int, float i_Float, double i_Double, AnotherTestClass i_Atc)
        {
            return "Parsed arg number: " + i_Atc.m_Number + " parsed arg text: " + i_Atc.m_Texty + " string: " + i_FullName + "int: " + i_Int.ToString() + " float: " + i_Float.ToString(CultureInfo.InvariantCulture) + " double: " + i_Double.ToString();
        }

        public TestClass(int i_Num, string i_Text)
        {
            this.m_Num = i_Num;
            this.m_Text = i_Text;
        }

        [ArgumentParser()]
        public static TestClass TestParser(string i_Text)
        {
            return new TestClass(99, i_Text.Replace('a', '9'));
        }

        [ArgumentParser()]
        public static AnotherTestClass AnotherTestParser(string i_Text)
        {   //stupid parser that take the string and replace each a -> 9 and create an instance of 'anothertestclass'
            return new AnotherTestClass(i_Text.Replace('a', '9'), 99);
        }
    }

    public class AnotherTestClass
    {
        public string m_Texty;
        public int m_Number;

        public AnotherTestClass(string i_Text, int i_Num)
        {
            this.m_Number = i_Num;
            this.m_Texty = i_Text;
        }
    }
}