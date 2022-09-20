using System;
using System.IO;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallCustomizationsAction : InstallAction
    {
        private CustomizationCollection customizationCollection;
        private WebServiceUtils utils;
        private int? _ParentLineNumber = null;

        public override string ActionName { get { return "Customizations"; } }
        public override InstallType ActionType { get { return InstallType.CustomizationAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallCustomizationsAction(CustomizationCollection custCollections, IDeploymentContext context, WebServiceUtils webServiceUtils, int parentLineNumber) : base(context)
        {
            customizationCollection = custCollections;
            utils = webServiceUtils;

            //load the customization collection from the XML file provided, should upgrade all other config to load from external config XML in future
            if (!string.IsNullOrEmpty(custCollections.CustomizationConfig))
            {
                string xmlFilePath = Path.GetFullPath(IOUtils.ReplaceStringTokens(custCollections.CustomizationConfig, Context.Tokens));
                Logger.LogInfo("Loading Customization from the file {0}...", xmlFilePath);

                //install the customizations from the Customization Config XML, generate a new Customization Collection object
                CustomizationCollection cust = new CustomizationCollection(xmlFilePath);
                customizationCollection = cust;
                _ParentLineNumber = parentLineNumber;
            }
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "Customization"); }
        }

        public override int Index
        {
            get
            {
                if (_ParentLineNumber == null)
                    return customizationCollection.ElementInformation.LineNumber;
                else
                    return _ParentLineNumber.Value;
            }
        }

        public override string GetExecutionErrorMessage()
        {
            return string.Format("Failed to install {0}.", ActionName);
        }

        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.CustomizationsFailed;
        }

        protected override void RunBackupAction()
        {

        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo(" ");
            Logger.LogInfo("Installing {0}...", ActionName);

            if (utils.CurrentOrgContext.MajorVersion >= 8)
            {
                //execute install for current supported CRM version
                Logger.LogInfo("Run Current Install Customization.");
                RunCurrentInstall();
            }
            else
            {
                //execute legacy install (for CRM2015 and below)
                Logger.LogInfo("Run Legacy Install Customization.");
                RunLegacyInstall();
            }
        }

        /// <summary>
        /// Run Install Customization for Current supported CRM (CRM version 8 or higher
        /// </summary>
        private void RunCurrentInstall()
        {
            bool anyError = false;

            //get the list of workflow that is being deployed in the CRM solution so that only these workflows will be deactivated and reactivated during the deployment
            string[] workflowList = null;
            if (!string.IsNullOrEmpty(customizationCollection.WorkflowList))
            {
                string workflowListFile = IOUtils.ReplaceStringTokens(customizationCollection.WorkflowList, Context.Tokens);
                workflowList = File.ReadAllLines(workflowListFile);
            }

            bool isBatchPublishRequired = false;
            foreach (CustomizationElement customization in customizationCollection)
            {
                string solutionName = IOUtils.ReplaceStringTokens(customization.SolutionName, Context.Tokens);
                string customizationFile = IOUtils.ReplaceStringTokens(customization.CustomizationFile, Context.Tokens);
                string isManagedText = IOUtils.ReplaceStringTokens(customization.IsManagedText, Context.Tokens);
                bool isManaged = bool.Parse(isManagedText);
                customization.IsManaged = isManaged;
                string fileName = IOUtils.ReplaceStringTokens(customization.SolutionName, Context.Tokens);
                string backupFile = Path.Combine(BackupDirectory, fileName) + ".zip";
                int waitTimeout = customization.WaitTimeout;
                int exportRetryTimeout = customization.ExportRetryTimeout;

                if (customization.Action.ToLower() == "import")
                {
                    if (customization.BackupBeforeImport)
                    {
                        Logger.LogInfo(string.Format("Backup the solution {0} to {1}", solutionName, backupFile));
                        utils.ExportSolution(backupFile, solutionName, customization.IsManaged, customization.PublishBeforeExport, ref anyError, exportRetryTimeout);
                    }

                    if (customization.DeactivateWorkflow)
                    {
                        Logger.LogInfo("Assign workflow to the executing user and Deactivate the workflows ");
                        utils.AssignAndDeactivateWorkflowsOnly(utils.CurrentOrgContext.CurrentUserId, workflowList);
                    }

                    Logger.LogInfo("Importing Solution: {0}", solutionName);
                    Logger.LogInfo("Importing the file: {0}", Path.GetFullPath(customizationFile));
                    utils.ImportSolution(Path.GetFullPath(customizationFile), solutionName, customization.IsManaged, ref anyError, waitTimeout);

                    if (!anyError && customizationCollection.PublishType == PublishType.Separately && !isManaged)
                    {
                        //only publish when solution import is not managed and publish separately
                        utils.PublishCustomizations(customizationCollection.PublishWorkflows, ref anyError, workflowList);
                    }

                    //mark publish batch required when we are importing unmanaged and that the publish is by batch
                    if (!anyError && customizationCollection.PublishType == PublishType.Batch && !isManaged)
                        isBatchPublishRequired = true;
                }
                else if (customization.Action.ToLower() == "export")
                {
                    Logger.LogInfo(string.Format("Exporting the solution {0} to {1}", solutionName, customizationFile));
                    utils.ExportSolution(Path.GetFullPath(customizationFile), solutionName, customization.IsManaged, customization.PublishBeforeExport, ref anyError, exportRetryTimeout);
                }
                else if (customization.Action.ToLower() == "upgrade")
                {
                    if (customization.BackupBeforeImport)
                    {
                        Logger.LogInfo(string.Format("Backup the solution {0} to {1}", solutionName, backupFile));
                        utils.ExportSolution(backupFile, solutionName, customization.IsManaged, customization.PublishBeforeExport, ref anyError, exportRetryTimeout);
                    }

                    if (customization.DeactivateWorkflow)
                    {
                        Logger.LogInfo("Assign workflow to the executing user and Deactivate the workflows ");
                        utils.AssignAndDeactivateWorkflowsOnly(utils.CurrentOrgContext.CurrentUserId, workflowList);
                    }

                    Logger.LogInfo(string.Format("Upgrade the solution {0}", solutionName));
                    SolutionManager.UpgradeSolutionCommand(customizationFile, solutionName, customization.IsManaged, utils, ref anyError);

                    //mark publish batch required when we are importing unmanaged and that the publish is by batch
                    if (!anyError && customizationCollection.PublishType == PublishType.Batch && !isManaged)
                        isBatchPublishRequired = true;
                }
            }

            if (!anyError && customizationCollection.PublishType == PublishType.Batch && isBatchPublishRequired)
            {
                //only publish in the end as a batch if it is required, no publish when we run Export only or import of Managed only
                utils.PublishCustomizations(customizationCollection.PublishWorkflows, ref anyError, workflowList);
            }

            if (anyError)
            {
                throw new Exception(string.Format("There was an error importing {0}.", ActionName));
            }
        }

        /// <summary>
        /// Run Install Customization for Legacy CRM (CRM version 7 or lower)
        /// </summary>
        private void RunLegacyInstall()
        {
            bool anyError = false;

            //get the list of workflow that is being deployed in the CRM solution so that only these workflows will be deactivated and reactivated during the deployment
            string[] workflowList = null;
            if (!string.IsNullOrEmpty(customizationCollection.WorkflowList))
            {
                string workflowListFile = IOUtils.ReplaceStringTokens(customizationCollection.WorkflowList, Context.Tokens);
                workflowList = File.ReadAllLines(workflowListFile);
            }

            foreach (CustomizationElement customization in customizationCollection)
            {
                string solutionName = IOUtils.ReplaceStringTokens(customization.SolutionName, Context.Tokens);
                string customizationFile = IOUtils.ReplaceStringTokens(customization.CustomizationFile, Context.Tokens);
                string isManagedText = IOUtils.ReplaceStringTokens(customization.IsManagedText, Context.Tokens);
                customization.IsManaged = bool.Parse(isManagedText);
                string fileName = IOUtils.ReplaceStringTokens(customization.SolutionName, Context.Tokens);
                string backupFile = Path.Combine(BackupDirectory, fileName) + ".zip";
                int waitTimeout = customization.WaitTimeout;
                int exportRetryTimeout = customization.ExportRetryTimeout;

                if (customization.Action.ToLower() == "import")
                {
                    if (customization.BackupBeforeImport)
                    {
                        Logger.LogInfo(string.Format("Backup the solution {0} to {1}", solutionName, backupFile));
                        utils.ExportSolution(backupFile, solutionName, customization.IsManaged, customization.PublishBeforeExport, ref anyError, exportRetryTimeout);
                    }

                    if (customization.DeactivateWorkflow)
                    {
                        Logger.LogInfo("Assign workflow to the executing user and Deactivate the workflows ");
                        utils.AssignAndDeactivateWorkflowsOnly(utils.CurrentOrgContext.CurrentUserId, workflowList);
                    }

                    Logger.LogInfo("Importing Solution: {0}", solutionName);
                    Logger.LogInfo("Importing the file: {0}", Path.GetFullPath(customizationFile));
                    utils.ImportSolution(Path.GetFullPath(customizationFile), solutionName, customization.IsManaged, ref anyError, waitTimeout);

                    if (!anyError && customizationCollection.PublishType == PublishType.Separately)
                    {
                        utils.PublishCustomizations(customizationCollection.PublishWorkflows, ref anyError, workflowList);
                    }
                }
                else if (customization.Action.ToLower() == "export")
                {
                    Logger.LogInfo(string.Format("Exporting the solution {0} to {1}", solutionName, customizationFile));
                    utils.ExportSolution(Path.GetFullPath(customizationFile), solutionName, customization.IsManaged, customization.PublishBeforeExport, ref anyError, exportRetryTimeout);
                }
                else if (customization.Action.ToLower() == "upgrade")
                {
                    if (customization.BackupBeforeImport)
                    {
                        Logger.LogInfo(string.Format("Backup the solution {0} to {1}", solutionName, backupFile));
                        utils.ExportSolution(backupFile, solutionName, customization.IsManaged, customization.PublishBeforeExport, ref anyError, exportRetryTimeout);
                    }

                    if (customization.DeactivateWorkflow)
                    {
                        Logger.LogInfo("Assign workflow to the executing user and Deactivate the workflows ");
                        utils.AssignAndDeactivateWorkflowsOnly(utils.CurrentOrgContext.CurrentUserId, workflowList);
                    }

                    Logger.LogInfo(string.Format("Upgrade the solution {0}", solutionName));
                    SolutionManager.UpgradeSolutionCommand(customizationFile, solutionName, customization.IsManaged, utils, ref anyError);
                }
            }
            if (!anyError && customizationCollection.PublishType == PublishType.Batch)
            {
                utils.PublishCustomizations(customizationCollection.PublishWorkflows, ref anyError, workflowList);
            }

            if (anyError)
            {
                throw new Exception(string.Format("There was an error importing {0}.", ActionName));
            }
        }

        protected override void RunUninstallAction()
        {
            Logger.LogInfo("Not Implemented.");
        }
    }
}