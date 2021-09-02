using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using RulesEngine.Models;

namespace RulesEngineEditor.Models
{
    public class WorkflowData : WorkflowRules
    {
        public new List<RuleData> Rules { get; set; }
        public new List<ScopedParamData> GlobalParams { get; set; }

        public new string WorkflowName { get; set; }

        public WorkflowData Copy()
        {
            return (WorkflowData)this.MemberwiseClone();
        }
    }
    public class RuleData : Rule
    {
        public new List<RuleData> Rules { get; set; }
        public new List<ScopedParamData> LocalParams { get; set; }
        [JsonIgnore]
        public bool? IsSuccess { get; set; }
        [JsonIgnore]
        public string ExceptionMessage { get; set; }

        public RuleData Copy()
        {
            return (RuleData)this.MemberwiseClone();
        }
    }
    public class ScopedParamData : ScopedParam
    {

    }
}