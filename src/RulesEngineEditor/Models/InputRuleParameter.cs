// Copyright (c) Alex Reich.
// Licensed under the CC BY 4.0 License.

using System.Collections.Generic;

namespace RulesEngineEditor.Models
{
    /// <summary>
    /// InputRuleParameter - Collection of InputParameters used in UI
    /// </summary>
    public class InputRuleParameter
    {
        public string InputRule { get; set; }
        public List<InputParameter> Parameter { get; set; }
    }

    /// <summary>
    /// InputRuleParameterDictionary - Used for Rules Engine as strings are evaled as primatives
    /// </summary>
    public class InputRuleParameterDictionary
    {
        public string InputRule { get; set; }
        public Dictionary<string, object> Parameter { get; set; }
    }
}
