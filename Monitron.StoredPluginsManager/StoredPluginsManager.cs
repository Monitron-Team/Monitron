using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ICSharpCode.SharpZipLib.Tar;
using System.IO;
using MongoDB.Bson;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monitron
{
    public class StoredPluginsManager
    {
        private readonly IMongoDatabase r_Database;

        private const string k_BucketName = "plugin_store";
        
        public StoredPluginsManager(IMongoDatabase i_Database)
        {
            r_Database = i_Database;
        }

        private PluginManifest readManifest(string i_PluginFile)
        {
            using (var fs = new FileStream(i_PluginFile, FileMode.Open, FileAccess.Read))
            {
                TarInputStream archive = new TarInputStream(fs);
                TarEntry entry;
                TarEntry manifestEntry = null;
                while ((entry = archive.GetNextEntry()) != null)
                {
                    if (!entry.IsDirectory && entry.Name.EndsWith("MANIFEST.xml"))
                    {
                        manifestEntry = entry;
                        break;
                    }
                }
                if (manifestEntry == null)
                {
                    throw new FileNotFoundException("Could not find plugin manifest");
                }

                XmlSerializer serializer = new XmlSerializer(typeof(PluginManifest));
                return (PluginManifest)serializer.Deserialize(archive);
            }
        }

        private IMongoDatabase getDatabase()
        {
            return r_Database;
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

        public Stream OpenPluginDownloadStream(string i_PluginId)
        {
            GridFSBucket bucket = getBucket();
            var stream = bucket.OpenDownloadStreamByName(i_PluginId);
            return stream;
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

        public async Task<string> GetPluginIdAsync(ObjectId id) {
            GridFSBucket bucket = getBucket();
            Task<GridFSFileInfo> metadata = await bucket
                .FindAsync(Builders<GridFSFileInfo>.Filter.Eq("_id", id))
                .ContinueWith((arg) => arg.Result.FirstAsync());
            return metadata.Result.Metadata.GetValue("id").AsString;
        }

        public PluginManifest GetManifest(string i_PluginId)
        {
            GridFSBucket bucket = getBucket();
            BsonDocument metadata = bucket
                .Find(Builders<GridFSFileInfo>.Filter.Eq("filename", i_PluginId))
                .First()
                .Metadata;

            return new PluginManifest
            {
                Description = metadata["description"].AsString,
                DllName = metadata["dll_name"].AsString,
                Id = metadata["id"].AsString,
                Name = metadata["name"].AsString,
                @Type = metadata["type"].AsString,
                Version = metadata["version"].AsString,
                DllPath = metadata["dll_path"].AsString,
            };
        }

        public async Task<PluginManifest> UploadPluginAsync(string i_PluginFilePath)
        {
            PluginManifest manifest = readManifest(i_PluginFilePath);

            using (var fs = new FileStream(i_PluginFilePath, FileMode.Open, FileAccess.Read))
            {
                GridFSBucket bucket = getBucket();
                await RemovePluginAsync(manifest.Id);
                fs.Seek(0, SeekOrigin.Begin);
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
                                { "dll_name", manifest.DllName },
                                { "dll_path", manifest.DllPath },
                                { "type", manifest.Type },
                            },
                    }
                );

                return manifest;
            }
        }
    }
}

