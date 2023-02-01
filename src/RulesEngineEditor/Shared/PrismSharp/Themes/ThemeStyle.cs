// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using Newtonsoft.Json;

namespace Orionsoft.PrismSharp.Themes
{
    /// <summary>
    /// Styling of a token or an entire block of code
    /// </summary>
    public sealed class ThemeStyle
    {
        /// <summary>
        /// Token type, eg. keyword
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Language, if the styling applies only to this language
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// Foreground color
        /// </summary>
        [JsonProperty("color")]
        public RgbaColor Color { get; set; }

        [JsonProperty("opacity")]
        public double? Opacity { get; set; }

        /// <summary>
        ///  Background color
        /// </summary>
        [JsonProperty("background")]
        public RgbaColor Background { get; set; }

        [JsonProperty("bold")]
        public bool? Bold { get; set; }

        [JsonProperty("italic")]
        public bool? Italic { get; set; }

        [JsonProperty("underline")]
        public bool? Underline { get; set; }

        public ThemeStyle Clone()
        {
            return new ThemeStyle { Background = Background?.Clone(), Bold = Bold, Color = Color?.Clone(), Italic = Italic, Underline = Underline, Opacity = Opacity, Type = Type, Language = Language };
        }

        /// <summary>
        /// Merges this styling with another that is used as a base style and undefined values are taken from this base style.
        /// </summary>
        /// <param name="baseStyle"></param>
        /// <returns></returns>
        public ThemeStyle MergeWith(ThemeStyle baseStyle)
        {
            if (baseStyle == null) return Clone();

            var res = new ThemeStyle
            {
                Color = Color?.Clone() ?? baseStyle.Color?.Clone(),
                Background = Background?.Clone() ?? baseStyle.Background?.Clone(),
                Bold = Bold ?? baseStyle.Bold,
                Italic = Italic ?? baseStyle.Italic,
                Underline = Underline ?? baseStyle.Underline
            };
            if (res.Opacity.HasValue && res.Opacity < 1)
            {
                res.Color.A = Opacity.Value;
                res.Background.A = Opacity.Value;
                res.Opacity = 1;
            }
            return res;
        }
    }
}