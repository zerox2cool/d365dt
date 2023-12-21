# Introduction 
D365 CE Deployment Tool, up to date for D365 v9.2. This version uses XML for configurations, a new version 2 (DTHelper) is being develop.

# Packages
The packages are publish to nuget.org via Azure DevOps Build.

## **Icon Library for D365 (Legacy)**

- [ZD365.DeploymentTool.Icons](https://www.nuget.org/packages/ZD365.DeploymentTool.Icons)

## **Deployment Tool for D365 (Legacy Tool)**

- [ZD365.DeploymentTool](https://www.nuget.org/packages/ZD365.DeploymentTool)
  - The legacy Deployment Tool that contain table/column schema generation, global choice generation.
  - [Release Notes](pkg/ZStudio.D365.DTPkg/ReleaseNote.md)

## **Deployment Tool Helper - DTHelper (New Deployment Tool v2)**

- [ZD365.DeploymentHelper](https://www.nuget.org/packages/ZD365.DeploymentHelper/)
  - A tool to be used as command line helper with the Power Platform Build Tools.
  - Useful helper will be ported or added to this new DTHelper.
  - [Release Notes](pkg/ZStudio.D365.DTHelperPkg/ReleaseNote.md)