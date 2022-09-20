using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Configuration;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Utils.SitemapCustomization;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;
using Microsoft.Win32;
using System.Text;

namespace ZD365DT.DeploymentTool
{
    public class InstallSitemapAction : InstallAction
    {
        private readonly SitemapCollection sitemapCollection;
        private readonly WebServiceUtils utils;

        public override string ActionName { get { return "Sitemap"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallSitemapAction(SitemapCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            sitemapCollection = collections;
            utils = webServiceUtils;
        }


        protected override string BackupDirectory
        {
            get { return Path.Combine(".", "ClientExtensionEditor"); }
        }


        public override int Index { get { return sitemapCollection.ElementInformation.LineNumber; } }


        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return "Failed to Export/Import the Sitemap.";
            }
            return "Failed to Export/Import the Sitemap.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.SitemapFailed;
        }


        protected override void RunBackupAction()
        {

        }


        protected override void RunInstallAction()
        {
            Logger.LogInfo("Installing {0} Collection...", ActionName);

            foreach (SitemapElement element in sitemapCollection)
            {
                if (element.Action.ToLower() == "import")
                    ImportAction(element);
                else if (element.Action.ToLower() == "export")
                    ExportAction(element);
                else
                    throw new Exception("Not Supported Action");
            }
        }

        private void ImportAction(SitemapElement element)
        {

            string fileName = IOUtils.ReplaceStringTokens(element.FileName, Context.Tokens);
            string solution = IOUtils.ReplaceStringTokens(element.Solution, Context.Tokens);

            Logger.LogInfo(" Importing Sitmap from file {0}...", fileName);
            ClientExtensionEditor editor = new ClientExtensionEditor(solution, utils, BackupDirectory);
            editor.UploadSiteMapXml(fileName);
        }

        private void ExportAction(SitemapElement element)
        {
            string fileName = IOUtils.ReplaceStringTokens(element.FileName, Context.Tokens);
            string solution = IOUtils.ReplaceStringTokens(element.Solution, Context.Tokens);

            Logger.LogInfo(" Export Sitmap to file {0}...", fileName);
            ClientExtensionEditor editor = new ClientExtensionEditor(solution, utils, BackupDirectory);
            editor.GetSiteMapXml(fileName);
        }

        protected override void RunUninstallAction()
        {


        }

    }
}
