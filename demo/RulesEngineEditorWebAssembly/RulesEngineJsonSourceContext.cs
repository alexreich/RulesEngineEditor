using System.Text.Json.Serialization;
using RulesEngine.Models;
using RulesEngineEditor.Models;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Workflow))]
[JsonSerializable(typeof(Rule))]
[JsonSerializable(typeof(WorkflowData))]
public partial class RulesEngineJsonSourceContext : JsonSerializerContext
{
}
