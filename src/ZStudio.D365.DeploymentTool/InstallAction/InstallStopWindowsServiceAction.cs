using System;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;

namespace ZD365DT.DeploymentTool
{
    public class InstallStopWindowsServiceAction : InstallAction
    {
        private StopWindowsServiceCollection stopCollection;
        private WebServiceUtils utils;

        public override string ActionName { get { return "Stop Windows Service"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallStopWindowsServiceAction(StopWindowsServiceCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            stopCollection = collections;
            utils = webServiceUtils;
        }


        public override int Index
        {
            get { return stopCollection.ElementInformation.LineNumber; }
        }

        protected override void RunBackupAction()
        {
            //Do nothing for Backup
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo("Executing {0}...", ActionName);
            if (stopCollection != null)
            {
                bool anyError = false;
                string errorString = string.Empty;
                foreach (StopWindowsServiceElement element in stopCollection)
                {

                    if (string.IsNullOrEmpty(element.ServiceName))
                    {
                        Logger.LogError("  ServiceName Name is empty , can not stop service");
                        anyError = true;
                        errorString += Environment.NewLine + string.Format("  ServiceName Name is empty , can not stop service");
                    }
                    else
                    {
                        try
                        {
                            string serviceName = IOUtils.ReplaceStringTokens(element.ServiceName, Context.Tokens);
                            string serverName = IOUtils.ReplaceStringTokens(element.ServerName, Context.Tokens);
                            if (string.IsNullOrEmpty(serverName))
                            {
                                if (WindowsServiceUtils.ServiceInstalled(serviceName))
                                {
                                    // Stop the service
                                    WindowsServiceUtils.StopService(serviceName);
                                    Logger.LogInfo("  Stoped Windows Service {0}", serviceName);
                                }
                                else
                                {
                                    Logger.LogError("  Service is not found");
                                    anyError = true;
                                    errorString += Environment.NewLine + string.Format("  Service is not found", serviceName);
                                }
                            }
                            else
                            {
                                if (WindowsServiceUtils.ServiceInstalled(serviceName, serverName))
                                {
                                    // Stop the service
                                    WindowsServiceUtils.StopService(serviceName, serverName);
                                    Logger.LogInfo("  Stoped Windows Service {0}", serviceName);
                                }
                                else
                                {
                                    Logger.LogError("  Service{0} is not found", serviceName);
                                    anyError = true;
                                    errorString += Environment.NewLine + string.Format("  Service is not found", serviceName);
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            anyError = true;
                            errorString += Environment.NewLine + ex.Message;
                        }
                    }
                }


                if (anyError)
                    throw new Exception(errorString);
            }
        }



        protected override void RunUninstallAction()
        {
            // throw new NotImplementedException();
        }

        public override string GetExecutionErrorMessage()
        {
            return "Failed to Stop Windows Service.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.StopService;
        }
    }
}
