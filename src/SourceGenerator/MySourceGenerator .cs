using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace OptionsGenerator
{
    [Generator]
    public class CustomGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);
            if (mainMethod == null)
            {
                return;
            }

            var startupNamespace = mainMethod.ContainingNamespace.ConstituentNamespaces[0].ToString();

            var appSettingsFile = context
                .AdditionalFiles
                .SingleOrDefault(f => f.Path.Contains("appsettings.json"));

            var jsonFileText = appSettingsFile?.GetText(context.CancellationToken);
            if (jsonFileText == null)
            {
                return;
            }

            var json = JsonDocument.Parse(jsonFileText.ToString());

            var properties = json.RootElement.EnumerateObject()
                .Where(f => f.Value.ValueKind == JsonValueKind.Object)
                .ToArray();

            foreach (var property in properties)
            {
                OptionClassGenerator.Generate(context, startupNamespace, property);
            }

            var startupGeneratedSource = SourceText.From($@"
using Microsoft.Extensions.DependencyInjection;

namespace {startupNamespace}
{{
    public partial class Startup
    {{
        private void RegisterOptions(IServiceCollection services)
        {{{GetOptionRegistrations(properties)}
        }}
    }}
}}", Encoding.UTF8);
            context.AddSource($"Startup.Generated.cs", startupGeneratedSource);
        }

        private static string GetOptionRegistrations(IEnumerable<JsonProperty> fieldsToGenerate)
        {
            var sb = new StringBuilder();

            foreach (var field in fieldsToGenerate)
            {
                sb.Append($"services.Configure<{field.Name}>(Configuration.GetSection(nameof({field.Name})));");
            }

            return sb.ToString();
        }
    }
}
