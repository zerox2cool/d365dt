using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk;

namespace ZD365DT.DeploymentTool.Utils.CrmMetadata
{
    public enum CrmAttributeDataType
    {
        SingleText = 5, //single line of text
        MultiText = 10, //multiple lines of text
        Int32 = 15, //whole number
        Decimal = 20, //decimal
        Float = 25, //floating point number
        Currency = 30, //currency
        TwoOptions = 35, //two options
        OptionSet = 40, //optionset
        MultiOptionSet = 41, //multi optionset
        DateTime = 45, //datetime
        Image = 50, //image
        Customer = 55, //customer
        Lookup = 60, //lookup
    }

    public enum CrmAttributeFieldType
    {
        Simple = 0,
        Calculated = 1,
        Rollup = 2,
    }

    public enum CrmAttributeFormatType
    {
        //whole number
        None = 0,
        Duration = 1,
        TimeZone = 2,
        Language = 3,

        //for single line of text
        Email = 10,
        Text = 11,
        TextArea = 12,
        Url = 13,
        TickerSymbol = 14,
        Phone = 15,

        //date time
        DateTime = 20,
        DateOnly = 21,
    }

    public enum CrmPrecisionSource
    {
        Custom = 0,
        Pricing = 1,
        Currency = 2,
    }

    public enum CrmDateTimeBehavior
    {
        UserLocal = 1,
        DateOnly = 2,
        TimeZoneIndependent = 3,
    }

    public enum CrmCascadeType
    {
        CascadeNone = 0,
        CascadeAll = 1,
        CascadeActive = 2,
        CascadeUserOwned = 3,
        RemoveLink = 4, //for delete
        Restrict = 5, //for delete
    }

    public enum CrmAssociatedMenuBehavior
    {
        UseCollectionName = 0,
        UseLabel = 1,
        DoNotDisplay = 2,
    }

    public enum CrmAssociatedMenuGroup
    {
        Details = 0,
        Sales = 1,
        Service = 1,
        Marketing = 1,
    }

    public class CrmAttributeCollection : ICollection<CrmAttribute>
    {
        private Dictionary<string, CrmAttribute> _list = new Dictionary<string, CrmAttribute>();

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

