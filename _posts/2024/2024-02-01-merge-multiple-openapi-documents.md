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

## Usage

The class `OpenApiMultiFileReader` is used to load an OpenAPI specifications document file locally or remotely using a YAML or JSON file. `OpenApiMultiFileReader` will automatically merge external references if the OAS file uses them. Merging external referenecs that the file is in the same folder as the main OAS file. When loading OAS files remotely, the external references must also be remote files. Currently, you can not load a remote OAS file that has external references to local files. 

```csharp
ReadResult result = await OpenApiMultiFileReader.Read("petstore.yaml");
OpenApiDocument document = result.OpenApiDocument;
```

In the example above, we have OpenAPI specifications that are split into multiple documents. **`petstore.yaml`** contains the **`paths`** and **`petstore.components.yaml`** contain the **`components/schemas`**

**`petstore.yaml`**

```yaml
openapi: 3.0.3
paths:
  /pet:
    post:
      tags:
      - pet
      summary: Add a new pet to the store
      description: Add a new pet to the store
      operationId: addPet
      requestBody:
        description: Create a new pet in the store
        content:
          application/json:
            schema:
              $ref: 'petstore.components.yaml#/components/schemas/Pet'          
        required: true
      responses:
        "200":
          description: Successful operation
          content:
            application/json:
              schema:
                $ref: 'petstore.components.yaml#/components/schemas/Pet'
```

**`petstore.components.yaml`**

```yaml
openapi: 3.0.3
components:
  schemas:
    Pet:
      required:
      - name
      - photoUrls
      type: object
      properties:
        id:
          type: integer
          format: int64
          example: 10
        name:
          type: string
          example: doggie
        category:
          $ref: '#/components/schemas/Category'
        photoUrls:
          type: array
          xml:
            wrapped: true
          items:
            type: string
            xml:
              name: photoUrl
        tags:
          type: array
          xml:
            wrapped: true
          items:
            $ref: '#/components/schemas/Tag'
        status:
          type: string
          description: pet status in the store
          enum:
          - available
          - pending
          - sold
    Category:
      type: object
      properties:
        id:
          type: integer
          format: int64
          example: 1
        name:
          type: string
          example: Dogs
      xml:
        name: category
```