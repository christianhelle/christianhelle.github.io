---
layout: post
title: Generate .http files and .env from OpenAPI specifications
date: '2024-05-28'
author: Christian Helle
tags: 
- REST
- OpenAPI
redirect_from:
- /2024/05/generate-http-file-env
- /2024/05/generate-http-file-env/
- /2024/generate-http-file-env
- /generate-http-file-env/
- /generate-http-file-env
---

Some time ago, I built a tool called [HTTP File Generator](2023/11/http-file-generator.html)
to replace Swagger UI and keep my workflow as much as possible
from within my editor.

I spend most of my time building API's running on Azure.
All of these API's have a few things in common. To name a few:

- They would have some form of OAuth2 authentication
that use Azure Entra ID (formerly Azure Active Directory) as the
Security Token Service (STS).
- They are deployed to at least 3 environments: Development, Staging, and Production.
- They can run locally, in development, staging, and production environments.

One of the first things I do when working with an API is to generate `.http`
files using HTTP File Generator and generate an `.env` file with the necessary
environment variables. I use the Azure CLI to get the access token for the API,
in every environment and use it to generate the `.env` file.

Here's an example of how I do this using PowerShell:

```PowerShell
httpgenerator `
   https://localhost:5001/swagger/v1/swagger.js `
   --load-authorization-header-from-environment `
   --base-url "{{baseUrl}}" `
   --skip-validation `
   --output .http

$api = "service.some.api"
$devJson = az account get-access-token --scope api://$api/dev/.default | ConvertFrom-Json
$stagingJson = az account get-access-token --scope api://$api/staging/.default | ConvertFrom-Json
$prodJson = az account get-access-token --scope api://$api/prod/.default | ConvertFrom-Json

$environment = @{
  local = @{
    authorization = "Bearer " + $devJson.accessToken
    baseUrl = "https://localhost:5001"
  }
  development = @{
    authorization = "Bearer " + $devJson.accessToken
    baseUrl = "https://clcpmsdevroaming.azurewebsites.net"
  }
  staging = @{
    authorization = "Bearer " + $stagingJson.accessToken
    baseUrl = "https://clcpmsstagingroaming.azurewebsites.net"
  }
  production = @{
    authorization = "Bearer " + $prodJson.accessToken
    baseUrl = "https://clcpmsprodroaming.azurewebsites.net"
  }
}

Set-Content -Path .http/http-client.env.json -Value ($environment | ConvertTo-Json -Depth 10)
```
