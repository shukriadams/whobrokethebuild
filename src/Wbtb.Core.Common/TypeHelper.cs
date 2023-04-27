using System;
using Wbtb.Core.Common.Plugins;

namespace Wbtb.Core.Common
{
    public class TypeHelper
    {
        static System.Reflection.Assembly _commonAssembly;

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

        public static Type GetCommonType(string typeNamespacedName)
        {
            return _commonAssembly.GetType(typeNamespacedName);
        }
    }
}
