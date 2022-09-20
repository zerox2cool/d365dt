using System;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;
using ZD365DT.DeploymentTool.Utils.Plugin;
using System.IO;

namespace ZD365DT.DeploymentTool
{
    public class InstallPluginXmlExport : InstallAction
    {

        private PluginXmlExportElement pluginXmlExportElement;
        private WebServiceUtils utils;

        public override string ActionName { get { return "Plugin & Workflow Export"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallPluginXmlExport(PluginXmlExportElement element, IDeploymentContext context, WebServiceUtils webServiceUtils)
            : base(context)
        {
            pluginXmlExportElement = element;
            utils = webServiceUtils;
        }



        public override int Index
        {
            get { return pluginXmlExportElement.ElementInformation.LineNumber; }
        }

        protected override void RunBackupAction()
        {

        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo(" ");
            Logger.LogInfo("Executing {0}...", ActionName);
            if (pluginXmlExportElement != null)
            {
                string solution = IOUtils.ReplaceStringTokens(pluginXmlExportElement.SolutionName, Context.Tokens);
                string fileName = IOUtils.ReplaceStringTokens(pluginXmlExportElement.FileName, Context.Tokens);
                fileName = Path.GetFullPath(fileName);
                RegisterPlugins.GetPlugins(solution, utils, fileName);
            }

        }

        protected override void RunUninstallAction()
        {
           // throw new NotImplementedException();
        }

        public override string GetExecutionErrorMessage()
        {
            return "Failed to Export Plugin Xml.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.PluginXmlExport;
        }
    }
}
