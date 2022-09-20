using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZD365DT.DeploymentTool.Configuration;
using Microsoft.Xrm.Sdk.Metadata;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using System.IO;

namespace ZD365DT.DeploymentTool
{
    public class InstallDataExportAction : InstallAction
    {
        private DataExportCollection exportCollection;
        private WebServiceUtils utils;
        private Dictionary<string, AttributeMetadata> cacheAttribute = new Dictionary<string, AttributeMetadata>();

        public override string ActionName { get { return "Data Export"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallDataExportAction(DataExportCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils)
            : base(context)
        {
            exportCollection = collections;
            utils = webServiceUtils;
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "DataBackup"); }
        }

        public override int Index
        {
            get { return exportCollection.ElementInformation.LineNumber; }
        }

        protected override void RunBackupAction()
        {
            
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo(" ");
            Logger.LogInfo("Executing {0}...", ActionName);
            if (exportCollection != null)
            {
                foreach (DataExportElement element in exportCollection)
                {
                    string entityName = IOUtils.ReplaceStringTokens(element.EntityName, Context.Tokens);
                    string fileName = IOUtils.ReplaceStringTokens(element.FileName, Context.Tokens);
                    utils.ExportCrmData(entityName, fileName);
                }
            }
            Logger.LogInfo("Data Export Done...");
        }

        protected override void RunUninstallAction()
        {
           
        }

        public override string GetExecutionErrorMessage()
        {
            return "Failed to Export Data.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.DeleteExportFailed;
        }
    }
}
