using System;
using System.IO;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Utils.Plugin;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallAssemblyModeAction : InstallAction
    {
        private readonly AssemblyModeCollection assemblyModeCollection;
        private WebServiceUtils utils;

        public override string ActionName { get { return "Plugin & Workflow Assembly Mode"; } }
        public override InstallType ActionType { get { return InstallType.PluginWorkflowAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallAssemblyModeAction(AssemblyModeCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            assemblyModeCollection = collections;
            utils = webServiceUtils;
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "AssemblyMode"); }
        }


        public override int Index { get { return assemblyModeCollection.ElementInformation.LineNumber; } }

        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return "Failed to install Plugin & Workflow Assembly Mode.";
            }
            return "Failed to uninstall Plugin & Workflow Assembly Mode.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.PluginsWorkflowFailed;
        }


        protected override void RunBackupAction()
        {
          
        }


        protected override void RunInstallAction()
        {
            bool anyErrors = false;
            Logger.LogInfo(" ");
            Logger.LogInfo("Installing {0}...", ActionName);
            
            foreach (AssemblyModeElement modeElement in assemblyModeCollection)
            {

                string name = modeElement.Name;
                bool isolationMode = modeElement.Sandboxed;

                //search and update the assembly mode
                bool result = RegisterPlugins.UpdateAssemblyIsolationMode(name, isolationMode, utils, true);
                if (result)
                {
                    Logger.LogInfo("Operation Successful for {0} (IsolationMode: {1}).", name, isolationMode);
                }
                else
                {
                    Logger.LogError("Error updating Assembly {0} isolation mode to {1}.", name, isolationMode);
                }
            }
            if (anyErrors)
            {
                throw new Exception("There was an error installing Plugin & Workflow Assembly Mode");
            }
        }


        protected override void RunUninstallAction()
        {
           
        }
    }
}