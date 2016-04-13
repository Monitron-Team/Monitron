using System;

namespace S22.Xmpp.Tests
{
    public struct TestStruct
    {
        public int IntField;
        public double DoubleField;
        public string StringField;

        public override bool Equals(object obj)
        {
            TestStruct other = (TestStruct)obj;
            return IntField == other.IntField
            && DoubleField == other.DoubleField
            && StringField == other.StringField;
        }

        public override int GetHashCode()
        {
            return IntField;
        }
    }
}

