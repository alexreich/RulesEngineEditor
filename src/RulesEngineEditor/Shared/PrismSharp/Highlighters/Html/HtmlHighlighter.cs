// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using Orionsoft.PrismSharp.Highlighters.Abstract;
using Orionsoft.PrismSharp.Themes;
using Orionsoft.PrismSharp.Tokenizing;
using System.Text;

namespace Orionsoft.PrismSharp.Highlighters.Html
{
    /// <summary>
    /// Highlighter creating output in html format (just HTML spans or a pre block). A PrismJS css theme must be included in the html document.
    /// </summary>
    public sealed class HtmlHighlighter : AbstractHighlighter<string>
    {
        private StringBuilder sb;

        /// <summary>
        /// If set, the resulting html spans are enclosed in &lt;pre&gt; tag
        /// </summary>
        public bool WrapByPre { get; set; }

        public HtmlHighlighter(Tokenizer tokenizer)
        {
            this.Tokenizer = tokenizer;
        }

        public HtmlHighlighter()
        {
            this.Tokenizer = new Tokenizer();
        }

        protected override ThemeStyle BeginDocument(string language, ThemeStyle docStyle)
        {
            sb = new StringBuilder();
            if (WrapByPre)
            {
                sb.Append($"<pre class=\"language-{language}\"><code class=\"language-{language}\">");
            }
            return docStyle;
        }

        protected override void EndDocument()
        {
            if (WrapByPre)
            {
                sb.Append("</code></pre>");
            }

            Result = sb.ToString();
        }

        protected override ThemeStyle BeginContainer(Token token, ThemeStyle style, ThemeStyle parentStyle)
        {
            AddOpenningTag(token);
            return parentStyle;
        }

        protected override void EndContainer()
        {
            sb.Append("</span>");
        }

        private void AddOpenningTag(Token token)
        {
            if (token.Type != null)
            {
                sb.Append($"<span class=\"token {token.Type}");
                foreach (var a in token.Aliases)
                {
                    sb.Append(" ");
                    sb.Append(a);
                }
                sb.Append("\">");
            }
            else
            {
                sb.Append("<span>");
            }
        }

        /// <summary>
        /// Escapes the text to be HTMl compatible
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Encode(string text)
        {
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace("\u00a0", " ");
        }

        protected override void AddSpan(string text, Token token, ThemeStyle style, ThemeStyle parentStyle)
        {
            AddOpenningTag(token);
            sb.Append(Encode(text));
            EndContainer();
        }
    }
}