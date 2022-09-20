using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZD365DT.DeploymentTool.Configuration;
using Microsoft.Xrm.Sdk.Metadata;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Utils;

namespace ZD365DT.DeploymentTool
{
    class InstallActivateWorkflowsAction : InstallAction
    {
        private ActivateWorkflowsCollection assignCollection;
        private WebServiceUtils utils;
        private Dictionary<string, AttributeMetadata> cacheAttribute = new Dictionary<string, AttributeMetadata>();

        public override string ActionName { get { return "Activate Workflow"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallActivateWorkflowsAction(ActivateWorkflowsCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils)
            : base(context)
        {
            assignCollection = collections;
            utils = webServiceUtils;
        }

        public override int Index
        {
            get { return assignCollection.ElementInformation.LineNumber; }
        }

        protected override void RunBackupAction()
        {
            //Do nothing for Backup
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo(" ");
            Logger.LogInfo("Executing {0}...", ActionName);
            if (assignCollection != null)
            {
                bool anyError = false;
                string errorString = string.Empty;

                if (assignCollection.AllWorkflows)                
                    utils.ActivateWorkflows(utils.CurrentOrgContext.CurrentUserId, string.Empty, assignCollection.AllWorkflows, ref anyError, ref errorString);                
                else
                {
                    foreach (ActivateWorkflowsElement element in assignCollection)
                    {
                        string workflowname = IOUtils.ReplaceStringTokens(element.Name, Context.Tokens);
                        utils.ActivateWorkflows(utils.CurrentOrgContext.CurrentUserId, workflowname,assignCollection.AllWorkflows,ref anyError, ref errorString);
                    }
                }
                
                

                if (anyError)
                    throw new Exception(errorString);
            }
        }

        protected override void RunUninstallAction()
        {
            // throw new NotImplementedException();
        }

        public override string GetExecutionErrorMessage()
        {
            return "Failed to Activate Workflows";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.ActivateWorkflowRole;
        }

    }
}