        public void Add(CrmAttribute item)
        {
            _list.Add(item.Name, item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(CrmAttribute item)
        {
            foreach (var o in _list)
            {
                if (o.Key == item.Name)
                    return true;
            }
            return false;
        }

        public void CopyTo(CrmAttribute[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<CrmAttribute> GetEnumerator()
        {
            return _list.Values.GetEnumerator();
        }

        public bool Remove(CrmAttribute item)
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
            foreach (CrmAttribute atr in this)
            {
                sb.Append(atr.ToOutput());
            }
            return sb.ToString();
        }
    }

    public class CrmAttribute : CrmMetadataObject
    {
        /// <summary>
        /// Attribute Logical Name, always in lower case
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Schema Name, the physical name in SQL Database
        /// </summary>
        public string SchemaName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsSystem { get; set; }

        /// <summary>
        /// Indicate that this attribute is a primary field of the entity.
        /// </summary>
        public bool IsPrimaryField { get; set; } = false;

        public int LanguageCode { get; set; }

        /// <summary>
        /// Attribute Field Requirement, can only be set to Required, Recommended or Optional. The value will be translated to <see cref="FieldRequirementLevel"/> property to be used by CRM.
        /// </summary>
        public string FieldRequirement { private get; set; }

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

        public bool IsValidForAdvancedFind { get; set; }
        public bool IsAuditEnabled { get; set; }
        public bool IsSecured { get; set; }

        public string DataType { private get; set; }
        public CrmAttributeDataType CrmDataType
        {
            get
            {
                CrmAttributeDataType result = CrmAttributeDataType.SingleText;
                if (Enum.TryParse<CrmAttributeDataType>(DataType, out result))
                    return result;
                else
                    throw new FormatException(string.Format("The data type '{0}' is invalid.", DataType));
            }
        }

        public string FieldType { private get; set; }
        public CrmAttributeFieldType CrmFieldType
        {
            get
            {
                //by default return simple
                CrmAttributeFieldType result = CrmAttributeFieldType.Simple;
                if (Enum.TryParse<CrmAttributeFieldType>(FieldType, out result))
                    return result;
                else
                    return CrmAttributeFieldType.Simple;
            }
        }

        public string FormatType { private get; set; }
        public CrmAttributeFormatType CrmFormatType
        {
            get
            {
                CrmAttributeFormatType result = CrmAttributeFormatType.Text;
                if (Enum.TryParse<CrmAttributeFormatType>(FormatType, out result))
                    return result;
                else
                {
                    //default to text for single text, default to none for whole number
                    if (CrmDataType == CrmAttributeDataType.SingleText)
                        return CrmAttributeFormatType.Text;
                    if (CrmDataType == CrmAttributeDataType.Int32)
                        return CrmAttributeFormatType.None;
                }

                throw new FormatException(string.Format("The format type '{0}' is invalid.", FormatType));
            }
        }

        public string PrecisionSource { private get; set; }
        public CrmPrecisionSource CrmPrecisionSource
        {
            get
            {
                //by default return currency
                CrmPrecisionSource result = CrmPrecisionSource.Currency;
                if (Enum.TryParse<CrmPrecisionSource>(PrecisionSource, out result))
                    return result;
                else
                    return CrmPrecisionSource.Currency;
            }
        }

        public int Precision { get; set; }

        public string DateTimeBehaviorValue { private get; set; }
        public CrmDateTimeBehavior CrmDateTimeBehavior
        {
            get
            {
                //by default return user local
                CrmDateTimeBehavior result = CrmDateTimeBehavior.UserLocal;
                if (Enum.TryParse<CrmDateTimeBehavior>(DateTimeBehaviorValue, out result))
                    return result;
                else
                    return CrmDateTimeBehavior.UserLocal;
            }
        }

        public bool TwoOptionsDefault { get; set; }
        public string TrueLabel { get; set; }
        public string TrueColor { get; set; }
        public string FalseLabel { get; set; }
        public string FalseColor { get; set; }

        public int? OptionSetDefault { get; set; }
        public string GlobalOptionSetName { get; set; }
        public bool IsSystemGlobalOptionSet { get; set; }
        public bool IsGlobalOptionSet
        {
            get
            {
                if (!string.IsNullOrEmpty(GlobalOptionSetName))
                    return true;
                else
                    return false;
            }
        }
        public CrmOptionCollection Options { get; set; }

        public string LookupRefEntityName { get; set; }
        public string LookupRefEntityPrimaryKey
        {
            get
            {
                return LookupRefEntityName + "id";
            }
        }
        public bool IsSystemRefEntity { get; set; }
        public string LookupRelationshipName { get; set; }
        public int LookupMenuOrder { get; set; }


        public string LookupMenuBehavior { private get; set; }
        public CrmAssociatedMenuBehavior CrmLookupMenuBehavior
        {
            get
            {
                //by default return use collection name
                CrmAssociatedMenuBehavior result = CrmAssociatedMenuBehavior.UseCollectionName;
                if (Enum.TryParse<CrmAssociatedMenuBehavior>(LookupMenuBehavior, out result))
                    return result;
                else
                    return CrmAssociatedMenuBehavior.UseCollectionName;
            }
        }
        public string LookupMenuCustomLabel { get; set; }

        public string LookupMenuGroup { private get; set; }
        public CrmAssociatedMenuGroup CrmLookupMenuGroup
        {
            get
            {
                //by default return details
                CrmAssociatedMenuGroup result = CrmAssociatedMenuGroup.Details;
                if (Enum.TryParse<CrmAssociatedMenuGroup>(LookupMenuGroup, out result))
                    return result;
                else
                    return CrmAssociatedMenuGroup.Details;
            }
        }

        public string LookupRelationshipBehaviorAssign { private get; set; }
        public CrmCascadeType CrmLookupRelationshipBehaviorAssign
        {
            get
            {
                //by default return cascade none (if the XML leave it as blank to use the default, it will be a referential relationship)
                CrmCascadeType result = CrmCascadeType.CascadeNone;
                if (Enum.TryParse<CrmCascadeType>(LookupRelationshipBehaviorAssign, out result))
                    return result;
                else
                    return CrmCascadeType.CascadeNone;
            }
        }
        public string LookupRelationshipBehaviorShare { private get; set; }
        public CrmCascadeType CrmLookupRelationshipBehaviorShare
        {
            get
            {
                //by default return cascade none (if the XML leave it as blank to use the default, it will be a referential relationship)
                CrmCascadeType result = CrmCascadeType.CascadeNone;
                if (Enum.TryParse<CrmCascadeType>(LookupRelationshipBehaviorShare, out result))
                    return result;
                else
                    return CrmCascadeType.CascadeNone;
            }
        }
        public string LookupRelationshipBehaviorUnshare { private get; set; }
        public CrmCascadeType CrmLookupRelationshipBehaviorUnshare
        {
            get
            {
                //by default return cascade none (if the XML leave it as blank to use the default, it will be a referential relationship)
                CrmCascadeType result = CrmCascadeType.CascadeNone;
                if (Enum.TryParse<CrmCascadeType>(LookupRelationshipBehaviorUnshare, out result))
                    return result;
                else
                    return CrmCascadeType.CascadeNone;
            }
        }
        public string LookupRelationshipBehaviorReparent { private get; set; }
        public CrmCascadeType CrmLookupRelationshipBehaviorReparent
        {
            get
            {
                //by default return cascade none (if the XML leave it as blank to use the default, it will be a referential relationship)
                CrmCascadeType result = CrmCascadeType.CascadeNone;
                if (Enum.TryParse<CrmCascadeType>(LookupRelationshipBehaviorReparent, out result))
                    return result;
                else
                    return CrmCascadeType.CascadeNone;
            }
        }
        public string LookupRelationshipBehaviorDelete { private get; set; }
        public CrmCascadeType CrmLookupRelationshipBehaviorDelete
        {
            get
            {
                //by default return remove link (if the XML leave it as blank to use the default, it will be a referential relationship)
                CrmCascadeType result = CrmCascadeType.RemoveLink;
                if (Enum.TryParse<CrmCascadeType>(LookupRelationshipBehaviorDelete, out result))
                    return result;
                else
                    return CrmCascadeType.RemoveLink;
            }
        }

        public CrmCascadeType CrmLookupRelationshipBehaviorMerge
        {
            get
            {
                //by default return cascade none (if the XML leave it as blank to use the default, it will be a referential relationship)
                return CrmCascadeType.CascadeNone;
            }
        }

        public string LookupRelationshipBehaviorRollupView { private get; set; }
        public CrmCascadeType CrmLookupRelationshipBehaviorRollupView
        {
            get
            {
                //by default return cascade none (if the XML leave it as blank to use the default)
                CrmCascadeType result = CrmCascadeType.CascadeNone;
                if (Enum.TryParse<CrmCascadeType>(LookupRelationshipBehaviorRollupView, out result))
                    return result;
                else
                    return CrmCascadeType.CascadeNone;
            }
        }

        public string CustomerAccountRelationshipName { get; set; }
        public string CustomerContactRelationshipName { get; set; }

        public string MinValue { internal get; set; }
        public string MaxValue { internal get; set; }

        public int MaxLength { get; set; }

        public decimal MinValueDecimal
        {
            get
            {
                return Convert.ToDecimal(MinValue);
            }
        }
        public decimal MaxValueDecimal
        {
            get
            {
                return Convert.ToDecimal(MaxValue);
            }
        }

        public int MinValueInt32
        {
            get
            {
                return Convert.ToInt32(MinValue);
            }
        }
        public int MaxValueInt32
        {
            get
            {
                return Convert.ToInt32(MaxValue);
            }
        }

        public double MinValueDouble
        {
            get
            {
                return Convert.ToDouble(MinValue);
            }
        }
        public double MaxValueDouble
        {
            get
            {
                return Convert.ToDouble(MaxValue);
            }
        }

        public string ImeMode { get; set; }
        public ImeMode ImeModeLevel
        {
            get
            {
                if (!string.IsNullOrEmpty(ImeMode))
                {
                    if (ImeMode.ToLower() == "auto")
                        return Microsoft.Xrm.Sdk.Metadata.ImeMode.Auto;
                    else if (ImeMode.ToLower() == "active")
                        return Microsoft.Xrm.Sdk.Metadata.ImeMode.Active;
                    else if (ImeMode.ToLower() == "inactive")
                        return Microsoft.Xrm.Sdk.Metadata.ImeMode.Inactive;
                    else
                        return Microsoft.Xrm.Sdk.Metadata.ImeMode.Disabled;
                }
                else
                    return Microsoft.Xrm.Sdk.Metadata.ImeMode.Auto;
            }
        }

        /// <summary>
        /// Stores the AttributeMetadata object to be used for an Update to CRM, this is generated as part of MergeWith, it will only have a value when there is an Update action
        /// </summary>
        public AttributeMetadata AttributeMetadataToUpdate { get; private set; }

        /// <summary>
        /// Return the summary of this object
        /// </summary>
        /// <returns></returns>
        public string ToOutput()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Name: {0}; Schema Name: {1}; Display Name: {2}; Data Type: {3};", Name, SchemaName, DisplayName, DataType);
            sb.AppendLine();
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
        /// Merge the AttributeMetadata provided into this defined copy to decide what action to take, if there are any changes that needs to be updated or no action
        /// </summary>
        /// <param name="atr"></param>
        /// <returns></returns>
        public string MergeWith(WebServiceUtils util, AttributeMetadata atr, out bool is1toNRelationship, out CrmOptionCollection localOptToUpdate)
        {
            localOptToUpdate = null;
            is1toNRelationship = false;
            bool noChange = true;
            LogMessage = string.Empty;

            AttributeMetadata attrMeta = null;
            switch (atr.AttributeType)
            {
                case AttributeTypeCode.String:
                    if (CrmDataType == CrmAttributeDataType.SingleText)
                    {
                        attrMeta = UpdateStringAttribute((StringAttributeMetadata)atr, out noChange);
                    }
                    else
                        throw new InvalidOperationException(string.Format("Attribute data type cannot be changed after it has been created. CRM Data Type: {0}", atr.AttributeType));
                    break;

                case AttributeTypeCode.Memo:
                    if (CrmDataType == CrmAttributeDataType.MultiText)
                    {
                        attrMeta = UpdateMemoAttribute((MemoAttributeMetadata)atr, out noChange);
                    }
                    else
                        throw new InvalidOperationException(string.Format("Attribute data type cannot be changed after it has been created. CRM Data Type: {0}", atr.AttributeType));
                    break;

                case AttributeTypeCode.Integer:
                    if (CrmDataType == CrmAttributeDataType.Int32)
                    {
                        attrMeta = UpdateIntegerAttribute((IntegerAttributeMetadata)atr, out noChange);
                    }
                    else
                        throw new InvalidOperationException(string.Format("Attribute data type cannot be changed after it has been created. CRM Data Type: {0}", atr.AttributeType));
                    break;

                case AttributeTypeCode.Decimal:
                    if (CrmDataType == CrmAttributeDataType.Decimal)
                    {
                        attrMeta = UpdateDecimalAttribute((DecimalAttributeMetadata)atr, out noChange);
                    }
                    else
                        throw new InvalidOperationException(string.Format("Attribute data type cannot be changed after it has been created. CRM Data Type: {0}", atr.AttributeType));
                    break;

                case AttributeTypeCode.Double:
                    if (CrmDataType == CrmAttributeDataType.Float)
                    {
                        attrMeta = UpdateDoubleAttribute((DoubleAttributeMetadata)atr, out noChange);
                    }
                    else
                        throw new InvalidOperationException(string.Format("Attribute data type cannot be changed after it has been created. CRM Data Type: {0}", atr.AttributeType));
                    break;

                case AttributeTypeCode.Money:
                    if (CrmDataType == CrmAttributeDataType.Currency)
                    {
                        attrMeta = UpdateMoneyAttribute((MoneyAttributeMetadata)atr, out noChange);
                    }
                    else
                        throw new InvalidOperationException(string.Format("Attribute data type cannot be changed after it has been created. CRM Data Type: {0}", atr.AttributeType));
                    break;

                case AttributeTypeCode.Boolean:
                    if (CrmDataType == CrmAttributeDataType.TwoOptions)
                    {
                        attrMeta = UpdateBooleanAttribute((BooleanAttributeMetadata)atr, out noChange);
                    }
                    else
                        throw new InvalidOperationException(string.Format("Attribute data type cannot be changed after it has been created. CRM Data Type: {0}", atr.AttributeType));
                    break;

                case AttributeTypeCode.DateTime:
                    if (CrmDataType == CrmAttributeDataType.DateTime)
                    {
                        attrMeta = UpdateDateTimeAttribute((DateTimeAttributeMetadata)atr, out noChange);
                    }
                    else
                        throw new InvalidOperationException(string.Format("Attribute data type cannot be changed after it has been created. CRM Data Type: {0}", atr.AttributeType));
                    break;

                case AttributeTypeCode.Picklist:
                    if (CrmDataType == CrmAttributeDataType.OptionSet)
                    {
                        attrMeta = UpdateOptionSetAttribute((PicklistAttributeMetadata)atr, out noChange, out localOptToUpdate);
                    }
                    else
                        throw new InvalidOperationException(string.Format("Attribute data type cannot be changed after it has been created. CRM Data Type: {0}", atr.AttributeType));
                    break;
                    
                case AttributeTypeCode.Customer:
                    if (CrmDataType == CrmAttributeDataType.Customer)
                    {
                        attrMeta = UpdateCustomerAttribute(atr, out noChange);
                        is1toNRelationship = true;
                    }
                    else
                        throw new InvalidOperationException(string.Format("Attribute data type cannot be changed after it has been created. CRM Data Type: {0}", atr.AttributeType));
                    break;

                case AttributeTypeCode.Lookup:
                    if (CrmDataType == CrmAttributeDataType.Lookup)
                    {
                        attrMeta = UpdateLookupAttribute(util, (LookupAttributeMetadata)atr, out noChange);
                        is1toNRelationship = true;
                    }
                    else
                        throw new InvalidOperationException(string.Format("Attribute data type cannot be changed after it has been created. CRM Data Type: {0}", atr.AttributeType));
                    break;

                case AttributeTypeCode.Virtual:
                    //multi select option set is a virtual
                    if (CrmDataType == CrmAttributeDataType.MultiOptionSet)
                    {
                        attrMeta = UpdateMultiOptionSetAttribute((MultiSelectPicklistAttributeMetadata)atr, out noChange, out localOptToUpdate);
                    }
                    else if (CrmDataType == CrmAttributeDataType.Image)
                    {
                        //for image just log a warning
                        LogMergeWarningMessage("Attribute data type cannot be changed after it has been created. CRM Data Type: {0}.", atr.AttributeType);
                    }
                    else
                        throw new InvalidOperationException(string.Format("Attribute data type cannot be changed after it has been created. CRM Data Type: {0}", atr.AttributeType));
                    break;

                default:
                    LogMergeWarningMessage("NOT IMPLEMENTED YET FOR THIS DATA TYPE {0}.", atr.AttributeType);
                    break;
            }

            //set the action
            if (noChange)
            {
                AttributeMetadataToUpdate = null;
                Action = InstallationAction.NoAction;
                LogMergeMessage("No Action: {0}", out noChange, Name);
            }
            else
            {
                AttributeMetadataToUpdate = attrMeta;
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

        #region UpdateAttribute
        /// <summary>
        /// Update Single Line of Text attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private StringAttributeMetadata UpdateStringAttribute(StringAttributeMetadata atr, out bool noChange)
        {
            noChange = true;
            StringAttributeMetadata output = new StringAttributeMetadata
            {
                LogicalName = Name, //Name
                SchemaName = SchemaName, //Schema Name
            };

            //set the format type for SingleText based on the Format Type in the XML using a case, StringFormatName is not an enum
            StringFormatName dataFormatName = StringFormatName.Text;
            switch (CrmFormatType)
            {
                case CrmAttributeFormatType.Email:
                    dataFormatName = StringFormatName.Email;
                    break;

                case CrmAttributeFormatType.Text:
                    dataFormatName = StringFormatName.Text;
                    break;

                case CrmAttributeFormatType.TextArea:
                    dataFormatName = StringFormatName.TextArea;
                    break;

                case CrmAttributeFormatType.Url:
                    dataFormatName = StringFormatName.Url;
                    break;

                case CrmAttributeFormatType.TickerSymbol:
                    dataFormatName = StringFormatName.TickerSymbol;
                    break;

                case CrmAttributeFormatType.Phone:
                    dataFormatName = StringFormatName.Phone;
                    break;
            }

            //build up the metadata as changes are detected
            if (atr.RequiredLevel.Value != FieldRequirementLevel)
            {
                if (atr.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired)
                    LogMergeWarningMessage("FieldRequirementLevel is {0} and cannot be updated to {1}. IsPrimaryField: {2}", atr.RequiredLevel.Value, FieldRequirementLevel, IsPrimaryField);
                else
                {
                    output.RequiredLevel = new AttributeRequiredLevelManagedProperty(FieldRequirementLevel);
                    LogMergeMessage("FieldRequirementLevel: {0} to {1}", out noChange, atr.RequiredLevel.Value, FieldRequirementLevel);
                }
            }
            if (!StringMetadataEqual(atr.DisplayName.UserLocalizedLabel.Label, DisplayName))
            {
                output.DisplayName = new Label(DisplayName, LanguageCode);
                LogMergeMessage("DisplayName: {0} to {1}", out noChange, atr.DisplayName.UserLocalizedLabel.Label, DisplayName);
            }
            if (!StringMetadataEqual(atr.Description.UserLocalizedLabel.Label, Description))
            {
                output.Description = new Label(Description, LanguageCode);
                LogMergeMessage("Description: {0} to {1}", out noChange, atr.Description.UserLocalizedLabel.Label, Description);
            }
            if (atr.IsValidForAdvancedFind.Value != IsValidForAdvancedFind)
            {
                output.IsValidForAdvancedFind = new BooleanManagedProperty(IsValidForAdvancedFind);
                LogMergeMessage("IsValidForAdvancedFind: {0} to {1}", out noChange, atr.IsValidForAdvancedFind.Value, IsValidForAdvancedFind);
            }
            if (atr.IsAuditEnabled.Value != IsAuditEnabled)
            {
                output.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled);
                LogMergeMessage("IsAuditEnabled: {0} to {1}", out noChange, atr.IsAuditEnabled.Value, IsAuditEnabled);
            }
            if (atr.IsSecured.Value != IsSecured)
            {
                output.IsSecured = IsSecured;
                LogMergeMessage("IsSecured: {0} to {1}", out noChange, atr.IsSecured.Value, IsSecured);
            }
            if (atr.MaxLength.Value != MaxLength)
            {
                output.MaxLength = MaxLength;
                LogMergeMessage("MaxLength: {0} to {1}", out noChange, atr.MaxLength.Value, MaxLength);
            }
            if (atr.FormatName.Value != dataFormatName)
            {
                output.FormatName = dataFormatName;
                LogMergeMessage("FormatName: {0} to {1}", out noChange, atr.FormatName.Value, dataFormatName);
            }
            if (atr.ImeMode.Value != ImeModeLevel)
            {
                output.ImeMode = ImeModeLevel;
                LogMergeMessage("ImeMode: {0} to {1}", out noChange, atr.ImeMode.Value, ImeModeLevel);
            }

            //metadata that can never be changed after creation
            if (atr.SourceType.Value != (int)CrmFieldType)
                LogMergeWarningMessage("CrmFieldType cannot be updated to {0}.", CrmFieldType);

            return output;
        }

        /// <summary>
        /// Update Multiple Lines of Text attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private MemoAttributeMetadata UpdateMemoAttribute(MemoAttributeMetadata atr, out bool noChange)
        {
            noChange = true;
            MemoAttributeMetadata output = new MemoAttributeMetadata
            {
                LogicalName = Name, //Name
                SchemaName = SchemaName, //Schema Name
            };

            //build up the metadata as changes are detected
            if (atr.RequiredLevel.Value != FieldRequirementLevel)
            {
                if (atr.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired)
                    LogMergeWarningMessage("FieldRequirementLevel is {0} and cannot be updated to {1}. IsPrimaryField: {2}", atr.RequiredLevel.Value, FieldRequirementLevel, IsPrimaryField);
                else
                {
                    output.RequiredLevel = new AttributeRequiredLevelManagedProperty(FieldRequirementLevel);
                    LogMergeMessage("FieldRequirementLevel: {0} to {1}", out noChange, atr.RequiredLevel.Value, FieldRequirementLevel);
                }
            }
            if (!StringMetadataEqual(atr.DisplayName.UserLocalizedLabel.Label, DisplayName))
            {
                output.DisplayName = new Label(DisplayName, LanguageCode);
                LogMergeMessage("DisplayName: {0} to {1}", out noChange, atr.DisplayName.UserLocalizedLabel.Label, DisplayName);
            }
            if (!StringMetadataEqual(atr.Description.UserLocalizedLabel.Label, Description))
            {
                output.Description = new Label(Description, LanguageCode);
                LogMergeMessage("Description: {0} to {1}", out noChange, atr.Description.UserLocalizedLabel.Label, Description);
            }
            if (atr.IsValidForAdvancedFind.Value != IsValidForAdvancedFind)
            {
                output.IsValidForAdvancedFind = new BooleanManagedProperty(IsValidForAdvancedFind);
                LogMergeMessage("IsValidForAdvancedFind: {0} to {1}", out noChange, atr.IsValidForAdvancedFind.Value, IsValidForAdvancedFind);
            }
            if (atr.IsAuditEnabled.Value != IsAuditEnabled)
            {
                output.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled);
                LogMergeMessage("IsAuditEnabled: {0} to {1}", out noChange, atr.IsAuditEnabled.Value, IsAuditEnabled);
            }
            if (atr.IsSecured.Value != IsSecured)
            {
                output.IsSecured = IsSecured;
                LogMergeMessage("IsSecured: {0} to {1}", out noChange, atr.IsSecured.Value, IsSecured);
            }
            if (atr.MaxLength.Value != MaxLength)
            {
                output.MaxLength = MaxLength;
                LogMergeMessage("MaxLength: {0} to {1}", out noChange, atr.MaxLength.Value, MaxLength);
            }
            if (atr.ImeMode.Value != ImeModeLevel)
            {
                output.ImeMode = ImeModeLevel;
                LogMergeMessage("ImeMode: {0} to {1}", out noChange, atr.ImeMode.Value, ImeModeLevel);
            }
            
            return output;
        }

        /// <summary>
        /// Update Whole Number attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private IntegerAttributeMetadata UpdateIntegerAttribute(IntegerAttributeMetadata atr, out bool noChange)
        {
            noChange = true;
            IntegerAttributeMetadata output = new IntegerAttributeMetadata
            {
                LogicalName = Name, //Name
                SchemaName = SchemaName, //Schema Name
            };

            //set the format type for Integer based on the Format Type in the XML
            IntegerFormat dataFormat = (IntegerFormat)((int)CrmFormatType);

            //build up the metadata as changes are detected
            if (atr.RequiredLevel.Value != FieldRequirementLevel && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                if (atr.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired)
                    LogMergeWarningMessage("FieldRequirementLevel is {0} and cannot be updated to {1}. IsPrimaryField: {2}", atr.RequiredLevel.Value, FieldRequirementLevel, IsPrimaryField);
                else
                {
                    output.RequiredLevel = new AttributeRequiredLevelManagedProperty(FieldRequirementLevel);
                    LogMergeMessage("FieldRequirementLevel: {0} to {1}", out noChange, atr.RequiredLevel.Value, FieldRequirementLevel);
                }
            }
            if (!StringMetadataEqual(atr.DisplayName.UserLocalizedLabel.Label, DisplayName))
            {
                output.DisplayName = new Label(DisplayName, LanguageCode);
                LogMergeMessage("DisplayName: {0} to {1}", out noChange, atr.DisplayName.UserLocalizedLabel.Label, DisplayName);
            }
            if (!StringMetadataEqual(atr.Description.UserLocalizedLabel.Label, Description))
            {
                output.Description = new Label(Description, LanguageCode);
                LogMergeMessage("Description: {0} to {1}", out noChange, atr.Description.UserLocalizedLabel.Label, Description);
            }
            if (atr.IsValidForAdvancedFind.Value != IsValidForAdvancedFind)
            {
                output.IsValidForAdvancedFind = new BooleanManagedProperty(IsValidForAdvancedFind);
                LogMergeMessage("IsValidForAdvancedFind: {0} to {1}", out noChange, atr.IsValidForAdvancedFind.Value, IsValidForAdvancedFind);
            }
            if (atr.IsAuditEnabled.Value != IsAuditEnabled && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                output.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled);
                LogMergeMessage("IsAuditEnabled: {0} to {1}", out noChange, atr.IsAuditEnabled.Value, IsAuditEnabled);
            }
            if (atr.IsSecured.Value != IsSecured)
            {
                output.IsSecured = IsSecured;
                LogMergeMessage("IsSecured: {0} to {1}", out noChange, atr.IsSecured.Value, IsSecured);
            }
            if (atr.Format.Value != dataFormat && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                output.Format = dataFormat;
                LogMergeMessage("Format: {0} to {1}", out noChange, atr.Format.Value, dataFormat);
            }
            if (atr.MinValue.Value != MinValueInt32 && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                output.MinValue = MinValueInt32;
                LogMergeMessage("MinValue: {0} to {1}", out noChange, atr.MinValue.Value, MinValueInt32);
            }
            if (atr.MaxValue.Value != MaxValueInt32 && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                output.MaxValue = MaxValueInt32;
                LogMergeMessage("MaxValue: {0} to {1}", out noChange, atr.MaxValue.Value, MaxValueInt32);
            }

            //metadata that can never be changed after creation
            if (atr.SourceType.Value != (int)CrmFieldType)
                LogMergeWarningMessage("CrmFieldType cannot be updated to {0}.", CrmFieldType);

            return output;
        }

        /// <summary>
        /// Update Decimal attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private DecimalAttributeMetadata UpdateDecimalAttribute(DecimalAttributeMetadata atr, out bool noChange)
        {
            noChange = true;
            DecimalAttributeMetadata output = new DecimalAttributeMetadata
            {
                LogicalName = Name, //Name
                SchemaName = SchemaName, //Schema Name
            };

            //build up the metadata as changes are detected
            if (atr.RequiredLevel.Value != FieldRequirementLevel && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                if (atr.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired)
                    LogMergeWarningMessage("FieldRequirementLevel is {0} and cannot be updated to {1}. IsPrimaryField: {2}", atr.RequiredLevel.Value, FieldRequirementLevel, IsPrimaryField);
                else
                {
                    output.RequiredLevel = new AttributeRequiredLevelManagedProperty(FieldRequirementLevel);
                    LogMergeMessage("FieldRequirementLevel: {0} to {1}", out noChange, atr.RequiredLevel.Value, FieldRequirementLevel);
                }
            }
            if (!StringMetadataEqual(atr.DisplayName.UserLocalizedLabel.Label, DisplayName))
            {
                output.DisplayName = new Label(DisplayName, LanguageCode);
                LogMergeMessage("DisplayName: {0} to {1}", out noChange, atr.DisplayName.UserLocalizedLabel.Label, DisplayName);
            }
            if (!StringMetadataEqual(atr.Description.UserLocalizedLabel.Label, Description))
            {
                output.Description = new Label(Description, LanguageCode);
                LogMergeMessage("Description: {0} to {1}", out noChange, atr.Description.UserLocalizedLabel.Label, Description);
            }
            if (atr.IsValidForAdvancedFind.Value != IsValidForAdvancedFind)
            {
                output.IsValidForAdvancedFind = new BooleanManagedProperty(IsValidForAdvancedFind);
                LogMergeMessage("IsValidForAdvancedFind: {0} to {1}", out noChange, atr.IsValidForAdvancedFind.Value, IsValidForAdvancedFind);
            }
            if (atr.IsAuditEnabled.Value != IsAuditEnabled && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                output.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled);
                LogMergeMessage("IsAuditEnabled: {0} to {1}", out noChange, atr.IsAuditEnabled.Value, IsAuditEnabled);
            }
            if (atr.IsSecured.Value != IsSecured)
            {
                output.IsSecured = IsSecured;
                LogMergeMessage("IsSecured: {0} to {1}", out noChange, atr.IsSecured.Value, IsSecured);
            }
            if (atr.ImeMode.Value != ImeModeLevel && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                output.ImeMode = ImeModeLevel;
                LogMergeMessage("ImeMode: {0} to {1}", out noChange, atr.ImeMode.Value, ImeModeLevel);
            }
            if (atr.MinValue.Value != MinValueDecimal && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                output.MinValue = MinValueDecimal;
                LogMergeMessage("MinValue: {0} to {1}", out noChange, atr.MinValue.Value, MinValueDecimal);
            }
            if (atr.MaxValue.Value != MaxValueDecimal && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                output.MaxValue = MaxValueDecimal;
                LogMergeMessage("MaxValue: {0} to {1}", out noChange, atr.MaxValue.Value, MaxValueDecimal);
            }
            if (atr.Precision.Value != Precision)
            {
                output.Precision = Precision;
                LogMergeMessage("Precision: {0} to {1}", out noChange, atr.Precision.Value, Precision);
            }

            //metadata that can never be changed after creation
            if (atr.SourceType.Value != (int)CrmFieldType)
                LogMergeWarningMessage("CrmFieldType cannot be updated to {0}.", CrmFieldType);
            
            return output;
        }

        /// <summary>
        /// Update Double (Floating Point Number) attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private DoubleAttributeMetadata UpdateDoubleAttribute(DoubleAttributeMetadata atr, out bool noChange)
        {
            noChange = true;
            DoubleAttributeMetadata output = new DoubleAttributeMetadata
            {
                LogicalName = Name, //Name
                SchemaName = SchemaName, //Schema Name
            };

            //build up the metadata as changes are detected
            if (atr.RequiredLevel.Value != FieldRequirementLevel)
            {
                if (atr.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired)
                    LogMergeWarningMessage("FieldRequirementLevel is {0} and cannot be updated to {1}. IsPrimaryField: {2}", atr.RequiredLevel.Value, FieldRequirementLevel, IsPrimaryField);
                else
                {
                    output.RequiredLevel = new AttributeRequiredLevelManagedProperty(FieldRequirementLevel);
                    LogMergeMessage("FieldRequirementLevel: {0} to {1}", out noChange, atr.RequiredLevel.Value, FieldRequirementLevel);
                }
            }
            if (!StringMetadataEqual(atr.DisplayName.UserLocalizedLabel.Label, DisplayName))
            {
                output.DisplayName = new Label(DisplayName, LanguageCode);
                LogMergeMessage("DisplayName: {0} to {1}", out noChange, atr.DisplayName.UserLocalizedLabel.Label, DisplayName);
            }
            if (!StringMetadataEqual(atr.Description.UserLocalizedLabel.Label, Description))
            {
                output.Description = new Label(Description, LanguageCode);
                LogMergeMessage("Description: {0} to {1}", out noChange, atr.Description.UserLocalizedLabel.Label, Description);
            }
            if (atr.IsValidForAdvancedFind.Value != IsValidForAdvancedFind)
            {
                output.IsValidForAdvancedFind = new BooleanManagedProperty(IsValidForAdvancedFind);
                LogMergeMessage("IsValidForAdvancedFind: {0} to {1}", out noChange, atr.IsValidForAdvancedFind.Value, IsValidForAdvancedFind);
            }
            if (atr.IsAuditEnabled.Value != IsAuditEnabled)
            {
                output.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled);
                LogMergeMessage("IsAuditEnabled: {0} to {1}", out noChange, atr.IsAuditEnabled.Value, IsAuditEnabled);
            }
            if (atr.IsSecured.Value != IsSecured)
            {
                output.IsSecured = IsSecured;
                LogMergeMessage("IsSecured: {0} to {1}", out noChange, atr.IsSecured.Value, IsSecured);
            }
            if (atr.ImeMode.Value != ImeModeLevel)
            {
                output.ImeMode = ImeModeLevel;
                LogMergeMessage("ImeMode: {0} to {1}", out noChange, atr.ImeMode.Value, ImeModeLevel);
            }
            if (atr.MinValue.Value != MinValueDouble)
            {
                output.MinValue = MinValueDouble;
                LogMergeMessage("MinValue: {0} to {1}", out noChange, atr.MinValue.Value, MinValueDouble);
            }
            if (atr.MaxValue.Value != MaxValueDouble)
            {
                output.MaxValue = MaxValueDouble;
                LogMergeMessage("MaxValue: {0} to {1}", out noChange, atr.MaxValue.Value, MaxValueDouble);
            }
            if (atr.Precision.Value != Precision)
            {
                output.Precision = Precision;
                LogMergeMessage("Precision: {0} to {1}", out noChange, atr.Precision.Value, Precision);
            }

            return output;
        }

