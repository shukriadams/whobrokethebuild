﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Wbtb.Core.Common.Plugins;

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

        public static string Name(Type type)
        {
            return $"{type.Namespace}.{type.Name}";
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

        public static Type? ResolveType(string namespacedType)
        {
            // TODO - cache type lookup for performance
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? concreteType = a.GetType(namespacedType);
                if (concreteType != null)
                    return concreteType;
            }

            return null;
        }

        public static Type GetCommonType(string typeNamespacedName)
        {
            return _commonAssembly.GetType(typeNamespacedName);
        }
    }
}
