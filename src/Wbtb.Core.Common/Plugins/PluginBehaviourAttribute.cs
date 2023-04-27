using System;

namespace Wbtb.Core.Common.Plugins
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public class PluginBehaviourAttribute : Attribute
    {
        /// <summary>
        /// If true, only one instance of a plugin can be bound at a time
        /// </summary>
        public bool IsExclusive{ get; private set; }

        public PluginBehaviourAttribute(bool isExclusive)
        {
            this.IsExclusive = isExclusive;
        }
    }
}
