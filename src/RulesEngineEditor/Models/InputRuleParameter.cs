// Copyright (c) Alex Reich.
// Licensed under the CC BY 4.0 License.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RulesEngineEditor.Models
{
    /// <summary>
    /// InputRuleParameter - Collection of InputParameters used in UI
    /// </summary>
    public class InputRuleParameter
    {
        [Obsolete("InputRule is deprecated. Use InputRuleName instead.")]
        public string InputRule { get { return InputRuleName; } set { InputRuleName = value; } }
        public string InputRuleName { get; set; }

        [Obsolete("Parameter is deprecated. Use Parameters instead.")]
        public List<InputParameter> Parameter { get { return Parameters; } set { Parameters = value; } }
        public List<InputParameter> Parameters { get; set; } = new List<InputParameter>();
    }

    /// <summary>
    /// InputRuleParameterDictionary - Used for Rules Engine as strings are evaled as primatives
    /// </summary>
    public class InputRuleParameterDictionary
    {
        public string InputRuleName { get; set; }
        //[Obsolete("Parameter is deprecated. Use Parameters instead.")]
        //public Dictionary<string, object> Parameter { get { return Parameters; } set { Parameters = value; } }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
