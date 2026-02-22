---
layout: post
title: "Building a REST API Client Code Generator with the New Visual Studio Extensibility Model"
date: 2026-01-04 12:00:00 +0100
categories: [visual-studio, extensibility, dotnet, csharp]
tags: [visual-studio, extensibility, dotnet, csharp, openapi, swagger, rest, api, client, codegen]
description: "A deep dive into migrating the REST API Client Code Generator extension to the new out-of-process Visual Studio Extensibility Model running on .NET 8.0."
image: /assets/images/rapicgen-background-task-progress.png
---

I'm excited to announce the release of a new version of the **REST API Client Code Generator** extension for Visual Studio! This new version is built from the ground up using the new **Visual Studio.Extensibility** model, allowing it to run out-of-process and leverage the full power of .NET 8.0.

The source code for the new extension is available at `..\apiclientcodegen\src\VSIX\ApiClientCodeGen.VSIX.Extensibility`.

## Why the Change?

The previous version of the extension (source at `..\apiclientcodegen\src\VSIX\ApiClientCodeGen.VSIX.Dev17`) was built using the traditional Visual Studio SDK. While powerful, this model runs **in-process** with Visual Studio, meaning it shares the same memory space and dependencies as the IDE itself.

### The "Dependency Hell"

One of the biggest challenges with the in-process model is dependency conflicts. For example, if your extension relies on a specific version of `Newtonsoft.Json` or `System.Text.Json`, but Visual Studio itself uses a different version, you run into runtime errors that are incredibly difficult to debug. This is known as "Dependency Hell".

By moving to the new out-of-process extensibility model, the extension runs in a separate process from Visual Studio. This isolation means:
1.  **No more dependency conflicts**: I can bring in any NuGet package version I need without worrying about what Visual Studio uses.
2.  **Performance**: The extension runs on .NET 8.0, which is significantly faster than the .NET Framework 4.8 runtime used by Visual Studio.
3.  **Stability**: If the extension crashes, it doesn't take down the entire IDE.

## A New User Experience

In the old version, the extension relied heavily on the **Single File Generator** (Custom Tool) mechanism. You would set the "Custom Tool" property on an OpenAPI file to `RefitterCodeGenerator` or `NSwagCodeGenerator`, and Visual Studio would automagically generate a dependent file.

The new extension moves away from this "magic" behavior. Instead, users explicitly right-click on an OpenAPI file and select **"Generate Client Code"**.

This change allows for better control, clearer feedback, and avoids the "silent failures" often associated with custom tools.

## Architecture Comparison

Let's look at how the code differs between the two models.

### Extension Entry Point

In the old VS SDK, the entry point was a class inheriting from `AsyncPackage`, decorated with numerous attributes to register menus, options pages, and UI contexts.

**Old VSPackage Entry Point (`VsPackage.cs`):**

```csharp
[Guid("47AFE4E1-5A52-4FE1-8CA7-EDB8310BDA4A")]
[ProvideMenuResource("Menus.ctmenu", 1)]
[ProvideUIContextRule(...)]
[ProvideOptionPage(typeof(GeneralOptionPage), VsixName, GeneralOptionPage.Name, 0, 0, true)]
public sealed class VsPackage : AsyncPackage
{
    private readonly ICommandInitializer[] commands = {
        new AutoRestCodeGeneratorCustomToolSetter(),
        new NSwagCodeGeneratorCustomToolSetter(),
        // ...
    };

    protected override async Task InitializeAsync(
        CancellationToken cancellationToken,
        IProgress<ServiceProgressData> progress)
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        await base.InitializeAsync(cancellationToken, progress);
        
        foreach (var command in commands)
            await command.InitializeAsync(this, cancellationToken);
    }
}
```

The new model is much cleaner. The entry point inherits from `Extension`, and features like menus are defined using a fluent, strongly-typed API rather than obscure `.vsct` files and attributes.

**New Extension Entry Point (`ExtensionEntrypoint.cs`):**

```csharp
[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
            id: "f7530eb1-1ce9-46ac-8fab-165b68cf3d61",
            version: ExtensionAssemblyVersion,
            displayName: "REST API Client Code Generator (PREVIEW)",
            description: "Generate REST API client code from OpenAPI/Swagger specifications")
    };

    [VisualStudioContribution]
    public static MenuConfiguration GenerateMenu => new("%ApiClientCodeGenerator.GroupDisplayName%")
    {
        Placements = [KnownPlacements.ItemNode],
        Children = [
            MenuChild.Command<Commands.GenerateRefitterCommand>(),
            MenuChild.Command<Commands.GenerateNSwagCommand>(),
            // ...
        ],
    };

    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ExtensionSettingsProvider>();
        base.InitializeServices(serviceCollection);
    }
}
```

