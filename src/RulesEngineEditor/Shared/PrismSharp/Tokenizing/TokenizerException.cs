// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using System;

namespace Orionsoft.PrismSharp.Tokenizing
{
    /// <summary>
    /// Exception that is thrown when a tokenizing error ocurs
    /// </summary>
    public class TokenizerException : Exception
    {
        public TokenizerException(string message) : base(message)
        {
        }

        public TokenizerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public TokenizerException()
        {
        }
    }
}