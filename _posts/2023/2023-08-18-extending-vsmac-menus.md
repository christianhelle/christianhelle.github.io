---
layout: post
title: Extending Menus and Commands in Visual Studio for Mac 2022
date: '2023-08-18'
author: Christian Helle
tags: 
- Visual Studio for Mac
redirect_from:
- /2023/03/extending-vsmac-menus/
- /2023/03/extending-vsmac-menus
- /2023/extending-vsmac-menus/
- /2023/extending-vsmac-menus
- /extending-vsmac-menus/
- /extending-vsmac-menus
---

This is step by step walkthrough guide to getting started with extending menus and commands in [Visual Studio for Mac 2022](https://visualstudio.microsoft.com/vs/mac?WT.mc_id=DT-MVP-5004822) with explanations, code examples, and a couple of links to offical documentation.

In a previous article called [Extending Visual Studio for Mac 2022](/2023/03/extending-vsmac), I explained the the following:

- Basics of extending Visual Studio for Mac
- The new Visual Studio for Mac SDK
- The AddIn manifest file
- Debugging your Visual Studio for Mac extension

For this article, I would like to take a deeper dive in extending menus and commands. But first, let's setup a Visual Studio for Mac extension project

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

Let's define some `AddIn` information in a file called `AddinInfo.cs`

```cs
using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(Id = "Sample", Namespace = "Sample", Version = "1.0")]
[assembly: AddinName("Custom Menus and Commands")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinDescription("Custom Menus and Commands")]
[assembly: AddinAuthor("Christian Resma Helle")]
```

## Implementing CommandHandler

## Extending the Edit menu


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