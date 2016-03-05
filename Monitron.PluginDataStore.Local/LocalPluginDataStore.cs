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
        private string dataStoreFilePath;
        private Dictionary<string, string> dataStoreDictionary;

        public LocalPluginDataStore(string i_DataStoreFilePath)
        {
            dataStoreFilePath = i_DataStoreFilePath;
            dataStoreDictionary = new Dictionary<string, string>();
            readFileToDictionary();
        }

        public void Delete(string i_Key)
        {
            try
            {
                dataStoreDictionary.Remove(i_Key);
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
                dataStoreDictionary.TryGetValue(i_Key, out valueString);
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
                dataStoreDictionary[i_Key] = stringValue;
            }
            else
            {
                dataStoreDictionary.Add(i_Key, stringValue);
            }

            writeDictionaryToFile();
        }

        private void readFileToDictionary()
        {
            using(StreamReader sr = new StreamReader(dataStoreFilePath, Encoding.UTF8))
            {
                string fileContentString = sr.ReadToEnd(); // TBD what if the file is very large?
                DataContractJsonSerializer ser = new DataContractJsonSerializer(dataStoreDictionary.GetType(),
                    new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(fileContentString)))
                {
                    dataStoreDictionary = (Dictionary<string, string>)ser.ReadObject(ms); //TBD couldn't use 'dataStoreDictionary.GetType()'
                }
            }
        }

        private void writeDictionaryToFile()
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(dataStoreDictionary.GetType(),
                    new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
            using (MemoryStream ms = new MemoryStream())
            {
                ser.WriteObject(ms, dataStoreDictionary);
                using (FileStream file = new FileStream(dataStoreFilePath, FileMode.Create, FileAccess.Write))
                {
                    ms.WriteTo(file);
                }
            }
        }

        private T deserialization<T>(string i_ValueString)
        {
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
            T deserializedValue = default(T);
            using (MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(valueString)))
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
                keyFound = dataStoreDictionary.ContainsKey(i_Key);
            }
            catch (ArgumentNullException)
            {
                // possible key is null.
            }
            return keyFound;
        }
    }
}
