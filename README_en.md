# Dry CLI

**[中文](./README.md)**

**dry** is an intelligent code assistance tool that primarily focuses on code generation. It analyzes your entities and intelligently helps you generate related data transfer objects, database read/write operations, and API interfaces.

It is provided as a `dotnet` command-line tool and also supports a `Web UI` operational interface.

## Features

- Intelligent analysis based on entity models, understanding the user's business intent
- Smart generation of DTO files, including common DTOs such as create, update, query, list, etc.
- Intelligent generation of data operations and business logic implementation, including common functionalities like add, update, filter, etc.
- Generation of partial testing code
- Generation of controller interfaces
- Generating Typescript interface types based on Swagger OpenAPI
- Generating request services for Angular or Axios based on Swagger OpenAPI
- Basic CRUD pages for Angular
- Plus many more advanced features

### Support for ASP.NET Core

The dry command-line tool can assist developers in generating common code templates based on entity models (.cs files), including:

- DTO files, such as create, update, query, list DTO files
- Repository files, data repository layer code
- Controller files
- Protobuf files
- Client request services

### Support for Typescript

For the frontend, it can generate the necessary code (.ts) for requests based on swagger OpenAPI's JSON content, including:

- Request services, `xxx.service.ts`
- Interface models, `xxx.ts`

## Project Template Support

You can use the [ater.web.templates](https://www.nuget.org/packages/ater.web.templates) project template, which is recommended for use in conjunction!

## Prerequisites

- Install the [`.NET SDK`](https://dotnet.microsoft.com/en-us/download)

## Versions

First, check the package version. The tool depends on .NET SDK, and the corresponding relationships are as follows:

| Package Version | .NET SDK Version | Supported |
|-|-|-|
| 8.x | 8.0+ | Current Version |

## Installing the Tool

Use the `dotnet tool` command to install:

```pwsh
dotnet tool install --global ater.dry.cli
```

You can check the latest version on [nuget](https://www.nuget.org/packages/ater.dry.cli)!

## Usage

### ⭐ Using the Graphical Interface

Start the UI interface with one command!

```pwsh
dry studio
```

### Using the Command Line

You can use `dry --help` to view command help information.

Or use `dry [command] --help` to view specific command help information.

## Documentation

[View the full documentation](https://docs.dusi.dev/en/ater.dry/Overview.html)
