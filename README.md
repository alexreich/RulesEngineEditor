# Rules Engine Editor
![RulesEngineEditor](content/RulesEngineEditor.svg)
Editor for Microsoft Rules Engine - Blazor UI library intended for integration in Web or Desktop  
[![CC BY 4.0][cc-by-shield]][cc-by]

![Animation of Rules Engine Editor Demo](content/RulesEngineEditor.gif)

## Overview

Rules Engine Editor is a library/NuGet package for use with [Microsoft Rules Engine](https://github.com/microsoft/RulesEngine) which itself is a package for abstracting business logic/rules/policies out of a system.

## Installation

To install this library, download the latest version of [NuGet Package](https://www.nuget.org/packages/RulesEngineEditor/) from [nuget.org](https://www.nuget.org/) and refer it into your project.  

## How to use it

There are several ways to populate workflows for the Rules Engine Editor as listed below.

You need to store the rules based on the [schema definition](https://github.com/microsoft/RulesEngine/blob/main/schema/workflow-schema.json) given and they can be stored in any store as deemed appropriate like Azure Blob Storage, Cosmos DB, Azure App Configuration, [Entity Framework](https://github.com/microsoft/RulesEngine#entity-framework), SQL Servers, file systems etc. For RuleExpressionType `LamdaExpression`, the rule is written as a [lambda expressions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/lambda-expressions).

The Rules Engine Editor can also use a format for Input Rule Parameter based on the [schema defintiion](schema/inputRuleParameter-schema.json) and can likewise be stored as the aforementioned workflow schema. Input Rule Parameters allow the seeding of arbitrary input data to be used in the Rules Engine. Consider Input Rule Parameters like interactive "unit tests" which allow for "What-If" type analysis.


#### Demo

##### WebAssembly
https://alexreich.github.io/RulesEngineEditor  
> You can also install this as a standalone PWA and use offline.

##### With Sample Data
https://alexreich.github.io/RulesEngineEditor/demo

#### Features
* Works in conjunction with [Microsoft Rules Engine](https://github.com/microsoft/RulesEngine)
* Real-time evaluation
* Edit in form view, JSON or switch between the two
* Drag, Drop objects to change order
* Nested Rule support
* Import, Download [compliant Workflow json](https://github.com/microsoft/RulesEngine/blob/main/schema/workflowRules-schema.json)
* Import, Download [Input Rule Parameters](https://github.com/microsoft/RulesEngine/wiki/Getting-Started#ruleparameter)
* Design Time Support:
  * Pass pre-constructed instance of Rules Engine
  * Supports 2-way binding of Workflows
  * "Starter" Input Rule Parameter JSON
* Allows for non-supported types in JSON

#### Usage:
Simple:  
```csharp 
<RulesEngineEditorPage />
```
Complex:
```csharp 
<RulesEngineEditorPage EditorRulesEngine="re" @bind-Workflows="Workflows" InputJSON="@Inputs" />
```

#### Install

[![NuGet](content/nuget-RulesEngineEditor-blue.svg)](https://www.nuget.org/packages/RulesEngineEditor/)

#### Blazor WebAssembly / Client-side Blazor

1. In `Program.cs` add
```csharp 
builder.Services.AddRulesEngineEditor();
```
2. In `_Imports.razor`
```csharp
@using RulesEngineEditor.Pages
```
3. Add relevant styles to your app:  
Either add the css included to your `site.css` or inside the `<head>` element of your `wwwroot/index.html`, add css statements below:
```html
<link href="_content/RulesEngineEditor/css/reeditor.css" rel="stylesheet" />
<link href="_content/RulesEngineEditor/css/dragdrop.css" rel="stylesheet" />
```
 

#### Server-side Blazor

1. In `Startup.cs` add
```csharp
services.AddRulesEngineEditor();
```
2. In `_Imports.razor`
```csharp
@using RulesEngineEditor.Pages
```
3. Add relevant styles to your app:  
Either add the css included to your `site.css` or inside the `<head>` element of your `Pages/_Host.cshtml`, add the css statements below:
```html
<link href="_content/RulesEngineEditor/css/reeditor.css" rel="stylesheet" />
<link href="_content/RulesEngineEditor/css/dragdrop.css" rel="stylesheet" />
```

#### What's Next
* Support for more types (ActionInfo, RuleActions, etc.)
* Synchronization with next release of Microsoft Rules Engine
* Support for .NET 6
***
This work is licensed under a
[Creative Commons Attribution 4.0 International License][cc-by].

[![CC BY 4.0][cc-by-image]][cc-by]

[cc-by]: http://creativecommons.org/licenses/by/4.0/
[cc-by-image]: https://i.creativecommons.org/l/by/4.0/88x31.png
[cc-by-shield]: https://img.shields.io/badge/License-CC%20BY%204.0-lightgrey.svg

[:heart: Sponsor](https://github.com/sponsors/alexreich)