        /// <summary>
        /// Update Currency attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private MoneyAttributeMetadata UpdateMoneyAttribute(MoneyAttributeMetadata atr, out bool noChange)
        {
            noChange = true;
            MoneyAttributeMetadata output = new MoneyAttributeMetadata
            {
                LogicalName = Name, //Name
                SchemaName = SchemaName, //Schema Name
            };

            //build up the metadata as changes are detected
            if (atr.RequiredLevel.Value != FieldRequirementLevel && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                if (atr.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired)
                    LogMergeWarningMessage("FieldRequirementLevel is {0} and cannot be updated to {1}. IsPrimaryField: {2}", atr.RequiredLevel.Value, FieldRequirementLevel, IsPrimaryField);
                else
                {
                    output.RequiredLevel = new AttributeRequiredLevelManagedProperty(FieldRequirementLevel);
                    LogMergeMessage("FieldRequirementLevel: {0} to {1}", out noChange, atr.RequiredLevel.Value, FieldRequirementLevel);
                }
            }
            if (!StringMetadataEqual(atr.DisplayName.UserLocalizedLabel.Label, DisplayName))
            {
                output.DisplayName = new Label(DisplayName, LanguageCode);
                LogMergeMessage("DisplayName: {0} to {1}", out noChange, atr.DisplayName.UserLocalizedLabel.Label, DisplayName);
            }
            if (!StringMetadataEqual(atr.Description.UserLocalizedLabel.Label, Description))
            {
                output.Description = new Label(Description, LanguageCode);
                LogMergeMessage("Description: {0} to {1}", out noChange, atr.Description.UserLocalizedLabel.Label, Description);
            }
            if (atr.IsValidForAdvancedFind.Value != IsValidForAdvancedFind)
            {
                output.IsValidForAdvancedFind = new BooleanManagedProperty(IsValidForAdvancedFind);
                LogMergeMessage("IsValidForAdvancedFind: {0} to {1}", out noChange, atr.IsValidForAdvancedFind.Value, IsValidForAdvancedFind);
            }
            if (atr.IsAuditEnabled.Value != IsAuditEnabled && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                output.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled);
                LogMergeMessage("IsAuditEnabled: {0} to {1}", out noChange, atr.IsAuditEnabled.Value, IsAuditEnabled);
            }
            if (atr.IsSecured.Value != IsSecured)
            {
                output.IsSecured = IsSecured;
                LogMergeMessage("IsSecured: {0} to {1}", out noChange, atr.IsSecured.Value, IsSecured);
            }
            if (atr.MinValue.Value != MinValueDouble && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                output.MinValue = MinValueDouble;
                LogMergeMessage("MinValue: {0} to {1}", out noChange, atr.MinValue.Value, MinValueDouble);
            }
            if (atr.MaxValue.Value != MaxValueDouble && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                output.MaxValue = MaxValueDouble;
                LogMergeMessage("MaxValue: {0} to {1}", out noChange, atr.MaxValue.Value, MaxValueDouble);
            }
            if (atr.PrecisionSource.Value != (int)CrmPrecisionSource)
            {
                output.PrecisionSource = (int)CrmPrecisionSource;
                LogMergeMessage("PrecisionSource: {0} to {1}", out noChange, atr.PrecisionSource.Value, CrmPrecisionSource);
            }
            if (atr.Precision.Value != Precision && CrmPrecisionSource == CrmPrecisionSource.Custom)
            {
                output.Precision = Precision;
                LogMergeMessage("Precision: {0} to {1}", out noChange, atr.Precision.Value, Precision);
            }

