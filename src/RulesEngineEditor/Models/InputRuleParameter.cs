using System.Collections.Generic;

namespace RulesEngineEditor.Models
{
    public class InputRuleParameter
    {
        public string InputRule { get; set; }
        public List<InputParam> Parameter { get; set; }
    }

    public class InputRuleParameterDictionary
    {
        public string InputRule { get; set; }
        public Dictionary<string, object> Parameter { get; set; }
    }
}
