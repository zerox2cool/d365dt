# Introduction 
D365 CE Deployment Tool, up to date for D365 v9.2.

# Packages
The packages are publish to nuget.org via Azure DevOps Build.

[ZD365.DeploymentTool](https://www.nuget.org/packages/ZD365.DeploymentTool)
[ZD365.DeploymentTool.Icons](https://www.nuget.org/packages/ZD365.DeploymentTool.Icons)

# Release Notes
v1.1.0.10 - 22-Sep-2022
- Updated to latest CRM SDK v9.0.2.45

v1.1.0.9 - 21-Mar-2022
- Updated to .NET Framework 4.6.2.
- Updated CRM SDK to v9.0.2.3
- Updated connection string to use OAuth and deprecate Office365 when connecting to D365 CDS.
- Added CrmSvcUtilExt DLL as part of the tool. Updated the CrmSvcUtil config to include connection string.
- Updated to utilize the latest ILMerge assembly from nuget.
- Create a separate nuget package to store the sample, schema and documentation to reduce file duplication when during implementation of the tool on projects.
- Added flag to include customization prefix on web resource display name.
- Added defaultPublisher configuration to update the instance default publisher.
- Updated install web resource to support SVG and RESX.
- Fix timeout issue with export, update the connection to configure the timeout in minutes configured.
- Allow entity schema to be defined without any attribute so that an entity can be created with the default primary field only, attributes section can be included without any attribute on it.
- Added support to update vector icons (SVG) on custom entity for D365 CE v9.
- Fix import customization to resume a suspended import solution job so it does not timeout.
- Added new attribute settings for plugin type to update the display name, description and workflow group.
- Fix missing DLL for solution packager.
- Fix issue with using Client Secret as connection string.
- Hide Client Secret from the logs.
- Fix plugin registration and export of steps with image.
- Fix on-premise connectivity issue.
- Updated increment solution to be able to set a version.
- Add new attribute to allow entity definition generation to exclude all assets of the entity from being added to a solution.
- Added check to ensure when CRM solution is being exported, no publish all operation executes.
- Added support for multi option set data type.
- Fix issue with plugin step register as associate message.
- Added new enabled attribute on the deployment configuration section on the XML config to enable or disable an installation.
- Added new export solution timeout and will retry two more times to workaround the issue with web resource export that fails to download the exported solution after in D365 that has not been warmed up.
- Updated to latest CRM SDK v9.0.2.42
- Added logic to auto-generate relationship name for custom lookup columns when no value is provided.
- Added support for rollup view update on a lookup relationship.
- Updated XSD schema name.