# Application Registry Collector

## Introduction

## Getting Started

### 1. Installation process

Collector is implemented as dotnet toll and can be installed via nuget.

To install Collector as global tool from command line invoke:

```shell
dotnet tool install ApplicationRegistry.Collector --global
```

During registration process alias for the tool is created.
Collector is available with command: `ar-collector`

### Parameters

## Build and Test

## Dependency collectors

| Developed | Collector name             | Description |
| --------- | -------------------------- | ----------- |
| [x]       | Autorest client dependency |             |
| [x]       | Nuget dependencies         |             |
| [ ]       | Dotnet version             |             |

## Specification generators

| Developed | Specification generator name | Description |
| --------- | ---------------------------- | ----------- |
| [x]       | Swagger specification        |             |
| [ ]       | Database specification       |             |

## Samples

### Sample invocation

```shell
ar-collector --path ApplicationRegistry.Collector\samples\ApplicationRegistry.SampleWebApplication\ApplicationRegistry.SampleWebApplication.csproj -e INT -a NewApp -v Snapshot -sd v1 -u https://localhost:44346
```
