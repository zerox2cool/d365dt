# DTHelper - ZD365.DeploymentHelper
# User Guide

This guide provides an overview of the DTHelper - ZD365.DeploymentHelper, a command line tool designed to assist with various deployment tasks in Dynamics 365 Customer Engagement (D365 CE).

## CLI Features
### General Helper
- **Test Connection**: Verify connectivity to a D365 instance.
- **Calculate Solution Version**: Calculate and output the next version of a solution based on specified criteria.

### Solution Management
- **Increment Solution Version**: Automatically increment the version of a solution.

### Turn On/Off Components
- **Set Cloud Flow Status**: Activate or deactivate cloud flows based on specified conditions.
- **Set Auto Record Rules Status**: Activate or deactivate automatic record rules.
- **Set SLA Status**: Activate or deactivate Service Level Agreements (SLAs).

### Security Configurations
- **Sync Teams to Column Security Profile**: Synchronize teams with the specified column security profile.

### Power Pages
- **Update Website**: Update the primary domain name of a Power Pages website during deployment.
- **Update Portal Site Setting**: Update site settings for Power Pages during deployment.
- **Delete Table Permission**: Remove table permissions for a specified website.

## Additional Features
- Support for Power Pages Enhanced Data Model: Option to handle deployments for websites using the enhanced data model.
- Token Replacement: Use tokens in configuration files for dynamic value replacement during deployment.
- Comprehensive Logging: Maintain detailed logs of operations for auditing and troubleshooting.
- Configuration via JSON: Use JSON files for flexible and easy configuration of deployment tasks.
- Debug Mode: Enable debug mode for more detailed output during execution.