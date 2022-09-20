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
using Microsoft.Xrm.Sdk.Metadata;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;
using ZD365DT.DeploymentTool.Utils.CrmMetadata;

namespace ZD365DT.DeploymentTool
{
    public class InstallEntityDefAction : InstallAction
    {
        private readonly EntMetadataCollection entCollection;
        private readonly WebServiceUtils utils;
        private int? _ParentLineNumber = null;
        private bool continueOnError = true;

        public override string ActionName { get { return "Entity Definition"; } }
        public override InstallType ActionType { get { return InstallType.EntityDefAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallEntityDefAction(EntMetadataCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils, int parentLineNumber) : base(context)
        {
            entCollection = collections;
            utils = webServiceUtils;
            continueOnError = collections.ContinueOnError;

            //load the Entities collection from the XML file provided, should upgrade all other config to load from external config XML in future
            if (!string.IsNullOrEmpty(entCollection.MetadataConfig))
            {
                string xmlFilePath = Path.GetFullPath(IOUtils.ReplaceStringTokens(entCollection.MetadataConfig, Context.Tokens));
                Logger.LogInfo("Loading Entity Metadata from the file {0}...", xmlFilePath);

                //install the Entities from the Metadata Config XML, generate a new Metadata Collection object
                EntMetadataCollection ent = new EntMetadataCollection(xmlFilePath);

                //copy any attributes config setup on the DeploymentConfig
                ent.ContinueOnError = entCollection.ContinueOnError;
                ent.SchemaLowerCase = entCollection.SchemaLowerCase;
                ent.SuffixLookupWithId = entCollection.SuffixLookupWithId;
                ent.SuffixOptionSetWithId = entCollection.SuffixOptionSetWithId;

                //replace the current collection
                entCollection = ent;

                _ParentLineNumber = parentLineNumber;
            }
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "EntityDef"); }
        }

