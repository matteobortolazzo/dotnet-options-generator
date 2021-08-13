using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;

namespace OptionsGenerator
{
    [Generator]
    public class NetOptionsGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
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

            if (!properties.Any())
            {
                return;
            }

            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);
            if (mainMethod == null)
            {
                return;
            }
            var @namespace = mainMethod.ContainingNamespace.ConstituentNamespaces[0].ToString();

            StartupClassGenerator.Generate(context, @namespace, properties);
            foreach (var property in properties)
            {
                OptionClassGenerator.Generate(context, @namespace, property);
            }
        }
    }
}
