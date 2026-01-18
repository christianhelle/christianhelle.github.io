---
layout: post
title: Building REST API Client Code Generator as an extension for multiple IDE's
date: 2025-12-01
author: Christian Helle
tags:
- .NET
- Visual Studio
- Visual Studio for Mac
- Visual Studio Code
- JetBrains Rider
redirect_from:
- /2025/11/extending-ides/
- /2025/11/extending-ides
- /2025/extending-ides
- /extending-ides
---

A decade ago, I built a Visual Studio extension for generating client code for REST APIs from OpenAPI specifications, I wrote about that in [this blog post](/2019/05/generating-rest-api-client-from-visual). I really just built it for myself to help me with my day-to-day work, as I needed to re-generate client code for REST APIs multiple times a day. I originally just kept it for myself and my team and just shared it by email as an attachment. I eventually open-sourced this tool on [Github](https://github.com/christianhelle/apiclientcodegen) and published it on the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=ChristianResmaHelle.ApiClientCodeGenerator2022). The extension eventually matured and gained popularity among developers who found it useful for their projects. It was originally built for Visual Studio 2019 and later ported to Visual Studio 2022 to support the 64-bit extension model. In the past 15 years, I have been switching between a Mac and a Windows machine, which lead me to eventually create a [Visual Studio for Mac extension](https://github.com/christianhelle/apiclientcodegen/blob/master/docs/VisualStudioForMac.md). When developing the extension, I faced several challenges, including compatibility issues with different versions of Visual Studio and testing the tool before release. I solved the testing problem by creating a CLI version that exposes every single feature that the Visual Studio extension provided, then I created a comprehensive suite of smoke tests written in Powershell that generates an API client in a ton of different configurations using all the features of the CLI tool. The CLI tool is published as a .NET Tool on [NuGet](https://www.nuget.org/packages/rapicgen/). It takes a good 15 minutes to run these but it saves me a ton of time in the long run. Because everything that the Visual Studio extension was exposed via a CLI tool, it didn't take that much effort to build a [VS Code extension](https://marketplace.visualstudio.com/items?itemName=ChristianResmaHelle.apiclientcodegen) that basically just called the CLI tool. Since the building the VS Code extension was not that hard, I also did the same for [JetBrains Rider](https://plugins.jetbrains.com/plugin/28472-rest-api-client-code-generator).
