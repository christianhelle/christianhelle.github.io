---
layout: post
title: Testing Refit interfaces using Alba
date: 2025-01-07
author: Christian Helle
tags:
- Refit
- OpenAPI
redirect_from:
- /2025/01/testing-refit-interfaces-with-alba
- /2025/01/testing-refit-interfaces-with-alba
- /2025/testing-refit-interfaces-with-alba/
- /2025/testing-refit-interfaces-with-alba
- /testing-refit-interfaces-with-alba/
- /testing-refit-interfaces-with-alba
---

I use [Refit](https://github.com/reactiveui/refit) to consume Web APIs.
The Refit interfaces that I work with are usually code generated using [Refitter](https://github.com/christianhelle/refitter).
Every now and then I stumble upon a bug regarding serialization of the payload.
I've found it interesting to test these interfaces,
and I've been looking for a way to do this in a more structured way.

For a while now, I've been using [Alba](https://jasperfx.github.io/alba/) to test my ASP.NET Core Web APIs.
Alba allows me to skip the heavy details of setting up the test server and client,
and instead focus on the actual test.
