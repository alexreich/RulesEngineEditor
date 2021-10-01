// Copyright (c) Alex Reich.
// Licensed under the CC BY 4.0 License.

using System.Collections.Generic;
using System.Text.Json.Serialization;
using RulesEngine.Models;

namespace RulesEngineEditor.Models
{
    /// <summary>
    /// WorkflowData has convenience methods (e.g. Lists) for RE Workflows
    /// </summary>
    public class WorkflowData : Workflow
    {
        /// <summary>
        /// Reserved for Database / Entity Framework implementations
        /// </summary>
        [JsonIgnore]
        public int? Id { get; set; }
        public new List<RuleData> Rules { get; set; }
        public new List<ScopedParamData> GlobalParams { get; set; }
        public new string WorkflowName { get; set; }
        [JsonIgnore]
        public int Seq { get; set; }
    }
    public class RuleData : Rule
    {
        /// <summary>
        /// Reserved for Database / Entity Framework implementations
        /// </summary>
        [JsonIgnore]
        public int? Id { get; set; }
        public new List<RuleData> Rules { get; set; }
        public new List<ScopedParamData> LocalParams { get; set; }
        [JsonIgnore]
        public bool? IsSuccess { get; set; }
        [JsonIgnore]
        public string ExceptionMessage { get; set; }
        [JsonIgnore]
        public int Seq { get; set; }
    }
    /// <summary>
    /// ScopedParamData - inherited class to continue naming convention / reserve future functionality
    /// </summary>
    public class ScopedParamData : ScopedParam
    {
        /// <summary>
        /// Reserved for Database / Entity Framework implementations
        /// </summary>
        [JsonIgnore]
        public int? Id { get; set; }
        [JsonIgnore]
        public int Seq { get; set; }
    }
}