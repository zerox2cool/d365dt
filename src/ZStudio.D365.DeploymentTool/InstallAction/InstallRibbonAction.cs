using System;
using System.IO;
using System.Xml;
using System.Web.Services.Protocols;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;
using System.Collections.Generic;

namespace ZD365DT.DeploymentTool
{
    public class InstallRibbonAction : InstallAction
    {
        private readonly RibbonCollection ribbonCollection;
        private readonly WebServiceUtils utils;

        public override string ActionName { get { return "Ribbon"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallRibbonAction(RibbonCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            ribbonCollection = collections;
            utils = webServiceUtils;
        }


        protected override string BackupDirectory
        {
            get { return Path.Combine(".", "ClientExtensionEditor"); }
        }


        public override int Index { get { return ribbonCollection.ElementInformation.LineNumber; } }


        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return "Failed to Export/Import the Ribbon.";
            }
            return "Failed to Export/Import the Ribbon.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.RibbonFailed;
        }


        protected override void RunBackupAction()
        {

        }


        protected override void RunInstallAction()
        {
            Logger.LogInfo("Installing {0} Collection...", ActionName);

            foreach (RibbonElement element in ribbonCollection)
            {
                if (element.Action.ToLower() == "import")
                    RibbonImportAction(element);
                else if (element.Action.ToLower() == "export")
                    RibbonExportAction(element);
                else
                    throw new Exception("Not Supported Action");
            }
        }

        private void RibbonImportAction(RibbonElement element)
        {

            string fileName = IOUtils.ReplaceStringTokens(element.FileName, Context.Tokens);
            string solution = IOUtils.ReplaceStringTokens(element.Solution, Context.Tokens);

            Logger.LogInfo(" Importing Ribbon from file {0}...", fileName);
            ClientExtensionEditor editor = new ClientExtensionEditor(solution, utils, BackupDirectory);
            editor.UploadRibbonXml(fileName);
        }

        private void RibbonExportAction(RibbonElement element)
        {
            string fileName = IOUtils.ReplaceStringTokens(element.FileName, Context.Tokens);
            string solution = IOUtils.ReplaceStringTokens(element.Solution, Context.Tokens);
            string entitiesList = IOUtils.ReplaceStringTokens(element.Entities, Context.Tokens);
            string[] entities = entitiesList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            Logger.LogInfo(" Export Ribbon to file {0}...", fileName);
            ClientExtensionEditor editor = new ClientExtensionEditor(solution, utils, BackupDirectory);
            editor.GetRibbonXml(entities, element.IncludeAppRibbon, fileName);
        }

        protected override void RunUninstallAction()
        {


        }
    }
}
