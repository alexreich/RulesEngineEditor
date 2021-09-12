# Rules Engine Editor
![RulesEngineEditor](content/RulesEngineEditor.svg)
Editor for Microsoft Rules Engine - Blazor UI library intended for integration in Web or Desktop  
[![CC BY 4.0][cc-by-shield]][cc-by]

![Animation of Rules Engine Editor Demo](content/RulesEngineEditor.gif)

## Overview

Rules Engine Editor is a library/NuGet package for use with [Microsoft Rules Engine](https://github.com/microsoft/RulesEngine) which itself is a package for abstracting business logic/rules/policies out of a system.

## Installation

To install this library, download the latest version of [NuGet Package](https://www.nuget.org/packages/RulesEngineEditor/) from [nuget.org](https://www.nuget.org/).  

## How to use it

There are several ways to populate workflows for the Rules Engine Editor as listed below.

Rules are based on the [Microsoft Rules Engine schema definition](https://github.com/microsoft/RulesEngine/blob/main/schema/workflow-schema.json) and can be stored in anything deemed appropriate like Azure Blob Storage, Cosmos DB, Azure App Configuration, [Entity Framework](https://github.com/microsoft/RulesEngine#entity-framework), SQL Servers, file systems etc. For RuleExpressionType `LamdaExpression`, the rule is written as a [lambda expressions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/lambda-expressions).

The Rules Engine Editor can also use a format for [Input Rule Parameters](https://github.com/microsoft/RulesEngine/wiki/Getting-Started#ruleparameter) based on the [schema defintiion](schema/inputRuleParameter-schema.json) and can likewise be stored as the aforementioned workflow schema. Input Rule Parameters allow the seeding of arbitrary input data to be used in the Rules Engine. Consider Input Rule Parameters like interactive "unit tests" which allow for "What-If" type analysis.


## Demo

### WebAssembly
https://alexreich.github.io/RulesEngineEditor  
> This can also be installed as a standalone PWA and used offline.

### With Sample Data
https://alexreich.github.io/RulesEngineEditor/demo

## Features
* Works in conjunction with [Microsoft Rules Engine](https://github.com/microsoft/RulesEngine)
* Real-time evaluation
* Add, Edit, Delete in form view, JSON or switch between them
* Drag, Drop objects to change order
* Nested Rule support
* Import, Download [compliant Workflow json](https://github.com/microsoft/RulesEngine/blob/main/schema/workflowRules-schema.json)
* Import, Download [compliant Input Rule Parameter json](schema/inputRuleParameter-schema.json)
* Design Time Support:
  * Pass pre-constructed instance of Rules Engine
  * Supports 2-way binding of Workflows
  * "Starter" Input Rule Parameter JSON
* Allows for non-supported types in JSON

## Usage
Simple:  
```csharp 
<RulesEngineEditorPage />
```
Complex:
```csharp 
<RulesEngineEditorPage EditorRulesEngine="re" @bind-Workflows="Workflows" InputJSON="@Inputs" />
```

## Install

[![NuGet](content/nuget-RulesEngineEditor-blue.svg)](https://www.nuget.org/packages/RulesEngineEditor/)

### Blazor WebAssembly / Client-side Blazor

1. In `Program.cs` add
```csharp 
builder.Services.AddRulesEngineEditor();
```
2. In `_Imports.razor`
```csharp
@using RulesEngineEditor.Pages
```
3. Add relevant styles, either add css included to `site.css` or inside the `<head>` element of `wwwroot/index.html` with the following statements:
```html
<link href="_content/RulesEngineEditor/css/reeditor.css" rel="stylesheet" />
<link href="_content/RulesEngineEditor/css/dragdrop.css" rel="stylesheet" />
```
 

### Server-side Blazor

1. In `Startup.cs` add
```csharp
services.AddRulesEngineEditor();
```
2. In `_Imports.razor`
```csharp
@using RulesEngineEditor.Pages
```
3. Add relevant styles, either add css included to `site.css` or inside the `<head>` element of `Pages/_Host.cshtml` with the following statements:
```html
<link href="_content/RulesEngineEditor/css/reeditor.css" rel="stylesheet" />
<link href="_content/RulesEngineEditor/css/dragdrop.css" rel="stylesheet" />
```

## What's Next
* Support for more types (ActionInfo, RuleActions, etc.)
* Synchronization with next release of Microsoft Rules Engine
* Support for .NET 6, Blazor Desktop
***
This work is licensed under a
[Creative Commons Attribution 4.0 International License][cc-by].

[![CC BY 4.0][cc-by-image]][cc-by]

[cc-by]: http://creativecommons.org/licenses/by/4.0/
[cc-by-image]: https://i.creativecommons.org/l/by/4.0/88x31.png
[cc-by-shield]: https://img.shields.io/badge/License-CC%20BY%204.0-lightgrey.svg

[:heart: Sponsor](https://github.com/sponsors/alexreich)
