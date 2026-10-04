---
layout: page
title: Projects
description: Open source .NET, Rust, Zig, and C++ projects for API development, database tools, testing, and AI-assisted developer workflows.
permalink: /projects/
---

Below is a curated list of my open source projects, with links to their repositories. These tools and libraries span .NET, Rust, Zig, API development, testing, data access, AI agent tooling, and developer productivity.

## [Refitter](https://github.com/christianhelle/refitter)

A tool for generating Refit interfaces and contracts from OpenAPI specifications. Refitter makes REST API consumption in .NET seamless and type-safe by automating the creation of strongly-typed interfaces.

## [REST API Client Code Generator](https://github.com/christianhelle/apiclientcodegen)

A suite of tools for generating REST API clients from OpenAPI specifications, with extensions for Visual Studio, Visual Studio Code, and JetBrains Rider, plus command-line support. It integrates NSwag, OpenAPI Generator, Swagger Codegen, Refitter, and Microsoft Kiota.

## [HTTP File Generator](https://github.com/christianhelle/httpgenerator)

Generate .http files from OpenAPI specifications for testing APIs directly in your IDE. This tool simplifies API testing and documentation by automating the creation of HTTP request files.

## [openapi2zig](https://github.com/christianhelle/openapi2zig)

A CLI tool written in Zig that generates API client code in Zig from OpenAPI specifications. This tool enables developers to create type-safe Zig clients from OpenAPI v2.0 and v3.0 specifications, supporting cross-platform development.

## [SQL Compact Query Analyzer](https://github.com/christianhelle/sqlcequery)

A fast and lightweight tool for querying and manipulating SQL CE databases, supporting versions 3.0 through 4.0. It provides a modern interface for database management and development.

## [cURL Request Generator](https://github.com/christianhelle/curlgenerator)

Generate cURL requests from OpenAPI specifications, perfect for API testing and documentation. This tool helps developers quickly create reproducible API requests for use in scripts or documentation.

## [AutoFaker](https://github.com/christianhelle/autofaker)

A Python library designed to minimize the setup/arrange phase of your unit tests by automatically generating fake data. AutoFaker accelerates test development and improves test coverage.

## [JMeter Test Plan Generator](https://github.com/christianhelle/jmetercodegen)

Generate Apache JMeter test plans from OpenAPI specifications for load testing and performance validation. This tool automates the creation of complex test plans, saving time and reducing errors.

## [ResW File Code Generator](https://github.com/christianhelle/reswcodegen)

Visual Studio custom tool for generating strongly-typed classes from ResW resource files, supporting Visual Studio 2015-2022. It streamlines localization and resource management in .NET projects.

## [Multi Document Reader for OpenAPI.NET](https://github.com/christianhelle/oasreader)

An OpenAPI reader that merges external references into a single document using the Microsoft OpenAPI toolset. This project simplifies working with complex, multi-file OpenAPI specifications.

## [Multi Document Reader for OpenAPI in Rust](https://github.com/christianhelle/oasreader-rs)

A Rust library that reads Swagger 2.0, OpenAPI 3.0, and OpenAPI 3.1 documents and merges external references into a single document. It is the Rust counterpart to [oasreader](https://github.com/christianhelle/oasreader) and is shared by Rust tools such as the HTTP File Generator and cURL Request Generator.

## [SQLite Query Analyzer](https://github.com/christianhelle/sqlitequery)

A modern, cross-platform SQLite database management tool built with C++ and Qt. Recently updated after a decade, it offers a fast and user-friendly interface for SQLite development.

## [SQL Query Analyzer](https://github.com/christianhelle/sqlquery)

A cross-platform desktop database tool for querying and editing SQLite, PostgreSQL, SQL Server, and MySQL/MariaDB databases. It extends SQLite Query Analyzer to work with database servers as well as local SQLite files.

## [Azure DevOps CLI](https://github.com/christianhelle/azdocli)

A standalone Rust CLI for interacting with Azure DevOps and automating common development and project-management tasks.

## [Energi Data Service Client for .NET](https://github.com/christianhelle/edsclient)

A .NET client library for consuming day-ahead energy prices from the Danish Energi Data Service API. It provides strongly-typed models, async APIs, and dependency injection support for applications that work with electricity market data.

## [HTTP File Runner](https://github.com/christianhelle/httprunner)

A fast, small, single binary tool for running .http files from the command line, built with Zig for optimal performance. It enables automated API testing and integration in CI pipelines.

## [HttpTestGen - .http File Testing Framework](https://github.com/christianhelle/httptestgen)

A powerful .NET source generator that automatically converts .http files into fully functional C# test code. This tool bridges the gap between API testing in IDEs (like Visual Studio Code with the REST Client extension) and automated testing in your .NET projects.

## [Argiope](https://github.com/christianhelle/argiope)

A web crawler built in Zig for broken-link detection and image downloading. Argiope efficiently discovers and validates links across websites while providing tools for batch image collection, making it ideal for site validation and content management workflows.

## [Clocz - Line of Code Counter](https://github.com/christianhelle/clocz)

A fast, multi-threaded command-line tool for counting lines of code, written in Zig. Clocz delivers performance and efficiency for analyzing codebase metrics across large projects, with support for multiple programming languages.

## [ZigFaker](https://github.com/christianhelle/zigfaker)

A Zig library for generating anonymous and contextually realistic fake data for unit tests using comptime type reflection. ZigFaker reduces test setup boilerplate and helps developers create expressive, repeatable test data with zero external dependencies.

## [GitHub Changelog Generator](https://github.com/christianhelle/chlogr)

A fast native CLI written in Zig that generates Markdown changelogs from GitHub tags, merged pull requests, and closed issues. It automates release note generation and organizes changes into clear sections for maintainers and users.

## [Azure SDK for Zig](https://github.com/christianhelle/azure-sdk-for-zig)

Azure SDK client libraries for the Zig programming language. This project enables developers to build cloud-native applications in Zig with access to Azure services through type-safe, performant bindings.

## [termos](https://github.com/christianhelle/termos)

A native Rust terminal UI for exploring Azure Cosmos DB. Browse accounts, databases, containers, and documents, run SQL queries, and edit container settings.

---

_Interested in contributing or learning more? Visit the project repositories or reach out through [GitHub](https://github.com/christianhelle)._
