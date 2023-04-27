using System.Collections.Generic;

namespace Wbtb.Core.Common
{
    public interface DONTUSEIConfig
    {
        IEnumerable<KeyValuePair<string, object>> Config { get; set; }
    }
}
