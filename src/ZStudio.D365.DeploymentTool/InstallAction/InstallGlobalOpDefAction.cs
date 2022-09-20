using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Configuration;
using System.Text;
using System.Collections.Specialized;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Utils.CrmMetadata;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallGlobalOpDefAction : InstallAction
    {
        private readonly GlobalOpMetadataCollection optCollection;
        private readonly WebServiceUtils utils;
        private int? _ParentLineNumber = null;
        private bool continueOnError = true;

        public override string ActionName { get { return "Global Optionset Definition"; } }
        public override InstallType ActionType { get { return InstallType.GlobalOpDefAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallGlobalOpDefAction(GlobalOpMetadataCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils, int parentLineNumber) : base(context)
        {
            optCollection = collections;
            utils = webServiceUtils;
            continueOnError = collections.ContinueOnError;

            //load the Global OptionSet collection from the XML file provided, should upgrade all other config to load from external config XML in future
            if (!string.IsNullOrEmpty(optCollection.MetadataConfig))
            {
                string xmlFilePath = Path.GetFullPath(IOUtils.ReplaceStringTokens(optCollection.MetadataConfig, Context.Tokens));
                Logger.LogInfo("Loading Global OptionSet Metadata from the file {0}...", xmlFilePath);

                //install the Global OptionSet from the Metadata Config XML, generate a new Metadata Collection object
                GlobalOpMetadataCollection opt = new GlobalOpMetadataCollection(xmlFilePath);

                //copy any attributes config setup on the DeploymentConfig
                opt.ContinueOnError = optCollection.ContinueOnError;
                opt.SchemaLowerCase = optCollection.SchemaLowerCase;

                //replace the current collection
                optCollection = opt;

                _ParentLineNumber = parentLineNumber;
            }
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "GlobalOp"); }
        }

        public override int Index
        {
            get
            {
                if (_ParentLineNumber == null)
                    return optCollection.ElementInformation.LineNumber;
                else
                    return _ParentLineNumber.Value;
            }
        }

        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return string.Format("Failed to Create/Update the {0}.", ActionName);
            }
            return string.Format("Failed to Create/Update the {0}.", ActionName);
        }

        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.GlobalOpDefFailed;
        }

        protected override void RunBackupAction()
        {
            //nothing to backup
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo("Installing {0} Collection...", ActionName);

            bool anyErrors = false;
            string errorMessage = string.Empty;
            foreach (MetadataElement element in optCollection)
            {
                try
                {
                    InstallGlobalOpDef(element);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    anyErrors = true;

                    if (!continueOnError)
                        throw ex;
                }
            }

            if (anyErrors)
            {
                throw new Exception(string.Format("There was an error executing Global OptionSet Definition install. Message: {0}", errorMessage));
            }
        }

        private void InstallGlobalOpDef(MetadataElement element)
        {
            bool anyErrors = false;
            string errorMessage = string.Empty;

            string uniqueSolutionName = IOUtils.ReplaceStringTokens(element.SolutionName, Context.Tokens);
            bool addToSolution = false;
            bool publish = element.Publish;
            string prefix = IOUtils.ReplaceStringTokens(element.CustomizationPrefix, Context.Tokens);
            string definitionXmlFilePath = Path.GetFullPath(IOUtils.ReplaceStringTokens(element.MetadataFile, Context.Tokens));
            int languageCode = element.LanguageCode;

            if (!string.IsNullOrEmpty(uniqueSolutionName))
                addToSolution = true;

            Logger.LogInfo(" ");
            Logger.LogInfo(" Installing {0}...", ActionName);
            Logger.LogInfo(" Installing from the file {0}...", definitionXmlFilePath);

            //find the xml file
            if (!File.Exists(definitionXmlFilePath))
            {
                Logger.LogError("  Global OptionSet Definition XML '{0}' was not found.", definitionXmlFilePath);
                throw new Exception("Global OptionSet Definition XML file not found.");
            }

            //find the solution
            if (addToSolution)
            {
                Solution sol = SolutionManager.GetSolutionRecord(utils.Service, uniqueSolutionName);
                if (sol == null)
                {
                    Logger.LogError("  Solution '{0}' does not exists, please provide a valid Solution for the global optionset.", uniqueSolutionName);
                    throw new Exception(string.Format("Solution '{0}' does not exists, please provide a valid Solution for the global optionset.", uniqueSolutionName));
                }
                else
                {
                    if (sol.SolutionId != null)
                        Logger.LogInfo("  Found Solution '{0}' with the ID: {1}.", uniqueSolutionName, sol.SolutionId.Value);
                    else
                        Logger.LogInfo("  Found Solution '{0}'.", uniqueSolutionName);
                }
            }

            //load the XML file and create/update global optionset
            CrmGlobalOpCollection globalOps = ConfigXmlHelper.LoadGlobalOpXml(definitionXmlFilePath, prefix, languageCode);
            if (globalOps != null)
            {
                //print summary as verbose message
                Logger.LogVerbose(globalOps.ToOutput());

                #region Create/Update OptionSet
                foreach (CrmGlobalOp gop in globalOps)
                {
                    //create/update global option set in CRM
                    Guid? globalOpId = Metadata.GetGlobalOptionSetIdByName(utils, gop.Name, true);
                    if (globalOpId != null)
                    {
                        try
                        {
                            //update
                            Logger.LogInfo("Global Optionset with the name '{0}' exists, UPDATE it.", gop.Name);
                            Metadata.UpdateGlobalOptionSet(utils, globalOpId.Value, gop, uniqueSolutionName);
                            Logger.LogInfo("Global Optionset with the ID '{0}' ({1}) UPDATED.", globalOpId.Value.ToString(), gop.Name);

                            //add to the solution just in case it is not already in it, only required when no action happened in the Update function above
                            if (gop.Action == InstallationAction.NoAction && addToSolution)
                                SolutionManager.AddSolutionComponent(utils.Service, uniqueSolutionName, globalOpId.Value, SolutionManager.ComponentType.OptionSet, false);

                            //update status to the CrmGlobalOp object
                            gop.MetadataId = globalOpId;
                            gop.Status = InstallationStatus.Installed;
                            gop.Action = InstallationAction.Update;
                        }
                        catch (Exception ex)
                        {
                            anyErrors = true;
                            errorMessage = ex.Message;

                            //update status to the CrmGlobalOp object
                            gop.MetadataId = globalOpId;
                            gop.Status = InstallationStatus.Error;
                            gop.Action = InstallationAction.Update;
                            gop.ExceptionMessage = string.Format("{0}; Stack: {1}", ex.Message, ex.StackTrace);

                            Logger.LogError(gop.ExceptionMessage);
                            if (!continueOnError)
                                throw ex;
                        }
                    }
                    else
                    {
                        try
                        {
                            //create
                            Logger.LogInfo("Global Optionset with the name '{0}' does not exists, CREATE it.", gop.Name);
                            globalOpId = Metadata.CreateGlobalOptionSet(utils.Service, gop, uniqueSolutionName);
                            Logger.LogInfo("Global Optionset with the ID '{0}' ({1}) CREATED.", globalOpId.Value.ToString(), gop.Name);

                            //RESOLVED, this issue is due to CRM CACHING, IISRESET WILL SOLVE THE PROBLEM
                            //add to solution externally instead of during creation to solve a bug where it will always default to a non-existance solution with ID 1f24d7d4-db70-e611-80d8-000d3aa137c3
                            //if (addToSolution)
                            //    SolutionManager.AddSolutionComponent(utils.Service, uniqueSolutionName, globalOpId.Value, SolutionManager.ComponentType.OptionSet, false);

                            //add the created optionset to the cached just in case there is any subsequent optionset being declared with the same name, more efficient to add it one by one than complete reload
                            Metadata.UpdateOptionSetMetadata(utils, globalOpId.Value);

                            //update status to the CrmGlobalOp object
                            gop.MetadataId = globalOpId;
                            gop.Status = InstallationStatus.Installed;
                            gop.Action = InstallationAction.Create;
                        }
                        catch (Exception ex)
                        {
                            anyErrors = true;
                            errorMessage = ex.Message;

                            //update status to the CrmGlobalOp object
                            gop.Status = InstallationStatus.Error;
                            gop.Action = InstallationAction.Create;
                            gop.ExceptionMessage = string.Format("{0}; Stack: {1}", ex.Message, ex.StackTrace);

                            Logger.LogError(gop.ExceptionMessage);
                            if (!continueOnError)
                                throw ex;
                        }
                    }
                }
                #endregion Create/Update OptionSet

                #region Publish
                if (publish)
                {
                    Logger.LogInfo("Publishing customizations...");
                    SolutionManager.PublishAllXmlRequest(utils.Service);
                    Logger.LogInfo("Published customizations...");
                }
                #endregion Publish
            }

            if (anyErrors)
            {
                throw new Exception(string.Format("Error(s) executing Global OptionSet install. Last Error Message: {0}", errorMessage));
            }
        }

        protected override void RunUninstallAction()
        {
            //no uninstall action
            Logger.LogWarning("Uninstall not supported for {0}.", ActionName);
        }
    }
}