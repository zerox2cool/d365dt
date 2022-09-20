using System;
using System.IO;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Utils.Plugin;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallPluginsAction : InstallAction
    {
        private readonly PluginCollection pluginCollection;
        private WebServiceUtils utils;

        public override string ActionName { get { return "Plugin & Workflow"; } }
        public override InstallType ActionType { get { return InstallType.PluginWorkflowAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallPluginsAction(PluginCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            pluginCollection = collections;
            utils = webServiceUtils;
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "Plugins"); }
        }


        public override int Index { get { return pluginCollection.ElementInformation.LineNumber; } }


        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return "Failed to install Plugins and Workflows.";
            }
            return "Failed to uninstall Plugins and Workflows.";
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

            try
            {
                CrmAsyncServiceUtil.StopCrmAsyncService();
            }
            catch
            {
                //ignore error from stopping CRM Async Service
            }

            foreach (PluginElement pluginElement in pluginCollection)
            {

                string SourceDirectory = IOUtils.ReplaceStringTokens(pluginCollection.SourceDirectory, Context.Tokens);
                string DefinitionFileName = IOUtils.ReplaceStringTokens(pluginElement.DefinitionFileName, Context.Tokens);

                string pluginDefinitionFile = Path.Combine(SourceDirectory, DefinitionFileName);

                if (File.Exists(pluginDefinitionFile))
                {
                    Logger.LogInfo(" ");
                    Logger.LogInfo("Registering Plugins and Workflows based on the file {0}...", pluginDefinitionFile);

                    //Register the plugin
                    RegisterPlugins.Register(pluginDefinitionFile, utils, Context.Tokens, pluginElement);
                }
                else
                {
                    anyErrors = true;
                    Logger.LogError("Error installing plugins, the definition file '{0}' was not found", pluginDefinitionFile);
                }
            }
            if (anyErrors)
            {
                throw new Exception("There was an error installing plugins");
            }
        }


        protected override void RunUninstallAction()
        {
           
        }
    }
}
