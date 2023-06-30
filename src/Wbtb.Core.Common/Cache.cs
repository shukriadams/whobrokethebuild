using System;
using System.IO;

namespace Wbtb.Core.Common
{
    public class Cache
    {
        public void Write(string index, string data)
        {
            if (index.Length < 2)
                throw new Exception("index must be at least 2 characters long");

            SimpleDI di = new SimpleDI();
            Configuration config = di.Resolve<Configuration>();
            string writePath = Path.Combine(config.CacheDirectory, index.Substring(0, 1), index.Substring(1, 1));
            Directory.CreateDirectory(writePath);
            File.WriteAllText(Path.Combine(writePath, index), data);
        }

        public string Get(string index) 
        {
            if (index.Length < 2)
                throw new Exception("index must be at least 2 characters long");

            SimpleDI di = new SimpleDI();
            Configuration config = di.Resolve<Configuration>();
            string cachePath = Path.Combine(config.CacheDirectory, index.Substring(0, 1), index.Substring(1, 1), index);
            if (!File.Exists(cachePath))
                return null;

            return File.ReadAllText(cachePath);

        }
    }
}
