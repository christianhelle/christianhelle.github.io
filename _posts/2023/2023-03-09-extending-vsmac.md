---
layout: post
title: Extending Visual Studio for Mac 2022
date: '2023-03-08'
author: Christian Helle
tags: 
- Visual Studio for Mac
redirect_from:
- /2023/03/extending-vsmac
- /2023/03/extending-vsmac
- /2023/extending-vsmac/
- /2023/extending-vsmac
- /extending-vsmac/
- /extending-vsmac
---

The extensibility story for [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) was almost non-existent for a while, and the [documentation for getting started](https://learn.microsoft.com/en-us/previous-versions/visualstudio/mac/extending-visual-studio-mac-walkthrough?WT.mc_id=DT-MVP-5004822) was really outdated. [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) was originally a re-branding of Xamarin Studio, which was built over MonoDevelop. The extensibility SDK's we used for the longest time was still all from the old MonoDevelop Addin libraries.

For [Visual Studio for Mac 2022](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) this has changed and now we can create [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) extensions using the [Microsoft.VisualStudioMac.Sdk](https://www.nuget.org/packages/Microsoft.VisualStudioMac.Sdk) library that we can install from [nuget.org]([Microsoft.VisualStudioMac.Sdk](https://www.nuget.org/packages/Microsoft.VisualStudioMac.Sdk)). To make things even better, we can now break free of our old .NET Framework 4.x shackles and start targetting .NET 7.0. and all it's goodness

As of the time I'm writing this, there is still no File->New->Extension Project, but it's not hard to get started either. 

## Walkthrough

In this walkthrough, we will build a [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) simple extension that adds a menu item to the **Edit** menu that just inserts the text `"// Hello"` to the active document from the current cursor position

### Step 1 - Create New Project

Let's start with creating a new project called `Sample.csproj`

Here's how a csproj file for an empty [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) extension project looks like:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.VisualStudioMac.Sdk" Version="17.0.0" />
  </ItemGroup>
</Project>
```

### Step 2 - Addin Manifest

Then we create a `Manifest.addin.xml` file

```xml
<?xml version="1.0" encoding="UTF-8"?>
<ExtensionModel>
    <Extension path = "/MonoDevelop/Ide/Commands/Edit">
        <Command id = "Sample.SampleCommands.InsertText"
            _label = "Insert Text"
            defaultHandler = "Sample.InsertTextHandler" />
    </Extension>

    <Extension path = "/MonoDevelop/Ide/MainMenu/Edit">
        <CommandItem id="Sample.SampleCommands.InsertText" />
    </Extension>
</ExtensionModel>
```

### Step 3 - Implementation

Next we will need to implement the `InsertTextHandler` that is only avaiable when an active document is open

```cs
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using System;

namespace Sample
{
    public class InsertTextHandler : CommandHandler
    {
        protected override void Run()
        {
            var textBuffer = IdeApp.Workbench.ActiveDocument.GetContent<ITextBuffer>();
            var textView = IdeApp.Workbench.ActiveDocument.GetContent<ITextView>();
            textBuffer.Insert(textView.Caret.Position.BufferPosition.Position, "// Hello");
        }

        protected override void Update(CommandInfo info)
        {
            var textBuffer = IdeApp.Workbench.ActiveDocument.GetContent<ITextBuffer>();
            if (textBuffer != null && textBuffer.AsTextContainer() is SourceTextContainer container)
            {
                var document = container.GetTextBuffer();
                if (document != null)
                {
                    info.Enabled = true;
                }
           }
        }
    }

    public enum SampleCommands
    {
        InsertText,
    }
}
```

### Step 4 - Addin info

Now we need to brand and version the extension, we do this by specifying some `AddIn` information

```cs
using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
    "Sample",
    Namespace = "Sample",
    Version = "1.0"
)]

[assembly: AddinName("My First Extension")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinDescription("My first Visual Studio for Mac extension")]
[assembly: AddinAuthor("Christian Resma Helle")]
```

### Step 5 - Package the extension

With the new SDK's you can build the project from the command line simply by using `dotnet build`. Running `dotnet build` will ONLY build the project, it will not create the distributable `.mpack` package. 

Let's start with building the project

```bash
$ dotnet build -c Release Sample.csproj
```

This will produce the `bin/Release/net7.0/Sample.dll` file

To create the `.mpack` package, we need to run the **Visual Studio Tool Runner** a.k.a. `vstool`. The Visual Studio Tool Runner is included in the [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) installation. The **Visual Studio Tool Runner** is available from the following path

```bash
$ /Applications/Visual\ Studio.app/Contents/MacOS/vstool
```

To create the `.mpack` package, you need to run the **Visual Studio Extension Setup Utility** `pack` command

```bash
$ /Applications/Visual\ Studio.app/Contents/MacOS/vstool setup pack [absolute path to main output DLL] -d:[absolute path to output folder]
```

A little tip for getting the absolute path is to use `$PWD`. So if you created your project under the `~/projects/my-extension` folder and this is currently your working directory then you can do something like

```bash
$ /Applications/Visual\ Studio.app/Contents/MacOS/vstool setup pack $PWD/Sample.dll -d:$PWD
```

The command above will produce the output `~/projects/my-extension/Sample.mpack`

### Step 6 - Test the extension

Debugging a [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) is possible, but doesn't come out of the box. To enable Debugging the extension from [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) we need to add the following to our C# project

```xml
<PropertyGroup Condition=" '$(RunConfiguration)' == 'Default' ">
  <StartAction>Program</StartAction>
  <StartProgram>\Applications\Visual Studio.app\Contents\MacOS\VisualStudio</StartProgram>
  <StartArguments>--no-redirect</StartArguments>
  <ExternalConsole>true</ExternalConsole>
</PropertyGroup>
```

Now our Sample project should look something like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(RunConfiguration)' == 'Default' ">
    <StartAction>Program</StartAction>
    <StartProgram>\Applications\Visual Studio.app\Contents\MacOS\VisualStudio</StartProgram>
    <StartArguments>--no-redirect</StartArguments>
    <ExternalConsole>true</ExternalConsole>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.VisualStudioMac.Sdk" Version="17.0.0" />
  </ItemGroup>
</Project>
```

Debugging the extension will basically start another instance of [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) where you can test your extension

Try it out and if all goes well the **Edit** menu should have the **Insert Text** item at the bottom

![](/assets/images/extending-vsmac-debug.png)

![](/assets/images/extending-vsmac-edit-menu.png)

### Step 7 - Install extension

If you followed Step 5, then you should already have a `.mpack` at hand. 

![](/assets/images/extending-vsmac-extensions.png)

![](/assets/images/extending-vsmac-install-from-file.png)

![](/assets/images/extending-vsmac-open-file.png)

![](/assets/images/extending-vsmac-install.png)

![](/assets/images/extending-vsmac-installed.png)

You need to restart [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) at this point before you can see our new extension under the **Edit** menu

![](/assets/images/extending-vsmac-edit-menu.png)