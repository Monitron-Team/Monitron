using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using MongoDB.Bson;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monitron.Plugins.Management
{
    public class StoredPluginsManager
    {
        private readonly MongoClient r_Client;

        private readonly string r_DatabaseName;

        private const string k_BucketName = "plugin_store";
        
        public StoredPluginsManager(MongoClientSettings i_MongoSettings, string i_DatabseName)
        {
            r_Client = new MongoClient(i_MongoSettings);
            r_DatabaseName = i_DatabseName;
        }

        private PluginManifest readManifest(ZipFile i_PluginFile)
        {
            ZipEntry manifestEntry = null;
            foreach (ZipEntry entry in i_PluginFile)
            {
                if (entry.IsFile && entry.Name.EndsWith("/MANIFEST.xml"))
                {
                    manifestEntry = entry;
                    break;
                }
            }

            if (manifestEntry == null)
            {
                throw new FileNotFoundException("Could not find plugin manifest");
            }

            using (Stream manifest_file = i_PluginFile.GetInputStream(manifestEntry))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PluginManifest));
                return (PluginManifest) serializer.Deserialize(manifest_file);
            }
        }

        private IMongoDatabase getDatabase()
        {
            return r_Client.GetDatabase(r_DatabaseName);
        }

        private GridFSBucket getBucket()
        {
            return new GridFSBucket(
                getDatabase(),
                new GridFSBucketOptions
                {
                    BucketName = k_BucketName,
                }
            );
        }

        public async Task DownloadPluginAsync(string i_PluginId, string i_TargetPath)
        {
            using (FileStream fs = new FileStream(i_TargetPath, FileMode.Create, FileAccess.Write))
            {
                GridFSBucket bucket = getBucket();
                await bucket.DownloadToStreamByNameAsync(
                    i_PluginId,
                    fs
                );
            }
        }

        public IEnumerable<PluginManifest> ListPlugins()
        {
            // FIXME: This is ugly and slow
            GridFSBucket bucket = getBucket();
            List<PluginManifest> manifests = new List<PluginManifest>();
            bucket.Find(Builders<GridFSFileInfo>.Filter.Empty).ForEachAsync((item) =>
                {
                    manifests.Add(new PluginManifest
                        {
                            Id = item.Metadata.GetValue("id").AsString,
                            Name = item.Metadata.GetValue("name").AsString,
                            Version = item.Metadata.GetValue("version").AsString,
                            Description = item.Metadata.GetValue("description").AsString,
                        });
                }).Wait();
            return manifests.AsEnumerable();
        }

        public async Task RemovePluginAsync(string i_PluginId)
        {
            GridFSBucket bucket = getBucket();
            await bucket.Find(Builders<GridFSFileInfo>.Filter.Eq("filename", i_PluginId)).ForEachAsync((item) =>
                {
                    bucket.Delete(item.Id);
                }
            );
        }

        public async Task<PluginManifest> UploadPluginAsync(string i_PluginFilePath)
        {
            ZipFile pluginFile = new ZipFile(i_PluginFilePath);
            PluginManifest manifest = readManifest(pluginFile);
            {
                GridFSBucket bucket = getBucket();
                await RemovePluginAsync(manifest.Id);
                using (FileStream fs = new FileStream(i_PluginFilePath, FileMode.Open, FileAccess.Read))
                await bucket.UploadFromStreamAsync(
                    manifest.Id,
                    fs,
                    new GridFSUploadOptions
                    {
                        Metadata = new BsonDocument
                        {
                            { "id", manifest.Id },
                            { "name", manifest.Name },
                            { "description", manifest.Description },
                            { "version", manifest.Version },
                        },
                    }
                );

                return manifest;
            }
        }
    }
}

