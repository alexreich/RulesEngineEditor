// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

namespace Orionsoft.PrismSharp.Tokenizing
{
    internal class RematchOptions
    {
        public GrammarToken Cause { get; set; }
        public int PatternIdx { get; set; }
        public int Reach { get; set; }
    }
}