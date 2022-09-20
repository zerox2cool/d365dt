using System;
using System.IO;
using System.Configuration;
//using Microsoft.Crm.SdkTypeProxy;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallReportsAction : InstallAction
    {
        private ReportsCollection reports;
        private WebServiceUtils utils;

        public override string ActionName { get { return "Reports"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallReportsAction(ReportsCollection reportsCollections, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            
                reports = reportsCollections;
                utils = webServiceUtils;
            
        }


        protected override string BackupDirectory 
        {
            get { return Path.Combine(base.BackupDirectory, "Reports"); } 
        }


        public override int Index { get { return reports.ElementInformation.LineNumber; } }


        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return "Failed to install Reports.";
            }
            return "Failed to uninstall Reports.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.ReportsFailed;
        }


        protected override void RunBackupAction()
        {
            Logger.LogInfo("Backing up Reports...");

            if (reports != null)
            {
                if (!Directory.Exists(BackupDirectory))
                {
                    Directory.CreateDirectory(BackupDirectory);
                }
                for (int i = 0; i < reports.Count; i++)
                {
                    Report rep = utils.GetReportFileNamed(reports[i].ReportName, reports[i].FileName);
                    if (rep != null)
                    {
                        string fileName = Path.GetFileName(reports[i].FileName);
                        string destinationFile = Path.Combine(BackupDirectory, fileName);
                        Logger.LogInfo("  Backing up Report {0} to {1}", rep.Name, destinationFile);
                        using (TextWriter writer = File.CreateText(destinationFile))
                        {
                            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                            writer.Write(rep.BodyText);
                        }
                    }
                }
            }
            else
            {
                Logger.LogWarning("  No reports defined in the configuration file");
            }
        }


        protected override void RunInstallAction()
        {
            bool anyErrors = false;
            Logger.LogInfo(" ");
            Logger.LogInfo("Installing {0}...", ActionName);

            // Deploy the Report files
            if (reports != null)
            {
                foreach(ReportElement report in reports)
                {
                    if (File.Exists(Path.GetFullPath(report.FileName)))
                    {
                        utils.PublishReport(report, Context.Tokens);
                    }
                    else
                    {
                        anyErrors = true;
                        Logger.LogError("Error creating report '{0}', source file '{1}' was not found", report.ReportName, report.FileName);
                    }
                }
            }
            else
            {
                Logger.LogWarning("  No reports defined in the configuration file");
            }
            if (anyErrors)
            {
                throw new Exception("There was an error installing Reports");
            }
        }


        protected override void RunUninstallAction()
        {
            Logger.LogInfo("Uninstalling Reports...");

            // Deploy the Report files
            if (reports != null)
            {
                for (int i = 0; i < reports.Count; i++)
                {
                    utils.DeleteReportNamed(reports[i].ReportName, reports[i].FileName);
                }
            }
            else
            {
                Logger.LogWarning("  No reports defined in the configuration file");
            }
        }
    }
}
