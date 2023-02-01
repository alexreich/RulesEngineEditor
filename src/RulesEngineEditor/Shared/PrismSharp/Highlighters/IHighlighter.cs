// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using Orionsoft.PrismSharp.Tokenizing;

namespace Orionsoft.PrismSharp.Highlighters
{
    /// <summary>
    /// Interface used by all highlighters
    /// </summary>
    /// <typeparam name="TResult">type of the result of highlighting, e.g. string</typeparam>
    public interface IHighlighter<out TResult>
    {
        /// <summary>
        /// Tokenizes and highlights a block of code
        /// </summary>
        /// <param name="language">name of programming language</param>
        /// <param name="code">source code to highlight</param>
        /// <returns></returns>
        TResult Highlight(string code, string language);

        /// <summary>
        /// Highlights a block of already tokenized code
        /// </summary>
        /// <param name="language">name of programming language</param>
        /// <param name="tokenizedCode"></param>
        /// <returns></returns>

        TResult Highlight(Token tokenizedCode, string language);

        /// <summary>
        /// Tokenizes and highlights a part of the code
        /// </summary>
        /// <param name="start">offset in characters from the beginning of the source code</param>
        /// <param name="length">length of the block to be higlighted in characters</param>
        /// <param name="language">name of programming language</param>
        /// <param name="code">source code to highlight</param>
        /// <returns></returns>
        TResult HighlightRange(string code, int start, int length, string language);

        /// <summary>
        /// Highlights a part of already tokenized code
        /// </summary>
        /// <param name="start">offset in characters from the beginning of the source code</param>
        /// <param name="length">length of the block to be higlighted in characters</param>
        /// <param name="language">name of programming language</param>
        /// <param name="tokenizedCode"></param>
        TResult HighlightRange(Token tokenizedCode, int start, int length, string language);
    }
}