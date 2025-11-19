using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using RulesEngine.Models;
using RulesEngineEditor.Models;

namespace RulesEngineEditor.Shared
{
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(Workflow[]))]
    [JsonSerializable(typeof(RuleData))]
    [JsonSerializable(typeof(RuleActions))]
    [JsonSerializable(typeof(List<WorkflowData>))] // Added for WorkflowData list
    [JsonSerializable(typeof(InputRuleParameterDictionary))] // Added for InputRuleParameterDictionary
    [JsonSerializable(typeof(List<InputRuleParameterDictionary>))] // Added for list of InputRuleParameterDictionary
    [JsonSerializable(typeof(JsonElement))] // Added for dynamic JSON deserialization
    // Primitive types for Dictionary<string, object> values in InputRuleParameterDictionary
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(long))]
    [JsonSerializable(typeof(double))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(decimal))]
    [JsonSerializable(typeof(object))]
    public partial class RulesEngineJsonSourceContext : JsonSerializerContext
    {
        // Removed manually implemented members.
    }
}
