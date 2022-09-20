using System;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;

namespace ZD365DT.DeploymentTool
{
    public class InstallStartWindowsServiceAction : InstallAction
    {
        private StartWindowsServiceCollection startCollection;
        private WebServiceUtils utils;

        public override string ActionName { get { return "Start Windows Service"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallStartWindowsServiceAction(StartWindowsServiceCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            startCollection = collections;
            utils = webServiceUtils;
        }


        public override int Index
        {
            get { return startCollection.ElementInformation.LineNumber; }
        }

        protected override void RunBackupAction()
        {
            //Do nothing for Backup
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo("Executing {0}...", ActionName);
            if (startCollection != null)
            {
                bool anyError = false;
                string errorString = string.Empty;
                foreach (StartWindowsServiceElement element in startCollection)
                {

                    if (string.IsNullOrEmpty(element.ServiceName))
                    {
                        Logger.LogError("  ServiceName Name is empty , can not start service");
                        anyError = true;
                        errorString += Environment.NewLine + string.Format("  ServiceName Name is empty , can not start service");
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
                                    WindowsServiceUtils.StartService(serviceName);
                                    Logger.LogInfo("  Started Windows Service {0}",serviceName);
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
                                    WindowsServiceUtils.StartService(serviceName, serverName);
                                    Logger.LogInfo("  Started Windows Service {0}", serviceName);
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
            return "Failed to Start Windows Service.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.StartService;
        }
    }
}
