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
    public class InstallManyToManyRelationDefAction : InstallAction
    {
        private readonly ManyToManyRelationMetadataCollection relCollection;
        private readonly WebServiceUtils utils;
        private int? _ParentLineNumber = null;
        private bool continueOnError = true;

        public override string ActionName { get { return "N:N Relationship Definition"; } }
        public override InstallType ActionType { get { return InstallType.ManyToManyRelationDefAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallManyToManyRelationDefAction(ManyToManyRelationMetadataCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils, int parentLineNumber) : base(context)
        {
            relCollection = collections;
            utils = webServiceUtils;
            continueOnError = collections.ContinueOnError;

            //load the Global OptionSet collection from the XML file provided, should upgrade all other config to load from external config XML in future
            if (!string.IsNullOrEmpty(relCollection.MetadataConfig))
            {
                string xmlFilePath = Path.GetFullPath(IOUtils.ReplaceStringTokens(relCollection.MetadataConfig, Context.Tokens));
                Logger.LogInfo("Loading N:N Relations Metadata from the file {0}...", xmlFilePath);

                //install the N:N Relations from the Metadata Config XML, generate a new Metadata Collection object
                ManyToManyRelationMetadataCollection opt = new ManyToManyRelationMetadataCollection(xmlFilePath);
                relCollection = opt;
                _ParentLineNumber = parentLineNumber;
            }
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "ManyToManyRelation"); }
        }

        public override int Index
        {
            get
            {
                if (_ParentLineNumber == null)
                    return relCollection.ElementInformation.LineNumber;
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
            return ExecutionReturnCode.ManyToManyRelationDefFailed;
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
            foreach (MetadataElement element in relCollection)
            {
                try
                {
                    InstallManyToManyRelationDef(element);
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
                throw new Exception(string.Format("There was an error executing N:N Relations Definition install. Message: {0}", errorMessage));
            }
        }

        private void InstallManyToManyRelationDef(MetadataElement element)
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
                Logger.LogError("  N:N Relations Definition XML '{0}' was not found.", definitionXmlFilePath);
                throw new Exception("N:N Relations Definition XML file not found.");
            }

            //find the solution
            if (addToSolution)
            {
                Solution sol = SolutionManager.GetSolutionRecord(utils.Service, uniqueSolutionName);
                if (sol == null)
                {
                    Logger.LogError("  Solution '{0}' does not exists, please provide a valid Solution for the N:N Relations.", uniqueSolutionName);
                    throw new Exception(string.Format("Solution '{0}' does not exists, please provide a valid Solution for the N:N Relations.", uniqueSolutionName));
                }
            }

            //load the XML file and create/update N:N Relations
            CrmManyToManyRelationCollection rels = ConfigXmlHelper.LoadNtoNRelationXml(definitionXmlFilePath, prefix, languageCode);
            if (rels != null)
            {
                //print summary as verbose message
                Logger.LogVerbose(rels.ToOutput());

                #region Create/Update N:N Relations
                foreach (CrmManyToManyRelation rel in rels)
                {
                    //create/update N:N Relations in CRM
                    Guid? relationId = Metadata.GetRelationshipMetadataIdByName(utils, rel.Name);
                    if (relationId != null)
                    {
                        try
                        {
                            //update
                            Logger.LogInfo("N:N relationship with the name '{0}' exists, update it...", rel.Name);
                            Logger.LogWarning("NOT IMPLEMENTED YET...");
                            //Logger.LogInfo("Updated N:N relationship with the ID: '{0}'", relationId.Value.ToString());
                            //if (addToSolution)
                            //    SolutionManager.AddSolutionComponent(utils.Service, uniqueSolutionName, relationId.Value, SolutionManager.ComponentType.Relationship, false);

                            //update status to the object
                            rel.MetadataId = relationId;
                            rel.Status = InstallationStatus.Installed;
                            rel.Action = InstallationAction.Update;
                        }
                        catch (Exception ex)
                        {
                            anyErrors = true;
                            errorMessage = ex.Message;

                            //update status to the object
                            rel.MetadataId = relationId;
                            rel.Status = InstallationStatus.Error;
                            rel.Action = InstallationAction.Update;
                            rel.ExceptionMessage = string.Format("{0}; Stack: {1}", ex.Message, ex.StackTrace);

                            Logger.LogError(rel.ExceptionMessage);
                            if (!continueOnError)
                                throw ex;
                        }
                    }
                    else
                    {
                        try
                        {
                            //create
                            Logger.LogInfo("N:N relationship with the name '{0}' does not exists, create it...", rel.Name);
                            relationId = Metadata.CreateCrmManyToManyRelation(utils, rel, uniqueSolutionName);
                            Logger.LogInfo("Created N:N relationship with the ID: '{0}'", relationId.Value.ToString());
                            
                            //update status to the object
                            rel.MetadataId = relationId;
                            rel.Status = InstallationStatus.Installed;
                            rel.Action = InstallationAction.Create;
                        }
                        catch (Exception ex)
                        {
                            anyErrors = true;
                            errorMessage = ex.Message;

                            //update status to the CrmGlobalOp object
                            rel.Status = InstallationStatus.Error;
                            rel.Action = InstallationAction.Create;
                            rel.ExceptionMessage = string.Format("{0}; Stack: {1}", ex.Message, ex.StackTrace);

                            Logger.LogError(rel.ExceptionMessage);
                            if (!continueOnError)
                                throw ex;
                        }
                    }
                }
                #endregion Create/Update N:N Relations

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
                throw new Exception(string.Format("Error(s) executing N:N Relations install. Last Error Message: {0}", errorMessage));
            }
        }

        protected override void RunUninstallAction()
        {
            //no uninstall action
            Logger.LogWarning("Uninstall not supported for {0}.", ActionName);
        }
    }
}