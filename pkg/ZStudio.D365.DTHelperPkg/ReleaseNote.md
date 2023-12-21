﻿# DTHelper - ZD365.DeploymentHelper
# Release Notes
## v0.0.0.6
### 21-Dec-2023
- Fix error handling on helper base class.
- Fix output result on DeleteTablePermission.
- Added activate/deactivate of automatic record rules action for all rules or rules in a solution (SetAutoRecordRulesStatus)
- Added activate/deactivate of SLA action for all SLA or SLA in a solution (SetSLAStatus)

## v0.0.0.5
### 13-Dec-2023
- Fix naming issue on UpdatePortalSiteSetting.

## v0.0.0.4
### 12-Dec-2023
- Introduce token data to be used as replacement value in JSON config file.
- Introduce interface to implement more helper tool (IHelperToolLogic).
- Fix IncrementSolutionVersion and CalculateSolutionVersion to reset the lesser version segment to 0 when incrementing higher version segment.
- Added update site setting (adx_sitesetting) value helper to update site setting for different environment during deployment (UpdatePortalSiteSetting).
- Added delete table permission (adx_entitypermission) data helper to delete all permission for a website (DeleteTablePermission).

## v0.0.0.3
### 05-Dec-2023
- Added increment solution version helper (IncrementSolutionVersion).
- Added calculate incremented solution version to an output file helper (CalculateSolutionVersion).

## v0.0.0.2
### 04-Dec-2023
- Draft Deployment Helper CmdLine tool.
- Support D365 Test Connection (TestConnection).