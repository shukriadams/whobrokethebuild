using System;
using YamlDotNet.Core.Tokens;

namespace Wbtb.Core.Web
{
    public class Rgb
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }

    public class ColorHelper
    {
        private static int lerp(int a, int b, float u)
        {
            return (int)Math.Round((double)((1 - u) * a + u * b), 0);
        }

        /**
         * 
         */
        private static Rgb getRGBForFactor(float factor, Rgb startColor, Rgb endColor)
        {
            return new Rgb
            {
                R = lerp(startColor.R, endColor.R, factor),
                G = lerp(startColor.G, endColor.G, factor),
                B = lerp(startColor.B, endColor.B, factor)
            };
        }

        private static string componentToHex(int c)
        {
            string hex = Convert.ToString(c, 16);
            return hex.Length == 1 ? '0' + hex : hex;
        }

        public static string RGBToHex(Rgb rgb)
        {
            if (rgb == null)
                rgb = new Rgb { R = 0, G = 0, B = 0 };

            return $"#{componentToHex(rgb.R)}{componentToHex(rgb.G)}{componentToHex(rgb.B)}";
        }

        /** 
         * Determines the  midpoint between two RGB colors based on a factor between 0 and 1.
         *
         * from https://stackoverflow.com/questions/7128675/from-green-to-red-color-depend-on-percentage/7128796
         */
        public static Rgb RgbBetween(float factor, Rgb startColor, Rgb endColor)
        {
            if (startColor == null)
                startColor = new Rgb { R = 0, G = 250, B = 0 };

            if (endColor == null)
                endColor = new Rgb { R = 250, G = 0, B = 0 };

            if (factor < 0)
                factor = 0;

            if (factor > 1)
                factor = 1;

            return getRGBForFactor(factor, startColor, endColor);
        }

        public static string HexBetween(int factor, Rgb startColor, Rgb endColor)
        {
            Rgb rgb = RgbBetween(factor, startColor, endColor);
            return RGBToHex(rgb);
        }
    }
}
