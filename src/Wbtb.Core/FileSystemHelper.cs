using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wbtb.Core
{
    public class FileSystemHelper
    {
        /// <summary>
        /// Deletes everything in a folder
        /// </summary>
        /// <param name="directory"></param>
        /// <returns>True on no errors, false if there were errors.</returns>
        public bool ClearDirectory(string directory)
        {
            DirectoryInfo dir = new DirectoryInfo(directory);
            if (!dir.Exists)
                return false;

            bool errors = false;

            foreach (FileInfo file in dir.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    errors = true;
                }
            }

            foreach (DirectoryInfo childDir in dir.GetDirectories())
            {
                try
                {
                    childDir.Delete(true);
                }
                catch
                {
                    errors = true;
                }
            }

            return errors;
        }

        /// <summary>
        /// Recursively copy directory
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="targetDir"></param>
        public void CopyDirectory(string sourceDir, string targetDir)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            DirectoryInfo[] childDirs = dir.GetDirectories();
            Directory.CreateDirectory(targetDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string filePath = Path.Combine(targetDir, file.Name);
                file.CopyTo(filePath);
            }

            foreach (DirectoryInfo childDir in childDirs)
            {
                string childDirRemapped = Path.Combine(targetDir, childDir.Name);
                CopyDirectory(childDir.FullName, childDirRemapped);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="folders"></param>
        private static void GetFoldersUnderInternal(
            string path,
            ICollection<string> folders
            )
        {
            try
            {
                if (!Directory.Exists(path))
                    return;

                folders.Add(path);

                // not empty, find folders and handle them
                DirectoryInfo dir = new DirectoryInfo(path);
                DirectoryInfo[] dirs = dir.GetDirectories();

                foreach (DirectoryInfo child in dirs)
                    GetFoldersUnderInternal(
                        child.FullName,
                        folders);
            }
            catch (UnauthorizedAccessException)
            {
                // suppress these exceptions
            }
            catch (PathTooLongException)
            {
                // suppress these, they will be hit a lot
            }
        }


        /// <summary>
        /// Returns true of directory or any child regardless of depth contains children.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool ContainsFiles(string path)
        {
            int fileCount = (from file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                             select file).Count();

            if (fileCount > 0)
                return true;

            DirectoryInfo dir = new DirectoryInfo(path);
            DirectoryInfo[] dirs = dir.GetDirectories();

            bool childrenContains = false;
            foreach (DirectoryInfo child in dirs)
            {
                if (ContainsFiles(child.FullName))
                {
                    childrenContains = true;
                    break;
                }
            }

            return childrenContains;
        }



        /// <summary>
        /// Maps an array of paths from one location to another.
        /// </summary>
        /// <returns></returns>
        public static string[] MapPaths(
            string[] sourceFiles,
            string srcRoot,
            string targetRoot
            )
        {
            // needs to remove any trailing "\" from paths
            if (srcRoot.EndsWith(Path.DirectorySeparatorChar.ToString()))
                srcRoot = srcRoot.Substring(0,
                    srcRoot.Length - Path.DirectorySeparatorChar.ToString().Length);

            if (targetRoot.EndsWith(Path.DirectorySeparatorChar.ToString()))
                targetRoot = targetRoot.Substring(0,
                    targetRoot.Length - Path.DirectorySeparatorChar.ToString().Length);

            // user lower case comparers to avoid false negatives
            string srcRootComparer = srcRoot.ToLower();

            string[] targetPaths = new string[sourceFiles.Length];

            for (int i = 0; i < sourceFiles.Length; i++)
            {
                string sourceFileComparer = sourceFiles[i].ToLower();

                // ensure that the source root on the current path is valid
                if (!sourceFileComparer.StartsWith(srcRootComparer))
                    throw new Exception($"mismatch {sourceFiles[i]}");

                targetPaths[i] = sourceFiles[i].Replace(srcRoot, targetRoot, StringComparison.CurrentCultureIgnoreCase);
            }

            return targetPaths;

        }

    }

}
