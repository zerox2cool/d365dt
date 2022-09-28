using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using ZD365DT.DeploymentTool.Context;

namespace ZD365DT.DeploymentTool.Utils.CrmMetadata
{
    public class CrmEntityCollection : ICollection<CrmEntity>
    {
        private Dictionary<string, CrmEntity> _list = new Dictionary<string, CrmEntity>();

        public static OrganizationContext CurrentOrgContext = null;

        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(CrmEntity item)
        {
            _list.Add(item.Name, item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(CrmEntity item)
        {
            foreach (var o in _list)
            {
                if (o.Key == item.Name)
                    return true;
            }
            return false;
        }

        public void CopyTo(CrmEntity[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<CrmEntity> GetEnumerator()
        {
            return _list.Values.GetEnumerator();
        }

        public bool Remove(CrmEntity item)
        {
            if (Contains(item))
            {
                _list.Remove(item.Name);
                return true;
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list.Values).GetEnumerator();
        }
        
        /// <summary>
        /// Return the list as string summary
        /// </summary>
        /// <returns></returns>
        public string ToOutput()
        {
            StringBuilder sb = new StringBuilder();
            foreach (CrmEntity en in this)
            {
                sb.AppendLine();
                sb.Append(en.ToOutput());
            }
            return sb.ToString();
        }
    }

    public class CrmEntity : CrmMetadataObject
    {
        /// <summary>
        /// Entity Logical Name, always in lower case
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Entity Schema Name, the physical name in SQL Database
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        /// Display Name
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Plural Name
        /// </summary>
        public string DisplayPluralName { get; set; }
        public string Description { get; set; }
        public bool IsSystem { get; set; }
        public int LanguageCode { get; set; }
        public CrmPrimaryField PrimaryField { get; set; }

        public string Ownership { private get; set; }

        public OwnershipTypes EntityOwnership
        {
            get
            {
                if (Ownership.ToLower() == "user")
                    return OwnershipTypes.UserOwned;
                else
                    return OwnershipTypes.OrganizationOwned;
            }
        }

        /// <summary>
        /// Define as an Activity entity
        /// </summary>
        public bool IsActivityEntity { get; set; }
        public int DisplayInActivityMenu { get; set; }

        public string Color { get; set; }

        public bool IsBusinessProcessEnabled { get; set; }

        public bool IsNotesEnabled { get; set; }
        public bool IsConnectionsEnabled { get; set; }
        public bool IsActivitiesEnabled { get; set; }

        public bool IsActivityPartyEnabled { get; set; }
        public bool IsMailMergeEnabled { get; set; }
        public bool IsDocumentManagementEnabled { get; set; }
        public bool IsAccessTeamsEnabled { get; set; }
        public bool IsQueuesEnabled { get; set; }
        public bool AutoRouteToOwnerQueue { get; set; }
        public bool IsKnowledgeManagementEnabled { get; set; }

        public bool IsQuickCreateEnabled { get; set; }
        public bool IsDuplicateDetectionEnabled { get; set; }
        public bool IsAuditEnabled { get; set; }
        public bool IsChangeTrackingEnabled { get; set; }

        public bool IsVisibleInPhoneExpress { get; set; }
        public bool IsVisibleInMobileClient { get; set; }
        public bool ReadOnlyInMobileClient { get; set; }
        public bool IsAvailableOffline { get; set; }

        public bool IsEntityHelpUrlEnabled { get; set; }
        public string EntityHelpUrl { get; set; }

        public bool IsDoNotIncludeSubComponents { get; set; }

        public string SmallIcon { get; set; }
        public string MediumIcon { get; set; }
        public string VectorIcon { get; set; }
        
        public CrmAttributeCollection Attributes { get; set; }

        /// <summary>
        /// Stores the EntityMetadata object to be used for an Update to CRM, this is generated as part of MergeWith, it will only have a value when there is an Update action
        /// </summary>
        public EntityMetadata EntityMetadataToUpdate { get; private set; }
        public UpdateEntityRequest EntityRequestToUpdate { get; private set; }

        /// <summary>
        /// Return the summary of this object
        /// </summary>
        /// <returns></returns>
        public string ToOutput()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Logical Name: {0}; Schema Name: {1}; Display Name: {2}; Plural Name: {3}; Language Code: {4};", Name, SchemaName, DisplayName, DisplayPluralName, LanguageCode);
            sb.AppendLine();
            sb.Append(PrimaryField.ToOutput());
            sb.AppendFormat("Total Attributes: {0}", Attributes.Count);
            sb.AppendLine();
            sb.Append(Attributes.ToOutput());
            return sb.ToString();
        }

        private bool StringMetadataEqual(string a, string b)
        {
            bool result = false;

            if (string.IsNullOrEmpty(a))
                a = string.Empty;
            if (string.IsNullOrEmpty(b))
                b = string.Empty;
            if (a == b)
                result = true;

            return result;
        }

        #region MergeWith
        /// <summary>
        /// Merge the EntityMetadata provided into this defined copy to decide what action to take, if there are any changes that needs to be updated or no action
        /// The UpdateEntityRequest will be created if entity needs to be updated
        /// </summary>
        /// <param name="options"></param>
        public string MergeWith(EntityMetadata ent)
        {
            bool noChange = true;
            LogMessage = string.Empty;
            EntityMetadataToUpdate = new EntityMetadata
            {
                SchemaName = SchemaName, //Schema Name
                LogicalName = Name, //Entity Logical Name
            };

            //build up the metadata as changes are detected
            if (!StringMetadataEqual(ent.DisplayName.UserLocalizedLabel.Label, DisplayName))
            {
                EntityMetadataToUpdate.DisplayName = new Label(DisplayName, LanguageCode);
                LogMergeMessage("DisplayName: {0} to {1}", out noChange, ent.DisplayName.UserLocalizedLabel.Label, DisplayName);
            }
            if (!StringMetadataEqual(ent.DisplayCollectionName.UserLocalizedLabel.Label, DisplayPluralName))
            {
                EntityMetadataToUpdate.DisplayCollectionName = new Label(DisplayPluralName, LanguageCode);
                LogMergeMessage("DisplayPluralName: {0} to {1}", out noChange, ent.DisplayCollectionName.UserLocalizedLabel.Label, DisplayPluralName);
            }
            if (!StringMetadataEqual(ent.Description.UserLocalizedLabel.Label, Description))
            {
                EntityMetadataToUpdate.Description = new Label(Description, LanguageCode);
                LogMergeMessage("Description: {0} to {1}", out noChange, ent.DisplayCollectionName.UserLocalizedLabel.Label, Description);
            }
            if (!StringMetadataEqual(ent.EntityColor, Color))
            {
                EntityMetadataToUpdate.EntityColor = Color;
                LogMergeMessage("Color: {0} to {1}", out noChange, ent.EntityColor, Color);
            }

            //update of icons, only for custom entities
            if (ent.IsCustomEntity.Value == true && !StringMetadataEqual(ent.IconMediumName, MediumIcon))
            {
                EntityMetadataToUpdate.IconMediumName = MediumIcon == null ? string.Empty : MediumIcon;
                LogMergeMessage("MediumIcon: {0} to {1}", out noChange, ent.IconMediumName, MediumIcon);
            }
            if (ent.IsCustomEntity.Value == true && !StringMetadataEqual(ent.IconSmallName, SmallIcon))
            {
                EntityMetadataToUpdate.IconSmallName = SmallIcon == null ? string.Empty : SmallIcon;
                LogMergeMessage("SmallIcon: {0} to {1}", out noChange, ent.IconSmallName, SmallIcon);
            }
            if (ent.IsCustomEntity.Value == true && !StringMetadataEqual(ent.IconVectorName, VectorIcon))
            {
                EntityMetadataToUpdate.IconVectorName = VectorIcon == null ? string.Empty : VectorIcon;
                LogMergeMessage("VectorIcon: {0} to {1}", out noChange, ent.IconVectorName, VectorIcon);
            }

            //update of flags
            if (ent.IsMailMergeEnabled.Value != IsMailMergeEnabled && ent.IsCustomEntity.Value == true)
            {
                EntityMetadataToUpdate.IsMailMergeEnabled = new BooleanManagedProperty(IsMailMergeEnabled);
                LogMergeMessage("IsMailMergeEnabled: {0} to {1}", out noChange, ent.IsMailMergeEnabled.Value, IsMailMergeEnabled);
            }
            if (ent.AutoCreateAccessTeams.Value != IsAccessTeamsEnabled)
            {
                EntityMetadataToUpdate.AutoCreateAccessTeams = IsAccessTeamsEnabled;
                LogMergeMessage("IsAccessTeamsEnabled: {0} to {1}", out noChange, ent.AutoCreateAccessTeams.Value, IsAccessTeamsEnabled);
            }

            if (ent.IsBusinessProcessEnabled.Value != IsBusinessProcessEnabled && ent.IsBusinessProcessEnabled.Value == false)
            {
                EntityMetadataToUpdate.IsBusinessProcessEnabled = IsBusinessProcessEnabled;
                LogMergeMessage("IsBusinessProcessEnabled: {0} to {1}", out noChange, ent.IsBusinessProcessEnabled.Value, IsBusinessProcessEnabled);
            }
            if (ent.IsConnectionsEnabled.Value != IsConnectionsEnabled && ent.IsConnectionsEnabled.Value == false)
            {
                EntityMetadataToUpdate.IsConnectionsEnabled = new BooleanManagedProperty(IsConnectionsEnabled);
                LogMergeMessage("IsConnectionsEnabled: {0} to {1}", out noChange, ent.IsConnectionsEnabled.Value, IsConnectionsEnabled);
            }
            if (ent.IsActivityParty.Value != IsActivityPartyEnabled && ent.IsActivityParty.Value == false)
            {
                EntityMetadataToUpdate.IsActivityParty = IsActivityPartyEnabled;
                LogMergeMessage("IsActivityParty: {0} to {1}", out noChange, ent.IsActivityParty.Value, IsActivityPartyEnabled);
            }
            if (ent.IsDocumentManagementEnabled.Value != IsDocumentManagementEnabled && ent.IsDocumentManagementEnabled.Value == false)
            {
                EntityMetadataToUpdate.IsDocumentManagementEnabled = IsDocumentManagementEnabled;
                LogMergeMessage("IsDocumentManagementEnabled: {0} to {1}", out noChange, ent.IsDocumentManagementEnabled.Value, IsDocumentManagementEnabled);
            }
            if (ent.IsValidForQueue.Value != IsQueuesEnabled && ent.IsValidForQueue.Value == false)
            {
                EntityMetadataToUpdate.IsValidForQueue = new BooleanManagedProperty(IsQueuesEnabled);
                EntityMetadataToUpdate.AutoRouteToOwnerQueue = AutoRouteToOwnerQueue;
                LogMergeMessage("IsQueuesEnabled: {0} to {1}", out noChange, ent.IsValidForQueue.Value, IsQueuesEnabled);
            }

            if (ent.IsQuickCreateEnabled.Value != IsQuickCreateEnabled && ent.IsCustomEntity.Value == true)
            {
                EntityMetadataToUpdate.IsQuickCreateEnabled = IsQuickCreateEnabled;
                LogMergeMessage("IsQuickCreateEnabled: {0} to {1}", out noChange, ent.IsQuickCreateEnabled.Value, IsQuickCreateEnabled);
            }
            if (ent.IsDuplicateDetectionEnabled.Value != IsDuplicateDetectionEnabled)
            {
                EntityMetadataToUpdate.IsDuplicateDetectionEnabled = new BooleanManagedProperty(IsDuplicateDetectionEnabled);
                LogMergeMessage("IsDuplicateDetectionEnabled: {0} to {1}", out noChange, ent.IsDuplicateDetectionEnabled.Value, IsDuplicateDetectionEnabled);
            }
            if (ent.IsAuditEnabled.Value != IsAuditEnabled)
            {
                EntityMetadataToUpdate.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled);
                LogMergeMessage("IsAuditEnabled: {0} to {1}", out noChange, ent.IsAuditEnabled.Value, IsAuditEnabled);
            }
            if (ent.ChangeTrackingEnabled.Value != IsChangeTrackingEnabled && ent.IsCustomEntity.Value == true)
            {
                EntityMetadataToUpdate.ChangeTrackingEnabled = IsChangeTrackingEnabled;
                LogMergeMessage("IsChangeTrackingEnabled: {0} to {1}", out noChange, ent.ChangeTrackingEnabled.Value, IsChangeTrackingEnabled);
            }

            if (ent.IsVisibleInMobile.Value != IsVisibleInPhoneExpress && ent.IsCustomEntity.Value == true)
            {
                EntityMetadataToUpdate.IsVisibleInMobile = new BooleanManagedProperty(IsVisibleInPhoneExpress);
                LogMergeMessage("IsVisibleInPhoneExpress: {0} to {1}", out noChange, ent.IsVisibleInMobile.Value, IsVisibleInPhoneExpress);
            }
            if (ent.IsVisibleInMobileClient.Value != IsVisibleInMobileClient && ent.IsCustomEntity.Value == true)
            {
                EntityMetadataToUpdate.IsVisibleInMobileClient = new BooleanManagedProperty(IsVisibleInMobileClient);
                EntityMetadataToUpdate.IsReadOnlyInMobileClient = new BooleanManagedProperty(ReadOnlyInMobileClient);
                LogMergeMessage("IsVisibleInMobileClient: {0} to {1}", out noChange, ent.IsVisibleInMobileClient.Value, IsVisibleInMobileClient);
            }
            if (ent.IsAvailableOffline.Value != IsAvailableOffline && ent.IsCustomEntity.Value == true)
            {
                EntityMetadataToUpdate.IsAvailableOffline = IsAvailableOffline;
                LogMergeMessage("IsAvailableOffline: {0} to {1}", out noChange, ent.IsAvailableOffline.Value, IsAvailableOffline);
            }

            if (ent.EntityHelpUrlEnabled.Value != IsEntityHelpUrlEnabled)
            {
                EntityMetadataToUpdate.EntityHelpUrlEnabled = IsEntityHelpUrlEnabled;
                EntityMetadataToUpdate.EntityHelpUrl = EntityHelpUrl;
                LogMergeMessage("EntityHelpUrlEnabled: {0} to {1}", out noChange, ent.EntityHelpUrlEnabled.Value, IsEntityHelpUrlEnabled);
            }

            //metadata that can never be DISABLED after they are enabled
            if (ent.IsBusinessProcessEnabled.Value != IsBusinessProcessEnabled && ent.IsBusinessProcessEnabled.Value == true && ent.IsCustomEntity.Value == true)
                LogMergeWarningMessage("IsBusinessProcessEnabled cannot be updated to {0} after it is enabled.", IsBusinessProcessEnabled);
            if (ent.IsConnectionsEnabled.Value != IsConnectionsEnabled && ent.IsConnectionsEnabled.Value == true && ent.IsCustomEntity.Value == true)
                LogMergeWarningMessage("IsConnectionsEnabled cannot be updated to {0} after it is enabled.", IsConnectionsEnabled);
            if (ent.IsActivityParty.Value != IsActivityPartyEnabled && ent.IsActivityParty.Value == true && ent.IsCustomEntity.Value == true)
                LogMergeWarningMessage("IsActivityPartyEnabled cannot be updated to {0} after it is enabled.", IsActivityPartyEnabled);
            if (ent.IsDocumentManagementEnabled.Value != IsDocumentManagementEnabled && ent.IsDocumentManagementEnabled.Value == true && ent.IsCustomEntity.Value == true)
                LogMergeWarningMessage("IsDocumentManagementEnabled cannot be updated to {0} after it is enabled.", IsDocumentManagementEnabled);
            if (ent.IsValidForQueue.Value != IsQueuesEnabled && ent.IsValidForQueue.Value == true && ent.IsCustomEntity.Value == true)
                LogMergeWarningMessage("IsValidForQueue cannot be updated to {0} after it is enabled.", IsQueuesEnabled);

            //metadata that can never be changed after creation
            if (ent.OwnershipType.Value != EntityOwnership)
                LogMergeWarningMessage("Ownership cannot be updated to {0}.", EntityOwnership.ToString());
            if (ent.IsActivity.Value != IsActivityEntity)
                LogMergeWarningMessage("IsActivityEntity cannot be updated to {0}.", IsActivityEntity);
            if (ent.ActivityTypeMask.Value != DisplayInActivityMenu)
                LogMergeWarningMessage("DisplayInActivityMenu cannot be updated to {0}.", DisplayInActivityMenu);
            if (ent.IsKnowledgeManagementEnabled.Value != IsKnowledgeManagementEnabled)
                LogMergeWarningMessage("IsKnowledgeManagementEnabled cannot be updated to {0}.", IsKnowledgeManagementEnabled);
            
            //set the action
            if (noChange)
            {
                EntityMetadataToUpdate = null;
                Action = InstallationAction.NoAction;
                LogMergeMessage("No Action: {0}", out noChange, Name);
            }
            else
            {
                EntityRequestToUpdate = new UpdateEntityRequest()
                {
                    HasActivities = IsActivitiesEnabled,
                    HasNotes = IsNotesEnabled,
                    Entity = EntityMetadataToUpdate,
                };

                Action = InstallationAction.Update;
                LogMergeMessage("Update: {0}", out noChange, Name);
            }

            return LogMessage;
        }

        /// <summary>
        /// To be used for MergeWith method only
        /// </summary>
        /// <param name="message"></param>
        /// <param name="noChange"></param>
        private void LogMergeMessage(string message, out bool noChange, params object[] args)
        {
            noChange = false;
            LogMessage += Environment.NewLine + string.Format(message, args);
        }

        /// <summary>
        /// To be used for MergeWith method only
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        private void LogMergeWarningMessage(string message, params object[] args)
        {
            LogMessage += Environment.NewLine + string.Format("[WARNING] " + message, args);
        }
        #endregion MergeWith

        #region EntityRequestToCreate
        /// <summary>
        /// Generate the CreateEntityRequest
        /// </summary>
        public CreateEntityRequest EntityRequestToCreate
        {
            get
            {
                CrmEntity ent = this;
                CreateEntityRequest req = new CreateEntityRequest
                {
                    HasNotes = ent.IsActivityEntity ? true : ent.IsNotesEnabled, //Notes (includes Attachment)
                    HasActivities = ent.IsActivityEntity ? false : ent.IsActivitiesEnabled, //Activities
                    //define the entity settings
                    Entity = new EntityMetadata
                    {
                        SchemaName = ent.SchemaName, //Schema Name
                        LogicalName = ent.Name, //Entity Logical Name
                        DisplayName = new Label(ent.DisplayName, ent.LanguageCode), //Display Name
                        DisplayCollectionName = new Label(ent.DisplayPluralName, ent.LanguageCode), //Plural Name
                        Description = new Label(ent.Description, ent.LanguageCode), //Description
                        OwnershipType = ent.IsActivityEntity ? OwnershipTypes.UserOwned : ent.EntityOwnership, //Ownership
                        IsActivity = ent.IsActivityEntity, //Define as an Activity Entity, to enable this, Feedback, Notes, Connection, Offline Capability must be true and Activities, Mail Merge, Sending Email, Knowledge Management must be false and Ownership must be User and Primary Field name must be Subject
                        ActivityTypeMask = ent.IsActivityEntity ? ent.DisplayInActivityMenu : 0, //Display in Activity Menus
                        EntityColor = ent.Color, //Color
                                                 //Enable for Interactive Experience only available on new SDK

                        IsBusinessProcessEnabled = ent.IsBusinessProcessEnabled, //Business Process Flows

                        IsConnectionsEnabled = ent.IsActivityEntity ? new BooleanManagedProperty(false) : new BooleanManagedProperty(ent.IsConnectionsEnabled), //Connections
                        IsActivityParty = ent.IsActivityEntity ? false : ent.IsActivityPartyEnabled, //Sending Email (if an email field does not exists, one will be created)
                        IsMailMergeEnabled = ent.IsActivityEntity ? new BooleanManagedProperty(false) : new BooleanManagedProperty(ent.IsMailMergeEnabled), //Mail Merge
                        IsDocumentManagementEnabled = ent.IsDocumentManagementEnabled, //Document Management
                        IsOneNoteIntegrationEnabled = false, //not used, require Document Management enabled
                        AutoCreateAccessTeams = ent.IsAccessTeamsEnabled, //Access Teams
                        IsValidForQueue = new BooleanManagedProperty(ent.IsQueuesEnabled), //Queues
                        AutoRouteToOwnerQueue = ent.IsQueuesEnabled ? ent.AutoRouteToOwnerQueue : false, //Automatically move records to owners default queue when record is created, set only when Queues is true
                        IsKnowledgeManagementEnabled = ent.IsActivityEntity ? false : ent.IsKnowledgeManagementEnabled, //Knowledge Management
                                                                                                                        //Enable for SLA only available on new SDK

                        IsQuickCreateEnabled = ent.IsQuickCreateEnabled, //Allow Quick Create
                        IsDuplicateDetectionEnabled = new BooleanManagedProperty(ent.IsDuplicateDetectionEnabled), //Duplicate Detection
                        IsAuditEnabled = new BooleanManagedProperty(ent.IsAuditEnabled), //Auditing
                        ChangeTrackingEnabled = ent.IsChangeTrackingEnabled, //Change Tracking

                        IsVisibleInMobile = new BooleanManagedProperty(ent.IsVisibleInPhoneExpress), //Enable for Phone Express
                        IsVisibleInMobileClient = new BooleanManagedProperty(ent.IsVisibleInMobileClient), //Enable for Mobile
                        IsReadOnlyInMobileClient = ent.IsVisibleInMobileClient ? new BooleanManagedProperty(ent.ReadOnlyInMobileClient) : new BooleanManagedProperty(false), //Read-only in Mobile, set only Enable for Mobile is true
                        IsAvailableOffline = ent.IsActivityEntity ? true : ent.IsAvailableOffline, //Offline Capability for CRM for Outlook

                        EntityHelpUrlEnabled = ent.IsEntityHelpUrlEnabled, //Use Custom Help
                        EntityHelpUrl = ent.IsEntityHelpUrlEnabled ? ent.EntityHelpUrl : null, //Help URL, only set when Use Custom Help is true
                        IsReadingPaneEnabled = false, //not used

                        IconSmallName = ent.SmallIcon, //16x16 icon for web application
                        IconMediumName = ent.MediumIcon, //32x32 icon for entity form
                    },
                    //define the primary field for the entity
                    PrimaryAttribute = new StringAttributeMetadata
                    {
                        SchemaName = ent.IsActivityEntity ? "Subject" : ent.PrimaryField.SchemaName,
                        LogicalName = ent.IsActivityEntity ? "subject" : ent.PrimaryField.Name,
                        RequiredLevel = new AttributeRequiredLevelManagedProperty(ent.PrimaryField.FieldRequirementLevel),
                        MaxLength = ent.PrimaryField.Length,
                        Format = StringFormat.Text,
                        DisplayName = new Label(ent.PrimaryField.DisplayName, ent.PrimaryField.LanguageCode),
                        Description = new Label(ent.PrimaryField.Description, ent.PrimaryField.LanguageCode),
                        IsValidForAdvancedFind = new BooleanManagedProperty(ent.PrimaryField.IsValidForAdvancedFind), //Searchable
                        IsAuditEnabled = new BooleanManagedProperty(ent.PrimaryField.IsAuditEnabled), //Auditing
                    }
                };

                //setup additional metadata for D365 v9 onwards
                if (CrmEntityCollection.CurrentOrgContext.MajorVersion >= 9)
                {
                    //vector icon for entity
                    req.Entity.IconVectorName = ent.VectorIcon;
                }

                return req;
            }
        }
        #endregion EntityRequestToCreate
    }

    public class CrmPrimaryField : CrmMetadataObject
    {
        /// <summary>
        /// Logical Name, always in lower case
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Schema Name, the physical name in SQL Database
        /// </summary>
        public string SchemaName { get; set; }

        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int Length { get; set; }
        public int LanguageCode { get; set; }

        public string FieldRequirement { get; set; }
        public bool IsValidForAdvancedFind { get; set; }
        public bool IsAuditEnabled { get; set; }

        public AttributeRequiredLevel FieldRequirementLevel
        {
            get
            {
                if (FieldRequirement.ToLower() == "required")
                    return AttributeRequiredLevel.ApplicationRequired;
                else if (FieldRequirement.ToLower() == "recommended")
                    return AttributeRequiredLevel.Recommended;
                else
                    return AttributeRequiredLevel.None;
            }
        }

        /// <summary>
        /// Return the summary of this object
        /// </summary>
        /// <returns></returns>
        public string ToOutput()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Primary Field Name: {0}; Display Name: {1}", Name, DisplayName);
            sb.AppendLine();
            sb.AppendFormat("Length: {0}", Length);
            sb.AppendLine();
            return sb.ToString();
        }
    }
}