using ZD365DT.DeploymentTool.Configuration;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZD365DT.DeploymentTool
{
    public class InstallDataMapAction : InstallAction
    {

        private readonly DataMapCollection dataMapCollection;
        private readonly WebServiceUtils utils;

        public override string ActionName { get { return "Data Map"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallDataMapAction(DataMapCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils)
            : base(context)
        {
            dataMapCollection = collections;
            utils = webServiceUtils;
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "DataMap"); }
        }


        public override int Index { get { return dataMapCollection.ElementInformation.LineNumber; } }

        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return "Failed to Install DataMaps.";
            }
            return "Failed to Uninstall DataMaps.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.DataMapFailed;
        }



        protected override void RunBackupAction()
        {
            Logger.LogInfo("Backing up DataMaps...");
            bool anyError = false;
            foreach (DataMapElement element in dataMapCollection)
            {
                if (!String.IsNullOrEmpty(element.DataMapName) && element.Action == Action.delete)
                {

                    string mapName = IOUtils.ReplaceStringTokens(element.DataMapName, Context.Tokens);
                    string target = IOUtils.ReplaceStringTokens(element.FileName, Context.Tokens);
                    string backup = Path.Combine(BackupDirectory, Path.GetFileName(target));

                    if (!string.IsNullOrEmpty(mapName) && !string.IsNullOrEmpty(target))
                    {
                        Logger.LogInfo("  Backing Data Map {0} to file {1}...", mapName, target);
                        string errorMessage = string.Empty;
                        utils.RetrieveMappingXml(mapName, backup, ref errorMessage);

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            anyError = true;
                            Logger.LogError(errorMessage);

                        }
                    }
                    else
                    {
                        anyError = true;
                        Logger.LogError("  Data Map Name or File Name is empty");
                    }

                }
            }
            if (anyError)
                throw new Exception("There was an error executing Data Maps Backup");
            Logger.LogInfo("Done...");

        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo(" ");
            Logger.LogInfo("Executing {0}...", ActionName);

            bool anyErrors = false;
            foreach (DataMapElement element in dataMapCollection)
            {
                string mapName = IOUtils.ReplaceStringTokens(element.DataMapName, Context.Tokens);
                string target = IOUtils.ReplaceStringTokens(element.FileName, Context.Tokens);

                if (element.Action == Action.export)
                {
                    if (!String.IsNullOrEmpty(element.DataMapName) && !string.IsNullOrEmpty(element.FileName))
                    {
                        Logger.LogInfo("  Exporting Data Map {0} to file {1}", mapName, target);
                        string errorMessage = string.Empty;
                        utils.RetrieveMappingXml(mapName, target, ref errorMessage);

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            anyErrors = true;
                            Logger.LogError(errorMessage);
                        }
                    }
                    else
                    {
                        anyErrors = true;
                        Logger.LogError("  Data Map Name or File Name is empty");
                    }
                }
                else if (element.Action == Action.import)
                {
                    if (!String.IsNullOrEmpty(element.DataMapName) && !string.IsNullOrEmpty(element.FileName))
                    {
                        Logger.LogInfo("  Importing Data Map from file {0}", target);
                        string errorMessage = string.Empty;
                        utils.ImportMappingsByXml(target, ref errorMessage);

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            anyErrors = true;
                            Logger.LogError(errorMessage);
                        }

                    }
                    else
                    {
                        anyErrors = true;
                        Logger.LogError("  Data Map Name or File Name is empty");
                    }
                }
                else if (element.Action == Action.delete)
                {
                    if (!String.IsNullOrEmpty(element.DataMapName))
                    {
                        Logger.LogInfo("  Deleting Data Map ", mapName);
                        string errorMessage = string.Empty;
                        utils.DeleteDataMap(mapName, ref errorMessage);

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            anyErrors = true;
                            Logger.LogError(errorMessage);
                        }

                    }
                    else
                    {
                        anyErrors = true;
                        Logger.LogError("  Data Map Name is empty");
                    }
                }

            }

            if (anyErrors)
            {
                throw new Exception("There was an error executing Data Maps");
            }

            Logger.LogInfo("Done...");
        }

        protected override void RunUninstallAction()
        {

        }
    }
}
