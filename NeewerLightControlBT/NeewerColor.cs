using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeewerLightControlBT
{
    public class NeewerColor
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public static NeewerColor FromHexString(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException(nameof(hex));

            if (hex.Length != 6)
                throw new ArgumentException("Hex string must be exactly 6 characters long.", nameof(hex));
            var r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

            return new NeewerColor()
            {
                R = (byte)r,
                G = (byte)g,
                B = (byte)b,
            };
        }
        public static string GetColorString(NeewerColor color)
        {
            return color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }
        public static readonly NeewerColor Black = new NeewerColor() { R = 0, B = 0, G = 0 };
        public static readonly NeewerColor AliceBlue = new NeewerColor() { R = 240, G = 248, B = 255 };
        public static readonly NeewerColor AntiqueWhite = new NeewerColor() { R = 250, G = 235, B = 215 };
        public static readonly NeewerColor Aqua = new NeewerColor() { R = 0, G = 255, B = 255 };
        public static readonly NeewerColor Aquamarine = new NeewerColor() { R = 127, G = 255, B = 212 };
        public static readonly NeewerColor Azure = new NeewerColor() { R = 240, G = 255, B = 255 };
        public static readonly NeewerColor Beige = new NeewerColor() { R = 245, G = 245, B = 220 };
        public static readonly NeewerColor Bisque = new NeewerColor() { R = 255, G = 228, B = 196 };
        public static readonly NeewerColor BlanchedAlmond = new NeewerColor() { R = 255, G = 235, B = 205 };
        public static readonly NeewerColor Blue = new NeewerColor() { R = 0, G = 0, B = 255 };
        public static readonly NeewerColor BlueViolet = new NeewerColor() { R = 138, G = 43, B = 226 };
        public static readonly NeewerColor Brown = new NeewerColor() { R = 165, G = 42, B = 42 };
        public static readonly NeewerColor BurlyWood = new NeewerColor() { R = 222, G = 184, B = 135 };
        public static readonly NeewerColor CadetBlue = new NeewerColor() { R = 95, G = 158, B = 160 };
        public static readonly NeewerColor Chartreuse = new NeewerColor() { R = 127, G = 255, B = 0 };
        public static readonly NeewerColor Chocolate = new NeewerColor() { R = 210, G = 105, B = 30 };
        public static readonly NeewerColor Coral = new NeewerColor() { R = 255, G = 127, B = 80 };
        public static readonly NeewerColor CornflowerBlue = new NeewerColor() { R = 100, G = 149, B = 237 };
        public static readonly NeewerColor Cornsilk = new NeewerColor() { R = 255, G = 248, B = 220 };
        public static readonly NeewerColor Crimson = new NeewerColor() { R = 220, G = 20, B = 60 };
        public static readonly NeewerColor Cyan = new NeewerColor() { R = 0, G = 255, B = 255 };
        public static readonly NeewerColor DarkBlue = new NeewerColor() { R = 0, G = 0, B = 139 };
        public static readonly NeewerColor DarkCyan = new NeewerColor() { R = 0, G = 139, B = 139 };
        public static readonly NeewerColor DarkGoldenrod = new NeewerColor() { R = 184, G = 134, B = 11 };
        public static readonly NeewerColor DarkGray = new NeewerColor() { R = 169, G = 169, B = 169 };
        public static readonly NeewerColor DarkGreen = new NeewerColor() { R = 0, G = 100, B = 0 };
        public static readonly NeewerColor DarkKhaki = new NeewerColor() { R = 189, G = 183, B = 107 };
        public static readonly NeewerColor DarkMagenta = new NeewerColor() { R = 139, G = 0, B = 139 };
        public static readonly NeewerColor DarkOliveGreen = new NeewerColor() { R = 85, G = 107, B = 47 };
        public static readonly NeewerColor DarkOrange = new NeewerColor() { R = 255, G = 140, B = 0 };
        public static readonly NeewerColor DarkOrchid = new NeewerColor() { R = 153, G = 50, B = 204 };
        public static readonly NeewerColor DarkRed = new NeewerColor() { R = 139, G = 0, B = 0 };
        public static readonly NeewerColor DarkSalmon = new NeewerColor() { R = 233, G = 150, B = 122 };
        public static readonly NeewerColor DarkSeaGreen = new NeewerColor() { R = 143, G = 188, B = 143 };
        public static readonly NeewerColor DarkSlateBlue = new NeewerColor() { R = 72, G = 61, B = 139 };
        public static readonly NeewerColor DarkSlateGray = new NeewerColor() { R = 47, G = 79, B = 79 };
        public static readonly NeewerColor DarkTurquoise = new NeewerColor() { R = 0, G = 206, B = 209 };
        public static readonly NeewerColor DarkViolet = new NeewerColor() { R = 148, G = 0, B = 211 };
        public static readonly NeewerColor DeepPink = new NeewerColor() { R = 255, G = 20, B = 147 };
        public static readonly NeewerColor DeepSkyBlue = new NeewerColor() { R = 0, G = 191, B = 255 };
        public static readonly NeewerColor DimGray = new NeewerColor() { R = 105, G = 105, B = 105 };
        public static readonly NeewerColor DodgerBlue = new NeewerColor() { R = 30, G = 144, B = 255 };
        public static readonly NeewerColor Firebrick = new NeewerColor { R = 178, G = 34, B = 34 };
        public static readonly NeewerColor FloralWhite = new NeewerColor { R = 255, G = 250, B = 240 };
        public static readonly NeewerColor ForestGreen = new NeewerColor { R = 34, G = 139, B = 34 };
        public static readonly NeewerColor Fuchsia = new NeewerColor { R = 255, G = 0, B = 255 };
        public static readonly NeewerColor Gainsboro = new NeewerColor { R = 220, G = 220, B = 220 };
        public static readonly NeewerColor GhostWhite = new NeewerColor { R = 248, G = 248, B = 255 };
        public static readonly NeewerColor Gold = new NeewerColor { R = 255, G = 215, B = 0 };
        public static readonly NeewerColor Goldenrod = new NeewerColor { R = 218, G = 165, B = 32 };
        public static readonly NeewerColor Gray = new NeewerColor { R = 128, G = 128, B = 128 };
        public static readonly NeewerColor Green = new NeewerColor { R = 0, G = 128, B = 0 };
        public static readonly NeewerColor GreenYellow = new NeewerColor { R = 173, G = 255, B = 47 };
        public static readonly NeewerColor Honeydew = new NeewerColor { R = 240, G = 255, B = 240 };
        public static readonly NeewerColor HotPink = new NeewerColor { R = 255, G = 105, B = 180 };
        public static readonly NeewerColor IndianRed = new NeewerColor { R = 205, G = 92, B = 92 };
        public static readonly NeewerColor Indigo = new NeewerColor { R = 75, G = 0, B = 130 };
        public static readonly NeewerColor Ivory = new NeewerColor { R = 255, G = 255, B = 240 };
        public static readonly NeewerColor Khaki = new NeewerColor { R = 240, G = 230, B = 140 };
        public static readonly NeewerColor Lavender = new NeewerColor { R = 230, G = 230, B = 250 };
        public static readonly NeewerColor LavenderBlush = new NeewerColor { R = 255, G = 240, B = 245 };
        public static readonly NeewerColor LawnGreen = new NeewerColor { R = 124, G = 252, B = 0 };
        public static readonly NeewerColor LemonChiffon = new NeewerColor { R = 255, G = 250, B = 205 };
        public static readonly NeewerColor LightBlue = new NeewerColor { R = 173, G = 216, B = 230 };
        public static readonly NeewerColor LightCoral = new NeewerColor { R = 240, G = 128, B = 128 };
        public static readonly NeewerColor LightCyan = new NeewerColor { R = 224, G = 255, B = 255 };
        public static readonly NeewerColor LightGoldenrodYellow = new NeewerColor { R = 250, G = 250, B = 210 };
        public static readonly NeewerColor LightGray = new NeewerColor { R = 211, G = 211, B = 211 };
        public static readonly NeewerColor LightGreen = new NeewerColor { R = 144, G = 238, B = 144 };
        public static readonly NeewerColor LightPink = new NeewerColor { R = 255, G = 182, B = 193 };
        public static readonly NeewerColor LightSalmon = new NeewerColor { R = 255, G = 160, B = 122 };
        public static readonly NeewerColor LightSeaGreen = new NeewerColor { R = 32, G = 178, B = 170 };
        public static readonly NeewerColor LightSkyBlue = new NeewerColor { R = 135, G = 206, B = 250 };
        public static readonly NeewerColor LightSlateGray = new NeewerColor { R = 119, G = 136, B = 153 };
        public static readonly NeewerColor LightSteelBlue = new NeewerColor { R = 176, G = 196, B = 222 };
        public static readonly NeewerColor LightYellow = new NeewerColor { R = 255, G = 255, B = 224 };
        public static readonly NeewerColor Lime = new NeewerColor { R = 0, G = 255, B = 0 };
        public static readonly NeewerColor LimeGreen = new NeewerColor { R = 50, G = 205, B = 50 };
        public static readonly NeewerColor Linen = new NeewerColor { R = 250, G = 240, B = 230 };
        public static readonly NeewerColor Magenta = new NeewerColor { R = 255, G = 0, B = 255 };
        public static readonly NeewerColor Maroon = new NeewerColor { R = 128, G = 0, B = 0 };
        public static readonly NeewerColor MediumAquamarine = new NeewerColor { R = 102, G = 205, B = 170 };
        public static readonly NeewerColor MediumBlue = new NeewerColor { R = 0, G = 0, B = 205 };
        public static readonly NeewerColor MediumOrchid = new NeewerColor { R = 186, G = 85, B = 211 };
        public static readonly NeewerColor MediumPurple = new NeewerColor { R = 147, G = 112, B = 219 };
        public static readonly NeewerColor MediumSeaGreen = new NeewerColor { R = 60, G = 179, B = 113 };
        public static readonly NeewerColor MediumSlateBlue = new NeewerColor { R = 123, G = 104, B = 238 };
        public static readonly NeewerColor MediumSpringGreen = new NeewerColor { R = 0, G = 250, B = 154 };
        public static readonly NeewerColor MediumTurquoise = new NeewerColor { R = 72, G = 209, B = 204 };
        public static readonly NeewerColor MediumVioletRed = new NeewerColor { R = 199, G = 21, B = 133 };
        public static readonly NeewerColor MidnightBlue = new NeewerColor { R = 25, G = 25, B = 112 };
        public static readonly NeewerColor MintCream = new NeewerColor { R = 245, G = 255, B = 250 };
        public static readonly NeewerColor MistyRose = new NeewerColor { R = 255, G = 228, B = 225 };
        public static readonly NeewerColor Moccasin = new NeewerColor { R = 255, G = 228, B = 181 };
        public static readonly NeewerColor NavajoWhite = new NeewerColor { R = 255, G = 222, B = 173 };
        public static readonly NeewerColor Navy = new NeewerColor { R = 0, G = 0, B = 128 };
        public static readonly NeewerColor OldLace = new NeewerColor { R = 253, G = 245, B = 230 };
        public static readonly NeewerColor Olive = new NeewerColor { R = 128, G = 128, B = 0 };
        public static readonly NeewerColor OliveDrab = new NeewerColor { R = 107, G = 142, B = 35 };
        public static readonly NeewerColor Orange = new NeewerColor { R = 255, G = 165, B = 0 };
        public static readonly NeewerColor OrangeRed = new NeewerColor { R = 255, G = 69, B = 0 };
        public static readonly NeewerColor Orchid = new NeewerColor { R = 218, G = 112, B = 214 };
        public static readonly NeewerColor PaleGoldenrod = new NeewerColor { R = 238, G = 232, B = 170 };
        public static readonly NeewerColor PaleGreen = new NeewerColor { R = 152, G = 251, B = 152 };
        public static readonly NeewerColor PaleTurquoise = new NeewerColor { R = 175, G = 238, B = 238 };
        public static readonly NeewerColor PaleVioletRed = new NeewerColor { R = 219, G = 112, B = 147 };
        public static readonly NeewerColor PapayaWhip = new NeewerColor { R = 255, G = 239, B = 213 };
        public static readonly NeewerColor PeachPuff = new NeewerColor { R = 255, G = 218, B = 185 };
        public static readonly NeewerColor Peru = new NeewerColor { R = 205, G = 133, B = 63 };
        public static readonly NeewerColor Pink = new NeewerColor { R = 255, G = 192, B = 203 };
        public static readonly NeewerColor Plum = new NeewerColor { R = 221, G = 160, B = 221 };
        public static readonly NeewerColor PowderBlue = new NeewerColor { R = 176, G = 224, B = 230 };
        public static readonly NeewerColor Purple = new NeewerColor { R = 128, G = 0, B = 128 };
        public static readonly NeewerColor Red = new NeewerColor { R = 255, G = 0, B = 0 };
        public static readonly NeewerColor RosyBrown = new NeewerColor { R = 188, G = 143, B = 143 };
        public static readonly NeewerColor RoyalBlue = new NeewerColor { R = 65, G = 105, B = 225 };
        public static readonly NeewerColor SaddleBrown = new NeewerColor { R = 139, G = 69, B = 19 };
        public static readonly NeewerColor Salmon = new NeewerColor { R = 250, G = 128, B = 114 };
        public static readonly NeewerColor SandyBrown = new NeewerColor { R = 244, G = 164, B = 96 };
        public static readonly NeewerColor SeaGreen = new NeewerColor { R = 46, G = 139, B = 87 };
        public static readonly NeewerColor SeaShell = new NeewerColor { R = 255, G = 245, B = 238 };
        public static readonly NeewerColor Sienna = new NeewerColor { R = 160, G = 82, B = 45 };
        public static readonly NeewerColor Silver = new NeewerColor { R = 192, G = 192, B = 192 };
        public static readonly NeewerColor SkyBlue = new NeewerColor { R = 135, G = 206, B = 235 };
        public static readonly NeewerColor SlateBlue = new NeewerColor { R = 106, G = 90, B = 205 };
        public static readonly NeewerColor SlateGray = new NeewerColor { R = 112, G = 128, B = 144 };
        public static readonly NeewerColor Snow = new NeewerColor { R = 255, G = 250, B = 250 };
        public static readonly NeewerColor SpringGreen = new NeewerColor { R = 0, G = 255, B = 127 };
        public static readonly NeewerColor SteelBlue = new NeewerColor { R = 70, G = 130, B = 180 };
        public static readonly NeewerColor Tan = new NeewerColor { R = 210, G = 180, B = 140 };
        public static readonly NeewerColor Teal = new NeewerColor { R = 0, G = 128, B = 128 };
        public static readonly NeewerColor Thistle = new NeewerColor { R = 216, G = 191, B = 216 };
        public static readonly NeewerColor Tomato = new NeewerColor { R = 255, G = 99, B = 71 };
        public static readonly NeewerColor Turquoise = new NeewerColor { R = 64, G = 224, B = 208 };
        public static readonly NeewerColor Violet = new NeewerColor { R = 238, G = 130, B = 238 };
        public static readonly NeewerColor Wheat = new NeewerColor { R = 245, G = 222, B = 179 };
        public static readonly NeewerColor White = new NeewerColor { R = 255, G = 255, B = 255 };
        public static readonly NeewerColor WhiteSmoke = new NeewerColor { R = 245, G = 245, B = 245 };
        public static readonly NeewerColor Yellow = new NeewerColor { R = 255, G = 255, B = 0 };
        public static readonly NeewerColor YellowGreen = new NeewerColor { R = 154, G = 205, B = 50 };
        public (float hue, float saturation, float luminosity) ToHSL()
        {
            double modifiedR, modifiedG, modifiedB, min, max, delta, h, s, l;
            modifiedR = (double)R / 255.0;
            modifiedG = (double)G / 255.0;
            modifiedB = (double)B / 255.0;
            min = new List<double>() { modifiedR, modifiedG, modifiedB }.Min();
            max = new List<double>() { modifiedR, modifiedG, modifiedB }.Max();
            delta = max - min;
            l = (min + max) / 2;

            if (delta == 0)
            {
                h = 0;
                s = 0;
            }
            else
            {
                s = (l <= 0.5) ? (delta / (min + max)) : (delta / (2 - max - min));

                if (modifiedR == max)
                {
                    h = (modifiedG - modifiedB) / 6 / delta;
                }
                else if (modifiedG == max)
                {
                    h = (1.0 / 3) + ((modifiedB - modifiedR) / 6 / delta);
                }
                else
                {
                    h = (2.0 / 3) + ((modifiedR - modifiedG) / 6 / delta);
                }

                h = (h < 0) ? ++h : h;
                h = (h > 1) ? --h : h;
            }

            return ((float)h,
            (float)s,
            (float)l);
        }
    }
}
