using System;

using Monitron.Common;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Monitron.PluginDataStore.Cloud
{
    public class CloudPluginDataStore : IPluginDataStore
    {
        private MongoClient r_Client;
        private readonly MongoUrl r_Url;
        
        public CloudPluginDataStore(string i_Location)
        {
            r_Url = MongoUrl.Create(i_Location);
            r_Client = new MongoClient(r_Url);
        }

        private IMongoDatabase GetDatabase()
        {
            return r_Client.GetDatabase(r_Url.DatabaseName);
        }

        private IMongoCollection<DataStoreEntry<T>> GetDataStore<T>()
        {
            IMongoDatabase db = GetDatabase();
            return db.GetCollection<DataStoreEntry<T>>(r_Url.Username + "_data_store");
        }

        public void Write<T>(string i_Key, T i_Value)
        {
            Delete(i_Key);
            GetDataStore<T>()
                .InsertOne(new DataStoreEntry<T>
                    {
                        Key = i_Key,
                        Value = i_Value,
                    }
                );
        }

        public T Read<T>(string i_Key)
        {
            return GetDataStore<T>()
                .Find(Builders<DataStoreEntry<T>>.Filter.Eq("Key", i_Key))
                .First()
                .Value;
        }

        public void Delete(string i_Key)
        {
            IMongoDatabase db = GetDatabase();
            db.GetCollection<BsonDocument>(r_Url.Username + "_data_store")
                .DeleteMany(Builders<BsonDocument>.Filter.Eq("Key", i_Key));
        }
    }
}

