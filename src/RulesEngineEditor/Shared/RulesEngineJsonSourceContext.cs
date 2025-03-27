using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using RulesEngine.Models;
using RulesEngineEditor.Models;

namespace RulesEngineEditor.Shared
{
    [JsonSourceGenerationOptions(
        WriteIndented = true,
        GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(Workflow[]))]
    [JsonSerializable(typeof(RuleData))]
    [JsonSerializable(typeof(RuleActions))]
    public partial class RulesEngineJsonSourceContext : JsonSerializerContext
    {
        // Removed manually implemented members.
    }
}
