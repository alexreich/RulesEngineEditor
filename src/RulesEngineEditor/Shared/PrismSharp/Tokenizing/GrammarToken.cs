// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Orionsoft.PrismSharp.Tokenizing
{
    internal class GrammarToken
    {
        private readonly dynamic grammarToken;
        private readonly Grammar root;

        public string Name { get; set; }
        public List<GrammarPattern> Patterns { get; set; }

        public GrammarToken(dynamic grammarToken, Grammar root)
        {
            this.grammarToken = grammarToken;
            this.root = root;
        }

        internal void Parse()
        {
            Name = grammarToken.Name;
            Patterns = new List<GrammarPattern>();
            if (grammarToken.Value.Type == Newtonsoft.Json.Linq.JTokenType.String || grammarToken.Value.Type == null)
            {
                Patterns.Add(new GrammarPattern(grammarToken.Value, root));
                Patterns.Last().Parse();
            }
            else if (grammarToken.Value.Type == Newtonsoft.Json.Linq.JTokenType.Array)
            {
                foreach (var i in grammarToken.Value)
                {
                    if (JsonHelper.IsCircular(i))
                    {
                        throw new NotImplementedException();
                    }
                    Patterns.Add(new GrammarPattern(i, root));
                    Patterns.Last().Parse();
                }
            }
            else
            {
                throw new ArgumentException("Unexpected grammar token type");
            }
        }

        public override string ToString()
        {
            return "GT " + Name;
        }
    }
}