# Usage Sample for Coverage Analysis Tool

In our project we have integrated the tool into an Azure DevOps pipeline as a separate [stage](https://docs.microsoft.com/en-us/azure/devops/pipelines/process/stages?view=azure-devops&tabs=yaml).
This integration is included here as sample. Additionally we have added the integration as GitHub Action as sample.

## Integration with Azure Pipelines

There are two files:

1) `azure-pipelines.yml`:
    * This is the default name of most pipelines in the root directory of the repository. 
    * In a separate stage it contains a reference to the template `service-coverage.yml`.
2) `service-coverage.yml`:
    * This file runs the coverage analysis tool on the specified source code.
    * The tool parameters are described in the main [readme.md](./readme.md).

## Integration with GitHub Actions