        public override int Index
        {
            get
            {
                if (_ParentLineNumber == null)
                    return entCollection.ElementInformation.LineNumber;
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
            return ExecutionReturnCode.EntityDefFailed;
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
            foreach (MetadataElement element in entCollection)
            {
                try
                {
                    InstallEntityDef(element, entCollection);
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
                throw new Exception(string.Format("There was an error executing Entity Definition install. Message: {0}", errorMessage));
            }
        }

        private void InstallEntityDef(MetadataElement element, EntMetadataCollection entCollection)
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
                Logger.LogError("  Entity Definition XML '{0}' was not found.", definitionXmlFilePath);
                throw new Exception("Entity Definition XML file not found.");
            }

            //find the solution
            if (addToSolution)
            {
                Solution sol = SolutionManager.GetSolutionRecord(utils.Service, uniqueSolutionName);
                if (sol == null)
                {
                    Logger.LogError("  Solution '{0}' does not exists, please provide a valid Solution for the entities.", uniqueSolutionName);
                    throw new Exception(string.Format("Solution '{0}' does not exists, please provide a valid Solution for the entities.", uniqueSolutionName));
                }
            }

            //load the XML file and create/update entities, attributes or global option sets
            CrmEntityCollection entities = ConfigXmlHelper.LoadEntityXml(definitionXmlFilePath, prefix, languageCode, entCollection.SchemaLowerCase, entCollection.SuffixLookupWithId, entCollection.SuffixOptionSetWithId);
            if (entities != null)
            {
                //print summary as verbose message
                Logger.LogVerbose(entities.ToOutput());

                #region Create/Update Entities
                //loop all the loaded entities from the XML and create/update the entity
                foreach (CrmEntity ent in entities)
                {
                    //create/update entity in CRM
                    EntityMetadata entityMeta = Metadata.GetEntityMetadataByName(utils, ent.Name);
                    if (entityMeta != null)
                    {
                        //update
                        try
                        {
                            Logger.LogInfo("Entity with the name '{0}' exists, UPDATE it.", ent.Name);
                            //merge entity metadata from CRM into the currently loaded one to decide what action to take, the entity metadata to be used for update will also be generated as part of this process
                            string mergeLogs = ent.MergeWith(entityMeta);
                            Logger.LogVerbose(mergeLogs);
                            if (ent.Action == InstallationAction.NoAction)
                                Logger.LogInfo("NO CHANGES for Entity {0}, not updated.", ent.Name);
                            else
                            {
                                //update entity
                                Metadata.UpdateCrmEntity(utils.Service, ent.EntityRequestToUpdate);
                                Logger.LogInfo("Entity with the ID '{0}' ({1}) UPDATED.", entityMeta.MetadataId.ToString(), ent.Name);

                                //update the updated entity to the cached just in case there is any subsequent entity being declared with the same name, more efficient to add it one by one than complete reload
                                Metadata.UpdateEntityMetadata(utils, ent.Name);
                            }

                            //add entity to solution, just in case it is not in it
                            if (!string.IsNullOrEmpty(uniqueSolutionName))
                                SolutionManager.AddSolutionComponent(utils.Service, uniqueSolutionName, entityMeta.MetadataId.Value, SolutionManager.ComponentType.Entity, false, ent.IsDoNotIncludeSubComponents);

                            //update status to the CrmEntity object
                            ent.MetadataId = entityMeta.MetadataId.Value;
                            ent.Status = InstallationStatus.Installed;
                        }
                        catch (Exception ex)
                        {
                            anyErrors = true;
                            errorMessage = ex.Message;

                            //update status to the CrmEntity object
                            ent.MetadataId = entityMeta.MetadataId.Value;
                            ent.Status = InstallationStatus.Error;
                            ent.Action = InstallationAction.Update;
                            ent.ExceptionMessage = string.Format("{0}; Stack: {1}", ex.Message, ex.StackTrace);

                            Logger.LogError(ent.ExceptionMessage);
                            if (!continueOnError)
                                throw ex;
                        }
                    }
                    else
                    {
                        //create
                        try
                        {
                            Logger.LogInfo("Entity with the name '{0}' does not exists, CREATE it.", ent.Name);
                            Guid entityId = Metadata.CreateCrmEntity(utils.Service, ent);
                            Logger.LogInfo("Entity with the ID '{0}' ({1}) CREATED.", entityId.ToString(), ent.Name);

                            //add entity to solution
                            if (!string.IsNullOrEmpty(uniqueSolutionName))
                                SolutionManager.AddSolutionComponent(utils.Service, uniqueSolutionName, entityId, SolutionManager.ComponentType.Entity, false, ent.IsDoNotIncludeSubComponents);

                            //add the created entity with attributes to the cached just in case there is any subsequent entity being declared with the same name, more efficient to add it one by one than complete reload
                            Metadata.UpdateEntityMetadata(utils, ent.Name, true);

                            //update status to the CrmEntity object
                            ent.MetadataId = entityId;
                            ent.Status = InstallationStatus.Installed;
                            ent.Action = InstallationAction.Create;
                        }
                        catch (Exception ex)
                        {
                            anyErrors = true;
                            errorMessage = ex.Message;

                            //update status to the CrmEntity object
                            ent.Status = InstallationStatus.Error;
                            ent.Action = InstallationAction.Create;
                            ent.ExceptionMessage = string.Format("{0}; Stack: {1}", ex.Message, ex.StackTrace);

                            Logger.LogError(ent.ExceptionMessage);
                            if (!continueOnError)
                                throw ex;
                        }
                    }
                }
                #endregion Create/Update Entities

                #region Create/Update Attributes
                //loop all the loaded entities from the XML and create/update the attributes in the entity
                foreach (CrmEntity ent in entities)
                {
                    //for all entity that has successfully been installed above, check the attributes
                    if (ent.Status == InstallationStatus.Installed)
                    {
                        //create/update when it is defined
                        if (ent.Attributes != null)
                        {
                            bool updateEntityMeta = false;
                            foreach (CrmAttribute atr in ent.Attributes)
                            {
                                //create/update attribute in CRM
                                AttributeMetadata attrMeta = Metadata.GetAttributeMetadataByName(utils, ent.Name, atr.Name);
                                if (attrMeta != null)
                                {
                                    //update
                                    try
                                    {
                                        bool updateMetadataCache = false;
                                        CrmOptionCollection localOptionToUpdate = null;
                                        bool is1toNRelationship = false;
                                        Logger.LogInfo("Attribute with the name '{0}' exists in the entity '{1}', UPDATE it.", atr.Name, ent.Name);
                                        //merge entity metadata from CRM into the currently loaded one to decide what action to take, the entity metadata to be used for update will also be generated as part of this process
                                        string mergeLogs = atr.MergeWith(utils, attrMeta, out is1toNRelationship, out localOptionToUpdate);
                                        Logger.LogVerbose(mergeLogs);
                                        if (atr.Action == InstallationAction.NoAction)
                                        {
                                            if (!string.IsNullOrEmpty(atr.LogMessage) && atr.LogMessage.Contains("[WARNING]"))
                                                Logger.LogWarning(atr.LogMessage);
                                            Logger.LogInfo("NO CHANGES for Attribute {0}, not updated.", atr.Name);
                                        }
                                        else
                                        {
                                            //update attribute
                                            Metadata.UpdateCrmAttribute(utils, attrMeta, ent, atr, is1toNRelationship, localOptionToUpdate, out updateMetadataCache, uniqueSolutionName);
                                            Logger.LogInfo("Attribute with the ID '{0}' ({1}) UPDATED.", attrMeta.MetadataId.ToString(), atr.Name);
                                        }

                                        //update the updated entity to the cached if changes to the attribute requires it
                                        if (updateMetadataCache)
                                            Metadata.UpdateEntityMetadata(utils, ent.Name);

                                        //update status to the CrmAttribute object
                                        atr.MetadataId = attrMeta.MetadataId.Value;
                                        atr.Status = InstallationStatus.Installed;
                                    }
                                    catch (Exception ex)
                                    {
                                        anyErrors = true;
                                        errorMessage = ex.Message;

                                        //update status to the CrmAttribute object
                                        atr.Status = InstallationStatus.Error;
                                        atr.Action = InstallationAction.Update;
                                        atr.ExceptionMessage = string.Format("{0}; Stack: {1}", ex.Message, ex.StackTrace);

                                        Logger.LogError(atr.ExceptionMessage);
                                        if (!continueOnError)
                                            throw ex;
                                    }
                                }
                                else
                                {
                                    //create
                                    try
                                    {
                                        Logger.LogInfo("Attribute with the name '{0}' does not exists, CREATE it.", atr.Name);
                                        Guid attributeId = Metadata.CreateCrmAttribute(utils, ent, atr, uniqueSolutionName);
                                        if (attributeId != Guid.Empty)
                                            Logger.LogInfo("Attribute with the ID '{0}' ({1}) CREATED.", attributeId.ToString(), atr.Name);
                                        else
                                            Logger.LogError("Failed to create Attribute. Unknown error. No exception thrown.");

                                        //flag for cache update
                                        updateEntityMeta = true;

                                        //update status to the CrmAttribute object
                                        atr.MetadataId = attributeId;
                                        atr.Status = InstallationStatus.Installed;
                                        atr.Action = InstallationAction.Create;
                                    }
                                    catch (Exception ex)
                                    {
                                        if (ex.Message.ToLower().Contains("attribute with name") && ex.Message.ToLower().Contains("already exists on entity"))
                                        {
                                            #warning TODO: RESOLVE THIS BUG IN THE FUTURE
                                            //a bug on new D365 where the primary field is not loaded after entity is created, thus the job tries to create it again
                                            //do nothing on such error, log this
                                            Logger.LogInfo("Attribute Error, trying to CREATE '{0}' which already exists, known issue.", atr.Name);

                                            //flag for cache update
                                            updateEntityMeta = true;
                                            atr.Status = InstallationStatus.Installed;
                                            atr.Action = InstallationAction.Create;
                                        }
                                        else if (ex.Message.ToLower().Contains("'createcustomerrelationshipsrequest'") && atr.CrmDataType == CrmAttributeDataType.Customer)
                                        {
                                            #warning TODO: RESOLVE THIS BUG IN THE FUTURE
                                            //a bug on CRM 2016 RTM when executing the CreateCustomerRelationshipsRequest using CRM SDK 8.1 DLL
                                            Logger.LogInfo("Error, trying to CREATE '{0}' as a Customer data type on CRM 2016 RTM using updated SDK DLLs (8.1), known issue.", atr.Name);

                                            //flag for cache update
                                            updateEntityMeta = true;
                                            atr.Status = InstallationStatus.Installed;
                                            atr.Action = InstallationAction.Create;
                                        }
                                        else
                                        {
                                            anyErrors = true;
                                            errorMessage = ex.Message;

                                            //update status to the CrmAttribute object
                                            atr.Status = InstallationStatus.Error;
                                            atr.Action = InstallationAction.Create;
                                            atr.ExceptionMessage = string.Format("{0}; Stack: {1}", ex.Message, ex.StackTrace);

                                            Logger.LogError(atr.ExceptionMessage);
                                            if (!continueOnError)
                                                throw ex;
                                        }
                                    }
                                }
                            }

                            //add the created attribute to the cached just in case there is any subsequent attribute being declared with the same name, just refresh the entity
                            if (updateEntityMeta)
                                Metadata.UpdateEntityMetadata(utils, ent.Name, true);
                        }
                        else
                        {
                            Logger.LogInfo("NO ATTRIBUTES DEFINED in XML to create/update.");
                        }
                    }
                }
                #endregion Create/Update Attributes

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
                throw new Exception(string.Format("Error(s) executing Entity install. Last Error Message: {0}", errorMessage));
            }
        }

        protected override void RunUninstallAction()
        {
            //no uninstall action
            Logger.LogWarning("Uninstall not supported for {0}.", ActionName);
        }
    }
}