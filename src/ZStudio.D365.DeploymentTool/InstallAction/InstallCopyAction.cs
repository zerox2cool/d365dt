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
    public class InstallCopyAction : InstallAction
    {
        private readonly CopyCollection copyCollection;
        private readonly WebServiceUtils utils;

        public override string ActionName { get { return "Copy Files & Directory"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallCopyAction(CopyCollection copyCollections, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            copyCollection = copyCollections;
            utils = webServiceUtils;
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "FilesAndFolders"); }
        }

        public override int Index { get { return copyCollection.ElementInformation.LineNumber; } }

        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return "Failed to copy files & directories.";
            }
            return "Failed to remove files & directories.";
        }

        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.CopyFailed;
        }


        protected override void RunBackupAction()
        {
            //if (utils.authenticationType != CrmAuthenticationType.OnPremise)
            //{
            //    throw new Exception(String.Format("{0} deployments do not support copying of files and directories.",utils.authenticationType));
            //}
            Logger.LogInfo("Backing up files & directories...");

            foreach (CopyElement elemet in copyCollection)
            {
                if (!String.IsNullOrEmpty(elemet.Target))
                {
                    string target = IOUtils.ReplaceStringTokens(elemet.Target, Context.Tokens);
                    string backup = Path.Combine(BackupDirectory, Path.GetFileName(target));
                    if (elemet.CopyType == CopyType.directory)
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
            //if (utils.authenticationType != CrmAuthenticationType.OnPremise)
            //{
            //    throw new Exception(String.Format("{0} deployments do not support copying of files and directories.", utils.authenticationType));
            //}
            Logger.LogInfo(" ");
            Logger.LogInfo("Executing {0}...", ActionName);

            foreach (CopyElement element in copyCollection)
            {
                if (!String.IsNullOrEmpty(element.Source) && !String.IsNullOrEmpty(element.Target))
                {
                    if (element.CopyType == CopyType.directory)
                    {
                        string sourceDirectory = IOUtils.ReplaceStringTokens(element.Source, Context.Tokens);
                        string targetDirectory = IOUtils.ReplaceStringTokens(element.Target, Context.Tokens);

                        sourceDirectory = Path.GetFullPath(sourceDirectory);
                        targetDirectory = Path.GetFullPath(targetDirectory);

                        Logger.LogInfo("  Copying direcory {0} to {1}...", sourceDirectory, targetDirectory);
                        IOUtils.CopyDirectoryReplaceTokens(sourceDirectory, targetDirectory, Context.Tokens);
                    }
                    else
                    {
                        string sourceFile = IOUtils.ReplaceStringTokens(element.Source, Context.Tokens);
                        string targetFile = IOUtils.ReplaceStringTokens(element.Target, Context.Tokens);

                        sourceFile = Path.GetFullPath(sourceFile);
                        targetFile = Path.GetFullPath(targetFile);

                        Logger.LogInfo("  Copying file {0} to {1}...", sourceFile, targetFile);
                        IOUtils.CopyFileReplaceTokens(sourceFile, targetFile, Context.Tokens);
                    }
                }
            }
        }


        protected override void RunUninstallAction()
        {
            //if (utils.authenticationType != CrmAuthenticationType.OnPremise)
            //{
            //    throw new Exception(String.Format("{0} deployments do not support copying of files and directories.", utils.authenticationType));
            //}
            Logger.LogInfo("Removing files & directories...");

            foreach (CopyElement element in copyCollection)
            {
                if (element.RemoveOnUninstall)
                {
                    if (!String.IsNullOrEmpty(element.Target))
                    {
                        if (element.CopyType == CopyType.directory)
                        {
                            string targetDirectory = IOUtils.ReplaceStringTokens(element.Target, Context.Tokens);
                            if (Directory.Exists(targetDirectory))
                            {
                                Logger.LogInfo("  Deleting directory {0}", targetDirectory);
                                IOUtils.SetAllWritableUnderDirectory(targetDirectory);
                                Directory.Delete(targetDirectory, true);
                            }
                        }

                        else
                        {
                            if (!String.IsNullOrEmpty(element.Target))
                            {
                                string targetFile = IOUtils.ReplaceStringTokens(element.Target, Context.Tokens);

                                if (File.Exists(targetFile))
                                {
                                    Logger.LogInfo("  Deleting file {0}", targetFile);
                                    File.SetAttributes(targetFile, FileAttributes.Normal);
                                    File.Delete(targetFile);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
