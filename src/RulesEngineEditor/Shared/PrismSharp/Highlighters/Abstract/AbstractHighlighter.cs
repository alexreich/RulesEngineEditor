// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using Orionsoft.PrismSharp.Themes;
using Orionsoft.PrismSharp.Tokenizing;
using System.Linq;
using System.Linq.Expressions;

namespace Orionsoft.PrismSharp.Highlighters.Abstract
{
    /// <summary>
    /// An abstract base class making it easy to implement a custom higlighter creating virtually any output format
    /// </summary>
    /// <typeparam name="TResult">type of the highlighting output, e.h. string</typeparam>
    public abstract class AbstractHighlighter<TResult> : IHighlighter<TResult>
    {
        private int rangeStart;
        private int rangeEnd;

        /// <summary>
        /// Instance of tokenizer in use
        /// </summary>
        protected Tokenizer Tokenizer { get; set; }

        /// <summary>
        /// Output of the highlighting
        /// </summary>
        protected TResult Result { get; set; }

        /// <summary>
        /// Currently used theme, if any
        /// </summary>
        public Theme Theme { get; private set; }

        /// <summary>
        /// Name of programming language used for highlighting
        /// </summary>
        public string Language { get; private set; }

        public TResult Highlight(string code, string language)
        {
            var token = Tokenizer.Tokenize(code, language);

            return Highlight(token, language);
        }

        public TResult Highlight(Token tokenizedCode, string language)
        {
            return HighlightRange(tokenizedCode, 0, tokenizedCode.Text.Length, language);
        }

        public TResult HighlightRange(string code, int start, int length, string language)
        {
            var token = Tokenizer.TokenizeRange(code, start, length, language);
            return HighlightRange(token, start, length, language);
        }

        public TResult HighlightRange(Token tokenizedCode, int start, int length, string language)
        {
            var docStyle = Theme?.GetDocumentStyle(language);
            var newParent = BeginDocument(language, docStyle);
            rangeStart = start;
            rangeEnd = rangeStart + length;
            this.Language = language;

            foreach (var token in tokenizedCode.Tokens)
            {
                if (token.AbsolutePos + token.Text.Length > start && token.AbsolutePos < rangeEnd) Walk(token, language, newParent);
            }

            if (!tokenizedCode.Tokens.Any())
            {
                AddSpan(TrimTokenText(tokenizedCode), tokenizedCode, null, newParent);
            }

            EndDocument();
            return Result;
        }

        /// <summary>
        /// Serves as a base constructor (as it is an abstract class without constructors)
        /// </summary>
        protected void Construct(Theme theme)
        {
            this.Tokenizer = new Tokenizer();
            this.Theme = theme;
        }

        /// <summary>
        /// Serves as a base constructor (as it is an abstract class without constructors)
        /// </summary>
        protected void Construct(Tokenizer tokenizer, Theme theme)
        {
            this.Tokenizer = tokenizer;
            this.Theme = theme;
        }

        protected void Construct(ThemeNames theme)
        {
            this.Tokenizer = new Tokenizer();
            this.Theme = Theme.Load(theme);
        }

        /// <summary>
        /// Serves as a base constructor (as it is an abstract class without constructors)
        /// </summary>
        protected void Construct(Tokenizer tokenizer, ThemeNames theme)
        {
            this.Tokenizer = tokenizer;
            this.Theme = Theme.Load(theme);
        }

        /// <summary>
        /// Serves as a base constructor (as it is an abstract class without constructors)
        /// </summary>
        protected void Construct(string themeFile)
        {
            this.Tokenizer = new Tokenizer();
            Theme = Theme.LoadFromFile(themeFile);
        }

        /// <summary>
        /// Serves as a base constructor (as it is an abstract class without constructors)
        /// </summary>
        protected void Construct(Tokenizer tokenizer, string themeFile)
        {
            this.Tokenizer = tokenizer;
            Theme = Theme.LoadFromFile(themeFile);
        }

        /// <summary>
        /// Method invoked before highlighting. It should prepare the output
        /// </summary>
        /// <param name="language"></param>
        /// <param name="docStyle"></param>
        /// <returns>style to be used as a parent for nested objects. If not modified return docStyle</returns>
        protected virtual ThemeStyle BeginDocument(string language, ThemeStyle docStyle) => null;

        /// <summary>
        /// Methode invoked after highlighting. It should finalize output in Result
        /// </summary>
        protected virtual void EndDocument() => Expression.Empty();

        /// <summary>
        /// Method invoked before highlighting a nested token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="style">resolved style for this token</param>
        /// <param name="parentStyle">style of the parent objectt</param>
        /// <returns>style to be used as a parent for nested objects. If not modified return style</returns>
        protected virtual ThemeStyle BeginContainer(Token token, ThemeStyle style, ThemeStyle parentStyle) => null;

        /// <summary>
        /// Method invoked after highlighting a nested token
        /// </summary>
        protected virtual void EndContainer() => Expression.Empty();

        /// <summary>
        /// Method invoked to render a text span to the output
        /// </summary>
        /// <param name="text">Actual text to output</param>
        /// <param name="token"></param>
        /// <param name="style">resolved style for this token</param>
        /// <param name="parentStyle">style of the parent objectt</param>
        protected abstract void AddSpan(string text, Token token, ThemeStyle style, ThemeStyle parentStyle);

        private void Walk(Token token, string language, ThemeStyle parentStyle = null)
        {
            if (!token.Tokens.Any())
            {
                if (token.Type == null)
                {
                    AddSpan(TrimTokenText(token), token, null, parentStyle);
                }
                else
                {
                    var style = Theme?.GetTokenStyle(token, language);
                    AddSpan(TrimTokenText(token), token, style, parentStyle);
                }
                return;
            }

            if (token.Tokens.Any())
            {
                var style = Theme?.GetTokenStyle(token, language);
                var newParent = BeginContainer(token, style, parentStyle);
                foreach (var t in token.Tokens)
                {
                    if (token.AbsolutePos + token.Text.Length > rangeStart && token.AbsolutePos < rangeEnd) Walk(t, language, newParent);
                }
                EndContainer();
            }
        }

        private string TrimTokenText(Token token)
        {
            string tokenText;
            if (token.AbsolutePos < rangeStart || token.AbsolutePos + token.Text.Length > rangeEnd)
            {
                var s = rangeStart > token.AbsolutePos ? rangeStart - token.AbsolutePos : 0;
                var e = token.AbsolutePos + token.Text.Length > rangeEnd ? rangeEnd - token.AbsolutePos : token.Text.Length;
                tokenText = token.Text.Substring(s, e - s);
            }
            else
            {
                tokenText = token.Text;
            }
            return tokenText;
        }
    }
}