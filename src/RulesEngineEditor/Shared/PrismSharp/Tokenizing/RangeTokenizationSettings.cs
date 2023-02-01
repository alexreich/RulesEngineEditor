// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Orionsoft.PrismSharp.Tokenizing
{
    /// <summary>
    /// Settings describing how to tokenize a range of code, to avoid things like incorrectly tokenizing parts of a long preceding comment or string as a code, etc
    /// </summary>
    public class RangeTokenizationSettings
    {
        /// <summary>
        /// Array of relatively safe points to start/end tokenization. Typically a new line
        /// </summary>
        [JsonProperty("safePoints")]
        public List<string> SafePoints { get; set; }

        /// <summary>
        /// A custom method name to find a safe point to start. For c-like languages it is "clike" and is automatically used
        /// </summary>
        [JsonProperty("safePointAdjusterName")]
        public string SafePointAdjusterName { get; set; }

        /// <summary>
        /// Tokenization can start x characters before the actual range, to provide a safety pillow
        /// </summary>
        [JsonProperty("preRange")]
        public int PreRange { get; set; }

        /// <summary>
        /// Tokenization can end x characters after the actual range, to provide a safety pillow
        /// </summary>
        [JsonProperty("postRange")]
        public int PostRange { get; set; }

        /// <summary>
        /// A method delegate provided to find a safe point to start.
        /// </summary>
        public Func<string, int, int> SafePointAdjuster { get; set; }
    }
}