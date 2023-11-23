---
layout: post
title: Generate .http files from OpenAPI specifications
date: '2023-11-19'
author: Christian Helle
tags: 
- REST
- OpenAPI
redirect_from:
- /2023/11/generate-http-files-from-openapi-spec/
- /2023/11/generate-http-files-from-openapi-spec
- /2023/generate-http-files-from-openapi-spec/
- /2023/generate-http-files-from-openapi-spec
- /generate-http-files-from-openapi-spec/
- /generate-http-files-from-openapi-spec
---

For a quite some time now, when building HTTP based API, I have been [Swagger UI](https://swagger.io/tools/swagger-ui/) for local API endpoint testing. Lately, I've been working on projects with rather large response payloads and Swagger UI gets really slow with this. I didn't take long before I completely swapped out using Swagger UI and went over to using `.http` files.

`.http` files were made popular by the Visual Studio Code extension [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client), which then was adopted by JetBrains IDE's, and later on [Visual Studio 2022](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.RestClient). Eventually, the 3rd party extension, REST Client, got baked into Visual Studio 2022 v17.5

The contents of a `.http` file contains the verb, path, headers, and the request body. It looks something like this:

```sh
### POST /pet Request

POST https://petstore.swagger.io/v2/pet
Content-Type: application/json

{
  "id": 0,
  "category": {
    "id": 0,
    "name": "name"
  },
  "name": "name",
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
}
```

A single `.http` file may contain multiple requests, but I found it more convenient if each endpoint has its own `.http` file. This is a personal preference and I'm in no way saying that this is the correct or best way to use `.http` files.

Most, if not all, ASP.NET API projects will expose a Swagger spec or more correctly, an OpenAPI specifications document. This lead me to take advantage of knowledge and experience OpenAPI specifications and with authoring code generators to create the tool [HTTP File Generator](https://github.com/christianhelle/httpgenerator).

HTTP File Generator is distributed as a .NET Tool via NuGet.org. To install, simply use the following command

```bash
dotnet tool install --global httpgenerator
```

This command line tool allows me to generate a set of `.http` files from an OpenAPI specifications document (locally and from a URL).

```sh
USAGE:
    httpgenerator [URL or input file] [OPTIONS]

EXAMPLES:
    httpgenerator ./openapi.json
    httpgenerator ./openapi.json --output ./
    httpgenerator https://petstore.swagger.io/v2/swagger.json
    httpgenerator https://petstore3.swagger.io/api/v3/openapi.json --base-url https://petstore3.swagger.io
    httpgenerator ./openapi.json --authorization-header Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c

ARGUMENTS:
    [URL or input file]    URL or file path to OpenAPI Specification file

OPTIONS:
                                           DEFAULT                                                                                                                           
    -h, --help                                                 Prints help information                                                                                       
    -o, --output <OUTPUT>                  ./                  Output directory                                                                                              
        --no-logging                                           Don't log errors or collect telemetry                                                                         
        --skip-validation                                      Skip validation of OpenAPI Specification file                                                                 
        --authorization-header <HEADER>                        Authorization header to use for all requests                                                                  
        --content-type <CONTENT-TYPE>      application/json    Default Content-Type header to use for all requests                                                           
        --base-url <BASE-URL>                                  Default Base URL to use for all requests. Use this if the OpenAPI spec doesn't explicitly specify a server URL
```

An example usage would be something like this:

```sh
httpgenerator https://petstore.swagger.io/v2/swagger.json
```

which outputs the following:

```sh
HTTP File Generator v0.1.1
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
-rw-r--r--  1 christian  staff  299 Nov 13 22:40 AddPet.http
-rw-r--r--  1 christian  staff  276 Nov 13 22:40 CreateUser.http
-rw-r--r--  1 christian  staff  332 Nov 13 22:40 CreateUsersWithArrayInput.http
-rw-r--r--  1 christian  staff  330 Nov 13 22:40 CreateUsersWithListInput.http
-rw-r--r--  1 christian  staff  135 Nov 13 22:40 DeleteOrder.http
-rw-r--r--  1 christian  staff  115 Nov 13 22:40 DeletePet.http
-rw-r--r--  1 christian  staff  123 Nov 13 22:40 DeleteUser.http
-rw-r--r--  1 christian  staff  119 Nov 13 22:40 FindPetsByStatus.http
-rw-r--r--  1 christian  staff  115 Nov 13 22:40 FindPetsByTags.http
-rw-r--r--  1 christian  staff  117 Nov 13 22:40 GetInventory.http
-rw-r--r--  1 christian  staff  129 Nov 13 22:40 GetOrderById.http
-rw-r--r--  1 christian  staff  109 Nov 13 22:40 GetPetById.http
-rw-r--r--  1 christian  staff  117 Nov 13 22:40 GetUserByName.http
-rw-r--r--  1 christian  staff  107 Nov 13 22:40 LoginUser.http
-rw-r--r--  1 christian  staff  109 Nov 13 22:40 LogoutUser.http
-rw-r--r--  1 christian  staff  250 Nov 13 22:40 PlaceOrder.http
-rw-r--r--  1 christian  staff  297 Nov 13 22:40 UpdatePet.http
-rw-r--r--  1 christian  staff  111 Nov 13 22:40 UpdatePetWithForm.http
-rw-r--r--  1 christian  staff  296 Nov 13 22:40 UpdateUser.http
-rw-r--r--  1 christian  staff  135 Nov 13 22:40 UploadFile.http
```

For me, .http files seems to be the way going forward.

There are of course a few challenges in adopting this in my daily workflows. Swagger UI has [Authentication and Authorization](https://swagger.io/docs/specification/authentication/) built-in and is extremely easy to implement and enable. In fact, enabling security is the first thing I do when I setup a new API project.

With the tool HTTP File Generator, adding `Authorization` headers to a .http file is trivial, as you can just specify it from the tool like this:

```sh
httpgenerator ./openapi.json --authorization-header Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

which will generate `.http` files that look something like this:

```sh
### POST /pet Request

POST https://petstore.swagger.io/v2/pet
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c

{
  "id": 0,
  "category": {
    "id": 0,
    "name": "name"
  },
  "name": "name",
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
}
```

The problem here is that you are not really interested in retrieving or even knowing what Authorization headers your HTTP requests are using. They should just be there if required

### Replacing SwaggerUI

I spend all my working hours building software that runs on [Microsoft Azure](https://learn.microsoft.com/en-us/training/azure/?WT.mc_id=DT-MVP-5004822) and I extensively use the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/get-started-with-azure-cli?WT.mc_id=DT-MVP-5004822) for various purposes. One of which is for retrieving an access token for the user I'm currently signed in as. With [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/get-started-with-azure-cli?WT.mc_id=DT-MVP-5004822), you can request an access token based on a scope. This works great if your API uses roles that are specified in [Microsoft Entra ID](https://learn.microsoft.com/en-us/training/entra/?WT.mc_id=DT-MVP-5004822).

Here's an advanced example of generating `.http` files for a REST API hosted on [Microsoft Azure](https://learn.microsoft.com/en-us/training/azure/?WT.mc_id=DT-MVP-5004822) that uses the [Microsoft Entra ID](https://learn.microsoft.com/en-us/training/entra/?WT.mc_id=DT-MVP-5004822) service as an STS.

For this example, I use [PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/learn/more-powershell-learning?WT.mc_id=DT-MVP-5004822) and [Azure CLI to retrieve an access token for the user I'm currently logged in](https://learn.microsoft.com/en-us/azure/databricks/dev-tools/user-aad-token?WT.mc_id=DT-MVP-5004822) with then I pipe the [access token](https://learn.microsoft.com/en-us/entra/identity-platform/access-tokens?WT.mc_id=DT-MVP-5004822) to [HttpGenerator](https://github.com/christianhelle/httpgenerator).

```powershell
dotnet tool update --global httpgenerator

az account get-access-token --scope [Some Application ID URI]/.default `
| ConvertFrom-Json `
| %{
    httpgenerator `
        https://api.example.com/swagger/v1/swagger.json `
        --authorization-header ("Bearer " + $_.accessToken) `
        --base-url https://api.example.com `
        --output ./HttpFiles 
}
```

This script is something that I have in all projects and I also configure git to ignore `.http` files, since I re-generated them multiple times a day. I basically run the script above every time I want to debug or test my API's and I have pretty much stopped using Swagger UI.


### Using .http files from Visual Studio Code

If you don't already have the REST Client extension installed then go search for **REST Client**

![VS Code REST Client Extension Install](/assets/images/vscode-rest-client-install.png)

Once the **REST Client** extension installed you should be able to see a **Send Request** label when openning .http files. Clicking on **Send Request** will open a tab containing the response

![VS Code REST Client Extension Usage](/assets/images/vscode-rest-client-request.png)

### Using .http files from JetBrains Rider

JetBrains IDE's come with a built-in HTTP Client that supports `.http` files.

![Rider HTTP Client - .http file](/assets/images/rider-http-file.png)

I find that the JetBrains HTTP Client is much smoother than the Visual Studio Code extension, but this is only noticeable when working with very large payloads.

![Rider HTTP Client Console](/assets/images/rider-http-file-console.png)

### Using .http files from Visual Studio 2022

The latest version of Visual Studio 2022 v17.5 now comes with a built-in HTTP Client that supports `.http` files.

![Visual Studio 2022 .http file](/assets/images/vs-http-file-request.png)

The Visual Studio 2022 unfortunately doesn't give a very smooth experience. Currently, it's a little slow but I'm hopeful that this will improve over time.

![Visual Studio 2022 .http response](/assets/images/vs-http-file-response.png)

The response is pretty decent and gives you options to view the response headers in tabular form

![Visual Studio 2022 .http response headers](/assets/images/vs-http-file-response-headers.png)
