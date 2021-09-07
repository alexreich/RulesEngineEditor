using System.Collections.Generic;
using System.Text.Json.Serialization;
using RulesEngine.Models;

namespace RulesEngineEditor.Models
{
    /// <summary>
    /// WorkflowData has convenience methods (e.g. Lists) for RE Workflows
    /// </summary>
    public class WorkflowData : WorkflowRules
    {
        public new List<RuleData> Rules { get; set; }
        public new List<ScopedParamData> GlobalParams { get; set; }

        public new string WorkflowName { get; set; }
    }
    public class RuleData : Rule
    {
        public new List<RuleData> Rules { get; set; }
        public new List<ScopedParamData> LocalParams { get; set; }
        [JsonIgnore]
        public bool? IsSuccess { get; set; }
        [JsonIgnore]
        public string ExceptionMessage { get; set; }
    }
    public class ScopedParamData : ScopedParam
    {

    }
}