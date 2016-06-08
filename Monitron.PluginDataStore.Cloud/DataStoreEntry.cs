using System;
using MongoDB.Bson;

namespace Monitron.PluginDataStore.Cloud
{
    internal class DataStoreEntry<T>
    {
        public ObjectId _id;
        public string Key;
        public T Value;
    }
}

