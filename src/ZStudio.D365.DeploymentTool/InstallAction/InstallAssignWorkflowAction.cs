using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZD365DT.DeploymentTool.Configuration;
using ZD365DT.DeploymentTool.Utils;
using Microsoft.Xrm.Sdk.Metadata;
using ZD365DT.DeploymentTool.Context;

namespace ZD365DT.DeploymentTool
{
    public class InstallAssignWorkflowAction : InstallAction
    {
        private AssignWorkflowsCollection assignCollection;
        private WebServiceUtils utils;
        private Dictionary<string, AttributeMetadata> cacheAttribute = new Dictionary<string, AttributeMetadata>();

        public override string ActionName { get { return "Assign Workflow"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallAssignWorkflowAction(AssignWorkflowsCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils)
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
            Logger.LogInfo("Executing {0}...", ActionName);
            if (assignCollection != null)
            {
                bool anyError = false;
                string errorString = string.Empty;
                foreach (AssignWorkflowsElement element in assignCollection)
                {
                    string user = IOUtils.ReplaceStringTokens(element.DomainUserName, Context.Tokens);
                    string workflowname = IOUtils.ReplaceStringTokens(element.WorkflowName, Context.Tokens);

                    Guid userId = utils.GetUserForDomain(user);

                    if (userId != Guid.Empty)
                    {
                        utils.AssignWorkflows(userId, workflowname, ref anyError, ref errorString);
                    }
                    else
                    {
                        anyError = true;
                        errorString += Environment.NewLine + string.Format("  User '{0}' cound not be found ", user);
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
            return "Failed to Assign Workflow to Specific users.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.AssignWorkflowRole;
        }

    }
}
