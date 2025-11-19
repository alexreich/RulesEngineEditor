using System.Text.Json.Serialization;
using RulesEngine.Models;
using RulesEngineEditor.Models;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Workflow))]
[JsonSerializable(typeof(Rule))]
[JsonSerializable(typeof(WorkflowData))]
[JsonSerializable(typeof(RuleActions))]  // <-- Added for RuleActions
[JsonSerializable(typeof(InputRuleParameterDictionary))] // Added for InputRuleParameterDictionary
[JsonSerializable(typeof(System.Collections.Generic.List<InputRuleParameterDictionary>))] // Added for list
public partial class RulesEngineJsonSourceContext : JsonSerializerContext
{
}
