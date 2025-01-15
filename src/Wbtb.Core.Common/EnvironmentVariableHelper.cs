using System;

namespace Wbtb.Core.Common
{
    // do not expose
    internal class EnvironmentVariableHelper
    {
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
    }
}
