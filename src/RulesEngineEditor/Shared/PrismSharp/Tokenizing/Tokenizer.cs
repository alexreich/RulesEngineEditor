// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Orionsoft.PrismSharp.Tokenizing
{
    /// <summary>
    /// Tokenizer engine converting the source code to a tree of tokens (code fractions with their logical meanings, e.g. foreach - keyword
    /// </summary>
    public sealed class Tokenizer
    {
        /// <summary>
        /// Returns list of supported languages
        /// </summary>
        public List<string> LanguageList { get => GetLanguageList(); }

        /// <summary>
        /// Directory with language definitions, should it be changed from default path
        /// </summary>
        public string GrammarDir { get; set; }

        internal Dictionary<string, Grammar> Grammars { get; }

        private readonly Regex stringRegex;
        private LanguageDescriptor[] languageDescriptors;
        private Dictionary<string, string> aliases;

        public Tokenizer()
        {
            Grammars = new Dictionary<string, Grammar>();
            GrammarDir = Path.Combine(Utils.AssemblyDirectory, "data/languages/");
            stringRegex = new Regex("([\"'])(?:\\\\(?:\\r\\n|[\\s\\S])|(?!\\1)[^\\\\\\r\\n])*\\1");
        }

        /// <summary>
        /// Tokenizes the source code according to the language rules
        /// </summary>
        /// <param name="code">source code to tokenize</param>
        /// <param name="language">programming language name or an alias</param>
        public Token Tokenize(string code, string language)
        {
            var grammar = LoadGrammar(language);
            var res = new Token();

            res.Tokenize(code, grammar, this);
            return res;
        }

        /// <summary>
        /// Tokenizes the specified range of the source code according to the language rules.
        /// Tokenization settings are loaded automatically according to the language param.
        /// </summary>
        /// <param name="code">source code to tokenize</param>
        /// <param name="language">programming language name or an alias</param>
        /// <param name="start"> starting position of the range within the code</param>
        /// <param name="length"> length of the range</param>
        public Token TokenizeRange(string code, int start, int length, string language)
        {
            Initialize();
            language = aliases.ContainsKey(language) ? aliases[language] : language;
            var settings = languageDescriptors.FirstOrDefault(x => x.Name == language)?.RangeTokenizationSettings;
            return TokenizeRange(code, start, length, language, settings);
        }

        /// <summary>
        /// Tokenizes the specified range of the source code according to the language rules and provided settings
        /// </summary>
        /// <param name="code">source code to tokenize</param>
        /// <param name="language">language name or an alias</param>
        /// <param name="start"> starting position of the range within the code</param>
        /// <param name="length"> length of the range</param>
        /// <param name="settings">custom range tokenization settings</param>
        public Token TokenizeRange(string code, int start, int length, string language, RangeTokenizationSettings settings)
        {
            var grammar = LoadGrammar(language);
            var res = new Token();

            int tokenizingStart = start;

            if (settings != null)
            {
                if (settings.SafePointAdjuster != null)
                {
                    tokenizingStart = MoveStartToSafePoint(code, start, settings.SafePoints);
                    tokenizingStart = settings.SafePointAdjuster(code, tokenizingStart);
                }
                else
                {
                    tokenizingStart = Math.Max(start - settings.PreRange, 0);
                    tokenizingStart = MoveStartToSafePoint(code, tokenizingStart, settings.SafePoints);
                }

                var tokenizingEnd = Math.Min(code.Length, start + length + settings.PostRange);

                tokenizingEnd = MoveEndToSafePoint(code, tokenizingEnd, settings.SafePoints);
                code = code.Substring(0, tokenizingEnd);
            }

            res.Tokenize(code, grammar, this, tokenizingStart);

            return res;
        }

        internal void Initialize()
        {
            // already initialized?
            if (aliases != null) return;

            languageDescriptors = JsonConvert.DeserializeObject<LanguageDescriptor[]>(File.ReadAllText(Path.Combine(GrammarDir, "languages.json")));
            aliases = languageDescriptors.Where(x => x.Aliases != null)
                .SelectMany(x => x.Aliases, (x, alias) => new KeyValuePair<string, string>(alias, x.Name))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private List<string> GetLanguageList()
        {
            Initialize();
            return languageDescriptors.Select(x => x.Name).ToList();
        }

        private int MoveStartToSafePoint(string code, int start, List<string> safePoints)
        {
            if (safePoints == null || !safePoints.Any()) return start;

            var candidates = new List<int>();
            foreach (var safePointDef in safePoints)
            {
                var safe = code.LastIndexOf(safePointDef, start);
                candidates.Add(safe == -1 ? 0 : safe + safePointDef.Length);
            }
            return candidates.Max();
        }

        private int MoveEndToSafePoint(string code, int end, List <string> safePoints)
        {
            if (safePoints == null || !safePoints.Any()) return end;

            var candidates = new List<int>();
            foreach (var safePointDef in safePoints)
            {
                var safe = code.IndexOf(safePointDef, end);
                candidates.Add(safe == -1 ? code.Length : safe);
            }
            return candidates.Min();
        }

        private int AdjustCLikeSavePoint(string code, int start)
        {
            // we try to find out, whether the start point is within a multiline comment and if so, adjust it to the beginning of the comment
            var maxLookBack = 1000000;
            var seekPos = start;
            while (seekPos > 0 && start - seekPos < maxLookBack)
            {
                var astPos = code.LastIndexOf('*', seekPos);
                if (astPos == -1) return start; // we're good - no comment

                if (code[astPos + 1] == '/')
                {
                    // closing tag of a comment found ?
                    if (!IsInString(code, astPos)) return start;
                    // keep looking
                }
                else if (astPos > 0 && code[astPos - 1] == '/')
                {
                    // comment opening tag?
                    if (!IsInString(code, astPos)) return Math.Max(0, astPos - 2);

                    // keep looking
                }
                else
                {
                    //just an unimportant *, keep looking
                }
                seekPos = astPos - 1;
            }
            return start;
        }

        private bool IsInString(string code, int astPos)
        {
            var lineStart = code.LastIndexOf('\n', astPos);

            if (lineStart == -1) lineStart = 0;
            else lineStart++;

            var quotesPos = code.IndexOf('\"', lineStart, astPos - lineStart);
            if (quotesPos == -1) return false; // no quoted string in this line before tag, so it is real

            // check with regex to be sure
            var lineEnd = code.IndexOf('\n', lineStart);
            lineEnd = lineEnd == -1 ? code.Length : lineEnd;
            var line = code.Substring(lineStart, lineEnd - lineStart);
            var matches = stringRegex.Matches(line);
            var ast = astPos - lineStart;
            foreach (Match m in matches)
            {
                if (m.Index < ast && m.Index + m.Length > ast) return true;
            }
            return false;
        }

        private Grammar LoadGrammar(string language)
        {
            Initialize();
            if (aliases.ContainsKey(language)) language = aliases[language];

            if (!Grammars.ContainsKey(language))
            {
                var jsonText = File.ReadAllText(Path.Combine(GrammarDir, language) + ".json");
                dynamic json = JObject.Parse(jsonText);
                var g = new Grammar(json);
                Grammars.Add(language, g);
                g.Parse();
                var desc = languageDescriptors.FirstOrDefault(x => x.Name == language);

                if (desc?.RangeTokenizationSettings?.SafePointAdjusterName == "clike") desc.RangeTokenizationSettings.SafePointAdjuster = AdjustCLikeSavePoint;
            }

            return Grammars[language];
        }
    }
}