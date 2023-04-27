using System;
using System.Reflection;

namespace Wbtb.Core.Common
{
    public class DevTypeHelper
    {
        public static Type?  ResolveType(string namespacedType)
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

    }
}
