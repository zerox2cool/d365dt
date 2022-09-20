using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallPatchCustomizationXmlAction : InstallAction
    {
        private const string CUSTOMIZATIONS_XML = "customizations.xml";

        private readonly PatchCustomizationXmlCollection patCollection;
        private readonly WebServiceUtils utils;
        private int? _ParentLineNumber = null;

        public override string ActionName { get { return "PatchCustomizationXml"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallPatchCustomizationXmlAction(PatchCustomizationXmlCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils, int parentLineNumber) : base(context)
        {
            patCollection = collections;
            utils = webServiceUtils;
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "PatchCustomizationXmls"); }
        }

        public override int Index
        {
            get
            {
                if (_ParentLineNumber == null)
                    return patCollection.ElementInformation.LineNumber;
                else
                    return _ParentLineNumber.Value;
            }
        }

        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return string.Format("Failed to Create/Update the {0}.", ActionName);
            }
            return string.Format("Failed to Create/Update the {0}.", ActionName);
        }

        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.PatchCustomizationXmlFailed;
        }

        protected override void RunBackupAction()
        {
            //nothing to backup
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo("Installing {0} Collection...", ActionName);

            bool anyErrors = false;
            string errorMessage = string.Empty;

            //loop thru the patch XML collection and install each of them
            foreach (PatchCustomizationXmlElement element in patCollection)
            {
                try
                {
                    PatchCrmSolution(element);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    anyErrors = true;
                }
            }
            
            if (anyErrors)
            {
                throw new Exception(string.Format("There was an error executing {0} install. Message: {1}", ActionName, errorMessage));
            }
        }

        private void PatchCrmSolution(PatchCustomizationXmlElement element)
        {
            string file = IOUtils.ReplaceStringTokens(element.File, Context.Tokens);
            string fullFilePath = Path.GetFullPath(file);

            Logger.LogInfo(" ");
            Logger.LogInfo(" Installing {0}...", ActionName);
            Logger.LogInfo(" Patching the file: {0}", fullFilePath);

            if (element.PatchType == PatchType.CustomControlDefaultConfigs)
            {
                Logger.LogInfo(" Patch Type: {0}", PatchType.CustomControlDefaultConfigs.ToString());

                FileInfo fileInfo = new FileInfo(fullFilePath);
                string extractPath = Path.Combine(fileInfo.DirectoryName, DateTime.Now.ToString("yyyyMMddHHmmss"));

                if (!Directory.Exists(extractPath))
                    Directory.CreateDirectory(extractPath);
                using (var archive = ZipFile.OpenRead(fullFilePath))
                {
                    var entry = archive.Entries.First(e => e.FullName.Equals(CUSTOMIZATIONS_XML, StringComparison.OrdinalIgnoreCase));
                    entry.ExtractToFile(Path.Combine(extractPath, entry.FullName));
                }

                string customizationFile = Path.Combine(extractPath, CUSTOMIZATIONS_XML);
                var doc = XDocument.Load(customizationFile);
                doc.Descendants("CustomControlDefaultConfigs").Descendants().Remove();
                doc.Save(customizationFile);

                using (var archive = ZipFile.Open(fullFilePath, ZipArchiveMode.Update))
                {
                    var oldCustomizationFile = archive.GetEntry(CUSTOMIZATIONS_XML);
                    oldCustomizationFile.Delete();
                    archive.CreateEntryFromFile(customizationFile, CUSTOMIZATIONS_XML);
                }

                //clear extracted files
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);
            }
            else
            {
                Logger.LogError(" Unsupported Patch Type: {0}", element.PatchType.ToString());
            }

            Logger.LogInfo(" Patching completed: {0}", fullFilePath);
        }

        protected override void RunUninstallAction()
        {
            //no uninstall action
            Logger.LogWarning("Uninstall not supported for {0}.", ActionName);
        }
    }
}