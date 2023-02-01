// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Orionsoft.PrismSharp.Tokenizing
{
    /// <summary>
    /// Object representing a token consisting of a source code text span or an array of nested tokens and a token type. It is returned by the tokenizer.
    /// </summary>
    public sealed class Token
    {
        private Tokenizer tokenizer;

        /// <summary>
        /// Text representation of the token in the source code
        /// </summary>
        public string Text { get; internal set; }

        /// <summary>
        /// Token type (if any), eg. keyword, punctuation, atc.
        /// </summary>
        public string Type { get; internal set; }

        /// <summary>
        /// Nested tokens, if the token has logical sub-sections
        /// </summary>
        public List<Token> Tokens { get; internal set; }

        /// <summary>
        /// position of the token text relative to the beginning of a parent token
        /// </summary>
        public int Pos { get; private set; }

        /// <summary>
        /// position of the token text relative to the beginning of a the tokenized document
        /// </summary>
        public int AbsolutePos { get; private set; }

        /// <summary>
        /// Aliases of the token type that are less specific. They are used by the highlighter, if the highlighter does not now the token type
        /// </summary>
        public List<string> Aliases { get; private set; }

        internal Token()
        {
            Tokens = new List<Token>();
        }

        internal void Tokenize(string text, Grammar grammar, Tokenizer tokenizer, int startPos = 0)
        {
            this.tokenizer = tokenizer;
            grammar.MergeRest();
            Text = text;
            Tokens.Add(new Token { Text = text.Substring(startPos) });
            MatchGrammar(text, grammar, startNode: -1, startPos);
        }

        public override string ToString()
        {
            return (Type ?? "TEXT") + ": " + Text;
        }

        private void MatchGrammar(string text, Grammar grammar, int startNode, int startPos, RematchOptions rematch = null)
        {
            foreach (var token in grammar.GrammarTokens)
            {
                for (var j = 0; j < token.Patterns.Count; ++j)
                {
                    if (rematch != null && rematch.Cause == token && rematch.PatternIdx == j)
                    {
                        return;
                    }

                    var patternObj = token.Patterns[j];
                    var greedy = patternObj.Greedy;
                    var alias = patternObj.Alias;
                    var inside = patternObj.Inside;

                    var pos = startPos;
                    for (var currentNode = startNode + 1; currentNode < Tokens.Count; currentNode++)
                    {
                        if (rematch != null && pos >= rematch.Reach)
                        {
                            break;
                        }

                        var str = Tokens[currentNode].Text;

                        if (Tokens.Count > text.Length)
                        {
                            // Something went terribly wrong, ABORT, ABORT!
                            return;
                        }

                        if (Tokens[currentNode].Type != null)
                        {
                            pos += Tokens[currentNode].Text.Length;
                            continue;
                        }

                        var removeCount = 1; // this is the to parameter of removeBetween
                        RxMatch match;

                        int from;

                        if (greedy)
                        {
                            match = patternObj.Match(text, pos);
                            if (match == null || match.Index >= text.Length)
                            {
                                break;
                            }

                            from = match.Index;
                            var to = match.Index + match.Match.Length;

                            var p = pos;

                            // find the node that contains the match
                            p += Tokens[currentNode].Text.Length;
                            while (from >= p)
                            {
                                currentNode++;
                                p += Tokens[currentNode].Text.Length;
                            }
                            // adjust pos (and p)
                            p -= Tokens[currentNode].Text.Length;
                            pos = p;

                            // the current node is a Token, then the match starts inside another Token, which is invalid
                            if (Tokens[currentNode].Type != null)
                            {
                                pos += Tokens[currentNode].Text.Length;
                                continue;
                            }

                            // find the last node which is affected by this match
                            for (
                            var k = currentNode;
                            k < Tokens.Count && (p < to || Tokens[k].Type == null);
                            k++
                        )
                            {
                                removeCount++;
                                p += Tokens[k].Text.Length;
                            }
                            removeCount--;

                            // replace with the new match
                            str = text.Substring(pos, p - pos);
                            match.Index -= pos;
                        }
                        else
                        {
                            match = patternObj.Match(str, 0);
                            if (match == null)
                            {
                                pos += Tokens[currentNode].Text.Length;
                                continue;
                            }
                        }

                        from = match.Index;
                        var matchStr = match.Match;
                        var before = str.Substring(0, from);
                        var after = str.Substring(from + matchStr.Length);
                        var reach = pos + str.Length;

                        if (rematch != null && reach > rematch.Reach)
                        {
                            rematch.Reach = reach;
                        }

                        var removeFrom = currentNode - 1;

                        if (before.Any())
                        {
                            Tokens.Insert(removeFrom + 1, new Token { Text = before, Pos = pos, AbsolutePos = AbsolutePos + pos });
                            removeFrom++;
                            pos += before.Length;
                        }

                        for (var i = 0; i < removeCount; i++) Tokens.RemoveAt(removeFrom + 1);

                        var wrapped = new Token { Text = matchStr, Type = token.Name, Pos = pos, AbsolutePos = AbsolutePos + pos, Aliases = alias ?? new List<string>() };
                        if (inside != null) wrapped.Tokenize(matchStr, inside, tokenizer);

                        Tokens.Insert(removeFrom + 1, wrapped);
                        currentNode = removeFrom + 1;
                        if (after.Any())
                        {
                            Tokens.Insert(currentNode + 1, new Token { Text = after, Pos = pos + matchStr.Length, AbsolutePos = AbsolutePos + pos + matchStr.Length });
                        }

                        if (removeCount > 1)
                        {
                            //	// at least one Token object was removed, so we have to do some rematching
                            //	// this can only happen if the current pattern is greedy
                            var nestedRematch = new RematchOptions
                            {
                                Cause = token,
                                PatternIdx = j,
                                Reach = reach
                            };

                            MatchGrammar(text, grammar, currentNode - 1, pos, nestedRematch);

                            //// the reach might have been extended because of the rematching
                            if (rematch != null && nestedRematch.Reach > rematch.Reach)
                            {
                                rematch.Reach = nestedRematch.Reach;
                            }
                        }
                        pos += Tokens[currentNode].Text.Length;
                    }
                }
            }

            if (Tokens.Count == 1 && Tokens[0].Type == null)
            {
                // the token does not actually contain children, it is the text
                Tokens.RemoveAt(0);
            }
        }

        #region Simple stream methods - for engine testing only

        internal StringBuilder ToSimpleStream(bool pretty, StringBuilder sb = null, int indent = 0)
        {
            sb = sb ?? new StringBuilder();

            var tokens = Tokens.Where(x => !pretty || x.Tokens.Any() || (x.Text?.Trim().Length ?? -1) > 0 || x.Type != null);

            if (Type != null || tokens.Any())
            {
                sb.Append(new string(' ', indent * 4));
                sb.Append("[");
                if (Type != null) sb.Append("\"" + Type + "\", ");

                if (tokens.Any())
                {
                    sb.AppendLine();
                    if (Type != null && tokens.Count() > 1)
                    { sb.Append(new string(' ', (1 + indent) * 4)); sb.AppendLine("["); }

                    foreach (var t in tokens)
                    {
                        t.ToSimpleStream(pretty, sb, indent + 2);
                        if (t != tokens.Last()) sb.AppendLine(",");
                    }
                    if (Type != null && tokens.Count() > 1)
                    { sb.Append("\r\n" + new string(' ', (1 + indent) * 4)); sb.AppendLine("]"); }

                    sb.AppendLine();
                    sb.Append(new string(' ', indent * 4));
                }
                else
                {
                    if (Type != null && Text.Trim().Length == 0) Text = Text.Trim();

                    sb.Append("\"" + Escape(Text) + "\"");
                }
                sb.Append("]");
            }
            else
            {
                sb.Append(new string(' ', indent * 4));
                sb.Append("\"" + Escape(Text) + "\"");
            }
            return sb;
        }

        internal bool IsPrettyEqual(Token other)
        {
            if ((!Tokens.Any(IsPrettyToken()) && Text?.Trim() != other.Text?.Trim())
                || Type != other.Type || Tokens.Count(IsPrettyToken()) != other.Tokens.Count(IsPrettyToken())) return false;

            for (var i = 0; i < Tokens.Count(IsPrettyToken()); i++)
            {
                if (!Tokens.Where(IsPrettyToken()).ToList()[i].IsPrettyEqual(other.Tokens.Where(IsPrettyToken()).ToList()[i])) return false;
            }
            return true;
        }

        internal string GetFirstDiff(Token other)
        {
            return FindDiff(other, "~.").Message;
        }

        internal static string Escape(string text)
        {
            if (text == null) return null;
            var res = Regex.Escape(text);

            // we dont want to escape # and spaces and dots and +, (, }
            res = res.Replace("\\#", "#").Replace("\\ ", " ").Replace("\\.", ".").Replace("\\+", "+")
            .Replace("\\(", "(").Replace("\\)", ")")
            .Replace("\\[", "[").Replace("\\]", "]")
            .Replace("\\{", "{").Replace("\\}", "}")
            .Replace("\\*", "*").Replace("\\^", "^")
            .Replace("\\?", "?").Replace("\\$", "$")
            .Replace("\\|", "|");
            return res;
        }

        internal static string UnEscape(string text)
        {
            return Regex.Unescape(text);
        }

        private (bool Success, string Message) FindDiff(Token other, string path)
        {
            if (Type != other.Type)
            {
                path += $"Type '{Type}' != '{other.Type}'";
                return (false, path);
            }
            if ((!Tokens.Any(IsPrettyToken()) && Text?.Replace("\r", "") != other.Text?.Replace("\r", "")))
            {
                path += $"Text '{Escape(Text)}' != '{Escape(other.Text)}'";
                return (false, path);
            }
            if (Tokens.Count(IsPrettyToken()) != other.Tokens.Count(IsPrettyToken()))
            {
                path += $"TokenCount {Tokens.Count} != {other.Tokens.Count}";
                return (false, path);
            }

            for (var i = 0; i < Tokens.Count(IsPrettyToken()); i++)
            {
                var token = Tokens.Where(IsPrettyToken()).ToList()[i];
                var otherToken = other.Tokens.Where(IsPrettyToken()).ToList()[i];
                var res = token.FindDiff(otherToken, path + $"[{i}]'{token.Type ?? "RAW"}'.");
                if (!res.Success) return res;
            }
            return (true, "no difference");
        }

        private static Func<Token, bool> IsPrettyToken()
        {
            return x => x.Tokens.Any() || (x.Text?.Trim().Length ?? -1) > 0 || x.Type != null;
        }

        #endregion Simple stream methods - for engine testing only
    }
}