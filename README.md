## Coverage Analysis Tool

This tool ensure that every handler classes in a targeted solution/projects are correctly covered by a Unit Test. Code implementation relies on [Roslyn APIs](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/).

It displays the list of all handler classes and missing Unit Tests. 

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
> .\CoverageAnalysis.exe -s -s C:\<...>\NewSolution\NewSolution.sln -a Main.CQS Import.CQS Export.CQS
```

### Current Rules

- Parsing all files in targeted project to identify handlers
- Handler is identified if class is implementing `CommandHandler`, `QueryHandler` or `AuditableCommandHandler`
- Handler is covered by a unit test if the associated class name is referenced with one of the following conditions:
    - The reference should be contained in a method with [`SetUp`] or [`Test`] attributes
    - The reference should be contained in a class implementing `HandlerTest<`HandlerName`>`

### Build

This code is developed with .Net Framework 4.7.2. Tried to migrate it to .Net Core, but there are compatibility issues when using Roslyn APIs. Still investigating the point. However, this tool can be used to target/analyze code coverage of .Net Core projects.

### Azure DevOps

`azure-pipelines-test-coverage.yml` can be used to build and publish the tool as an artifact.

