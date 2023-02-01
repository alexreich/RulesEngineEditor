// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Orionsoft.PrismSharp.Tokenizing
{
    internal class Grammar
    {
        private readonly dynamic grammar;
        private readonly Grammar root;

        public List<GrammarToken> GrammarTokens { get; set; }
        public Grammar Rest { get; internal set; }

        public Grammar(dynamic grammar, Grammar root = null)
        {
            root = root ?? this;
            this.grammar = grammar;
            this.root = root;

            GrammarTokens = new List<GrammarToken>();
        }

        /// <summary>
        /// Parses the language definition to a usable form
        /// </summary>
        internal void Parse()
        {
            foreach (var item in grammar)
            {
                object circular = null;

                if (JsonHelper.IsCircular(item.Value))
                {
                    circular = JsonHelper.FindCircularReference((string)item.Value, root);
                }

                if (item.Name == "rest")
                {
                    if (circular != null)
                    {
                        Rest = circular as Grammar;
                    }
                    else if (item.Value.Type == Newtonsoft.Json.Linq.JTokenType.Null)
                    {
                        Rest = null;
                    }
                    else
                    {
                        Rest = new Grammar(item.Value, root);
                        Rest.Parse();
                    }
                }
                else
                {
                    if (circular != null)
                    {
                        GrammarTokens.Add(circular as GrammarToken);
                    }
                    else if (item.Value.Type == Newtonsoft.Json.Linq.JTokenType.Null)
                    {
                        GrammarTokens.Add(new GrammarToken(item, null));
                    }
                    else
                    {
                        GrammarTokens.Add(new GrammarToken(item, root));
                        GrammarTokens.Last().Parse();
                    }
                }
            }
        }

        internal void MergeRest()
        {
            if (Rest != null)
            {
                foreach (var i in Rest.GrammarTokens)
                {
                    if (GrammarTokens.Any(x => x.Name == i.Name))
                    {
                        Console.WriteLine("Replacing existing token in grammar: " + i.Name);
                        var pos = GrammarTokens.IndexOf(GrammarTokens.FirstOrDefault(x => x.Name == i.Name));
                        GrammarTokens.RemoveAt(pos);
                        GrammarTokens.Insert(pos, i);
                    }
                    else
                    {
                        GrammarTokens.Add(i);
                    }
                }
                Rest = null;
            }
        }
    }
}