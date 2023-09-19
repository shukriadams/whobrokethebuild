using System;
using System.Text;
using System.Text.RegularExpressions;
using Wbtb.Core.Common;

namespace Wbtb.Common
{
    /// <summary>
    /// Obscures/restores strings with base64. This is for cosmetic reasons (to mask complex data structure in public ids), and for sanitizing text for
    /// writing as filesystem names.
    /// </summary>
    public static class PublicIdentifierHelper
    {
        public static string Encode(string input)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }

        public static string Decode(string input)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(input));
            }
            catch (FormatException)
            {
                throw new PublicIdentifierException(input);
            }
        }

        public static PublicIdentifier ParsePublicBuildId(string publicBuildId) 
        {
            Regex regex = new Regex(@"(.*)____(.*)");
            publicBuildId = Decode(publicBuildId);
            Match match = regex.Match(publicBuildId);
            if (!match.Success)
                return null;

            return new PublicIdentifier { 
                BuildIdentifer = match.Groups[1].Value, 
                JobKey = match.Groups[2].Value 
            };
        }
    }
}
