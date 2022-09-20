using System;
using System.IO;
using System.Xml;
using System.Web.Services.Protocols;
//using Microsoft.Crm.SdkTypeProxy;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallDeleteAction : InstallAction
    {
        private readonly DeleteCollection deleteCollection;
        private readonly WebServiceUtils utils;

        public override string ActionName { get { return "Delete Files & Directories"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallDeleteAction(DeleteCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils)
            : base(context)
        {
            deleteCollection = collections;
            utils = webServiceUtils;
        }


        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "FilesAndFolders"); }
        }


        public override int Index { get { return deleteCollection.ElementInformation.LineNumber; } }


        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return "Failed to Delete files & directories.";
            }
            return "Failed to Delete files & directories.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.DeleteFailed;
        }


        protected override void RunBackupAction()
        {
          
            Logger.LogInfo("Backing up files & directories...");

            foreach (DeleteElement element in deleteCollection)
            {
                if (!String.IsNullOrEmpty(element.Target))
                {
                    string target = IOUtils.ReplaceStringTokens(element.Target, Context.Tokens);
                    string backup = Path.Combine(BackupDirectory, Path.GetFileName(target));
                    if (element.DeleteType == DeleteType.directory)
                    {
                        if (Directory.Exists(target))
                        {
                            Logger.LogInfo("  Backing up directory {0}...", target);
                            IOUtils.CopyDirectory(target, backup);
                        }
                    }
                    else
                    {
                        if (File.Exists(target))
                        {
                            Logger.LogInfo("  Backing up file {0}...", target);
                            IOUtils.CopyFile(target, backup);
                        }
                    }
                }
            }
        }


        protected override void RunInstallAction()
        {
            Logger.LogInfo(" ");
            Logger.LogInfo("Executing {0}...", ActionName);

            foreach (DeleteElement element in deleteCollection)
            {
                if (!String.IsNullOrEmpty(element.Target))
                {
                    if (element.DeleteType == DeleteType.directory)
                    {
                       
                        string targetDirectory = IOUtils.ReplaceStringTokens(element.Target, Context.Tokens);                       
                        targetDirectory = Path.GetFullPath(targetDirectory);                       

                        if (element.TargetType == TargetType.all) 
                        {
                            Logger.LogInfo("  Deleting root directory {0} ...", targetDirectory);
                            Directory.Delete(targetDirectory,true);
                        }
                        else
                        {
                            Logger.LogInfo("  Deleting files and subfolders in directory {0} ...", targetDirectory);
                            DirectoryInfo downloadedMessageInfo = new DirectoryInfo(targetDirectory);
                            if (element.TargetType == TargetType.allcontent || element.TargetType == TargetType.files)
                            {
                                foreach (FileInfo file in downloadedMessageInfo.GetFiles())
                                {
                                    file.Delete();
                                }
                            }
                            if (element.TargetType == TargetType.allcontent || element.TargetType == TargetType.subfolder)
                            {
                                foreach (DirectoryInfo dir in downloadedMessageInfo.GetDirectories())
                                {
                                    dir.Delete(true);
                                }
                            }
                        }
                    }
                    else
                    {                        
                        string targetFile = IOUtils.ReplaceStringTokens(element.Target, Context.Tokens);
                        Logger.LogInfo("  Deleting file {0} ...", targetFile);
                        targetFile = Path.GetFullPath(targetFile);
                        File.Delete(targetFile);
                    }
                }
            }
        }


        protected override void RunUninstallAction()
        {          
            
           
        }
    }
}
