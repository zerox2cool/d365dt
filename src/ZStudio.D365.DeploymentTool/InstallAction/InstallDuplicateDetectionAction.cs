using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Utils.DuplicateDetection;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallDuplicateDetectionAction : InstallAction
    {
        private DuplicateDetectionCollection dupDetectionCollections;
        private WebServiceUtils utils;

        public override string ActionName { get { return "Duplicate Detection Rules"; } }
        public override InstallType ActionType { get { return InstallType.DuplicateDetectionAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallDuplicateDetectionAction(DuplicateDetectionCollection dupDetectionColl, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            dupDetectionCollections = dupDetectionColl;
            utils = webServiceUtils;
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "DuplicateDetection"); }
        }

        public override int Index { get { return dupDetectionCollections.ElementInformation.LineNumber; } }

        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return "Failed to install " + ActionName;
            }
            return "Failed to uninstall " + ActionName;
        }

        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.DuplicateDetectionFailed;
        }

        protected override void RunBackupAction()
        {   
        }

        protected override void RunInstallAction()
        {
            bool anyError = false;
            Logger.LogInfo(" ");
            Logger.LogInfo("Installing {0}...", ActionName);

            foreach (DuplicateDetectionElement dup in dupDetectionCollections)
            {
                string name = dup.Name;
                string sourceFile = IOUtils.ReplaceStringTokens(dup.SourceFile, Context.Tokens);
                string targetFullFilePath = Path.GetFullPath(sourceFile);
                string action = dup.Action.ToLower();

                if (action == "export")
                {
                    Logger.LogInfo("Export {0} for {1}...", ActionName, name);
                    Logger.LogInfo("  Exporting configuration to {0}...", targetFullFilePath);
                    DuplicateDetectionManager.Utils = utils;
                    DuplicateDetection export = DuplicateDetectionManager.RetrieveAllRules();
                    Serialize(targetFullFilePath, export);
                    Logger.LogInfo("{0} Export Completed for {1}...", ActionName, name);
                }
                else if (action == "import")
                {
                    Logger.LogInfo("Import {0} for {1}...", ActionName, name);
                    Logger.LogInfo("  Reading configuration from {0}...", targetFullFilePath);
                    DuplicateDetection dupDetection = Deserialize(targetFullFilePath);
                    DuplicateDetectionManager.Utils = utils;
                    bool success = DuplicateDetectionManager.Publish(dupDetection.DuplicateDetectionRules, ref anyError);

                    if (success)
                        Logger.LogInfo("{0} Import Completed for {1}...", ActionName, name);
                }
                else if (action == "delete")
                {
                    Logger.LogInfo("Delete {0} for {1}...", ActionName, name);
                    Logger.LogInfo("  Reading configuration from {0}...", targetFullFilePath);
                    DuplicateDetection dupDetection = Deserialize(targetFullFilePath);
                    DuplicateDetectionManager.Utils = utils;
                    bool success = DuplicateDetectionManager.Remove(dupDetection.DuplicateDetectionRules, ref anyError);

                    if (success)
                        Logger.LogInfo("{0} Delete Completed for {1}...", ActionName, name);
                }
            }

            if (anyError)
                throw new Exception(string.Format("There was an error installing {0}.", ActionName));
            else
                Logger.LogInfo("{0} Install Completed...", ActionName);
        }

        protected override void RunUninstallAction()
        {
            bool anyError = false;

            Logger.LogInfo(" ");
            foreach (DuplicateDetectionElement dup in dupDetectionCollections)
            {
                string name = dup.Name;
                string targetFile = IOUtils.ReplaceStringTokens(dup.SourceFile, Context.Tokens);
                string importFile = Path.GetFullPath(targetFile);

                Logger.LogInfo("Uninstalling {0} for {1}...", ActionName, name);
                Logger.LogInfo("  Removing existing {0} from the system...", ActionName);
                Logger.LogInfo("  Reading configuration from {0}...", importFile);
                DuplicateDetection dupDetection = Deserialize(importFile);
                DuplicateDetectionManager.Utils = utils;
                bool success = DuplicateDetectionManager.Remove(dupDetection.DuplicateDetectionRules, ref anyError);
            }

            if (anyError)
                throw new Exception(string.Format("There was an error uninstalling {0}.", ActionName));
            else
                Logger.LogInfo("{0} Uninstall Completed...", ActionName);
        }

        #region Serialize / De-serialize Duplicate Detection rules
        private static DuplicateDetection Deserialize(string fileName)
        {
            DuplicateDetection result = null;
            XmlSerializer serializer = new XmlSerializer(typeof(DuplicateDetection));
            using (TextReader reader = File.OpenText(fileName))
            {
                result = serializer.Deserialize(reader) as DuplicateDetection;
            }
            return result;
        }

        private static void Serialize(string fileName, DuplicateDetection value)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.OmitXmlDeclaration = true;
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "  ";
            xmlSettings.NewLineHandling = NewLineHandling.Entitize;
            using (XmlWriter writer = XmlWriter.Create(fileName, xmlSettings))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DuplicateDetection));
                serializer.Serialize(writer, value, ns);
            }
        }
        #endregion Serialize / De-serialize Duplicate Detection rules
    }
}