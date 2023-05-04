using System;
using System.IO;
using System.Reflection;

namespace Wbtb.Core.Common
{
    public class ResourceHelper
    {
        public static string ReadResourceAsString(Assembly assembly, string resourceName)
        {
            string assemblyName = assembly.ManifestModule.Name;
            assemblyName = assemblyName.Substring(0, assemblyName.Length - 4); // clip off ".dll"
            string resourceFullName = $"{assemblyName}.{resourceName}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceFullName))
            {
                if (stream == null)
                    throw new Exception($"Failed to load resource {resourceFullName} in assembly {assembly.FullName}.");

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static string ReadResourceAsString(Type typeInTargetAssembly, string resourceName)
        {
            Assembly assembly = Assembly.GetAssembly(typeInTargetAssembly);
            if (assembly == null)
                throw new Exception($"Failed to find assemby for type {typeInTargetAssembly.FullName}");

            return ReadResourceAsString(assembly, resourceName);
        }

        public static string LoadFromLocalJsonOrLocalResourceAsString(Type callerType, string diskPath, string resourcePath)
        {
            string rawJson = null;

            if (File.Exists(diskPath))
                rawJson = File.ReadAllText(diskPath);
            else
            {
                if (!ResourceHelper.ResourceExists(callerType, resourcePath))
                {
                    Console.WriteLine($"Did not find embedded resource {resourcePath} in assembly defined by type {callerType}");
                    return null;
                }

                rawJson = ResourceHelper.ReadResourceAsString(callerType, resourcePath);
            }

            return rawJson;
        }

        public static dynamic LoadFromLocalJsonOrLocalResource(Type callerType, string diskPath, string resourcePath) 
        {
            string rawJson = LoadFromLocalJsonOrLocalResourceAsString(callerType, diskPath, resourcePath);
            if (rawJson == null)
                return null;

            return JsonConvert.DeserializeObject(rawJson);
        }

        public static bool ResourceExists(Assembly assembly, string resourceName)
        {
            string assemblyName = assembly.ManifestModule.Name;
            assemblyName = assemblyName.Substring(0, assemblyName.Length - 4); // clip off ".dll"
            string resourceFullName = $"{assembly.GetName().Name}.{resourceName}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceFullName))
            {
                return stream != null;
            }
        }

        public static bool ResourceExists(Type typeInTargetAssembly, string resourceName)
        {
            Assembly assembly = Assembly.GetAssembly(typeInTargetAssembly);
            if (assembly == null)
                throw new Exception($"Failed to find assemby for type {typeInTargetAssembly.FullName}");

            return ResourceExists(assembly, resourceName);
        }
    }
}
