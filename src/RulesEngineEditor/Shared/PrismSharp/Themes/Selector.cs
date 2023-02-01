// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Orionsoft.PrismSharp.Themes
{
    internal class Selector
    {
        [JsonProperty("classes")]
        public List<string> Classes { get; set; }

        [JsonProperty("languages")]
        public List<string> Languages { get; set; }
    }
}