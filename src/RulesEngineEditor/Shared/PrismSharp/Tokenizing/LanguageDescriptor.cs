// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using Newtonsoft.Json;

namespace Orionsoft.PrismSharp.Tokenizing
{
    internal class LanguageDescriptor
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("aliases")]
        public string[] Aliases { get; set; }

        [JsonProperty("rangeTokenizationSettings")]
        public RangeTokenizationSettings RangeTokenizationSettings { get; set; }
    }
}