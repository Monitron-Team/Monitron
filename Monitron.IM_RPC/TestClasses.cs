namespace Monitron.ImRpc
{


    public class Testclass
    {
        int m_Num;
        string m_Text;

        [RemoteCommand(m_MethodName = "print_details")]
        public string Printdetails(string i_FullName)
        {
            return "Age is: " + this.m_Num + "and my address is: " + this.m_Text + "and Input name was: " + i_FullName;
        }


        [RemoteCommand(m_MethodName = "print_Numbers")]
        public string PrintNumbers(string i_FullName, int i_Int, float i_Float, double i_double)
        {
            return "string: " + i_FullName + "int: " + i_Int.ToString() + "float: " + i_Float.ToString() + "double: " + i_double.ToString();
        }

        [RemoteCommand(m_MethodName = "print_with_Args_Parser")]
        public string PrintwithArgsParser(string i_FullName, int i_Int, float i_Float, double i_double, Anothertestclass i_ATC)
        {
            return "Parsed arg number: " + i_ATC.m_Number + " parsed arg text: " + i_ATC.texty + " string: " + i_FullName + "int: " + i_Int.ToString() + " float: " + i_Float.ToString() + " double: " + i_double.ToString();
        }

        public Testclass(int i_Num, string i_Text)
        {
            this.m_Num = i_Num;
            this.m_Text = i_Text;
        }

        [ArgumentParser()]
        public static Testclass TestParser(string i_Text)
        {
            return new Testclass(99, i_Text.Replace('a', '9'));
        }
    }
    public class Anothertestclass
    {
        public string texty;
        public int m_Number;

        public Anothertestclass(string i_Text, int i_Num)
        {
            this.m_Number = i_Num;
            texty = i_Text;
        }

        [ArgumentParser()]
        public static Anothertestclass TestParser(string i_Text)
        {   //stupid parser that take the string and replace each a -> 9 and create an instance of 'anothertestclass'
            return new Anothertestclass(i_Text.Replace('a', '9'), 99);
        }

    }
}