            //metadata that can never be changed after creation
            if (atr.SourceType.Value != (int)CrmFieldType)
                LogMergeWarningMessage("CrmFieldType cannot be updated to {0}.", CrmFieldType);

            return output;
        }

        /// <summary>
        /// Update Two Options attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private BooleanAttributeMetadata UpdateBooleanAttribute(BooleanAttributeMetadata atr, out bool noChange)
        {
            noChange = true;
            BooleanAttributeMetadata output = new BooleanAttributeMetadata
            {
                LogicalName = Name, //Name
                SchemaName = SchemaName, //Schema Name
            };

            OptionMetadata trueOpt = new OptionMetadata(new Label(TrueLabel, LanguageCode), 1);
            OptionMetadata falseOpt = new OptionMetadata(new Label(FalseLabel, LanguageCode), 0);

            //build up the metadata as changes are detected
            if (atr.RequiredLevel.Value != FieldRequirementLevel && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                if (atr.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired)
                    LogMergeWarningMessage("FieldRequirementLevel is {0} and cannot be updated to {1}. IsPrimaryField: {2}", atr.RequiredLevel.Value, FieldRequirementLevel, IsPrimaryField);
                else
                {
                    output.RequiredLevel = new AttributeRequiredLevelManagedProperty(FieldRequirementLevel);
                    LogMergeMessage("FieldRequirementLevel: {0} to {1}", out noChange, atr.RequiredLevel.Value, FieldRequirementLevel);
                }
            }
            if (!StringMetadataEqual(atr.DisplayName.UserLocalizedLabel.Label, DisplayName))
            {
                output.DisplayName = new Label(DisplayName, LanguageCode);
                LogMergeMessage("DisplayName: {0} to {1}", out noChange, atr.DisplayName.UserLocalizedLabel.Label, DisplayName);
            }
            if (!StringMetadataEqual(atr.Description.UserLocalizedLabel.Label, Description))
            {
                output.Description = new Label(Description, LanguageCode);
                LogMergeMessage("Description: {0} to {1}", out noChange, atr.Description.UserLocalizedLabel.Label, Description);
            }
            if (atr.IsValidForAdvancedFind.Value != IsValidForAdvancedFind)
            {
                output.IsValidForAdvancedFind = new BooleanManagedProperty(IsValidForAdvancedFind);
                LogMergeMessage("IsValidForAdvancedFind: {0} to {1}", out noChange, atr.IsValidForAdvancedFind.Value, IsValidForAdvancedFind);
            }
            if (atr.IsAuditEnabled.Value != IsAuditEnabled && CrmFieldType == CrmAttributeFieldType.Simple)
            {
                output.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled);
                LogMergeMessage("IsAuditEnabled: {0} to {1}", out noChange, atr.IsAuditEnabled.Value, IsAuditEnabled);
            }
            if (atr.IsSecured.Value != IsSecured)
            {
                output.IsSecured = IsSecured;
                LogMergeMessage("IsSecured: {0} to {1}", out noChange, atr.IsSecured.Value, IsSecured);
            }
            if (atr.DefaultValue.Value != TwoOptionsDefault)
            {
                output.DefaultValue = TwoOptionsDefault;
                LogMergeMessage("TwoOptionsDefault: {0} to {1}", out noChange, atr.DefaultValue.Value, TwoOptionsDefault);
            }
            if (!StringMetadataEqual(atr.OptionSet.TrueOption.Label.UserLocalizedLabel.Label, TrueLabel) || !StringMetadataEqual(atr.OptionSet.FalseOption.Label.UserLocalizedLabel.Label, FalseLabel))
            {
                output.OptionSet = new BooleanOptionSetMetadata(trueOpt, falseOpt);
                LogMergeMessage("TwoOptionsLabel: {0} to {1}; {2} to {3}", out noChange, atr.OptionSet.TrueOption.Label.UserLocalizedLabel.Label, TrueLabel, atr.OptionSet.FalseOption.Label.UserLocalizedLabel.Label, FalseLabel);
            }

            //metadata that can never be changed after creation
            if (atr.SourceType.Value != (int)CrmFieldType)
                LogMergeWarningMessage("CrmFieldType cannot be updated to {0}.", CrmFieldType);

