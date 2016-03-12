using System;
using Monitron.Common;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Monitron.PluginDataStore.Local
{
    public class LocalPluginDataStore : IPluginDataStore
    {
        private readonly string r_DataStoreFilePath;
        private Dictionary<string, string> m_DataStoreDictionary;

        public LocalPluginDataStore(string i_DataStoreFilePath)
        {
            r_DataStoreFilePath = i_DataStoreFilePath;
            m_DataStoreDictionary = new Dictionary<string, string>();
            readFileToDictionary();
        }

        public void Delete(string i_Key)
        {
            try
            {
                m_DataStoreDictionary.Remove(i_Key);
                writeDictionaryToFile();
            }
            catch (Exception)
            {
                // possible key is null.
            }
        }

        public T Read<T>(string i_Key)
        {
            T valueT = default(T);
            String valueString = string.Empty;
            bool keyFound = isKeyExists(i_Key);

            if (keyFound == true)
            {
                m_DataStoreDictionary.TryGetValue(i_Key, out valueString);
                valueT = deserialization<T>(valueString);
            }
            else
            {
                // key was not found in the dictionary.
                //  returning a default (null?) T object 
            }

            return valueT;
        }

        public void Write<T>(string i_Key, T i_Value)
        {
            bool keyFound = isKeyExists(i_Key);
            string stringValue = serialization<T>(i_Value);

            if (keyFound == true)
            {
                m_DataStoreDictionary[i_Key] = stringValue;
            }
            else
            {
                m_DataStoreDictionary.Add(i_Key, stringValue);
            }

            writeDictionaryToFile();
        }

        private void readFileToDictionary()
        {
            if (!File.Exists(r_DataStoreFilePath))
            {
                m_DataStoreDictionary = new Dictionary<string, string>();
            }
            else
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(m_DataStoreDictionary.GetType(),
                                                 new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
                using (FileStream ms = new FileStream(r_DataStoreFilePath, FileMode.Open))
                {
                    m_DataStoreDictionary = (Dictionary<string, string>)ser.ReadObject(ms);
                }
            }
        }

        private void writeDictionaryToFile()
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(m_DataStoreDictionary.GetType(),
                    new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
            using (MemoryStream ms = new MemoryStream())
            {
                ser.WriteObject(ms, m_DataStoreDictionary);
                using (FileStream file = new FileStream(r_DataStoreFilePath, FileMode.Create, FileAccess.Write))
                {
                    ms.WriteTo(file);
                }
            }
        }

        private T deserialization<T>(string i_ValueString)
        {
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
            T deserializedValue = default(T);
            using (MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(i_ValueString)))
            {
                deserializedValue = (T)js.ReadObject(ms); //TBD if the object types are not the same - exception
            }
            return deserializedValue;
        }

        private string serialization<T>(T i_Value)
        {
            string stringValue = string.Empty;
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                js.WriteObject(ms, i_Value);
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                {
                    stringValue = sr.ReadToEnd();
                }
            }
            return stringValue;
        }

        private bool isKeyExists(string i_Key)
        {
            bool keyFound = false;
            try
            {
                keyFound = m_DataStoreDictionary.ContainsKey(i_Key);
            }
            catch (ArgumentNullException)
            {
                // possible key is null.
            }
            return keyFound;
        }
    }
}
