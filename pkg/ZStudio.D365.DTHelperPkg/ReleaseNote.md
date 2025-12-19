# DTHelper - ZD365.DeploymentHelper
[User Guide](ReadmeGuide.md)

# Release Notes
## v0.0.0.12
### 19-Dec-2025
- Added new helper to sync teams to column security profile (SyncTeamsToColumnSecurityProfile).

## v0.0.0.10
### 17-Dec-2025
- Added new optional parameter to enable ignore prefix (IgnorePrefix) to be customised by caller for activate/deactivate of Cloud Flows components (SetCloudFlowStatus).
- Added new optional parameter to turn off ignore prefix (TurnOffIgnoredFlows) cloud flows found for activate/deactivate of Cloud Flows components (SetCloudFlowStatus).

## v0.0.0.9
### 19-Nov-2025
- Added new optional parameter to support Power Pages Enhanced Data Model, pass in true/false to portalEnhanced parameter to indicate if the website is on Enhanced Data Model or not. Default to false.
- Added update website record (adx_website/mspp_website) helper to update primary domain name (adx_primarydomainname/mspp_primarydomainname) for different environment during deployment (UpdateWebsite).
- Updated delete table permission (adx_entitypermission/mspp_entitypermission) data helper to support both classic and enhanced data model website (DeleteTablePermission).
- Updated update site setting (adx_sitesetting/mspp_sitesetting) value helper to support both classic and enhanced data model website (UpdatePortalSiteSetting).

## v0.0.0.8
### 23-Dec-2023
- Optimised and fix bug on delete table permission (adx_entitypermission) data helper (DeleteTablePermission).

## v0.0.0.7
### 22-Dec-2023
- Added activate/deactivate of Cloud Flows components when there are option to select all (custom only) or those within a solution (SetCloudFlowStatus).
  - Status Code: Draft = 0; Active = 1

## v0.0.0.6
### 21-Dec-2023
- Fix error handling on helper base class.
- Fix output result on DeleteTablePermission.
- Added activate/deactivate of automatic record rules components when there are option to select all or those within a solution (SetAutoRecordRulesStatus)
  - Status Code: Draft = 0; Active = 1
- Added activate/deactivate of SLA components when there are option to select all or those within a solution (SetSLAStatus)
  - Status Code: Draft = 0; Active = 1

## v0.0.0.5
### 13-Dec-2023
- Fix naming issue on UpdatePortalSiteSetting.

## v0.0.0.4
### 12-Dec-2023
- Introduce token data to be used as replacement value in JSON config file.
- Introduce interface to implement more helper tool (IHelperToolLogic).
- Fix IncrementSolutionVersion and CalculateSolutionVersion to reset the lesser version segment to 0 when incrementing higher version segment.
- Added update site setting (adx_sitesetting) value helper to update site setting value (adx_value) for different environment during deployment (UpdatePortalSiteSetting).
- Added delete table permission (adx_entitypermission) data helper to delete all permission for a website (DeleteTablePermission).

## v0.0.0.3
### 05-Dec-2023
- Added increment solution version helper (IncrementSolutionVersion).
- Added calculate incremented solution version to an output file helper (CalculateSolutionVersion).

## v0.0.0.2
### 04-Dec-2023
- Draft Deployment Helper CmdLine tool.
- Support D365 Test Connection (TestConnection).
