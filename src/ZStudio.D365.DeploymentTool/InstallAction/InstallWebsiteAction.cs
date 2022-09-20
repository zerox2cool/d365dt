using System;
using System.IO;
using System.Configuration;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallWebsiteAction : InstallAction
    {
        private WebsiteCollection websites;
        private WebServiceUtils utils;

        public override string ActionName { get { return "Website"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallWebsiteAction(WebsiteCollection websiteCollections, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            websites = websiteCollections;
            utils = webServiceUtils;
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "WebSites"); }
        }


        public override int Index { get { return websites.ElementInformation.LineNumber; } }


        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return "Failed to install Website.";
            }
            return "Failed to uninstall Website.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.WebsiteFailed;
        }


        protected override void RunBackupAction()
        {
            //if (utils.authenticationType != CrmAuthenticationType.OnPremise)
            //{
            //    throw new Exception(String.Format("{0} deployments do not support website installations.", utils.authenticationType));
            //}
            //IisUtils.StopIis();

            foreach (WebsiteElement website in websites)
            {
                string websiteName = IOUtils.ReplaceStringTokens(website.Name, Context.Tokens);
                string targetDir = IOUtils.ReplaceStringTokens(website.TargetDirectory, Context.Tokens);

                Logger.LogInfo("Backing up {0}...", websiteName);

                if (Directory.Exists(targetDir))
                {
                    Logger.LogInfo("  Backing up {0}...", targetDir);

                    string backupDirectory = Path.Combine(BackupDirectory, Path.GetFileName(targetDir));
                    IOUtils.CopyDirectory(targetDir, backupDirectory);
                }
            }

            IisUtils.StartIis();
         }


        protected override void RunInstallAction()
        {
            //if (utils.authenticationType != CrmAuthenticationType.OnPremise)
            //{
            //    throw new Exception(String.Format("{0} deployments do not support website installations.", utils.authenticationType));
            //}
            bool anyErrors = false;

            RunUninstallAction();

            IisUtils.StopIis();

            foreach (WebsiteElement website in websites)
            {
                string sourceDir = IOUtils.ReplaceStringTokens(website.SourceDirectory, Context.Tokens);
                string targetDir = IOUtils.ReplaceStringTokens(website.TargetDirectory, Context.Tokens);
                string appPool = IOUtils.ReplaceStringTokens(website.ApplicationPool, Context.Tokens);
                string virtualDirectory = IOUtils.ReplaceStringTokens(website.VirtualDirectory, Context.Tokens);
                string name = IOUtils.ReplaceStringTokens(website.Name, Context.Tokens);

                Logger.LogInfo("Installing {0}...", name);

                if (Directory.Exists(sourceDir))
                {
                    Logger.LogInfo("  Copying new {0} to {1}...", name, targetDir);
                    IOUtils.CopyDirectoryReplaceTokens(sourceDir, targetDir, Context.Tokens);

                    Logger.LogInfo("  Setting file access permissions on {0}...", targetDir);
                    IOUtils.GrantCascadingUserAndNetworkServicePermissionsOnDirectory(targetDir);

                    // Create the AppPool
                    if (!IisUtils.AppPoolExists(appPool))
                    {
                        IisUtils.CreateAppPool(appPool);
                    }
                    else
                    {
                        Logger.LogInfo("  Application Pool '{0}' already exists", appPool);
                    }

                    // Create the Virtual Directory in IIS
                    IisUtils.CreateVirtualDirectory(virtualDirectory, targetDir, website.AllowAnonymousAccess);

                    // Assign the Activity Diary Virtual Directory to the Activity Diary AppPool
                    IisUtils.AssignVirtualDirectoryToAppPool(virtualDirectory, appPool);

                    // Set the Virtual Directory to .Net 2.0
                    IisUtils.AddDotNet2AssociationTo(virtualDirectory);

                    foreach (WebsiteMimeElement mime in website)
                    {
                        // Assign the MIME Types
                        IisUtils.AssignMimeTypeToVirtualDirectory(virtualDirectory, mime.Extension, mime.Type);
                    }
                }
                else
                {
                    anyErrors = true;
                    Logger.LogError("Error creating website '{0}', source directory '{1}' was not found", name, sourceDir);
                }
            }

            IisUtils.StartIis();

            if (anyErrors)
            {
                throw new Exception("There was an error installing Custom Websites");
            }
        }


        protected override void RunUninstallAction()
        {
            //if (utils.authenticationType != CrmAuthenticationType.OnPremise)
            //{
            //    throw new Exception(String.Format("{0} deployments do not support website installations.", utils.authenticationType));
            //}
            foreach (WebsiteElement website in websites)
            {
                string sourceDir = IOUtils.ReplaceStringTokens(website.SourceDirectory, Context.Tokens);
                string targetDir = IOUtils.ReplaceStringTokens(website.TargetDirectory, Context.Tokens);
                string appPool = IOUtils.ReplaceStringTokens(website.ApplicationPool, Context.Tokens);
                string virtualDirectory = IOUtils.ReplaceStringTokens(website.VirtualDirectory, Context.Tokens);
                string name = IOUtils.ReplaceStringTokens(website.Name, Context.Tokens);

                Logger.LogInfo("Uninstalling {0}...", name);

                // Remove the Virtual Directory if it exists
                IisUtils.RemoveVirtualDirectory(virtualDirectory);

                // Remove the AppPool if it exists
                if (!IisUtils.AppPoolUsed(appPool))
                {
                    IisUtils.RemoveAppPool(appPool);
                }
                else
                {
                    Logger.LogInfo("  Application Pool '{0}' is still in use and has not been removed", appPool);
                }

                if (Directory.Exists(targetDir))
                {
                    Logger.LogInfo("  Removing existing {0} from {1}...", name, targetDir);
                    IOUtils.SetAllWritableUnderDirectory(targetDir);
                    Directory.Delete(targetDir, true);
                }
            }
        }
    }
}
