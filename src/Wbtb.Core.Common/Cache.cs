using System;
using System.IO;
using System.Reflection;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// Writes/ reads things from disk cache. Thread-safe because cross thread collisions are common under load.
    /// </summary>
    public class Cache
    {
        private static object Lock = new object();

        private readonly Configuration _config;

        private readonly Logger _logger;

        public Cache(Configuration config, Logger logger) 
        {
            _config = config;
            _logger = logger;
        }

        public void Write(IPlugin source, Job job, Build build, string index, string data)
        {
            Write(source.ContextPluginConfig.Manifest.Key, job, build, index, data);
        }

        public void Clear(string pluginTypeName, Job job, Build build, string index) 
        {
            string itemPath = Path.Combine(_config.PluginDataPersistDirectory, pluginTypeName, job.Key, build.UniquePublicKey, "__cache", index.Substring(0, 1), index.Substring(1, 1));
            
            try
            {
                if (File.Exists(itemPath))
                {
                    File.Delete(itemPath);
                    _logger.Status(this, $"Removed cached file {itemPath}");
                }
                else
                {
                    _logger.Status(this, $"Cached file not found, skipping ({itemPath}).");
                }

            }
            catch (Exception ex)
            {
                _logger.Error(this, ex);
            }
        }

        /// <summary>
        /// Clears all build data for plugin
        /// </summary>
        /// <param name="pluginTypeName"></param>
        /// <param name="job"></param>
        /// <param name="build"></param>
        public void Clear(string pluginTypeName, Job job, Build build)
        {
            string itemPath = Path.Combine(_config.PluginDataPersistDirectory, pluginTypeName, job.Key, build.UniquePublicKey);

            try
            {
                if (Directory.Exists(itemPath))
                {
                    Directory.Delete(itemPath, true);
                    _logger.Status(this, $"Removed cache directory for build {itemPath}");
                }
                else 
                {
                    _logger.Status(this, $"Cache directory for build not found, skipping ({itemPath})");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(this, ex);
            }
        }

        public void Clear(string pluginTypeName, Job job)
        {
            string itemPath = Path.Combine(_config.PluginDataPersistDirectory, pluginTypeName, job.Key);

            try
            {
                if (Directory.Exists(itemPath))
                {
                    Directory.Delete(itemPath, true);
                    _logger.Status(this, $"Removed cache directory for job {itemPath}");
                }
                else
                {
                    _logger.Status(this, $"Cache directory for job not found, skipping ({itemPath})");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(this, ex);
            }
        }

        public void Write(string pluginTypeName, Job job, Build build, string index, string data)
        {
            if (index.Length < 2)
                throw new Exception("index must be at least 2 characters long");

            string writePath = Path.Combine(_config.PluginDataPersistDirectory, pluginTypeName, job.Key, build.UniquePublicKey, "__cache", index.Substring(0, 1), index.Substring(1, 1));
            
            lock(Lock)
            {
                Directory.CreateDirectory(writePath);
                File.WriteAllText(Path.Combine(writePath, index), data);
            }
        }

        public void Write(string pluginTypeName, string index, string data)
        {
            if (index.Length < 2)
                throw new Exception("index must be at least 2 characters long");

            string writePath = Path.Combine(_config.PluginDataPersistDirectory, pluginTypeName, "__cache", index.Substring(0, 1), index.Substring(1, 1));

            lock (Lock)
            {
                Directory.CreateDirectory(writePath);
                File.WriteAllText(Path.Combine(writePath, index), data);
            }
        }

        public CachePayload Get(IPlugin plugin, Job job, Build build, string index)
        {
            return Get(plugin.ContextPluginConfig.Key, job, build, index);
        }

        public CachePayload Get(string pluginTypeName, string index)
        {
            if (index.Length < 2)
                throw new Exception("index must be at least 2 characters long");

            string cachePath = Path.Combine(_config.PluginDataPersistDirectory, pluginTypeName, "__cache", index.Substring(0, 1), index.Substring(1, 1), index);

            lock (Lock)
            {
                if (!File.Exists(cachePath))
                    return new CachePayload();

                return new CachePayload
                {
                    Payload = File.ReadAllText(cachePath),
                    Key = cachePath
                };
            }
        }

        public CachePayload Get(string pluginTypeName, Job job, Build build, string index) 
        {
            if (index.Length < 2)
                throw new Exception("index must be at least 2 characters long");

            string cachePath = Path.Combine(_config.PluginDataPersistDirectory, pluginTypeName, job.Key, build.UniquePublicKey, "__cache", index.Substring(0, 1), index.Substring(1, 1), index);

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
