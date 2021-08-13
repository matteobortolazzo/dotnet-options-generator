using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace OptionsGenerator
{
    internal static class OptionClassGenerator
    {
        public static void Generate(GeneratorExecutionContext context, string @namespace, JsonProperty property)
        {
            var source = SourceText.From($@"
namespace {@namespace}
{{
    public class {property.Name}
    {{{GetProperties(context, @namespace, property)}
    }}
}}", Encoding.UTF8);
            context.AddSource($"{property.Name}.cs", source);
        }

        private static string GetProperties(GeneratorExecutionContext context, string @namespace, JsonProperty root)
        {
            var sb = new StringBuilder();

            foreach (var property in root.Value.EnumerateObject())
            {
                if (property.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
                {
                    continue;
                }

                string? type;
                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    Generate(context, @namespace, property);
                    type = property.Name;
                }
                else
                {
                    type = GetType(property.Value);
                }

                sb.Append($"public {type} {property.Name} {{ get; set; }}\n");
            }
            return sb.ToString();
        }

        private static string GetType(JsonElement propertyValue)
        {
            if (propertyValue.ValueKind == JsonValueKind.Array)
            {
                var enumerator = propertyValue.EnumerateArray();
                enumerator.MoveNext();
                var itemType = GetType(enumerator.Current);
                return $"{itemType}[]";
            }

            return propertyValue.ValueKind switch
            {
                JsonValueKind.Number => propertyValue.TryGetDouble(out _) ? "float" : "int",
                JsonValueKind.String => "string",
                JsonValueKind.True or JsonValueKind.False => "bool",
                _ => throw new ArgumentException($"Unexpected value type: {propertyValue.ValueKind}.")
            };
        }
    }
}
