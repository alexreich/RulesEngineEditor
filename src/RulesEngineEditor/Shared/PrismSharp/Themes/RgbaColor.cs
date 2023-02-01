// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using System.Globalization;

namespace Orionsoft.PrismSharp.Themes
{
    /// <summary>
    /// RGB color with an alpha channel (opacity)
    /// </summary>
    public sealed class RgbaColor
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public double A { get; set; }

        public RgbaColor()
        {
            A = 1;
        }

        public RgbaColor Clone()
        {
            return new RgbaColor { R = R, G = G, B = B, A = A };
        }

        internal bool IsEqual(RgbaColor color)
        {
            return color.R == R && color.B == B && color.G == G && color.A == A;
        }

        /// <summary>
        /// Returns the color in css-like string: rgba(r, g, b, a)
        /// </summary>
        public string ToColorString()
        {
            return $"rgba({R}, {G}, {B}, {A.ToString(CultureInfo.InvariantCulture)})";
        }

        /// <summary>
        /// Adjusts the color so that it is mixed with the background color according to the alpha channel level.
        /// </summary>
        /// <param name="background"></param>
        public void ApplyAlpha(RgbaColor background)
        {
            if (A == 1) return;

            R = (byte)(R * A + background.R * (1 - A));
            G = (byte)(G * A + background.G * (1 - A));
            B = (byte)(B * A + background.B * (1 - A));
            A = 1;
        }
    }
}