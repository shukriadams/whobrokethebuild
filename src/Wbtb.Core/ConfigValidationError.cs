using System;

namespace Wbtb.Core
{
    public class ConfigValidationError
    {
        public bool IsValid { get;set;}

        public string Message { get; set; }

        public Exception InnerException { get; set; }

        public ConfigValidationError()
        { 
            this.Message = string.Empty;
        }
    }
}
