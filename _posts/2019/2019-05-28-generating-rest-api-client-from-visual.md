---
layout: post
title: Generating a REST API Client from Visual Studio 2017 and 2019
date: '2019-05-28T18:06:00.002+02:00'
author: Christian Resma Helle
tags: 
- Visual Studio 
- REST
modified_time: '2019-06-11T18:55:15.085+02:00'
thumbnail: /assets/images/solution-explorer-context-menu.jpg
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-1786649372449479397
blogger_orig_url: https://christian-helle.blogspot.com/2019/05/generating-rest-api-client-from-visual.html
---

For the past year or so, I have been doing a lot of development that involves producing an OpenAPI specification document from a .NET Core based REST API and generating client code using things like [AutoRest](https://github.com/Azure/autorest), [Swagger Codegen](https://github.com/swagger-api/swagger-codegen), [OpenAPI Codegen](https://github.com/OpenAPITools/openapi-generator), and [NSwag](https://github.com/RicoSuter/NSwag). My problem with these tools is that I often need to leave Visual Studio and quite often update the tool before I can re-generate my REST API client code. After doing this a couple of times I thought that I should just build a Visual Studio extension to make my life easier. At the end of last year I started work on a Visual Studio extension called the [REST API Client Code Generator](https://marketplace.visualstudio.com/items?itemName=ChristianResmaHelle.APIClientCodeGenerator), A collection of Visual Studio custom tools for generating a strongly typed REST API Client from an Open API / Swagger specification file

With this tool I can easily switch from NSwag, AutoRest, Swagger Codegen, and OpenAPI Codegen, and re-generate my code by making changes directly to the OpenAPI specification document I have in my project.  

[![](/assets/images/solution-explorer-context-menu.jpg)](/assets/images/solution-explorer-context-menu.jpg)

I built [Visual Studio Custom Tools](https://docs.microsoft.com/en-us/visualstudio/extensibility/internals/custom-tools?view=vs-2019?WT.mc_id=DT-MVP-5004822) for each code generator so every time I make changes to the OpenAPI specification document in my project, the client code gets automatically re-generated.  

[![](/assets/images/autorestcodegenerator-custom-tool.jpg)](/assets/images/autorestcodegenerator-custom-tool.jpg)

[![](/assets/images/openapicodegenerator-custom-tool.jpg)](/assets/images/openapicodegenerator-custom-tool.jpg)

[![](/assets/images/swaggercodegenerator-custom-tool.jpg)](/assets/images/swaggercodegenerator-custom-tool.jpg)

[![](/assets/images/nswagcodegenerator-custom-tool.jpg)](/assets/images/nswagcodegenerator-custom-tool.jpg)

You can include an [NSwag Studio](https://github.com/RicoSuter/NSwag/wiki/NSwagStudio) file in the project and right click and re-generate my client code  

[![](/assets/images/nswagstudio-context-menu.jpg)](/assets/images/nswagstudio-context-menu.jpg)

And a feature that I just built today, adding a dialog for adding a new OpenAPI specification document file.  

[![](/assets/images/add-new-menu.png)](/assets/images/add-new-menu.png)

[![](/assets/images/add-new-dialog.png)](/assets/images/add-new-dialog.png)

This project is open source and you get browse the repository from [here](https://github.com/christianhelle/apiclientcodegen) and download the VSIX file from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=ChristianResmaHelle.APIClientCodeGenerator)