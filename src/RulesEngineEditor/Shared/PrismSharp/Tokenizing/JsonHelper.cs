// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Orionsoft.PrismSharp.Tokenizing
{
    internal static class JsonHelper
    {
        internal static string GetString(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (!value.StartsWith("s:")) throw new ArgumentException("string with incorrect prefix: " + value);
            return value.Substring(2);
        }

        internal static (string Regex, string Flags) GetRegex(string value)
        {
            if (string.IsNullOrEmpty(value)) return ("", "");

            if (!value.StartsWith("r:")) throw new ArgumentException("regex with incorrect prefix: " + value);
            var match = Regex.Match(value.Substring(2), @"^\/(.*)\/([imsuyg]*)$");
            if (!match.Success) throw new ArgumentException("regex with incorrect form: " + value);

            return (match.Groups[1].Value, match.Groups[2].Value);
        }

        internal static bool IsCircular(dynamic value)
        {
            if (value.Type != Newtonsoft.Json.Linq.JTokenType.String) return false;

            var match = Regex.Match((string)value, "(?:^s:\\[Circular ~)(.*)\\]");
            return match.Success;
        }

        internal static object FindCircularReference(string circular, Grammar root)
        {
            var match = Regex.Match(circular, "(?:^s:\\[Circular ~)(.*)\\]");
            if (!match.Success) throw new TokenizerException("Invalid circular ref");
            circular = match.Groups[1].Value;

            if (string.IsNullOrEmpty(circular)) return root;
            var split = circular.Split(new[] { '.' });

            split = split.Skip(1).ToArray();
            var res = ScanCirculars(root, split);
            return res;
        }

        private static object ScanCirculars(object obj, string[] split)
        {
            if (!split.Any()) return obj;

            var id = split[0];
            int index = -1;
            if (split.Count() > 1 && int.TryParse(split[1], out index))
            {
                split = split.Skip(1).ToArray();
            }
            else
            {
                index = -1;
            }
            if (obj is Grammar)
            {
                if (id == "rest")
                {
                    obj = (obj as Grammar).Rest;
                    split = split.Skip(1).ToArray();
                    obj = ScanCirculars(obj, split);
                    if (!split.Any()) return obj;
                }
                else
                {
                    if (index == -1)
                    {
                        obj = (obj as Grammar).GrammarTokens.FirstOrDefault(x => x.Name == id);
                    }
                    else
                    {
                        obj = (obj as Grammar).GrammarTokens.FirstOrDefault(x => x.Name == id).Patterns[index];
                    }
                    split = split.Skip(1).ToArray();
                    obj = ScanCirculars(obj, split);
                    if (!split.Any()) return obj;
                }
            }
            else if (obj is GrammarToken)
            {
                if (id == "inside")
                {
                    if (index == -1)
                    {
                        obj = (obj as GrammarToken).Patterns[0].Inside;
                    }
                    else
                    {
                        obj = (obj as GrammarToken).Patterns[index];
                    }
                    split = split.Skip(1).ToArray();
                    obj = ScanCirculars(obj, split);
                    if (!split.Any()) return obj;
                }
                else
                {
                    obj = (obj as Grammar).GrammarTokens.FirstOrDefault(x => x.Name == id);
                    split = split.Skip(1).ToArray();
                    obj = ScanCirculars(obj, split);
                    if (!split.Any()) return obj;
                }
            }
            else if (obj is GrammarPattern)
            {
                if (id == "inside")
                {
                    obj = (obj as GrammarPattern).Inside;
                    split = split.Skip(1).ToArray();
                    obj = ScanCirculars(obj, split);
                    if (!split.Any()) return obj;
                }
                else throw new NotImplementedException();
            }

            return obj;
        }
    }
}