Notice the use of **Dependency Injection** (`InitializeServices`) and the declarative menu configuration.

### Commands and Async Execution

The old model required strict thread management. You had to constantly switch to the UI thread using `ThreadHelper.ThrowIfNotOnUIThread()` to interact with the IDE, which could lead to UI freezes.

**Old Command (`SingleFileCodeGenerator.cs`):**

```csharp
public int Generate(
    string wszInputFilePath,
    string bstrInputFileContents,
    string wszDefaultNamespace,
    IntPtr[] rgbOutputFileContents,
    out uint pcbOutput,
    IVsGeneratorProgress pGenerateProgress)
{
    // Strict thread affinity check
    if (!TestingUtility.IsRunningFromUnitTest)
        ThreadHelper.ThrowIfNotOnUIThread();

    // Blocking generation on the UI thread
    var codeGenerator = Factory.Create(...);
    var code = codeGenerator.GenerateCode(progressReporter);
    
    // ...
}
```

The new model is **async by default** and runs on a background thread. Interaction with the IDE is done via asynchronous service calls.

**New Command (`RefitterCommands.cs`):**

```csharp
[VisualStudioContribution]
public class GenerateRefitterCommand(TraceSource traceSource, ExtensionSettingsProvider settingsProvider)
    : GenerateRefitterBaseCommand(traceSource, settingsProvider)
{
    public override async Task ExecuteCommandAsync(
        IClientContext context,
        CancellationToken cancellationToken)
    {
        // Fully async execution
        await GenerateCodeAsync(
            await context.GetInputFileAsync(cancellationToken),
            await context.GetDefaultNamespaceAsync(cancellationToken),
            cancellationToken);
    }
}
```

### Progress Reporting

Since operations run asynchronously, reporting progress is crucial. The new SDK provides a nice API for this, which shows a background task indicator in Visual Studio.

![Background Task Progress](/assets/images/rapicgen-background-task-progress.png)

Here is how progress reporting is implemented in the new command:

```csharp
public async Task GenerateCodeAsync(...)
{
    using var progress = await Extensibility.Shell().StartProgressReportingAsync(
        "Generating code with Refitter",
        new ProgressReporterOptions(true), // true = indeterminate/cancellable
        cancellationToken);

    progress.Report(10, $"Starting Refitter code generation for: {inputFilename}");

    // Do heavy lifting...
    var csharpCode = await GenerateCodeInternalAsync(..., progress, cancellationToken);

    progress.Report(90, "Writing generated code...");
    await File.WriteAllTextAsync(outputFile, csharpCode, cancellationToken);
}
```

### Settings API

The new Settings API is a huge improvement over the old `DialogPage` implementation. Settings are defined as data contracts and can be easily injected into commands.

![Settings Overview](/assets/images/rapicgen-vs-settings-overview.png)

### Built-in Prompt Dialogs

For simple inputs, like asking for a URL, we no longer need to create custom XAML windows or WinForms dialogs. The `ShowPromptAsync` method handles this natively.

Here is how we ask the user for an OpenAPI URL when adding a new file (`CommandExtensions.cs`):

```csharp
public static async Task<string?> AddNewOpenApiFileAsync(
    this Command command,
    IClientContext context,
    CancellationToken cancellationToken)
{
    var inputUrl = await command.Extensibility.Shell().ShowPromptAsync(
        "Enter URL to OpenAPI Specifications",
        new InputPromptOptions
        {
            DefaultText = "Example: https://petstore3.swagger.io/api/v3/openapi.json",
            Icon = ImageMoniker.KnownValues.URLInputBox,
            Title = "REST API Client Code Generator",
        },
        cancellationToken);

    return inputUrl;
}
```

## Reusing Core Logic

Despite the massive architectural changes in the presentation layer (the VSIX), the core business logic remains the same. Both the old and new extensions share the same **Core Library** (`..\apiclientcodegen\src\Core\ApiClientCodeGen.Core`).

This library contains the actual generator implementations for Refitter, NSwag, AutoRest, etc., ensuring that the generated code is identical regardless of which version of the extension you use.

## Conclusion

Migrating to the new Visual Studio Extensibility model has been a rewarding journey. The performance benefits of .NET 8.0 and the stability of the out-of-process model make it a clear winner for future development.

If you're interested in trying out the new extension, you can find the source code [here](..\apiclientcodegen\src\VSIX\ApiClientCodeGen.VSIX.Extensibility). I'd love to hear your feedback!
