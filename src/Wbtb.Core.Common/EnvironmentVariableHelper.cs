using System;
using System.Collections.Generic;
using System.Linq;

namespace Wbtb.Core.Common
{
    // do not expose
    internal class EnvironmentVariableHelper
    {
        public static IEnumerable<string> GetList(string envVarName, IEnumerable<string> defaultValue)
        {
            object var = Environment.GetEnvironmentVariable(envVarName);
            if (var == null)
                return defaultValue;

            return var.ToString().Split(",").Select(t => t.Trim());
        }

        public static bool GetBool(string envVarName, bool defaultValue = false)
        {
            object var = Environment.GetEnvironmentVariable(envVarName);
            if (var == null)
                return defaultValue;
            
            string val = var.ToString().ToLower();

            return val == "1" || val == "true";
        }

        public static string GetString(string envVarName, string defaultValue = "")
        {
            object var = Environment.GetEnvironmentVariable(envVarName);
            if (var == null)
                return defaultValue;
            return var.ToString();
        }

        public static int GetInt(string envVarName, int defaultValue = 0)
        {
            object var = Environment.GetEnvironmentVariable(envVarName);
            if (var == null)
                return defaultValue;

            int.TryParse(var.ToString(), out defaultValue);
            return defaultValue;
        }
    }
}
