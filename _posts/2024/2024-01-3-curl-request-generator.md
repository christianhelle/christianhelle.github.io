---
layout: post
title: Generate cURL requests from OpenAPI specifications
date: '2024-01-03'
author: Christian Helle
tags: 
- REST
- OpenAPI
redirect_from:
- /2023/11/generate-curl-requests-from-openapi-spec/
- /2023/11/generate-curl-requests-from-openapi-spec
- /2023/generate-curl-requests-from-openapi-spec/
- /2023/generate-curl-requests-from-openapi-spec
- /generate-curl-requests-from-openapi-spec/
- /generate-curl-requests-from-openapi-spec
---

In a [previous post](/2023/11/http-file-generator.html), I wrote about a tool I built to generate `.http` files from OpenAPI specifications. This came out of necessity as the API's I work with have very large payloads by design and SwaggerUI really doesn't like that. I work with API's that implement standardized communication protocols that are not subject to change. I love learning, and learning things outside my usual tech stack can be a lot of fun. Over the christmas holidays, I thought of learning a different programming workflow using text based editors, like [NeoVim](https://neovim.io/) and staying in the terminal without interacting with the mouse.

I spend most of my time in a modern IDE, specifically [Jetbrains Rider](https://www.jetbrains.com/rider/), so working in a text-based editor felt very unproductive to me. I'm a big fan of using [different view modes](https://www.jetbrains.com/help/rider/IDE_Viewing_Modes.html) in Rider, particularly, the Zen Mode, which technically isn't that far from a text-based editor, since all I see is, well, text. Productivity wasn't the point of this exercise, the point was to learn a new coding workflow. I tried building a .NET based API and when testing it out I wanted stay purely on the terminal and didn't want to switch to a browser to run SwaggerUI or to send requests through Postman.

I've used [cURL](https://curl.se/) in the past, and for basic requests without a body, this was simple enough and looks something like this:

```bash
curl -X GET https://petstore3.swagger.io/api/v3/pet/1 -H 'Accept: application/json' -H 'Content-Type: application/json'
```

Very soon, I needed to test more complex endpoints that had a request body. This is tedious to do by hand using [cURL](https://curl.se/), and tools like SwaggerUI and Postman are generally good at generating [cURL](https://curl.se/) requests. A [cURL](https://curl.se/) request with a request body looks something like this:

```bash
curl -X POST https://petstore3.swagger.io/api/v3/pet -H 'Accept: application/json' -H 'Content-Type: application/json' -d '{
  "id": 0,
  "name": "name",
  "category": {
    "id": 0,
    "name": "name"
  },
  "photoUrls": [
    ""
  ],
  "tags": [
    {
      "id": 0,
      "name": "name"
    }
  ],
  "status": "available"
}'
```

This got very tedious very fast and being the lazy person that I am, I thought to myself, "I can build a tool to do this for me". After some hours and a couple of espresso shots, the [cURL Request Generator](https://github.com/christianhelle/curlgenerator) was born. This tool was basically a fork of [HTTP File Generator](https://github.com/christianhelle/httpgenerator) which had everything I needed to parse OpenAPI specifications and build [cURL](https://curl.se/) requests. I switch between using MacOS and Windows several times a day, and my scripting language of choice is [Powershell Core](https://github.com/PowerShell/PowerShell) since is cross platform and can use .NET types directly.

The first cURL request I generated looked something like this powershell script:

```powershell
<#
  Request: GET /pet/{petId}
  Summary: Find pet by ID
  Description: Returns a single pet
#>
param(
   <# ID of pet to return #>
   [Parameter(Mandatory=$True)]
   [String] $petId
)

curl -X GET https://petstore3.swagger.io/api/v3/pet/$petId `
  -H 'Accept: application/json'
```

This allows me to run scripts like `GetPetById.ps1` and provide route parameters as powershell script parameters. Using [cURL Request Generator](https://github.com/christianhelle/curlgenerator), I can generate complex requests wrapped in powershell scripts like this:

```powershell
<#
  Request: POST /pet
  Summary: Add a new pet to the store
  Description: Add a new pet to the store
#>

curl -X POST https://petstore3.swagger.io/api/v3/pet `
  -H 'Accept: application/json' `
  -H 'Content-Type: application/json' `
  -d '{
  "id": 0,
  "name": "name",
  "category": {
    "id": 0,
    "name": "name"
  },
  "photoUrls": [
    ""
  ],
  "tags": [
    {
      "id": 0,
      "name": "name"
    }
  ],
  "status": "available"
}'
```

[cURL Request Generator](https://github.com/christianhelle/curlgenerator) is distributed as a .NET Tool via [NuGet.org](https://www.nuget.org/packages/CurlGenerator).

To install, simply use the following command

```sh
dotnet tool install --global curlgenerator
```

The tool provides some usage instructions and examples when running

```sh
curlgenerator --help
```

```sh
USAGE:
    curlgenerator [URL or input file] [OPTIONS]

EXAMPLES:
    curlgenerator ./openapi.json
    curlgenerator ./openapi.json --output ./
    curlgenerator https://petstore.swagger.io/v2/swagger.json
    curlgenerator https://petstore3.swagger.io/api/v3/openapi.json --base-url https://petstore3.swagger.io
    curlgenerator ./openapi.json --authorization-header Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9
    curlgenerator ./openapi.json --azure-scope [Some Application ID URI]/.default

ARGUMENTS:
    [URL or input file]    URL or file path to OpenAPI Specification file

OPTIONS:
                                           DEFAULT                                                                                                                            
    -h, --help                                                  Prints help information                                                                                       
    -v, --version                                               Prints version information                                                                                    
    -o, --output <OUTPUT>                  ./                   Output directory                                                                                              
        --no-logging                                            Don't log errors or collect telemetry                                                                         
        --skip-validation                                       Skip validation of OpenAPI Specification file                                                                 
        --authorization-header <HEADER>                         Authorization header to use for all requests                                                                  
        --content-type <CONTENT-TYPE>      application/json     Default Content-Type header to use for all requests                                                           
        --base-url <BASE-URL>                                   Default Base URL to use for all requests. Use this if the OpenAPI spec doesn't explicitly specify a server URL
        --azure-scope <SCOPE>                                   Azure Entra ID Scope to use for retrieving Access Token for Authorization header                              
        --azure-tenant-id <TENANT-ID>                           Azure Entra ID Tenant ID to use for retrieving Access Token for Authorization header                          
```

An example usage would be something like this:

```sh
curlgenerator https://petstore3.swagger.io/v2/swagger.json
```

which outputs the following:

```sh
cURL Request Generator v0.1.1
Support key: mbmbqvd

OpenAPI statistics:
 - Path Items: 14
 - Operations: 20
 - Parameters: 14
 - Request Bodies: 9
 - Responses: 20
 - Links: 0
 - Callbacks: 0
 - Schemas: 67

Files: 20
Duration: 00:00:02.3089450
```

and produces the following files:

```sh
-rw-r--r-- 1 christian 197121  593 Dec 10 10:44 DeleteOrder.ps1        
-rw-r--r-- 1 christian 197121  231 Dec 10 10:44 DeletePet.ps1
-rw-r--r-- 1 christian 197121  358 Dec 10 10:44 DeleteUser.ps1
-rw-r--r-- 1 christian 197121  432 Dec 10 10:44 GetFindPetsByStatus.ps1
-rw-r--r-- 1 christian 197121  504 Dec 10 10:44 GetFindPetsByTags.ps1  
-rw-r--r-- 1 christian 197121  371 Dec 10 10:44 GetInventory.ps1       
-rw-r--r-- 1 christian 197121  247 Dec 10 10:44 GetLoginUser.ps1       
-rw-r--r-- 1 christian 197121  291 Dec 10 10:44 GetLogoutUser.ps1      
-rw-r--r-- 1 christian 197121  540 Dec 10 10:44 GetOrderById.ps1
-rw-r--r-- 1 christian 197121  275 Dec 10 10:44 GetPetById.ps1
-rw-r--r-- 1 christian 197121  245 Dec 10 10:44 GetUserByName.ps1
-rw-r--r-- 1 christian 197121  513 Dec 10 10:44 PostAddPet.ps1
-rw-r--r-- 1 christian 197121  521 Dec 10 10:44 PostCreateUser.ps1
-rw-r--r-- 1 christian 197121  610 Dec 10 10:44 PostCreateUsersWithListInput.ps1
-rw-r--r-- 1 christian 197121  464 Dec 10 10:44 PostPlaceOrder.ps1
-rw-r--r-- 1 christian 197121  299 Dec 10 10:44 PostUpdatePetWithForm.ps1
-rw-r--r-- 1 christian 197121  274 Dec 10 10:44 PostUploadFile.ps1
-rw-r--r-- 1 christian 197121  513 Dec 10 10:44 PutUpdatePet.ps1
-rw-r--r-- 1 christian 197121  541 Dec 10 10:44 PutUpdateUser.ps1
```

I'm probably not going to use this tool myself that much since after my experiments with text-based editors, I quickly realized that this is probably not for me. [cURL Request Generator](https://github.com/christianhelle/curlgenerator) has the same (well almost the same) feature set as [HTTP File Generator](https://github.com/christianhelle/httpgenerator), so naturally this also supports generator requests with `Authorization` headers. It's important to note that the scripts generated by [cURL Request Generator](https://github.com/christianhelle/curlgenerator) are not meant to be added to source control. They were designed like [HTTP File Generator](https://github.com/christianhelle/httpgenerator) in the way that they replace the SwaggerUI flow, where the requests are reset every time you load SwaggerUI. **These generated cURL requests in powershell scripts containing Authorization headers should NEVER be committed to source control**

### Replacing SwaggerUI

I spend all my working hours building software that runs on [Microsoft Azure](https://learn.microsoft.com/en-us/training/azure/?WT.mc_id=DT-MVP-5004822) and I extensively use the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/get-started-with-azure-cli?WT.mc_id=DT-MVP-5004822) for various purposes. One of which is for retrieving an access token for the user I'm currently signed in as. With [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/get-started-with-azure-cli?WT.mc_id=DT-MVP-5004822), you can [request an access token](https://learn.microsoft.com/en-us/entra/identity-platform/access-tokens?WT.mc_id=DT-MVP-5004822) based on a [scope](https://learn.microsoft.com/en-us/entra/identity-platform/scopes-oidc?WT.mc_id=DT-MVP-5004822). This works great if your API uses roles that are specified in [Microsoft Entra ID](https://learn.microsoft.com/en-us/training/entra/?WT.mc_id=DT-MVP-5004822).

Here's an advanced example of generating cURL requests in powershell scripts for a REST API that uses the [Microsoft Entra ID](https://learn.microsoft.com/en-us/training/entra/?WT.mc_id=DT-MVP-5004822) service as an STS.

For this example, I use [PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/learn/more-powershell-learning?view=powershell-7.4&WT.mc_id=DT-MVP-5004822) and [Azure CLI to retrieve an access token for the user I'm currently logged in with](https://learn.microsoft.com/en-us/azure/databricks/dev-tools/user-aad-token?WT.mc_id=DT-MVP-5004822) then I pipe the [access token](https://learn.microsoft.com/en-us/entra/identity-platform/access-tokens?WT.mc_id=DT-MVP-5004822) to [cURL File Generator](https://github.com/christianhelle/curlgenerator).

```powershell
az account get-access-token --scope [Some Application ID URI]/.default `
| ConvertFrom-Json `
| %{
    curlgenerator `
        https://api.example.com/swagger/v1/swagger.json `
        --authorization-header ("Bearer " + $_.accessToken) `
        --base-url https://api.example.com `
        --output ./HttpFiles 
}
```

This script is something that you can have in projects and configure git to ignore the folder containing the generated cURL script files, since I re-generated them multiple times a day and they always contain `Authorization` headers.

You can also use the `--azure-scope` and `azure-tenant-id` arguments internally use `DefaultAzureCredentials` from the `Microsoft.Extensions.Azure` NuGet package to retrieve an access token for the specified `scope`.

```powershell
curlgenerator `
    https://api.example.com/swagger/v1/swagger.json `
    --azure-scope [Some Application ID URI]/.default `
    --base-url https://api.example.com `
    --output ./HttpFiles 
```

I hope you found this useful and get inspired to try new things or to start building tools like this of your own. [cURL Request Generator](https://github.com/christianhelle/curlgenerator) is free and open-source so please check it out.
