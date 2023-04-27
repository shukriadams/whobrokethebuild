using System;

namespace Wbtb.Extensions.Data.Postgres
{
    public class EnumParse
    {
        /// <summary>
        /// Single line safe enum parse
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="fallbackValue"></param>
        /// <returns></returns>
        public static T Safe<T>(string value, object fallbackValue)
        {
            object v;
            if (Enum.TryParse(typeof(T), value, out v))
                return (T)v;
            else
                return (T)fallbackValue;
            
        }
    }
}
