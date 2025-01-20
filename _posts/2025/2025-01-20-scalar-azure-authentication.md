---
layout: post
title: Azure Entra ID Authentication with Scalar and .NET 9.0
date: 2025-01-20
author: Christian Helle
tags:
- Scalar
- OpenAPI
- .NET 9.0
redirect_from:
- /2025/01/scalar-azure-authentication/
- /2025/01/scalar-azure-authentication
- /2025/scalar-azure-authentication
- /2025/scalar-azure-authentication
- /scalar-azure-authentication/
- /scalar-azure-authentication
---

[Swagger UI](https://swagger.io/tools/swagger-ui/) and [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) are two popular libraries for working with OpenAPI for ASP.NET Core Web APIs.
Both tools have come with benefits and problems,
but that said, most .NET developers have gotten used to it.

In May 2024, Microsoft [announced that Swashbuckle.AspNetCore will be removed from .NET 9.0](https://github.com/dotnet/aspnetcore/issues/54599). For the past year or so, I started using .http files in favor of Swagger UI. Using .http files immediately lead me to develop [HTTP File Generator](https://github.com/christianhelle/httpgenerator), a tool that can [generate a suite of .http files from OpenAPI specifications](/2023/11/http-file-generator.html)

The use of .http files were not immediately adopted by my teams so I started looking at other alternatives, particularly, [Scalar](https://scalar.com). Other teams I work with have built a work flow based on sharing [Postman Collections](https://www.postman.com/collection/) configured with Authentication and multiple environments, like Dev, Test, and Production.
[Scalar](https://scalar.com) feels a lot like [Postman](https://www.postman.com) which caught the interest of teams around me

You can try out Scalar [here](https://docs.scalar.com/swagger-editor) and it looks like this:

![Scalar demo](/assets/images/scalar.png)

In any project I'm involved in, one of the first thing I do, even before setting up a CI/CD pipeline, is to configure security.
This usually uses [OAuth2 with the implicit (or authorization code) flow](https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-implicit-grant-flow) and Azure Entra ID as a Secure Token Service (STS) for authentication.

This post will show you how to setup a .NET 9.0 project that produces an OpenAPI docment
and will demonstrate how to use [Scalar](https://scalar.com) instead of Swagger UI.
We will configure Scalar to authenticate against Azure Entra ID.
