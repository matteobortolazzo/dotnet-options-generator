[![Downloads](https://img.shields.io/nuget/dt/OptionsGenerator.svg)](https://www.nuget.org/packages/OptionsGenerator/)

# OptionsGenerator 

Generate and register `IOptions` from `appsettings.json` at compile-time with C# source generators.

## Getting started

* Install it from NuGet: [https://www.nuget.org/packages/OptionsGenerator/0.1.0-preview](https://www.nuget.org/packages/OptionsGenerator/0.1.0-preview)
* Make `Startup` a partial class:
```csharp
public partial class Startup { .... }
```
* In `ConfigureServices` add `RegisterOptions(services);`:
```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    RegisterOptions(services);
    ...
}
```
* If not there already, create a `Configuration` property:
```csharp
public IConfiguration Configuration { get; }
```
* In the `.csproj` add the following:
```xml
<ItemGroup>
    <AdditionalFiles Include="appsettings.json" />
</ItemGroup>
```
* Add objects to `appsettings.json`:
```json
{
  "MyOtherOptions": {
    "MyString": "any",
    "MyInt": 1,
    "MyDouble": 1.1,
    "MyBool": true,
    "MyObject": {
      "MyObjectString": "any"
    },
    "MyArray": [ "any" ]
  }
}
```
* Inject `IOptions<>`. Any change to `appsettings.json` will recreated the classes at compile time.
```csharp
public TodoController(
    ILogger<TodoController> logger,
    IOptions<MyOtherOptions> options)
{
    ...
}
```

## How it works

1. The generator parse `appsettings.json`;
1. For each objects in the root it creates an equivalent C# class.
1. Creates `Startup.Generated.cs` partial class with a private method `RegisterOptions`.
1. Inside the method, it adds a call to `services.Configure`.
