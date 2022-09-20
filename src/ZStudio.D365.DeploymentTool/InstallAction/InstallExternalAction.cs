using System;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallExternalAction : InstallAction
    {
        private ExternalActionCollection externalActions;

        public override string ActionName { get { return "External Action"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallExternalAction(ExternalActionCollection externalAction, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            if (context.DeploymentConfig != null)
            {
                externalActions = externalAction;
            }
        }


        public override string GetExecutionErrorMessage()
        {
            return "Failed to execute external actions.";
        }


        public override int Index { get { return externalActions.ElementInformation.LineNumber; } }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.ExternalActionFailed;
        }


        protected override void RunBackupAction()
        {
            // Do nothing for backup
        }


        protected override void RunInstallAction()
        {
            if (externalActions != null)
            {
                Logger.LogInfo(" ");
                Logger.LogInfo("Executing External Actions for install...");

                bool anyErrors = ExecuteExternalActions(ExecuteActionOn.Intall);
                if (anyErrors)
                {
                    throw new Exception("There were errors executing external actions");
                }
            }
        }


        protected override void RunUninstallAction()
        {
            if (externalActions != null)
            {
                Logger.LogInfo("Executing External Actions for uninstall...");

                bool anyErrors = ExecuteExternalActions(ExecuteActionOn.Uninstall);
                if (anyErrors)
                {
                    throw new Exception("There were errors executing external actions");
                }
            }
        }

        #region Execute External Action Methods

        private bool ExecuteExternalActions(ExecuteActionOn executeActionOn)
        {
            bool anyErrors = false;

            if (externalActions != null)
            {
                foreach (ExternalActionElement externalAction in externalActions)
                {
                    try
                    {
                        //if (externalAction.ExectuteActionOn == executeActionOn)
                        //{
                        string executable = IOUtils.ReplaceStringTokens(externalAction.Executable, Context.Tokens);
                        string executableFile = Path.GetFullPath(executable);
                        if (File.Exists(executableFile))
                        {
                            Logger.LogInfo("  Executing External Action '{0}'", externalAction.Name);

                            LoggingProcess process = new LoggingProcess();
                            string workingDir= IOUtils.ReplaceStringTokens(externalAction.WorkingDirectory, Context.Tokens);
                            string workingDirectory = String.IsNullOrEmpty(workingDir) ? Environment.CurrentDirectory : workingDir;
                            string arguments = IOUtils.ReplaceStringTokens(externalAction.Arguments, Context.Tokens);

                            int returnCode = process.Execute(
                                executableFile,
                                arguments,
                                Path.GetFullPath(workingDirectory),
                                externalAction.CaptureOutput);

                            if (externalAction.ExpectZeroReturnCode && returnCode != 0)
                            {
                                anyErrors = true;
                                Logger.LogError("  Error executing '{0}'.  Recieved return code {1}", externalAction.Name, returnCode);
                            }
                        }
                        else
                        {
                            anyErrors = true;
                            Logger.LogError("  Error executing '{0}', executable'{1}' was not found", externalAction.Name, externalAction.Executable);
                        }
                        //  }
                    }
                    catch (Exception ex)
                    {
                        anyErrors = true;
                        Logger.LogError("  Error executing '{0}'.  Message {1}", externalAction.Name, ex.Message);
                    }
                }
            }
            else
            {
                Logger.LogWarning("  No external actions defined in the configuration file");
            }
            return anyErrors;
        }

        #endregion Execute External Action Methods

    }
}
