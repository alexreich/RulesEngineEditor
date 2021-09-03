using System.Collections.Generic;

namespace RulesEngineEditor.Models
{
    public class Input
    {
        public string InputName { get; set; }
        public List<InputParam> Parameter { get; set; }
    }

    public class NewInput
    {
        public string InputName { get; set; }
        public Dictionary<string, object> Parameter { get; set; }
    }
}