            return output;
        }

        /// <summary>
        /// Update Date Time attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private DateTimeAttributeMetadata UpdateDateTimeAttribute(DateTimeAttributeMetadata atr, out bool noChange)
        {
            noChange = true;
            DateTimeAttributeMetadata output = new DateTimeAttributeMetadata
            {
                LogicalName = Name, //Name
                SchemaName = SchemaName, //Schema Name
            };

            DateTimeFormat dateFormat = DateTimeFormat.DateOnly;
            //set the format type for DateTime based on the Format Type in the XML
            switch (CrmFormatType)
            {
                case CrmAttributeFormatType.DateTime:
                    dateFormat = DateTimeFormat.DateAndTime;
                    break;

                case CrmAttributeFormatType.DateOnly:
                    dateFormat = DateTimeFormat.DateOnly;
                    break;
            }

            //set the date time behavior using a case, DateTimeBehavior is not an enum
            DateTimeBehavior behavior = DateTimeBehavior.UserLocal;
            switch (CrmDateTimeBehavior)
            {
                case CrmDateTimeBehavior.UserLocal:
                    behavior = DateTimeBehavior.UserLocal;
                    break;

                case CrmDateTimeBehavior.DateOnly:
                    //if behavior is date only, force the date format to be date only as well
                    behavior = DateTimeBehavior.DateOnly;
                    dateFormat = DateTimeFormat.DateOnly;
                    break;

                case CrmDateTimeBehavior.TimeZoneIndependent:
                    behavior = DateTimeBehavior.TimeZoneIndependent;
                    break;
            }

            //build up the metadata as changes are detected
            if (atr.RequiredLevel.Value != FieldRequirementLevel)
            {
                if (atr.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired)
                    LogMergeWarningMessage("FieldRequirementLevel is {0} and cannot be updated to {1}. IsPrimaryField: {2}", atr.RequiredLevel.Value, FieldRequirementLevel, IsPrimaryField);
                else
                {
                    output.RequiredLevel = new AttributeRequiredLevelManagedProperty(FieldRequirementLevel);
                    LogMergeMessage("FieldRequirementLevel: {0} to {1}", out noChange, atr.RequiredLevel.Value, FieldRequirementLevel);
                }
            }
            if (!StringMetadataEqual(atr.DisplayName.UserLocalizedLabel.Label, DisplayName))
            {
                output.DisplayName = new Label(DisplayName, LanguageCode);
                LogMergeMessage("DisplayName: {0} to {1}", out noChange, atr.DisplayName.UserLocalizedLabel.Label, DisplayName);
            }
            if (!StringMetadataEqual(atr.Description.UserLocalizedLabel.Label, Description))
            {
                output.Description = new Label(Description, LanguageCode);
                LogMergeMessage("Description: {0} to {1}", out noChange, atr.Description.UserLocalizedLabel.Label, Description);
            }
            if (atr.IsValidForAdvancedFind.Value != IsValidForAdvancedFind)
            {
                output.IsValidForAdvancedFind = new BooleanManagedProperty(IsValidForAdvancedFind);
                LogMergeMessage("IsValidForAdvancedFind: {0} to {1}", out noChange, atr.IsValidForAdvancedFind.Value, IsValidForAdvancedFind);
            }
            if (atr.IsAuditEnabled.Value != IsAuditEnabled)
            {
                output.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled);
                LogMergeMessage("IsAuditEnabled: {0} to {1}", out noChange, atr.IsAuditEnabled.Value, IsAuditEnabled);
            }
            if (atr.IsSecured.Value != IsSecured)
            {
                output.IsSecured = IsSecured;
                LogMergeMessage("IsSecured: {0} to {1}", out noChange, atr.IsSecured.Value, IsSecured);
            }
            if (atr.ImeMode.Value != ImeModeLevel)
            {
                output.ImeMode = ImeModeLevel;
                LogMergeMessage("ImeMode: {0} to {1}", out noChange, atr.ImeMode.Value, ImeModeLevel);
            }
            if (atr.Format.Value != dateFormat && atr.DateTimeBehavior.Value != DateTimeBehavior.DateOnly)
            {
                //datetime format cannot be updated if the behavior is DateOnly
                output.Format = dateFormat;
                LogMergeMessage("Format: {0} to {1}", out noChange, atr.Format.Value, dateFormat);
            }
            //ensure the format is not change if behavior is already DateOnly
            if (atr.DateTimeBehavior.Value == DateTimeBehavior.DateOnly)
                output.Format = null;
            if (atr.DateTimeBehavior.Value != behavior && atr.DateTimeBehavior.Value != DateTimeBehavior.TimeZoneIndependent && atr.DateTimeBehavior.Value != DateTimeBehavior.DateOnly)
            {
                //datetime behavior cannot be updated after it is set to TimeZoneIndependent or DateOnly
                output.DateTimeBehavior = behavior;
                LogMergeMessage("DateTimeBehavior: {0} to {1}", out noChange, atr.DateTimeBehavior.Value, behavior);
            }
            //ensure the behavior is not change if it is already TimeZoneIndependent or DateOnly
            if (atr.DateTimeBehavior.Value == DateTimeBehavior.TimeZoneIndependent || atr.DateTimeBehavior.Value == DateTimeBehavior.DateOnly)
                output.DateTimeBehavior = null;

            //metadata that can never be changed after creation
            if (atr.SourceType.Value != (int)CrmFieldType)
                LogMergeWarningMessage("CrmFieldType cannot be updated to {0}.", CrmFieldType);
            
            return output;
        }

        /// <summary>
        /// Update Option Set attribute (global or local optionset)
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="util"></param>
        /// <returns></returns>
        private PicklistAttributeMetadata UpdateOptionSetAttribute(PicklistAttributeMetadata atr, out bool noChange, out CrmOptionCollection optToUpdate)
        {
            optToUpdate = null;
            noChange = true;
            PicklistAttributeMetadata output = new PicklistAttributeMetadata
            {
                LogicalName = Name, //Name
                SchemaName = SchemaName, //Schema Name
            };

            //build up the metadata as changes are detected
            if (atr.RequiredLevel.Value != FieldRequirementLevel)
            {
                if (atr.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired)
                    LogMergeWarningMessage("FieldRequirementLevel is {0} and cannot be updated to {1}. IsPrimaryField: {2}", atr.RequiredLevel.Value, FieldRequirementLevel, IsPrimaryField);
                else
                {
                    output.RequiredLevel = new AttributeRequiredLevelManagedProperty(FieldRequirementLevel);
                    LogMergeMessage("FieldRequirementLevel: {0} to {1}", out noChange, atr.RequiredLevel.Value, FieldRequirementLevel);
                }
            }
            if (!StringMetadataEqual(atr.DisplayName.UserLocalizedLabel.Label, DisplayName))
            {
                output.DisplayName = new Label(DisplayName, LanguageCode);
                LogMergeMessage("DisplayName: {0} to {1}", out noChange, atr.DisplayName.UserLocalizedLabel.Label, DisplayName);
            }
            if (!StringMetadataEqual(atr.Description.UserLocalizedLabel.Label, Description))
            {
                output.Description = new Label(Description, LanguageCode);
                LogMergeMessage("Description: {0} to {1}", out noChange, atr.Description.UserLocalizedLabel.Label, Description);
            }
            if (atr.IsValidForAdvancedFind.Value != IsValidForAdvancedFind)
            {
                output.IsValidForAdvancedFind = new BooleanManagedProperty(IsValidForAdvancedFind);
                LogMergeMessage("IsValidForAdvancedFind: {0} to {1}", out noChange, atr.IsValidForAdvancedFind.Value, IsValidForAdvancedFind);
            }
            if (atr.IsAuditEnabled.Value != IsAuditEnabled)
            {
                output.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled);
                LogMergeMessage("IsAuditEnabled: {0} to {1}", out noChange, atr.IsAuditEnabled.Value, IsAuditEnabled);
            }
            if (atr.IsSecured.Value != IsSecured)
            {
                output.IsSecured = IsSecured;
                LogMergeMessage("IsSecured: {0} to {1}", out noChange, atr.IsSecured.Value, IsSecured);
            }
            if (atr.DefaultFormValue != null && atr.DefaultFormValue.Value != OptionSetDefault)
            {
                output.DefaultFormValue = OptionSetDefault;
                LogMergeMessage("OptionSetDefault: {0} to {1}", out noChange, atr.DefaultFormValue.Value, OptionSetDefault);
            }
            else if (atr.DefaultFormValue == null && OptionSetDefault != null)
            {
                output.DefaultFormValue = OptionSetDefault;
                LogMergeMessage("OptionSetDefault: {0} to {1}", out noChange, atr.DefaultFormValue.Value, OptionSetDefault);
            }

            //update the optionset if it is a local optionset
            if (!atr.OptionSet.IsGlobal.Value && Options != null)
            {
                Options.MergeWith(atr.OptionSet.Options.ToArray());
                if (Options.OptionToCreate().Length > 0 || Options.OptionToUpdate().Length > 0 || Options.OptionToDelete().Length > 0)
                {
                    //return the merged options as optionset to update
                    optToUpdate = Options;
                    LogMergeMessage("OptionSetValues Updated.", out noChange);
                }
            }

            //metadata that can never be changed after creation
            if (atr.SourceType.Value != (int)CrmFieldType)
                LogMergeWarningMessage("CrmFieldType cannot be updated to {0}.", CrmFieldType);
            if (atr.OptionSet.IsGlobal.Value != IsGlobalOptionSet)
                LogMergeWarningMessage("IsGlobalOptionSet cannot be updated to {0}.", IsGlobalOptionSet);
            if (atr.OptionSet.IsGlobal.Value && atr.OptionSet.Name != GlobalOptionSetName)
                LogMergeWarningMessage("GlobalOptionSetName cannot be updated to {0}.", GlobalOptionSetName);

            return output;
        }

        /// <summary>
        /// Update Multi Option Set attribute (global or local optionset)
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="util"></param>
        /// <returns></returns>
        private MultiSelectPicklistAttributeMetadata UpdateMultiOptionSetAttribute(MultiSelectPicklistAttributeMetadata atr, out bool noChange, out CrmOptionCollection optToUpdate)
        {
            optToUpdate = null;
            noChange = true;
            MultiSelectPicklistAttributeMetadata output = new MultiSelectPicklistAttributeMetadata
            {
                LogicalName = Name, //Name
                SchemaName = SchemaName, //Schema Name
            };

            //build up the metadata as changes are detected
            if (atr.RequiredLevel.Value != FieldRequirementLevel)
            {
                if (atr.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired)
                    LogMergeWarningMessage("FieldRequirementLevel is {0} and cannot be updated to {1}. IsPrimaryField: {2}", atr.RequiredLevel.Value, FieldRequirementLevel, IsPrimaryField);
                else
                {
                    output.RequiredLevel = new AttributeRequiredLevelManagedProperty(FieldRequirementLevel);
                    LogMergeMessage("FieldRequirementLevel: {0} to {1}", out noChange, atr.RequiredLevel.Value, FieldRequirementLevel);
                }
            }
            if (!StringMetadataEqual(atr.DisplayName.UserLocalizedLabel.Label, DisplayName))
            {
                output.DisplayName = new Label(DisplayName, LanguageCode);
                LogMergeMessage("DisplayName: {0} to {1}", out noChange, atr.DisplayName.UserLocalizedLabel.Label, DisplayName);
            }
            if (!StringMetadataEqual(atr.Description.UserLocalizedLabel.Label, Description))
            {
                output.Description = new Label(Description, LanguageCode);
                LogMergeMessage("Description: {0} to {1}", out noChange, atr.Description.UserLocalizedLabel.Label, Description);
            }
            if (atr.IsValidForAdvancedFind.Value != IsValidForAdvancedFind)
            {
                output.IsValidForAdvancedFind = new BooleanManagedProperty(IsValidForAdvancedFind);
                LogMergeMessage("IsValidForAdvancedFind: {0} to {1}", out noChange, atr.IsValidForAdvancedFind.Value, IsValidForAdvancedFind);
            }
            if (atr.IsAuditEnabled.Value != IsAuditEnabled)
            {
                output.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled);
                LogMergeMessage("IsAuditEnabled: {0} to {1}", out noChange, atr.IsAuditEnabled.Value, IsAuditEnabled);
            }
            if (atr.IsSecured.Value != IsSecured)
            {
                output.IsSecured = IsSecured;
                LogMergeMessage("IsSecured: {0} to {1}", out noChange, atr.IsSecured.Value, IsSecured);
            }
            if (atr.DefaultFormValue != null && atr.DefaultFormValue.Value != OptionSetDefault)
            {
                output.DefaultFormValue = OptionSetDefault;
                LogMergeMessage("OptionSetDefault: {0} to {1}", out noChange, atr.DefaultFormValue.Value, OptionSetDefault);
            }
            else if (atr.DefaultFormValue == null && OptionSetDefault != null)
            {
                output.DefaultFormValue = OptionSetDefault;
                LogMergeMessage("OptionSetDefault: {0} to {1}", out noChange, atr.DefaultFormValue.Value, OptionSetDefault);
            }

            //update the optionset if it is a local optionset
            if (!atr.OptionSet.IsGlobal.Value && Options != null)
            {
                Options.MergeWith(atr.OptionSet.Options.ToArray());
                if (Options.OptionToCreate().Length > 0 || Options.OptionToUpdate().Length > 0 || Options.OptionToDelete().Length > 0)
                {
                    //return the merged options as optionset to update
                    optToUpdate = Options;
                    LogMergeMessage("OptionSetValues Updated.", out noChange);
                }
            }

            //metadata that can never be changed after creation
            if (atr.SourceType.Value != (int)CrmFieldType)
                LogMergeWarningMessage("CrmFieldType cannot be updated to {0}.", CrmFieldType);
            if (atr.OptionSet.IsGlobal.Value != IsGlobalOptionSet)
                LogMergeWarningMessage("IsGlobalOptionSet cannot be updated to {0}.", IsGlobalOptionSet);
            if (atr.OptionSet.IsGlobal.Value && atr.OptionSet.Name != GlobalOptionSetName)
                LogMergeWarningMessage("GlobalOptionSetName cannot be updated to {0}.", GlobalOptionSetName);

            return output;
        }

        /// <summary>
        /// Update Lookup attribute
        /// </summary>
        /// <param name="atr"></param>
        /// <param name="noChange"></param>
        /// <returns></returns>
        private LookupAttributeMetadata UpdateLookupAttribute(WebServiceUtils util, LookupAttributeMetadata atr, out bool noChange)
        {
            noChange = true;
            LookupAttributeMetadata output = new LookupAttributeMetadata
            {
                LogicalName = Name, //Name
                SchemaName = SchemaName, //Schema Name
            };

            //build up the metadata as changes are detected
            if (atr.RequiredLevel.Value != FieldRequirementLevel)
            {
                if (atr.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired)
                    LogMergeWarningMessage("FieldRequirementLevel is {0} and cannot be updated to {1}. IsPrimaryField: {2}", atr.RequiredLevel.Value, FieldRequirementLevel, IsPrimaryField);
                else
                {
                    output.RequiredLevel = new AttributeRequiredLevelManagedProperty(FieldRequirementLevel);
                    LogMergeMessage("FieldRequirementLevel: {0} to {1}", out noChange, atr.RequiredLevel.Value, FieldRequirementLevel);
                }
            }
            if (!StringMetadataEqual(atr.DisplayName.UserLocalizedLabel.Label, DisplayName))
            {
                output.DisplayName = new Label(DisplayName, LanguageCode);
                LogMergeMessage("DisplayName: {0} to {1}", out noChange, atr.DisplayName.UserLocalizedLabel.Label, DisplayName);
            }
            if (!StringMetadataEqual(atr.Description.UserLocalizedLabel.Label, Description))
            {
                output.Description = new Label(Description, LanguageCode);
                LogMergeMessage("Description: {0} to {1}", out noChange, atr.Description.UserLocalizedLabel.Label, Description);
            }
            if (atr.IsValidForAdvancedFind.Value != IsValidForAdvancedFind)
            {
                output.IsValidForAdvancedFind = new BooleanManagedProperty(IsValidForAdvancedFind);
                LogMergeMessage("IsValidForAdvancedFind: {0} to {1}", out noChange, atr.IsValidForAdvancedFind.Value, IsValidForAdvancedFind);
            }
            if (atr.IsAuditEnabled.Value != IsAuditEnabled)
            {
                output.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled);
                LogMergeMessage("IsAuditEnabled: {0} to {1}", out noChange, atr.IsAuditEnabled.Value, IsAuditEnabled);
            }
            if (atr.IsSecured.Value != IsSecured)
            {
                output.IsSecured = IsSecured;
                LogMergeMessage("IsSecured: {0} to {1}", out noChange, atr.IsSecured.Value, IsSecured);
            }

            //get the 1:N relationship to check if it has changed for custom lookup
            if (!IsSystem)
            {
                OneToManyRelationshipMetadata rel = Metadata.GetOneToManyRelationshipMetadata(util, LookupRelationshipName);
                if (rel != null)
                {
                    //detect for any changes
                    if ((int)rel.AssociatedMenuConfiguration.Behavior.Value != (int)CrmLookupMenuBehavior)
                    {
                        LogMergeMessage("AssociatedMenuConfiguration.Behavior: {0} to {1}", out noChange, rel.AssociatedMenuConfiguration.Behavior.Value, CrmLookupMenuBehavior);
                    }
                    if ((int)rel.AssociatedMenuConfiguration.Group.Value != (int)CrmLookupMenuGroup)
                    {
                        LogMergeMessage("AssociatedMenuConfiguration.Group: {0} to {1}", out noChange, rel.AssociatedMenuConfiguration.Group.Value, CrmLookupMenuGroup);
                    }
                    if (rel.AssociatedMenuConfiguration.Label != null && !StringMetadataEqual(rel.AssociatedMenuConfiguration.Label.UserLocalizedLabel?.Label, LookupMenuCustomLabel))
                    {
                        LogMergeMessage("AssociatedMenuConfiguration.Label: {0} to {1}", out noChange, rel.AssociatedMenuConfiguration.Label.UserLocalizedLabel?.Label, LookupMenuCustomLabel);
                    }
                    if (rel.AssociatedMenuConfiguration.Order != null && rel.AssociatedMenuConfiguration.Order.Value != LookupMenuOrder)
                    {
                        LogMergeMessage("AssociatedMenuConfiguration.Order: {0} to {1}", out noChange, rel.AssociatedMenuConfiguration.Order, LookupMenuOrder);
                    }
                    if (rel.CascadeConfiguration.Assign != null && (int)rel.CascadeConfiguration.Assign.Value != (int)CrmLookupRelationshipBehaviorAssign)
                    {
                        LogMergeMessage("CascadeConfiguration.Assign: {0} to {1}", out noChange, rel.CascadeConfiguration.Assign, CrmLookupRelationshipBehaviorAssign);
                    }
                    if (rel.CascadeConfiguration.Share != null && (int)rel.CascadeConfiguration.Share.Value != (int)CrmLookupRelationshipBehaviorShare)
                    {
                        LogMergeMessage("CascadeConfiguration.Share: {0} to {1}", out noChange, rel.CascadeConfiguration.Share, CrmLookupRelationshipBehaviorShare);
                    }
                    if (rel.CascadeConfiguration.Unshare != null && (int)rel.CascadeConfiguration.Unshare.Value != (int)CrmLookupRelationshipBehaviorUnshare)
                    {
                        LogMergeMessage("CascadeConfiguration.Unshare: {0} to {1}", out noChange, rel.CascadeConfiguration.Unshare, CrmLookupRelationshipBehaviorUnshare);
                    }
                    if (rel.CascadeConfiguration.Reparent != null && (int)rel.CascadeConfiguration.Reparent.Value != (int)CrmLookupRelationshipBehaviorReparent)
                    {
                        LogMergeMessage("CascadeConfiguration.Reparent: {0} to {1}", out noChange, rel.CascadeConfiguration.Reparent, CrmLookupRelationshipBehaviorReparent);
                    }
                    if (rel.CascadeConfiguration.Merge != null && (int)rel.CascadeConfiguration.Merge.Value != (int)CrmLookupRelationshipBehaviorMerge)
                    {
                        LogMergeMessage("CascadeConfiguration.Merge: {0} to {1}", out noChange, rel.CascadeConfiguration.Merge, CrmLookupRelationshipBehaviorMerge);
                    }
                    if (rel.CascadeConfiguration.RollupView != null && (int)rel.CascadeConfiguration.RollupView.Value != (int)CrmLookupRelationshipBehaviorRollupView)
                    {
                        LogMergeMessage("CascadeConfiguration.RollupView: {0} to {1}", out noChange, rel.CascadeConfiguration.RollupView, CrmLookupRelationshipBehaviorRollupView);
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Update Customer attribute
        /// </summary>
        /// <param name="atr"></param>
        /// <param name="noChange"></param>
        /// <returns></returns>
        private AttributeMetadata UpdateCustomerAttribute(AttributeMetadata atr, out bool noChange)
        {
            noChange = true;
            AttributeMetadata output = atr;

            //build up the metadata as changes are detected
            if (atr.RequiredLevel.Value != FieldRequirementLevel)
            {
                if (atr.RequiredLevel.Value == AttributeRequiredLevel.SystemRequired)
                    LogMergeWarningMessage("FieldRequirementLevel is {0} and cannot be updated to {1}. IsPrimaryField: {2}", atr.RequiredLevel.Value, FieldRequirementLevel, IsPrimaryField);
                else
                {
                    output.RequiredLevel = new AttributeRequiredLevelManagedProperty(FieldRequirementLevel);
                    LogMergeMessage("FieldRequirementLevel: {0} to {1}", out noChange, atr.RequiredLevel.Value, FieldRequirementLevel);
                }
            }
            string crmDisplayName = atr.DisplayName.UserLocalizedLabel.Label;
            if (!StringMetadataEqual(crmDisplayName, DisplayName))
            {
                output.DisplayName = new Label(DisplayName, LanguageCode);
                LogMergeMessage("DisplayName: {0} to {1}", out noChange, crmDisplayName, DisplayName);
            }
            string crmDescription = atr.Description.UserLocalizedLabel.Label;
            if (!StringMetadataEqual(crmDescription, Description))
            {
                output.Description = new Label(Description, LanguageCode);
                LogMergeMessage("Description: {0} to {1}", out noChange, crmDescription, Description);
            }
            if (atr.IsValidForAdvancedFind.Value != IsValidForAdvancedFind)
            {
                output.IsValidForAdvancedFind = new BooleanManagedProperty(IsValidForAdvancedFind);
                LogMergeMessage("IsValidForAdvancedFind: {0} to {1}", out noChange, atr.IsValidForAdvancedFind.Value, IsValidForAdvancedFind);
            }
            if (atr.IsAuditEnabled.Value != IsAuditEnabled)
            {
                output.IsAuditEnabled = new BooleanManagedProperty(IsAuditEnabled);
                LogMergeMessage("IsAuditEnabled: {0} to {1}", out noChange, atr.IsAuditEnabled.Value, IsAuditEnabled);
            }
            if (atr.IsSecured.Value != IsSecured)
            {
                output.IsSecured = IsSecured;
                LogMergeMessage("IsSecured: {0} to {1}", out noChange, atr.IsSecured.Value, IsSecured);
            }

            return output;
        }
        #endregion UpdateAttribute
        #endregion MergeWith

        #region CreateCrmAttributeMetadata
        public AttributeMetadata CreateCrmAttributeMetadata(WebServiceUtils util, out bool create1toNRelationship)
        {
            create1toNRelationship = false;

            CrmAttribute attr = this;
            AttributeMetadata attrMeta = null;
            switch (attr.CrmDataType)
            {
                case CrmAttributeDataType.SingleText:
                    attrMeta = CreateStringAttribute(attr);
                    break;

                case CrmAttributeDataType.MultiText:
                    attrMeta = CreateMemoAttribute(attr);
                    break;

                case CrmAttributeDataType.Int32:
                    attrMeta = CreateIntegerAttribute(attr);
                    break;

                case CrmAttributeDataType.Decimal:
                    attrMeta = CreateDecimalAttribute(attr);
                    break;

                case CrmAttributeDataType.Float:
                    attrMeta = CreateDoubleAttribute(attr);
                    break;

                case CrmAttributeDataType.Currency:
                    attrMeta = CreateMoneyAttribute(attr);
                    break;

                case CrmAttributeDataType.TwoOptions:
                    attrMeta = CreateBooleanAttribute(attr);
                    break;

                case CrmAttributeDataType.OptionSet:
                    attrMeta = CreateOptionSetAttribute(attr, util);
                    break;

                case CrmAttributeDataType.MultiOptionSet:
                    attrMeta = CreateMultiOptionSetAttribute(attr, util);
                    break;

                case CrmAttributeDataType.DateTime:
                    attrMeta = CreateDateTimeAttribute(attr);
                    break;

                case CrmAttributeDataType.Image:
                    attrMeta = CreateImageAttribute(attr);
                    break;

                case CrmAttributeDataType.Customer:
                case CrmAttributeDataType.Lookup:
                    attrMeta = CreateLookupAttribute(attr);
                    create1toNRelationship = true;
                    break;

                default:
                    throw new FormatException(string.Format("The data type for the attribute '{0}' is not supported.", attr.Name));
            }

            return attrMeta;
        }

        #region CreateAttribute
        /// <summary>
        /// Create Single Line of Text attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private StringAttributeMetadata CreateStringAttribute(CrmAttribute attr)
        {
            //set the format type for SingleText based on the Format Type in the XML using a case, StringFormatName is not an enum
            StringFormatName dataFormatName = StringFormatName.Text;
            switch (attr.CrmFormatType)
            {
                case CrmAttributeFormatType.Email:
                    dataFormatName = StringFormatName.Email;
                    break;

                case CrmAttributeFormatType.Text:
                    dataFormatName = StringFormatName.Text;
                    break;

                case CrmAttributeFormatType.TextArea:
                    dataFormatName = StringFormatName.TextArea;
                    break;

                case CrmAttributeFormatType.Url:
                    dataFormatName = StringFormatName.Url;
                    break;

                case CrmAttributeFormatType.TickerSymbol:
                    dataFormatName = StringFormatName.TickerSymbol;
                    break;

                case CrmAttributeFormatType.Phone:
                    dataFormatName = StringFormatName.Phone;
                    break;
            }

            StringAttributeMetadata output = new StringAttributeMetadata
            {
                LogicalName = attr.Name, //Name
                SchemaName = attr.SchemaName, //Schema Name
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attr.FieldRequirementLevel), //Field Requirement
                DisplayName = new Label(attr.DisplayName, attr.LanguageCode), //Display Name
                Description = new Label(attr.Description, attr.LanguageCode), //Description
                IsValidForAdvancedFind = new BooleanManagedProperty(attr.IsValidForAdvancedFind), //Searchable
                IsAuditEnabled = new BooleanManagedProperty(attr.IsAuditEnabled), //Auditing
                IsSecured = attr.IsSecured, //Field Security
                SourceType = (int)attr.CrmFieldType, //Field Type, either Simple or Calculated
                ImeMode = attr.ImeModeLevel, //IME mode
                FormatName = dataFormatName, //Format
                MaxLength = attr.MaxLength, //Maximum Length, applicable for text, 4,000
            };

            return output;
        }

        /// <summary>
        /// Create Multiple Lines of Text attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private MemoAttributeMetadata CreateMemoAttribute(CrmAttribute attr)
        {
            MemoAttributeMetadata output = new MemoAttributeMetadata
            {
                LogicalName = attr.Name, //Name
                SchemaName = attr.SchemaName, //Schema Name
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attr.FieldRequirementLevel), //Field Requirement
                DisplayName = new Label(attr.DisplayName, attr.LanguageCode), //Display Name
                Description = new Label(attr.Description, attr.LanguageCode), //Description
                IsValidForAdvancedFind = new BooleanManagedProperty(attr.IsValidForAdvancedFind), //Searchable
                IsAuditEnabled = new BooleanManagedProperty(attr.IsAuditEnabled), //Auditing
                IsSecured = attr.IsSecured, //Field Security
                ImeMode = attr.ImeModeLevel, //IME mode
                MaxLength = attr.MaxLength, //Maximum Length, applicable for text, 1,048,576
            };

            return output;
        }

        /// <summary>
        /// Create Whole Number attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private IntegerAttributeMetadata CreateIntegerAttribute(CrmAttribute attr)
        {
            //set the format type for Integer based on the Format Type in the XML
            IntegerFormat dataFormat = (IntegerFormat)((int)attr.CrmFormatType);
            IntegerAttributeMetadata output = new IntegerAttributeMetadata
            {
                LogicalName = attr.Name, //Name
                SchemaName = attr.SchemaName, //Schema Name
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attr.FieldRequirementLevel), //Field Requirement
                DisplayName = new Label(attr.DisplayName, attr.LanguageCode), //Display Name
                Description = new Label(attr.Description, attr.LanguageCode), //Description
                IsValidForAdvancedFind = new BooleanManagedProperty(attr.IsValidForAdvancedFind), //Searchable
                IsAuditEnabled = new BooleanManagedProperty(attr.IsAuditEnabled), //Auditing
                IsSecured = attr.IsSecured, //Field Security
                SourceType = (int)attr.CrmFieldType, //Field Type, either Simple or Calculated or Rollup
                Format = dataFormat, //Format
                MinValue = attr.MinValueInt32, //Minimum Value, -2,147,483,648
                MaxValue = attr.MaxValueInt32, //Maximum Value, 2,147,483,647
            };

            return output;
        }

        /// <summary>
        /// Create Decimal attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private DecimalAttributeMetadata CreateDecimalAttribute(CrmAttribute attr)
        {
            DecimalAttributeMetadata output = new DecimalAttributeMetadata
            {
                LogicalName = attr.Name, //Name
                SchemaName = attr.SchemaName, //Schema Name
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attr.FieldRequirementLevel), //Field Requirement
                DisplayName = new Label(attr.DisplayName, attr.LanguageCode), //Display Name
                Description = new Label(attr.Description, attr.LanguageCode), //Description
                IsValidForAdvancedFind = new BooleanManagedProperty(attr.IsValidForAdvancedFind), //Searchable
                IsAuditEnabled = new BooleanManagedProperty(attr.IsAuditEnabled), //Auditing
                IsSecured = attr.IsSecured, //Field Security
                SourceType = (int)attr.CrmFieldType, //Field Type, either Simple or Calculated or Rollup
                ImeMode = attr.ImeModeLevel, //IME mode
                Precision = attr.Precision, //Precision, value between 0 to 10
                MinValue = attr.MinValueDecimal, //Minimum Value, -100,000,000,000
                MaxValue = attr.MaxValueDecimal, //Maximum Value, 100,000,000,000
            };

            return output;
        }

        /// <summary>
        /// Create Double (Floating Point Number) attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private DoubleAttributeMetadata CreateDoubleAttribute(CrmAttribute attr)
        {
            DoubleAttributeMetadata output = new DoubleAttributeMetadata
            {
                LogicalName = attr.Name, //Name
                SchemaName = attr.SchemaName, //Schema Name
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attr.FieldRequirementLevel), //Field Requirement
                DisplayName = new Label(attr.DisplayName, attr.LanguageCode), //Display Name
                Description = new Label(attr.Description, attr.LanguageCode), //Description
                IsValidForAdvancedFind = new BooleanManagedProperty(attr.IsValidForAdvancedFind), //Searchable
                IsAuditEnabled = new BooleanManagedProperty(attr.IsAuditEnabled), //Auditing
                IsSecured = attr.IsSecured, //Field Security
                SourceType = (int)attr.CrmFieldType, //Field Type, either Simple or Calculated or Rollup
                ImeMode = attr.ImeModeLevel, //IME mode
                Precision = attr.Precision, //Precision, value between 0 to 10
                MinValue = attr.MinValueDouble, //Minimum Value, -100,000,000,000
                MaxValue = attr.MaxValueDouble, //Maximum Value, 100,000,000,000
            };

            return output;
        }

        /// <summary>
        /// Create Currency attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private MoneyAttributeMetadata CreateMoneyAttribute(CrmAttribute attr)
        {
            MoneyAttributeMetadata output = new MoneyAttributeMetadata
            {
                LogicalName = attr.Name, //Name
                SchemaName = attr.SchemaName, //Schema Name
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attr.FieldRequirementLevel), //Field Requirement
                DisplayName = new Label(attr.DisplayName, attr.LanguageCode), //Display Name
                Description = new Label(attr.Description, attr.LanguageCode), //Description
                IsValidForAdvancedFind = new BooleanManagedProperty(attr.IsValidForAdvancedFind), //Searchable
                IsAuditEnabled = new BooleanManagedProperty(attr.IsAuditEnabled), //Auditing
                IsSecured = attr.IsSecured, //Field Security
                SourceType = (int)attr.CrmFieldType, //Field Type, either Simple or Calculated or Rollup
                PrecisionSource = (int)attr.CrmPrecisionSource, //Precision Source, 0 = Use Precision Setting; 1 = Pricing Decimal Precision (Organization); 2 = Currency Decimal Precision (selected Currency on the record),
                MinValue = attr.MinValueDouble, //Minimum Value, -922,337,203,685,477
                MaxValue = attr.MaxValueDouble, //Maximum Value, 922,337,203,685,477
            };

            //set precision when source is custom
            if (attr.CrmPrecisionSource == CrmPrecisionSource.Custom)
                output.Precision = attr.Precision; //Precision, 0 to 4, applicable when PrecisionSource = 0

            return output;
        }

        /// <summary>
        /// Create Two Options attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private BooleanAttributeMetadata CreateBooleanAttribute(CrmAttribute attr)
        {
            OptionMetadata trueOpt = new OptionMetadata(new Label(attr.TrueLabel, attr.LanguageCode), 1);
            OptionMetadata falseOpt = new OptionMetadata(new Label(attr.FalseLabel, attr.LanguageCode), 0);

            BooleanAttributeMetadata output = new BooleanAttributeMetadata
            {
                LogicalName = attr.Name, //Name
                SchemaName = attr.SchemaName, //Schema Name
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attr.FieldRequirementLevel), //Field Requirement
                DisplayName = new Label(attr.DisplayName, attr.LanguageCode), //Display Name
                Description = new Label(attr.Description, attr.LanguageCode), //Description
                IsValidForAdvancedFind = new BooleanManagedProperty(attr.IsValidForAdvancedFind), //Searchable
                IsAuditEnabled = new BooleanManagedProperty(attr.IsAuditEnabled), //Auditing
                IsSecured = attr.IsSecured, //Field Security
                SourceType = (int)attr.CrmFieldType, //Field Type, either Simple or Calculated
                DefaultValue = attr.TwoOptionsDefault, //Default Value
                OptionSet = new BooleanOptionSetMetadata(trueOpt, falseOpt), //Two Options Picklist
            };

            return output;
        }

        /// <summary>
        /// Create Option Set attribute (global or local optionset)
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="util"></param>
        /// <returns></returns>
        private PicklistAttributeMetadata CreateOptionSetAttribute(CrmAttribute attr, WebServiceUtils util)
        {
            OptionSetMetadata opt = null;

            //check if global optionset name is provided
            if (attr.IsGlobalOptionSet)
            {
                //set the global optionset name to be used
                opt = new OptionSetMetadata
                {
                    IsGlobal = true,
                    Name = attr.GlobalOptionSetName
                };
            }
            else
            {
                //local optionset, create the metadata
                if (attr.Options != null)
                {
                    opt = new OptionSetMetadata(attr.Options.ToOptionMetadataCollection());
                    opt.IsGlobal = false;
                }
                else
                    throw new FormatException("The option values is not provided for a local optionset attribute.");
            }

            PicklistAttributeMetadata output = new PicklistAttributeMetadata
            {
                LogicalName = attr.Name, //Name
                SchemaName = attr.SchemaName, //Schema Name
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attr.FieldRequirementLevel), //Field Requirement
                DisplayName = new Label(attr.DisplayName, attr.LanguageCode), //Display Name
                Description = new Label(attr.Description, attr.LanguageCode), //Description
                IsValidForAdvancedFind = new BooleanManagedProperty(attr.IsValidForAdvancedFind), //Searchable
                IsAuditEnabled = new BooleanManagedProperty(attr.IsAuditEnabled), //Auditing
                IsSecured = attr.IsSecured, //Field Security
                SourceType = (int)attr.CrmFieldType, //Field Type, either Simple or Calculated
                DefaultFormValue = attr.OptionSetDefault, //Default Value
                OptionSet = opt, //OptionSet Picklist, either global or local optionset
            };

            return output;
        }

        /// <summary>
        /// Create Multi Option Set attribute (global or local optionset)
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="util"></param>
        /// <returns></returns>
        private MultiSelectPicklistAttributeMetadata CreateMultiOptionSetAttribute(CrmAttribute attr, WebServiceUtils util)
        {
            OptionSetMetadata opt = null;

            //check if global optionset name is provided
            if (attr.IsGlobalOptionSet)
            {
                //set the global optionset name to be used
                opt = new OptionSetMetadata
                {
                    IsGlobal = true,
                    Name = attr.GlobalOptionSetName
                };
            }
            else
            {
                //local optionset, create the metadata
                if (attr.Options != null)
                {
                    opt = new OptionSetMetadata(attr.Options.ToOptionMetadataCollection());
                    opt.IsGlobal = false;
                }
                else
                    throw new FormatException("The option values is not provided for a local optionset attribute.");
            }

            MultiSelectPicklistAttributeMetadata output = new MultiSelectPicklistAttributeMetadata
            {
                LogicalName = attr.Name, //Name
                SchemaName = attr.SchemaName, //Schema Name
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attr.FieldRequirementLevel), //Field Requirement
                DisplayName = new Label(attr.DisplayName, attr.LanguageCode), //Display Name
                Description = new Label(attr.Description, attr.LanguageCode), //Description
                IsValidForAdvancedFind = new BooleanManagedProperty(attr.IsValidForAdvancedFind), //Searchable
                IsAuditEnabled = new BooleanManagedProperty(attr.IsAuditEnabled), //Auditing
                IsSecured = attr.IsSecured, //Field Security
                SourceType = (int)attr.CrmFieldType, //Field Type, either Simple or Calculated
                DefaultFormValue = attr.OptionSetDefault, //Default Value
                OptionSet = opt, //OptionSet Picklist, either global or local optionset
            };

            return output;
        }

        /// <summary>
        /// Create Date Time attribute
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private DateTimeAttributeMetadata CreateDateTimeAttribute(CrmAttribute attr)
        {
            DateTimeFormat dateFormat = DateTimeFormat.DateOnly;
            //set the format type for DateTime based on the Format Type in the XML
            switch (attr.CrmFormatType)
            {
                case CrmAttributeFormatType.DateTime:
                    dateFormat = DateTimeFormat.DateAndTime;
                    break;

                case CrmAttributeFormatType.DateOnly:
                    dateFormat = DateTimeFormat.DateOnly;
                    break;
            }

            //set the date time behavior using a case, DateTimeBehavior is not an enum
            //DateTimeBehavior behavior = (DateTimeBehavior)Enum.ToObject(typeof(DateTimeBehavior), (int)attr.CrmDateTimeBehavior);
            DateTimeBehavior behavior = DateTimeBehavior.UserLocal;
            switch (attr.CrmDateTimeBehavior)
            {
                case CrmDateTimeBehavior.UserLocal:
                    behavior = DateTimeBehavior.UserLocal;
                    break;

                case CrmDateTimeBehavior.DateOnly:
                    //if behavior is date only, force the date format to be date only as well
                    behavior = DateTimeBehavior.DateOnly;
                    dateFormat = DateTimeFormat.DateOnly;
                    break;

                case CrmDateTimeBehavior.TimeZoneIndependent:
                    behavior = DateTimeBehavior.TimeZoneIndependent;
                    break;
            }

            DateTimeAttributeMetadata output = new DateTimeAttributeMetadata
            {
                LogicalName = attr.Name, //Name
                SchemaName = attr.SchemaName, //Schema Name
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attr.FieldRequirementLevel), //Field Requirement
                DisplayName = new Label(attr.DisplayName, attr.LanguageCode), //Display Name
                Description = new Label(attr.Description, attr.LanguageCode), //Description
                IsValidForAdvancedFind = new BooleanManagedProperty(attr.IsValidForAdvancedFind), //Searchable
                IsAuditEnabled = new BooleanManagedProperty(attr.IsAuditEnabled), //Auditing
                IsSecured = attr.IsSecured, //Field Security
                SourceType = (int)attr.CrmFieldType, //Field Type, either Simple or Calculated or Rollup
                ImeMode = attr.ImeModeLevel, //IME mode
                Format = dateFormat, //Format
                DateTimeBehavior = behavior, //Behavior
            };

            return output;
        }

        /// <summary>
        /// Create Image attribute, the schema name will is fixed to entityimage, only one image can be created per entity
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private ImageAttributeMetadata CreateImageAttribute(CrmAttribute attr)
        {
            ImageAttributeMetadata output = new ImageAttributeMetadata
            {
                LogicalName = attr.Name, //Name
                SchemaName = attr.SchemaName, //Name, fixed to EntityImage (this has been coded in the XmlHelper when loading the entity definition XML)
                RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None), //Field Requirement, fixed to Optional
                DisplayName = new Label(attr.DisplayName, attr.LanguageCode), //Display Name
                Description = new Label(attr.Description, attr.LanguageCode), //Description
                IsValidForAdvancedFind = new BooleanManagedProperty(false), //Searchable, fixed to false
                IsAuditEnabled = new BooleanManagedProperty(attr.IsAuditEnabled), //Auditing
                IsSecured = attr.IsSecured, //Field Security
            };

            return output;
        }

        /// <summary>
        /// Create Lookup attribute, this lookup attribute will be passed into a create relationship request
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private LookupAttributeMetadata CreateLookupAttribute(CrmAttribute attr)
        {
            LookupAttributeMetadata output = new LookupAttributeMetadata
            {
                LogicalName = attr.Name, //Name
                SchemaName = attr.SchemaName, //Schema Name
                RequiredLevel = new AttributeRequiredLevelManagedProperty(attr.FieldRequirementLevel), //Field Requirement
                DisplayName = new Label(attr.DisplayName, attr.LanguageCode), //Display Name
                Description = new Label(attr.Description, attr.LanguageCode), //Description
                IsValidForAdvancedFind = new BooleanManagedProperty(attr.IsValidForAdvancedFind), //Searchable
                IsAuditEnabled = new BooleanManagedProperty(attr.IsAuditEnabled), //Auditing
                IsSecured = attr.IsSecured, //Field Security
            };

            return output;
        }
        #endregion CreateAttribute
        #endregion CreateCrmAttributeMetadata

        #region CreateOneToManyRelationship
        /// <summary>
        /// Create one-to-many relationship metadata
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="ent"></param>
        /// <returns></returns>
        public OneToManyRelationshipMetadata CreateOneToManyRelationshipLookup(CrmEntity ent)
        {
            CrmAttribute attr = this;
            string customLabel = attr.LookupMenuCustomLabel;
            if (string.IsNullOrEmpty(customLabel))
                customLabel = string.Empty;

            OneToManyRelationshipMetadata output = new OneToManyRelationshipMetadata()
            {
                AssociatedMenuConfiguration = new AssociatedMenuConfiguration()
                {
                    Behavior = (AssociatedMenuBehavior)((int)attr.CrmLookupMenuBehavior), //associated menu behavior of the reference entity
                    Group = (AssociatedMenuGroup)((int)attr.CrmLookupMenuGroup), //associated menu group of the reference entity
                    Label = new Label(customLabel, attr.LanguageCode),
                    Order = attr.LookupMenuOrder,
                },
                CascadeConfiguration = new CascadeConfiguration()
                {
                    Assign = (CascadeType)((int)attr.CrmLookupRelationshipBehaviorAssign),
                    Share = (CascadeType)((int)attr.CrmLookupRelationshipBehaviorShare),
                    Unshare = (CascadeType)((int)attr.CrmLookupRelationshipBehaviorUnshare),
                    Reparent = (CascadeType)((int)attr.CrmLookupRelationshipBehaviorReparent),
                    Delete = (CascadeType)((int)attr.CrmLookupRelationshipBehaviorDelete),
                    Merge = (CascadeType)((int)attr.CrmLookupRelationshipBehaviorMerge),
                    RollupView = (CascadeType)((int)attr.CrmLookupRelationshipBehaviorRollupView),
                },
                IsValidForAdvancedFind = attr.IsValidForAdvancedFind, //Searchable
                ReferencedEntity = attr.LookupRefEntityName, //lookup reference entity logical name
                ReferencedAttribute = attr.LookupRefEntityPrimaryKey, //lookup reference primary key attribute name
                ReferencingEntity = ent.Name, //entity logical name of the referencing entity
                SchemaName = attr.LookupRelationshipName, //one-to-many relationship name
            };

            return output;
        }

        /// <summary>
        /// Create one-to-many relationship metadata array for Customer lookup
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="ent"></param>
        /// <returns></returns>
        public OneToManyRelationshipMetadata[] CreateOneToManyRelationshipCustomer(CrmEntity ent)
        {
            CrmAttribute attr = this;
            OneToManyRelationshipMetadata[] output = new OneToManyRelationshipMetadata[]
            {
                new OneToManyRelationshipMetadata()
                {
                    ReferencedEntity = "account", //lookup reference entity logical name
                    ReferencingEntity = ent.Name, //entity logical name of the referencing entity
                    SchemaName = attr.CustomerAccountRelationshipName, //one-to-many relationship name to account
                },
                new OneToManyRelationshipMetadata()
                {
                    ReferencedEntity = "contact", //lookup reference entity logical name
                    ReferencingEntity = ent.Name, //entity logical name of the referencing entity
                    SchemaName = attr.CustomerContactRelationshipName, //one-to-many relationship name to contact
                }
            };

            return output;
        }
        #endregion CreateOneToManyRelationship
    }
}