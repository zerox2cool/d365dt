using System;
using System.IO;
using System.Configuration;
using System.Xml;
using System.Runtime.InteropServices;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallWindowsServiceAction : InstallAction
    {
        private WindowsServiceCollection windowsServices;
        private WebServiceUtils utils;

        public override string ActionName { get { return "Install Windows Service"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        #region Private Constants

        private const string SERVICE_INSTALLER = "InstallUtil.exe";

        #endregion Private Constants

        public InstallWindowsServiceAction(WindowsServiceCollection windowsCollections ,IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            windowsServices =windowsCollections;
            utils = webServiceUtils;
        }


        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return "Failed to install Windows Service.";
            }
            return "Failed to uninstall Windows Service.";
        }


        public override int Index { get { return windowsServices.ElementInformation.LineNumber; } }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.WindowsServiceFailed;
        }


        /// <summary>
        /// Backs up the PPMS Data Feed Service if it exists
        /// </summary>
        protected override void RunBackupAction()
        {
            
            foreach (WindowsServiceElement service in windowsServices)
            {
                string targetDirectory = IOUtils.ReplaceStringTokens(service.TargetDirectory, Context.Tokens);
                string executable = IOUtils.ReplaceStringTokens(service.Executable, Context.Tokens);
                string name = IOUtils.ReplaceStringTokens(service.Name, Context.Tokens);

                string serviceTarget = Path.Combine(targetDirectory, executable);

                if (File.Exists(serviceTarget))
                {
                    Logger.LogInfo("Backing up {0}...", name);
                    if (WindowsServiceUtils.ServiceInstalled(name))
                    {
                        // Stop the service before uninstalling it
                        WindowsServiceUtils.StopService(name);

                        string filename = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), SERVICE_INSTALLER);
                        string arguments = "/U /LogToConsole=true /LogFile= /ShowCallStack=true \"" + serviceTarget + "\"";

                        Logger.LogInfo("Removing {0} {1}...", name, filename);
                        LoggingProcess proc = new LoggingProcess();
                        int returnCode = proc.Execute(filename, arguments, Environment.CurrentDirectory);

                        if (returnCode != 0)
                        {
                            throw new ApplicationException(String.Format("Error Backing up {0}.", name));
                        }
                    }
                    Logger.LogInfo("Backing up {0} files from {1} to {2}...", name, targetDirectory, BackupDirectory);
                    if (Directory.Exists(targetDirectory))
                    {
                        IOUtils.SetAllWritableUnderDirectory(targetDirectory);
                        string backupDirectory = Path.Combine(BackupDirectory, Path.GetFileName(targetDirectory));
                        IOUtils.CopyDirectory(targetDirectory, backupDirectory);
                    }
                }
            }
        }


        /// <summary>
        /// Installs the PPMS Data Feed Windows Service
        /// </summary>
        protected override void RunInstallAction()
        {
            
            RunUninstallAction();
            bool anyErrors = false;

            foreach (WindowsServiceElement service in windowsServices)
            {
                
                string targetDirectory = IOUtils.ReplaceStringTokens(service.TargetDirectory, Context.Tokens);
                string sourceDirectory = IOUtils.ReplaceStringTokens(service.SourceDirectory, Context.Tokens);
                string executable = IOUtils.ReplaceStringTokens(service.Executable, Context.Tokens);
                string name = IOUtils.ReplaceStringTokens(service.Name, Context.Tokens);

                string serviceTarget = Path.Combine(targetDirectory, executable);
                string serviceSource = Path.Combine(sourceDirectory, executable);

                Logger.LogInfo("Installing {0}...", name);

                if (!Directory.Exists(sourceDirectory))
                {
                    anyErrors = true;
                    Logger.LogError("Error installing windows service '{0}', source directory '{1}' was not found", name, sourceDirectory);
                }
                else if (!File.Exists(serviceSource))
                {
                    anyErrors = true;
                    Logger.LogError("Error installing windows service '{0}', executable '{1}' was not found", name, serviceSource);
                }
                else
                {
                    Logger.LogInfo("  Copying {0} files from {1} to {2}...", name, serviceSource, serviceTarget);
                    if (Directory.Exists(serviceTarget))
                    {
                        IOUtils.SetAllWritableUnderDirectory(serviceTarget);
                    }
                    IOUtils.CopyDirectoryReplaceTokens(serviceSource, serviceTarget, Context.Tokens);

                    LoggingProcess proc = new LoggingProcess();

                    string filename = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), SERVICE_INSTALLER);
                    string arguments = String.Format("/I /LogToConsole=true /LogFile= /ShowCallStack=true \"{0}\"", serviceTarget);

                    Logger.LogInfo("  Installing {0} {1}...", name, filename);
                    int returnCode = proc.Execute(filename, arguments, Environment.CurrentDirectory);

                    if (returnCode != 0)
                    {
                        throw new ApplicationException(String.Format("Error Installing {0}.", name));
                    }

                    // Start the service after installing it
                    WindowsServiceUtils.StartService(name);
                }
            }
            if (anyErrors)
            {
                throw new Exception("There was an error installing Windows Services");
            }
        }


        /// <summary>
        /// Uninstalls the PPMS Data Feed Windows Service
        /// </summary>
        protected override void RunUninstallAction()
        {
           
            foreach (WindowsServiceElement service in windowsServices)
            {
                
                string targetDirectory = IOUtils.ReplaceStringTokens(service.TargetDirectory, Context.Tokens);
                string sourceDirectory = IOUtils.ReplaceStringTokens(service.SourceDirectory, Context.Tokens);
                string executable = IOUtils.ReplaceStringTokens(service.Executable, Context.Tokens);
                string name = IOUtils.ReplaceStringTokens(service.Name, Context.Tokens);

                string serviceTarget = Path.Combine(targetDirectory, executable);
                string serviceSource = Path.Combine(sourceDirectory, executable);

                Logger.LogInfo("Uninstalling {0}...", name);
                if (WindowsServiceUtils.ServiceInstalled(name))
                {
                    Logger.LogInfo("  Stopping service...");
                    WindowsServiceUtils.StopService(name);
                    string filename = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), SERVICE_INSTALLER);
                    string arguments = "/U /LogToConsole=true /LogFile= /ShowCallStack=true \"" + serviceTarget + "\"";
                    Logger.LogInfo("  Deleting {0}...", name);
                    LoggingProcess proc = new LoggingProcess();
                    int returnCode = proc.Execute(filename, arguments, Environment.CurrentDirectory);

                    if (returnCode != 0)
                    {
                        throw new ApplicationException(String.Format("Error deleting {0}.", name));
                    }
                }

                if (Directory.Exists(targetDirectory))
                {
                    IOUtils.SetAllWritableUnderDirectory(targetDirectory);
                    Logger.LogInfo("  Deleting folder {0} and its contents...", targetDirectory);
                    Directory.Delete(targetDirectory, true);
                }
            }
        }
    }
}
