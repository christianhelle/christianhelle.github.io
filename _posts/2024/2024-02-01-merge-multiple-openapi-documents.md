---
layout: post
title: Merge multiple OpenAPI documents using Microsoft OpenAPI.NET
date: '2024-02-01'
author: Christian Helle
tags:
- OpenAPI
redirect_from:
- /2024/02/merge-multiple-openapi-documents/
- /2024/02/merge-multiple-openapi-documents
- /2024/merge-multiple-openapi-documents/
- /2024/merge-multiple-openapi-documents
- /merge-multiple-openapi-documents/
- /merge-multiple-openapi-documents
---

A couple of months ago, I received a request to [Support $ref references to separate files in OpenAPI specifications](https://github.com/christianhelle/refitter/issues/192) in one of my open-source projects, [Refitter](https://github.com/christianhelle/refitter). This problem took a while to solve but eventually after a couple of weeks I added [Add support for OAS files with external references](https://github.com/christianhelle/refitter/pull/260). Since I build a lot of code generators over OpenAPI specifications, I had to write a library to merge OpenAPI documents with external references using OpenAPI.NET. It's called [Multi Document Reader for OpenAPI.NET](https://github.com/christianhelle/oasreader) and is open-source and the repository is hosted at [Github](https://github.com/christianhelle/oasreader)

[Multi Document Reader for OpenAPI.NET](https://github.com/christianhelle/oasreader) is an OpenAPI reader that merges external references into a single document using the [Microsoft OpenAPI](https://www.nuget.org/packages/Microsoft.OpenApi.readers) toolset. This is based on the work done by Jan Kokenberg and contains [source code](https://dev.azure.com/janbaarssen/Open%20API%20Generator/_git/OpenApi.Merger) from the [dotnet-openapi-merger](https://www.nuget.org/packages/dotnet-openapi-merger) CLI tool
