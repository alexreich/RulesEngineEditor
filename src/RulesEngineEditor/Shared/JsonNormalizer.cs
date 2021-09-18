// Copyright (c) Alex Reich.
// Licensed under the CC BY 4.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace RulesEngineEditor.Shared
{
    //based on https://github.com/microsoft/PowerApps-Language-Tooling/blob/master/src/PAModel/Utility/JsonNormalizer.cs
    internal class JsonNormalizer
    {
        public static string Normalize(string jsonStr)
        {
            if (jsonStr == null || jsonStr == "[]")
            {
                return jsonStr;
            }
            using (JsonDocument doc = JsonDocument.Parse(jsonStr))
            {
                return Normalize(doc.RootElement);
            }
        }

        public static string Normalize(JsonElement je)
        {
            var ms = new MemoryStream();
            JsonWriterOptions opts = new JsonWriterOptions {
                Indented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
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

                    //organize property names
                    List<JsonProperty> lo = new List<JsonProperty>();
                    var idProperties = property.EnumerateObject().Where(o => o.Name.Contains("Id"));
                    if (idProperties.Count() > 0)
                        lo.Add(idProperties.First());
                    var namedProperties = property.EnumerateObject().Where(o => o.Name.Contains("Name"));
                    if (namedProperties.Count() > 0)
                        lo.Add(namedProperties.First());
                    var paramsProperties = property.EnumerateObject().Where(o => o.Name.Contains("Param"));
                    if (paramsProperties.Count() > 0)
                        lo.Add(paramsProperties.First());
                    lo.AddRange(property.EnumerateObject().Except(idProperties).Except(namedProperties).Except(paramsProperties));
                    foreach (JsonProperty jp in lo)
                    {
                        if (jp.Value.ValueKind is JsonValueKind.Array)
                        {
                            //drop empty arrays
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
                    break;
            }
        }

    }
}
