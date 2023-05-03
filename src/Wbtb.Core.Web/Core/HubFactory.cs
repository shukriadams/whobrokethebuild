using System;
using System.Collections.Generic;
using Wbtb.Core.Common;

namespace Wbtb.Core.Web
{
    /// <summary>
    /// Workaround storage for SignalR hubs. Hub instances can be return from M$ DI only, so we do that at start of app and cache them here. Horribe workaround, but
    /// M$ took away all control to create custom factories for hubs. See https://github.com/aspnet/SignalR/issues/1831 for reasons. 
    /// </summary>
    public class HubFactoryGlobals 
    {
        private static Dictionary<string, object> hubs = new Dictionary<string, object>();

        public static void Add<T>(object hub)
        {
            Type t = typeof(T);
            Type[] generics = t.GetGenericArguments();
            string name = TypeHelper.Name(t, true);
            foreach (Type generic in generics)
                name += $".{TypeHelper.Name(generic, true)}";

            hubs.Add(name, hub);
        }

        public static object Get(Type t) 
        {
            Type[] generics = t.GetGenericArguments();
            string name = TypeHelper.Name(t, true);
            foreach (Type generic in generics)
                name += $".{TypeHelper.Name(generic, true)}";

            if (!hubs.ContainsKey(name))
                throw new Exception($"No hub with type {name} was registered");

            return hubs[name];
        }
    }
    public class HubFactory : ISimpleDIFactory
    {
        public object Resolve<T>()
        {
            return (T)this.Resolve(typeof(T));
        }

        public object Resolve(Type service)
        {
            Type[] generics = service.GetGenericArguments();
            return HubFactoryGlobals.Get(service);
        }
    }
}
