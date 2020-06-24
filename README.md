## Coverage Analysis Tool

This tool ensures that every classes in a targeted solution/projects are correctly covered by a unit test. Code implementation relies on [Roslyn APIs](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/).

It displays the list of all classes and missing unit tests. 

### Context 

We were working on a project with a [CQRS](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs) architecture pattern with hundreds of command and query handlers, each one defined in a dedicated class.

In theory, unit tests should be written in association with each handler and pushed at the same time in the code repository. But in practice, and for good and bad reasons, lot of tests were missing .. and it was complex to list manually them (and especially long task). 

So we decided to write a tool with following main objectives : 

- Parse all classes in the project and identify handlers (so a class implementing a specific interface)
- Check if these classes were referenced in the test project
- List all missing handlers
- Easy way to integrate this application into a CI pipeline to decline a PR if no test is associated with the handler code 


### Usage

```cmd
> .\CoverageAnalysis.exe -v
CoverageAnalysis 1.0.0
Copyright (C) 2020 CoverageAnalysis

  -v, --verbose       Set output to verbose messages.

  -s, --solution      Required. Set solution path.

  -a, --assemblies    Required. Set assembly names.

  --help              Display this help screen.

  --version           Display version information.
```

### Sample

```cmd
> .\CoverageAnalysis.exe -s C:\<...>\Solution\Solution.sln -a Project
```

This repository contains a **Solution** folder to test this application. You will get the following outcome by using previous command line.

```cmd
Using MSBuild at 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin' to load projects.
Loading solution 'C:\Users\xxx\Projects\GitHub\CoverageAnalysis\Solution\Solution.sln'
Evaluate        0:00.7810291    Project.csproj
Build           0:01.0809756    Project.csproj
Resolve         0:00.3021702    Project.csproj (netcoreapp3.1)
Evaluate        0:00.0414383    Project.Tests.csproj
Build           0:00.2559200    Project.Tests.csproj
Resolve         0:00.0226604    Project.Tests.csproj (netcoreapp3.1)
Finished loading solution 'C:\Users\xxx\Projects\GitHub\CoverageAnalysis\Solution\Solution.sln'
Retrieving the produced Compilation for Project.
Starting to parse 5 syntax trees.
Success Project AddHandler
Success Project SubstractHandler

Project contains 2 handlers with 0 missing unit tests.
```

### Rules Settings

`appsettings.json` contains the following configuration. 

```json
{
  "Source": {
    "SyntaxType": "ClassDeclaration",
    "BaseTypes": [ "IHandler" ],
    "IgnoreAbstract": true
  },
  "Targets": [
    {
      "SyntaxType": "MethodDeclaration",
      "Attributes": [ "Test", "SetUp" ]
    }
  ]
}
```

#### Source 

- "SyntaxType": "ClassDeclaration"
    - Parsing all class in targeted project (only supported option)
- "BaseTypes": [ "IHandler" ]
    - Handler is identified if class is implementing `IHandler`. This is an array, you can add multiple base types.
- "IgnoreAbstract": true
    - We ignore abstract class.

#### Targets 

- "SyntaxType": "MethodDeclaration"
    - Handler is covered by a unit test if its name is referenced in a method with [`SetUp`] or [`Test`] attributes (here we are using [NUnit](https://nunit.org/) attributes).
- "SyntaxType": "ClassDeclaration"
    - Handler is covered by a unit test if its name is referenced in a class implementing a specific base type (for instance `HandlerTest`).
    - `$$` characters can be used to reference the name of the handler. For instance, if you inherits from a generic test class. 

More complex configuration sample: 

```json
{
  "Source": {
    "SyntaxType": "ClassDeclaration",
    "BaseTypes": [ "CommandHandler", "QueryHandler", "AuditableCommandHandler" ],
    "IgnoreAbstract": true
  },
  "Targets": [
    {
      "SyntaxType": "MethodDeclaration",
      "Attributes": [ "Test", "SetUp" ]
    },
    {
      "SyntaxType": "ClassDeclaration",
      "BaseTypes": [ "HandlerTest<$$>" ]
    }
  ]
}
```

### Build

This code is developed with .Net Framework 4.7.2. Tried to migrate it to .Net Core, but there are compatibility issues when using Roslyn APIs. Still investigating the point. However, this tool can be used to target/analyze code coverage of .Net Core projects.

### Azure DevOps

`azure-pipelines-test-coverage.yml` can be used to build and publish the tool as an artifact.

