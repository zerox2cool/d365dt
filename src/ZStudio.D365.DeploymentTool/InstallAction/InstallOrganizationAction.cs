using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZD365DT.DeploymentTool.Configuration;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Client;


namespace ZD365DT.DeploymentTool
{
    public class InstallOrganizationAction : InstallAction
    {
        private OrganizationCollection orgCollection;
        private WebServiceUtils utils;

        public override string ActionName { get { return "Organization"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallOrganizationAction(OrganizationCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            orgCollection = collections;
            utils = webServiceUtils;

        }

        public override int Index
        {
            get { return orgCollection.ElementInformation.LineNumber; }
        }

        protected override void RunBackupAction()
        {
            //Not implemented
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo("Executing {0} Installation...", ActionName);
            if (orgCollection != null && utils.authenticationType == CrmAuthenticationType.AD)
            {
                bool anyError = false;
                if (utils.DeployService == null)
                {
                    throw new Exception("crmdeployment section is not defined or Deployment Service is null ");
                }
                else
                {
                    foreach (OrganizationElement element in orgCollection)
                    {
                        string errorString = string.Empty;
                        if (element.Action == Configuration.Action.create)
                        {
                          
                            string UniqueName = IOUtils.ReplaceStringTokens(element.UniqueName, Context.Tokens);
                            string SqlServerName = IOUtils.ReplaceStringTokens(element.SqlServerName, Context.Tokens);
                            string SrsUrl = IOUtils.ReplaceStringTokens(element.SrsUrl, Context.Tokens);
                            string BaseCurrencyCode = IOUtils.ReplaceStringTokens(element.BaseCurrencyCode, Context.Tokens);
                            string BaseCurrencyName = IOUtils.ReplaceStringTokens(element.BaseCurrencyName, Context.Tokens);
                            string BaseCurrencySymbol = IOUtils.ReplaceStringTokens(element.BaseCurrencySymbol, Context.Tokens);                            
                            string FriendlyName = IOUtils.ReplaceStringTokens(element.FriendlyName, Context.Tokens);                            

                            Logger.LogInfo("Creating Org : " + element.UniqueName);
                            OrganizationUtils.CreateOrganization(utils.DeployService, UniqueName, FriendlyName, SqlServerName, SrsUrl,
                                BaseCurrencyCode, BaseCurrencyName, BaseCurrencySymbol, element.BaseCurrencyPrecision, out errorString);

                        }
                        if (element.Action == Configuration.Action.delete)
                        {
                            OrganizationUtils.DeleteOrganization(utils.DeployService, element.UniqueName, out errorString);
                        }
                        if (element.Action == Configuration.Action.enable)
                        {
                            OrganizationUtils.DisableEnableOrganization(utils.DeployService, element.UniqueName, OrganizationState.Enabled, out errorString);
                        }
                        if (element.Action == Configuration.Action.disable)
                            OrganizationUtils.DisableEnableOrganization(utils.DeployService, element.UniqueName, OrganizationState.Disabled, out errorString);

                        if (!string.IsNullOrEmpty(errorString))
                            anyError = true;
                    }
                    if (anyError)
                    {
                        throw new Exception("Organization Action Failed");
                    }
                }
            }
            else
            {
                throw new Exception("Authentication Type Must be OnPremise to create an Organization");
            }
        }

        protected override void RunUninstallAction()
        {
            // throw new NotImplementedException();
        }

        public override string GetExecutionErrorMessage()
        {
            return "Failed Organization Install Action ";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.OrgCreateDelete;
        }

    }
}
