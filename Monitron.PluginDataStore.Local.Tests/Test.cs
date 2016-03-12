using System.Runtime.Serialization;
using System.IO;

using NUnit.Framework;

using Monitron.Common;
using Monitron.PluginDataStore;

namespace Monitron.PluginDataStore.Local.Tests
{
    [TestFixture()]
    public class Test
    {
        // Who ever runs these tests - please choose the path for the DB file:
        private readonly string k_Path = Path.GetTempPath() + "/test.dict";
        private IPluginDataStore m_LocalDataStore;

        [SetUp()]
        public void SetUp()
        {
            m_LocalDataStore = new LocalPluginDataStore(k_Path);
        }

        [TearDown()]
        public void TearDown()
        {
            File.Delete(k_Path);
        }

        [Test()]
        public void Write()
        {
            Person person1 = new Person("Bob", 1845, false);
            Person person2 = new Person("Steve", 2016, true);
            Person person3 = new Person("Katy Perry", 1984, true);
            m_LocalDataStore.Write<Person>("key #1", person1);
            m_LocalDataStore.Write<Person>("key #2", person2);
            m_LocalDataStore.Write<Person>("key #3", person3);
        }

        [Test()]
        public void Read()
        {
            Person person3 = new Person("Katy Perry", 1984, true);
            m_LocalDataStore.Write<Person>("key #3", person3);
            Person katy = m_LocalDataStore.Read<Person>("key #3");
            Assert.AreEqual(katy.Name, "Katy Perry");
            Assert.AreEqual(katy.BirthYear, 1984);
            Assert.AreEqual(katy.IsAlive, true);
        }

        [Test()]
        public void Delete()
        {
            string keyToDelete = "key #1";
            m_LocalDataStore.Delete(keyToDelete);
            Person deletedPerson = m_LocalDataStore.Read<Person>(keyToDelete);
            Assert.IsNull(deletedPerson);
        }

        [DataContract]
        public class Person
        {
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public int BirthYear { get; set; }
            [DataMember]
            public bool IsAlive { get; set; }

            public Person(string i_Name, int i_BirthYear, bool i_IsAlive)
            {
                Name = i_Name;
                BirthYear = i_BirthYear;
                IsAlive = i_IsAlive;
            }
        }
    }
}
