using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace OptionsGenerator
{
    internal static class StartupClassGenerator
    {
        private const string Indent = "            ";

        public static void Generate(GeneratorExecutionContext context, string @namespace, IList<JsonProperty> properties)
        {
            var source = SourceText.From($@"
using Microsoft.Extensions.DependencyInjection;

namespace {@namespace}
{{
    public partial class Startup
    {{
        private void RegisterOptions(IServiceCollection services)
        {{
{GetOptionRegistrations(properties)}
        }}
    }}
}}", Encoding.UTF8);
            context.AddSource("Startup.Generated.cs", source);
        }

        private static string GetOptionRegistrations(IEnumerable<JsonProperty> fieldsToGenerate)
        {
            var sb = new StringBuilder();

            foreach (var field in fieldsToGenerate)
            {
                sb.AppendLine($"{Indent}services.Configure<{field.Name}>(Configuration.GetSection(nameof({field.Name})));");
            }
            
            return sb.ToString();
        }
    }
}
