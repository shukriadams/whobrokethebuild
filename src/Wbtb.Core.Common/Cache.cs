using System;
using System.IO;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Writes/ reads things from disk cache. Thread-safe because cross thread collisions are common under load.
    /// </summary>
    public class Cache
    {
        private static object Lock = new object();
        
        public void Write(IPlugin source, string index, string data)
        {
            Write(source.ContextPluginConfig.Manifest.Key, index, data);
        }

        public void Write(string source, string index, string data)
        {
            if (index.Length < 2)
                throw new Exception("index must be at least 2 characters long");

            SimpleDI di = new SimpleDI();
            Configuration config = di.Resolve<Configuration>();
            string writePath = Path.Combine(config.PluginDataPersistDirectory, source, "__cache", index.Substring(0, 1), index.Substring(1, 1));
            
            lock(Lock)
            {
                Directory.CreateDirectory(writePath);
                File.WriteAllText(Path.Combine(writePath, index), data);
            }
        }

        public CachePayload Get(IPlugin plugin, string index)
        {
            return Get(plugin.ContextPluginConfig.Manifest.Key, index);
        }

        public CachePayload Get(string source, string index) 
        {
            if (index.Length < 2)
                throw new Exception("index must be at least 2 characters long");

            SimpleDI di = new SimpleDI();
            Configuration config = di.Resolve<Configuration>();
            string cachePath = Path.Combine(config.PluginDataPersistDirectory, source, "__cache", index.Substring(0, 1), index.Substring(1, 1), index);

            lock (Lock)
            {
                if (!File.Exists(cachePath))
                    return new CachePayload();

                return new CachePayload {
                    Payload = File.ReadAllText(cachePath),
                    Key = cachePath
                };
            }
        }
    }
}
