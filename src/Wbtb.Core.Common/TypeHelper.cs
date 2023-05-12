﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Wbtb.Core.Common
{
    public class TypeHelper
    {
        static Assembly _commonAssembly;

        static TypeHelper()
        {
            _commonAssembly = typeof(IPlugin).Assembly;
        }

        public static string Name<T>()
        {
            return Name(typeof(T));
        }

        public static string Name(object obj)
        {
            return Name(obj.GetType());
        }

        public static string Name(Type type, bool removeGeneric = false)
        {
            string name = $"{type.Namespace}.{type.Name}";
            if (name.EndsWith("`1"))
                name = name.Substring(0, name.Length - 2);

            return name;
        }

        public static Assembly GetAssembly(string namespc) 
        {
            Assembly assembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => a.GetName().Name == namespc)
                .FirstOrDefault();

            if (assembly == null)
                assembly = Assembly.Load(namespc);

            return assembly;
        }

        public static Type GetRequiredProxyType(string namespc) 
        {
            Type type = ResolveType(namespc);
            if (type == null)
                throw new Exception($"String {namespc} could not be resolved");

            return GetRequiredProxyType(type);
        }

        /// <summary>
        /// Gets the WBTB proxy type for a given type. Raises exception if that type does not properly define a proxy.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Type GetRequiredProxyType(Type type) 
        {
            PluginProxyAttribute attribute = TypeDescriptor.GetAttributes(type).OfType<PluginProxyAttribute>().SingleOrDefault();
            if (attribute == null)
                throw new Exception($"Type {TypeHelper.Name(type)} does not implement {TypeHelper.Name<PluginProxyAttribute>()}");

            return attribute.ProxyType;
        }

        /// <summary>
        /// Gets first occurrence of the given attribute on the source type.
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(Type source) 
        {
            return TypeDescriptor.GetAttributes(source).OfType<T>().FirstOrDefault();
        }

        public static Type? ResolveType(string namespacedType)
        {
            // TODO - cache type lookup for performance
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? concreteType = a.GetType(namespacedType);
                if (concreteType != null)
                    return concreteType;
            }

            // couldn't resolve type, does it live in an assembly that needs to be loaded?

            return null;
        }

        public static Type GetCommonType(string typeNamespacedName)
        {
            return _commonAssembly.GetType(typeNamespacedName);
        }
    }
}
