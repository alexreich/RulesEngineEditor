// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using Orionsoft.PrismSharp.Highlighters.Abstract;
using Orionsoft.PrismSharp.Themes;
using Orionsoft.PrismSharp.Tokenizing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Orionsoft.PrismSharp.Highlighters.Rtf
{
    /// <summary>
    /// Highlighter creating output in RTF format. Can be used in GUI componets like RichBox
    /// </summary>
    public sealed class RtfHighlighter : AbstractHighlighter<string>
    {
        private readonly StringBuilder sb = new StringBuilder();
        private readonly StringBuilder preamble = new StringBuilder();

        private readonly ThemeStyle lastStyle = new ThemeStyle { Opacity = 1 };

        private readonly List<RgbaColor> colorTable = new List<RgbaColor>();

        /// <summary>
        /// Name of font to use, otherwise the default font is used
        /// </summary>
        public string Font { get; set; }

        /// <summary>
        /// Size of font to use, otherwise the default font size is used
        /// </summary>
        public double FontSize { get; set; }

        public RtfHighlighter(Theme theme)
        {
            Construct(theme);
        }

        public RtfHighlighter(Tokenizer tokenizer, Theme theme)
        {
            Construct(tokenizer, theme);
        }

        public RtfHighlighter(Tokenizer tokenizer, string themeFileName)
        {
            Construct(tokenizer, themeFileName);
        }

        public RtfHighlighter(ThemeNames theme)
        {
            Construct(theme);
        }

        public RtfHighlighter(Tokenizer tokenizer, ThemeNames theme)
        {
            Construct(tokenizer, theme);
        }

        public RtfHighlighter(string themeFileName)
        {
            Construct(themeFileName);
        }

        protected override ThemeStyle BeginDocument(string language, ThemeStyle docStyle)
        {
            sb.Clear();
            preamble.Clear();
            preamble.Append(@"{\rtf1\ansi\deff0");

            if (!string.IsNullOrEmpty(Font))
            {
                preamble.Append(@" {\fonttbl {\f0 " + Font + @";}}\f0");
            }

            if (FontSize > 0)
            {
                preamble.Append($"\\fs{(int)(FontSize * 2)} ");
            }

            return docStyle;
        }

        protected override void EndDocument()
        {
            sb.Append(@"}");
            CreateColorTable();
            Result = preamble.Append(sb).ToString();
        }

        private void CreateColorTable()
        {
            if (!colorTable.Any()) return;
            preamble.Append(@"{\colortbl");
            foreach (var c in colorTable)
            {
                preamble.Append($"\\red{c.R}\\green{c.G}\\blue{c.B};");
            }
            preamble.Append("}");
        }

        protected override ThemeStyle BeginContainer(Token token, ThemeStyle style, ThemeStyle parentStyle)
        {
            return style?.MergeWith(parentStyle) ?? parentStyle.Clone();
        }

        protected override void AddSpan(string text, Token token, ThemeStyle style, ThemeStyle parentStyle)
        {
            var mergedStyle = style?.MergeWith(parentStyle) ?? parentStyle.Clone();

            if (mergedStyle != null)
            {
                mergedStyle.Background.ApplyAlpha(Theme.GetDocumentStyle(Language).Background);

                if (lastStyle.Color == null || !lastStyle.Color.IsEqual(mergedStyle.Color))
                {
                    lastStyle.Color = mergedStyle.Color;
                    int idx = GetColorIndex(mergedStyle.Color);
                    sb.Append("\\cf" + idx + " ");
                }

                if (lastStyle.Background == null || !lastStyle.Background.IsEqual(mergedStyle.Background))
                {
                    lastStyle.Background = mergedStyle.Background;
                    int idx = GetColorIndex(mergedStyle.Background);
                    sb.Append("\\chshdng0\\chcbpat" + idx + "\\cb" + idx + " ");
                }

                if (lastStyle.Bold != mergedStyle.Bold)
                {
                    if (mergedStyle.Bold.HasValue && mergedStyle.Bold.Value)
                        sb.Append("\\b ");
                    else
                        sb.Append("\\b0 ");
                    lastStyle.Bold = mergedStyle.Bold;
                }

                if (lastStyle.Italic != mergedStyle.Italic)
                {
                    if (mergedStyle.Italic.HasValue && mergedStyle.Italic.Value)
                        sb.Append("\\i ");
                    else
                        sb.Append("\\i0 ");
                    lastStyle.Italic = mergedStyle.Italic;
                }

                if (lastStyle.Underline != mergedStyle.Underline)
                {
                    if (mergedStyle.Underline.HasValue && mergedStyle.Underline.Value)
                        sb.Append("\\ul ");
                    else
                        sb.Append("\\ul0 ");
                    lastStyle.Underline = mergedStyle.Underline;
                }
            }
            sb.Append(Encode(text));
        }

        private int GetColorIndex(RgbaColor color)
        {
            var inTable = colorTable.FirstOrDefault(x => x.IsEqual(color));
            var idx = inTable == null ? -1 : colorTable.IndexOf(inTable);
            if (idx == -1)
            {
                colorTable.Add(color);
                idx = colorTable.Count - 1;
            }

            return idx;
        }

        private string Encode(string text)
        {
            var res = new StringBuilder();
            foreach (var ch in text)
            {
                switch (ch)
                {
                    case '\r': continue;
                    case '\n': res.Append("\\line "); continue;
                    case '{': res.Append("\\{"); continue;
                    case '}': res.Append("\\}"); continue;
                    case '\\': res.Append("\\\\"); continue;
                }

                if (ch > 127)
                {
                    if (ch <= 255)
                    {
                        res.Append("\\\'" + String.Format("{0:x}", (int)ch));
                    }
                    else
                    {
                        res.Append("\\uc1\\u" + ((int)ch).ToString(CultureInfo.InvariantCulture) + "*");
                    }
                }
                else { res.Append(ch); }
            }
            return res.ToString();
        }
    }
}