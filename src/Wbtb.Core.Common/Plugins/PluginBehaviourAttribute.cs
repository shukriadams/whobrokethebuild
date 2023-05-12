using System;

namespace Wbtb.Core.Common
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public class PluginBehaviourAttribute : Attribute
    {
        /// <summary>
        /// If true, only one instance of a plugin can be bound at a time
        /// </summary>
        public bool AllowMultiple{ get; private set; }

        public PluginBehaviourAttribute(bool allowMultiple)
        {
            this.AllowMultiple = allowMultiple;
        }
    }
}
