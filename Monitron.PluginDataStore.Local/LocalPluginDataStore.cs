using System;
using System.Linq;
using Monitron.Common;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;

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
            T obj = default(T);
            string data;
            m_DataStoreDictionary.TryGetValue(i_Key, out data);
            if (data != null)
            {
                obj = JsonConvert.DeserializeObject<T>(data);
            }

            return obj;
        }

        public void Write<T>(string i_Key, T i_Value)
        {
            
            m_DataStoreDictionary.Add(i_Key, JsonConvert.SerializeObject(i_Value));
            writeDictionaryToFile();
        }

        private void readFileToDictionary()
        {
            if (!File.Exists(r_DataStoreFilePath))
            {
                m_DataStoreDictionary = new Dictionary<string, string>();
                writeDictionaryToFile();
            }
            else
            {
                m_DataStoreDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    File.ReadAllText(r_DataStoreFilePath, Encoding.UTF8)
                );
            }
        }

        private void writeDictionaryToFile()
        {
            string data = JsonConvert.SerializeObject(m_DataStoreDictionary);
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                using (FileStream file = new FileStream(r_DataStoreFilePath, FileMode.Create, FileAccess.Write))
                {
                    ms.WriteTo(file);
                }
            }
        }
    }
}

