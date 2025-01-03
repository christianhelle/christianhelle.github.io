---
layout: post
title: Generate .http files with IntelliJ tests from OpenAPI specifications
date: '2024-12-20'
author: Christian Helle
tags:

- REST
- OpenAPI
redirect_from:
- /2024/12/generate-http-file-with-intellij-tests/
- /2024/12/generate-http-file-with-intellij-tests
- /2024/generate-http-file-with-intellij-tests/
- /2024/generate-http-file-with-intellij-tests
- /generate-http-file-with-intellij-tests/
- /generate-http-file-with-intellij-tests

---

Some time ago, I built a tool called [HTTP File Generator](2023/11/http-file-generator.html) to replace Swagger UI and keep my workflow as much as possible from within my editor. 

These days, for .NET development I'm mostly on JetBrains Rider. Most, if not all, JetBrains IDE's are built on top of IntelliJ IDEA and supports executing javascript code from `.http` files using their built in [HTTP Client](https://www.jetbrains.com/help/idea/http-client-in-product-code-editor.html). This is very useful for chaining HTTP requests and handling responses.

You can handle the response using JavaScript. Type the `>` character after the request and specify the path and name of the JavaScript file or put the response handler script code wrapped in `{% ... %}`.

```javascript
GET https://httpbin.org/get

> /path/to/responseHandler.js
```

or

```javascript
GET https://httpbin.org/get

> {%
    client.global.set("my_cookie", response.headers.valuesOf("Set-Cookie")[0]);
%}
```

I suggest [Exploring the HTTP Client syntax](https://www.jetbrains.com/help/idea/exploring-http-syntax.html)

HTTP Response handler scripts are written in JavaScript ES6, with coding assistance and documentation handled by the bundled HTTP Response Handler library.

The library exposes two objects to be used for composing response handler scripts:

- `client` stores the session metadata, which can be modified inside the script. The client's state is preserved until you close IntelliJ IDEA.
- `response` holds information about the received response: its content type, status, response body, and so on.

For example, to create several tests to verify an HTTP response would be something like this:

```javascript
### Check response status, headers, and content-type
GET https://foo.com/bar

> {%
    client.test("Request executed successfully", function() {
        client.assert(response.status === 200, "Response status is not 200");
    });

    client.test("Headers option exists", function() {
        client.assert(response.body.hasOwnProperty("headers"), "Cannot find 'headers' option in response");
    });

    client.test("Response content-type is json", function() {
        var type = response.contentType.mimeType;
        client.assert(type === "application/json", "Expected 'application/json' but received '" + type + "'");
    });
%}
```
