// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Orionsoft.PrismSharp.Tokenizing
{
    internal class GrammarPattern
    {
        private readonly dynamic grammarPattern;
        private readonly Grammar root;

        private Regex compiledPattern = null;
        public string Pattern { get; internal set; }

        public bool LookBehind { get; internal set; }
        public bool Greedy { get; internal set; }
        public List<string> Alias { get; internal set; }
        public Grammar Inside { get; internal set; }

        public GrammarPattern(dynamic grammarPatten, Grammar root)
        {
            this.grammarPattern = grammarPatten;
            this.root = root;
        }

        internal void Parse()
        {
            if (grammarPattern is string || grammarPattern.Type == Newtonsoft.Json.Linq.JTokenType.String)
            {
                Pattern = (string)grammarPattern;
                return;
            }
            foreach (var i in grammarPattern)
            {
                switch (i.Name)
                {
                    case "pattern":
                        if (JsonHelper.IsCircular(i))
                        {
                            throw new NotImplementedException();
                        }
                        Pattern = (string)i.Value; break;

                    case "lookbehind":
                        LookBehind = (bool)i.Value; break;

                    case "greedy":
                        Greedy = (bool)i.Value; break;

                    case "alias":
                        {
                            if (i.Value.Type == Newtonsoft.Json.Linq.JTokenType.String)
                            {
                                Alias = new List<string> { JsonHelper.GetString((string)i.Value) };
                            }
                            else if (i.Value.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                            {
                                Alias = new List<string>();
                                foreach (var a in (Newtonsoft.Json.Linq.JArray)i.Value)
                                {
                                    Alias.Add(JsonHelper.GetString((string)a));
                                }
                            }
                            else
                            {
                                throw new NotSupportedException();
                            }
                            break;
                        }
                    case "inside":
                        {
                            if ((JsonHelper.IsCircular(i.Value)))
                            {
                                Inside = JsonHelper.FindCircularReference((string)i.Value, root) as Grammar;
                                if (Inside == null)
                                {
                                    throw new TokenizerException("Circular definition error");
                                }
                            }
                            else
                            {
                                Inside = new Grammar(i.Value, root);
                                Inside.Parse();
                            }
                            break;
                        }

                    default:
                        break;
                }
            }
        }

        internal RxMatch Match(string text, int pos = 0)
        {
            if (compiledPattern == null)
            {
                var (pattern, flags) = JsonHelper.GetRegex(Pattern);
                var options = RegexOptions.ECMAScript // RegexOptions.None
                    | (flags.Contains("i") ? RegexOptions.IgnoreCase : RegexOptions.None)
                    | (flags.Contains("m") ? RegexOptions.Multiline : RegexOptions.None);

                if (flags.Contains("s")) throw new TokenizerException("Incompatible flag /i");

                compiledPattern = new Regex(pattern, options);
            }

            var match = compiledPattern.Match(text, pos);
            if (!match.Success) return null;

            var matched = match.Groups[0].Value;

            if (LookBehind)
            {
                return new RxMatch { Index = match.Index + match.Groups[1].Length, Match = matched.Substring(match.Groups[1].Length) };
            }
            else
            {
                return new RxMatch { Index = match.Index, Match = matched };
            }
        }

        #region javascript regex to .net regex translator

        /// <summary>
        /// Converter of js regex. Currently not used as it is ported to jsTools and regexes are translated during language exports for performance reasons
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
#pragma warning disable S1144 // Unused private types or members should be removed

        private (string pattern, string flags) Translate(string pattern, string flags)
        {
            var unescapedDot = @"(?<!\\)(?:\\\\)*(\.)";
            var unescapedDollar = @"(?<!\\)(?:\\\\)*(\$)";
            var charRange = @"(?<!\\)(?:\\\\)*\[(((?<!\\)\\])|((?<!\\)\\\\\\])|([^\]]))+\]";
            var emptyRange = @"(?<!\\)(?:\\\\)*(\[\])";
            var emptyNegativeRange = @"(?<!\\)(?:\\\\)*(\[\^\])";

            var dotRx = new Regex(unescapedDot);
            var dollarRx = new Regex(unescapedDollar);
            var rangeRx = new Regex(charRange);

            var toFix = new List<(int pos, char type)>();

            var rangeMatches = rangeRx.Matches(pattern);
            var dotMatches = dotRx.Matches(pattern);
            var dollarMatches = dollarRx.Matches(pattern);
            var erMatches = new Regex(emptyRange).Matches(pattern);
            var enrMatches = new Regex(emptyNegativeRange).Matches(pattern);

            AddNonRangedMatches(toFix, dotMatches, rangeMatches, '.');
            AddNonRangedMatches(toFix, dollarMatches, rangeMatches, '$');
            AddNonRangedMatches(toFix, erMatches, rangeMatches, '[');
            AddNonRangedMatches(toFix, enrMatches, rangeMatches, '^');

            foreach (var (pos, type) in toFix.OrderByDescending(x => x.pos))
            {
                Console.WriteLine(pattern);
                pattern = pattern.Remove(pos, 1);
                switch (type)
                {
                    case '.':
                        pattern = pattern.Insert(pos, "[^\\r\\n]"); break;

                    case '$':
                        pattern = pattern.Insert(pos, "(?:(?=\\r$)|$)"); break;

                    case '[':
                        pattern = pattern.Remove(pos, 1);
                        pattern = pattern.Insert(pos, @"[^\s\S]"); break;

                    case '^':
                        pattern = pattern.Remove(pos, 2);
                        pattern = pattern.Insert(pos, @"[\s\S]"); break;
                }
                Console.WriteLine(pattern);
            }

            return (pattern, flags);
        }

#pragma warning restore S1144 // Unused private types or members should be removed

        private void AddNonRangedMatches(List<(int pos, char type)> toFix, MatchCollection matches, MatchCollection rangeMatches, char id)
        {
            foreach (Match dm in matches)
            {
                var consumed = false;
                foreach (Match rm in rangeMatches)
                {
                    if (dm.Groups[1].Index > rm.Index && dm.Groups[1].Index + dm.Groups[1].Length <= rm.Index + rm.Length) consumed = true;
                }
                if (!consumed) toFix.Add((dm.Groups[1].Index, id));
            }
        }

        #endregion javascript regex to .net regex translator
    }
}