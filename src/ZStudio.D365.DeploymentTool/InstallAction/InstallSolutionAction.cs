using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Configuration;
using System.Text;
using System.Collections.Specialized;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallSolutionAction : InstallAction
    {
        private readonly SolutionCollection solCollection;
        private readonly WebServiceUtils utils;
        private int? _ParentLineNumber = null;

        public override string ActionName { get { return "Solution"; } }
        public override InstallType ActionType { get { return InstallType.SolutionAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallSolutionAction(SolutionCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils, int parentLineNumber) : base(context)
        {
            solCollection = collections;
            utils = webServiceUtils;

            //load the solution collection from the XML file provided, should upgrade all other config to load from external config XML in future
            if (!string.IsNullOrEmpty(solCollection.SolutionConfig))
            {
                string xmlFilePath = Path.GetFullPath(IOUtils.ReplaceStringTokens(solCollection.SolutionConfig, Context.Tokens));
                Logger.LogInfo("Loading Solution from the file {0}...", xmlFilePath);

                //install the solutions from the Solution Config XML, generate a new Solution Collection object
                SolutionCollection sol = new SolutionCollection(xmlFilePath);
                solCollection = sol;
                _ParentLineNumber = parentLineNumber;
            }
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "Solutions"); }
        }

        public override int Index
        {
            get
            {
                if (_ParentLineNumber == null)
                    return solCollection.ElementInformation.LineNumber;
                else
                    return _ParentLineNumber.Value;
            }
        }

        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return string.Format("Failed to Create/Update the {0}.", ActionName);
            }
            return string.Format("Failed to Create/Update the {0}.", ActionName);
        }

        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.SolutionFailed;
        }

        protected override void RunBackupAction()
        {
            //nothing to backup
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo(" ");
            Logger.LogInfo("Installing {0} Collection...", ActionName);
            
            bool anyErrors = false;
            string errorMessage = string.Empty;
            foreach (SolutionElement element in solCollection)
            {
                try
                {
                    InstallSolution(element);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    anyErrors = true;
                }
            }
            
            if (anyErrors)
            {
                throw new Exception(string.Format("There was an error executing Solution install. Message: {0}", errorMessage));
            }
        }

        private void InstallSolution(SolutionElement element)
        {
            string uniqueName = IOUtils.ReplaceStringTokens(element.Name, Context.Tokens);
            string displayName = IOUtils.ReplaceStringTokens(element.DisplayName, Context.Tokens);
            string publisherUniqueName = IOUtils.ReplaceStringTokens(element.PublisherUniqueName, Context.Tokens);
            string version = IOUtils.ReplaceStringTokens(element.Version, Context.Tokens);
            string solDescription = element.Description;

            Logger.LogInfo(" ");
            Logger.LogInfo(" Installing {0}...", ActionName);

            //find the publisher
            Publisher pub = SolutionManager.GetPublisherRecord(utils.Service, publisherUniqueName);
            if (pub == null)
            {
                Logger.LogError("  Publisher '{0}' does not exists, please provide a valid Publisher for the solution.", publisherUniqueName);
                throw new Exception(string.Format("Publisher '{0}' does not exists, please provide a valid Publisher for the solution.", publisherUniqueName));
            }
            
            //check if solution exists
            bool updateExisting = false;
            Solution enSol = SolutionManager.GetSolutionRecord(utils.Service, uniqueName);
            if (enSol != null)
            {
                //check if existing solution can be updated, if it is managed, it cannot be modified
                if (enSol.IsManaged != null && enSol.IsManaged.Value)
                {
                    Logger.LogError("  Found the Solution '{0}' in the system, it is a Managed solution, it cannot be updated.", uniqueName);
                    throw new Exception(string.Format("Found the Solution '{0}' in the system, it is a Managed solution, it cannot be updated.", uniqueName));
                }
                else
                {
                    //update existing
                    updateExisting = true;
                    Logger.LogInfo("  Found the Solution '{0}' in the system, proceed to update the Solution header information.", uniqueName);
                }
            }
            else
            {
                //create new record
                enSol = new Solution
                {
                    UniqueName = uniqueName
                };
                Logger.LogInfo("  Solution '{0}' does not exists, proceed to create the solution.", uniqueName);
            }

            //set solution values
            enSol.FriendlyName = displayName;
            enSol.Description = solDescription;
            enSol.PublisherId = new EntityReference(Publisher.EntityLogicalName, pub.Id);

            Guid? solutionId = null;
            if (updateExisting)
            {
                solutionId = enSol.Id;
                utils.Service.Update(enSol);
            }
            else
            {
                //default to v1.0.0.0 when nothing is provided
                if (string.IsNullOrEmpty(version))
                    version = "1.0.0.0";

                enSol.Version = version;
                solutionId = utils.Service.Create(enSol);

                //publish customization for newly created solution
                Logger.LogInfo("  Publishing customizations...");
                SolutionManager.PublishAllXmlRequest(utils.Service);
                Logger.LogInfo("  Published customizations...");
            }
            if (solutionId != null)
                Logger.LogInfo("  SolutionID '{0}'.", solutionId.Value.ToString());
            else
                throw new Exception("Fail to retrieve Solution GUID.");
        }

        protected override void RunUninstallAction()
        {
            //no uninstall action
            Logger.LogWarning("Uninstall not supported for {0}.", ActionName);
        }
    }
}