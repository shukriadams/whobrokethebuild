using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wbtb.Core.CLI
{
    public class FileSystemHelper
    {
        /// <summary>
        /// Returns a list of all files nested under a given directory.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetFilesUnder(string directory) 
        {
            IEnumerable<string> thisFiles = new List<string>();

            foreach (string childDir in Directory.GetDirectories(directory))
                thisFiles = thisFiles.Concat(GetFilesUnder(childDir));

            thisFiles = thisFiles.Concat(Directory.GetFiles(directory));
            return thisFiles;
        }
    }
}
