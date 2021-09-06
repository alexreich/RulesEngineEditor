using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RulesEngineEditor.Shared
{
    //originally from https://github.com/microsoft/PowerApps-Language-Tooling/blob/master/src/PAModel/Utility/JsonNormalizer.cs
    internal class JsonNormalizer
    {
        public static string Normalize(string jsonStr)
        {
            using (JsonDocument doc = JsonDocument.Parse(jsonStr))
            {
                return Normalize(doc.RootElement);
            } // free up array pool rent
        }

        public static string Normalize(JsonElement je)
        {
            var ms = new MemoryStream();
            JsonWriterOptions opts = new JsonWriterOptions
            {
                Indented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                //Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            using (var writer = new Utf8JsonWriter(ms, opts))
            {
                Write(je, writer);
            }

            var bytes = ms.ToArray();
            var str = Encoding.UTF8.GetString(bytes);
            return str;
        }

        private static void Write(JsonElement property, Utf8JsonWriter writer)
        {
            switch (property.ValueKind)
            {
                case JsonValueKind.Array:
                    int x = 0;
                    foreach (JsonElement je in property.EnumerateArray())
                    {
                        if (x == 0)
                        {
                            writer.WriteStartArray();
                        }
                        Write(je, writer);
                        x++;
                    }
                    writer.WriteEndArray();
                    break;
                case JsonValueKind.Object:
                    int y = 0;
                    List<JsonProperty> lo = new List<JsonProperty>();
                    var namedProperties = property.EnumerateObject().Where(o => o.Name.Contains("Name"));
                    if (namedProperties.Count() > 0)
                        lo.Add(namedProperties.First());
                    var paramsProperties = property.EnumerateObject().Where(o => o.Name.Contains("Param"));
                    if (paramsProperties.Count() > 0)
                        lo.Add(paramsProperties.First());
                    lo.AddRange(property.EnumerateObject().Except(namedProperties).Except(paramsProperties));
                    //var eo = property.EnumerateObject().OrderBy(o => o.Name.Contains("Name"));
                    foreach (JsonProperty jp in lo)
                    {
                        if (jp.Value.ValueKind is JsonValueKind.Array)
                        {
                            if (jp.Value.EnumerateArray().Count() == 0)
                            {
                                continue;
                            }
                        }
                        if (jp.Name.Contains("RuleExpressionType"))
                        {
                            continue;
                        }
                        if (y == 0)
                        {
                            writer.WriteStartObject();
                        }
                        writer.WritePropertyName(jp.Name);
                        Write(jp.Value, writer);
                        y++;
                    }
                    if (y > 0)
                    {
                        writer.WriteEndObject();
                    }
                    break;
                default:
                    property.WriteTo(writer);
                    //writer.Flush();
                    break;
            }
        }

    }
}
