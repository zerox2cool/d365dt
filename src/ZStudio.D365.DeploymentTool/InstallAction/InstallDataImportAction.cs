using System;
using System.IO;
using System.Web.Services.Protocols;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallDataImportAction : InstallAction
    {
        private DataImportCollection dataImport;
        private WebServiceUtils utils;

        public override string ActionName { get { return "Data Import"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallDataImportAction(DataImportCollection importCollections, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            dataImport = importCollections;
            utils = webServiceUtils;
        }


        public override int Index { get { return dataImport.ElementInformation.LineNumber; } }


        public override string GetExecutionErrorMessage()
        {
            return "Failed to execute Data Import.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.DataImportFailed;
        }


        protected override void RunBackupAction()
        {
            // Do nothing for backup
        }


        protected override void RunInstallAction()
        {
            bool anyErrors = false;
            Logger.LogInfo(" ");
            Logger.LogInfo("Executing {0}...", ActionName);
            if (dataImport != null)
            {
                foreach (DataImportElement element in dataImport)
                {
                    string filePath = Path.GetFullPath(IOUtils.ReplaceStringTokens(element.FileName, Context.Tokens));
                    
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            string fileContents = IOUtils.ReadFileReplaceTokens(filePath, Context.Tokens);
                            bool success = false;
                            if (element.IsManyToMany)
                            {
                                //Import ManyToMany data
                                success = utils.ImportDataManyToMany(fileContents, element, Context.Tokens);    
                            }
                            else
                            {
                                success = utils.ImportData(fileContents, element, Context.Tokens);    
                            }
                            
                            if (!success)
                            {
                                anyErrors = true;
                                Logger.LogError("Failed to import data from file '{0}' or the import took too long to complete", filePath);
                            }
                        }
                        catch (SoapException soapEx)
                        {
                            anyErrors = true;
                            Logger.LogError("Error importing data from file '{0}'.  Error detail: {1}{2}", filePath, Environment.NewLine, soapEx.Detail.InnerXml);
                        }
                        catch (Exception ex)
                        {
                            anyErrors = true;
                            Logger.LogError("Error importing data from file '{0}'.  Error message: {1}", filePath, ex.Message);
                        }
                    }
                    else
                    {
                        anyErrors = true;
                        Logger.LogError("Data import file '{0}' was not found", filePath);
                    }
                }
            }
            else
            {
                Logger.LogWarning("  No Data Imports in the configuration file");
            }
            if (anyErrors)
            {
                throw new Exception("There was an error executing Data Imports");
            }
        }


        protected override void RunUninstallAction()
        {
            Logger.LogInfo("Data Import actions do not execute during an uninstall.");
        }
    }
}
