using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZD365DT.DeploymentTool.Utils.CrmMetadata;

namespace ZD365DT.DeploymentTool.Utils
{
    public static class ConfigXmlHelper
    {
        private const string CUSTOM_SCHEMANAME_FORMAT = "{0}_{1}";
        private const string SUFFIX_ID = "Id";

        #region LoadXmlDocument
        /// <summary>
        /// Load XML file to XmlDocument, output the NamespaceManager as well if available
        /// </summary>
        /// <param name="xmlFilePath"></param>
        /// <param name="nsmgr"></param>
        /// <param name="xmlNamespace"></param>
        /// <returns></returns>
        public static XmlDocument LoadXmlDoc(string xmlFilePath, out XmlNamespaceManager nsmgr, string xmlNamespace = null)
        {
            nsmgr = null;

            if (File.Exists(xmlFilePath))
            {
                //load xml file
                XmlDocument doc = null;
                doc = new XmlDocument();
                doc.Load(xmlFilePath);
                if (doc == null)
                    throw new Exception(string.Format("Unable to load the XML file '{0}' into a XmlDocument.", xmlFilePath));
                else if (!string.IsNullOrEmpty(xmlNamespace))
                {
                    nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace(ConfigXml.Namespace.NS_PREFIX, xmlNamespace);
                }
                return doc;
            }
            else
                throw new Exception(string.Format("The XML file '{0}' is not found.", xmlFilePath));
        }
        #endregion LoadXmlDocument

        #region GetXmlValue
        /// <summary>
        /// Get the value from the VALUE attribute in an element
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public static string GetXmlNodeAttributeValue(XmlDocument doc, string elementName, string defaultValue = null)
        {
            string result = defaultValue;

            if (doc != null)
            {
                XmlNode node = doc.GetElementsByTagName(elementName).Count > 0 ? doc.GetElementsByTagName(elementName)[0] : null;
                result = GetXmlNodeAttribute(node, ConfigXml.ATTR_VALUE, defaultValue);
            }

            return result;
        }

        /// <summary>
        /// Get the value from the VALUE attribute in an element
        /// </summary>
        /// <param name="xnode"></param>
        /// <param name="elementName"></param>
        /// <param name="nsmgr"></param>
        /// <returns></returns>
        public static string GetXmlNodeAttributeValue(XmlNode xnode, string elementName, XmlNamespaceManager nsmgr = null, string defaultValue = null)
        {
            string result = defaultValue;

            if (xnode != null)
            {
                XmlNode node = null;
                if (nsmgr != null)
                    node = xnode.SelectSingleNode(ConfigXml.Namespace.NS_PREFIX + ":" + elementName, nsmgr);
                else
                    node = xnode.SelectSingleNode(elementName);
                result = GetXmlNodeAttribute(node, ConfigXml.ATTR_VALUE, defaultValue);
            }

            return result;
        }

        /// <summary>
        /// Get the value from the NAME attribute in an element
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public static string GetXmlNodeAttributeName(XmlDocument doc, string elementName, string defaultValue = null)
        {
            string result = defaultValue;

            if (doc != null)
            {
                XmlNode node = doc.GetElementsByTagName(elementName).Count > 0 ? doc.GetElementsByTagName(elementName)[0] : null;
                result = GetXmlNodeAttribute(node, ConfigXml.ATTR_NAME, defaultValue);
            }

            return result;
        }

        /// <summary>
        /// Get the value from the NAME attribute in an element
        /// </summary>
        /// <param name="xnode"></param>
        /// <param name="elementName"></param>
        /// <param name="nsmgr"></param>
        /// <returns></returns>
        public static string GetXmlNodeAttributeName(XmlNode xnode, string elementName, XmlNamespaceManager nsmgr = null, string defaultValue = null)
        {
            string result = defaultValue;

            if (xnode != null)
            {
                XmlNode node = null;
                if (nsmgr != null)
                    node = xnode.SelectSingleNode(ConfigXml.Namespace.NS_PREFIX + ":" + elementName, nsmgr);
                else
                    node = xnode.SelectSingleNode(elementName);
                result = GetXmlNodeAttribute(node, ConfigXml.ATTR_NAME, defaultValue);
            }

            return result;
        }

        /// <summary>
        /// Get the value from the attribute passed in for an element
        /// </summary>
        /// <param name="xnode"></param>
        /// <param name="elementName"></param>
        /// <param name="attributeName"></param>
        /// <param name="nsmgr"></param>
        /// <returns></returns>
        public static string GetXmlNodeAttribute(XmlNode xnode, string elementName, string attributeName, XmlNamespaceManager nsmgr = null, string defaultValue = null)
        {
            string result = defaultValue;

            if (xnode != null)
            {
                XmlNode node = null;
                if (nsmgr != null)
                    node = xnode.SelectSingleNode(ConfigXml.Namespace.NS_PREFIX + ":" + elementName, nsmgr);
                else
                    node = xnode.SelectSingleNode(elementName);
                result = GetXmlNodeAttribute(node, attributeName, defaultValue);
            }

            return result;
        }

        /// <summary>
        /// Get the value from the attribute passed in for an element, return as boolean
        /// </summary>
        /// <param name="xnode"></param>
        /// <param name="elementName"></param>
        /// <param name="attributeName"></param>
        /// <param name="nsmgr"></param>
        /// <returns></returns>
        public static bool GetXmlNodeAttributeBool(XmlNode xnode, string elementName, string attributeName, XmlNamespaceManager nsmgr = null, bool defaultValue = false)
        {
            bool result = defaultValue;

            if (xnode != null)
            {
                XmlNode node = null;
                if (nsmgr != null)
                    node = xnode.SelectSingleNode(ConfigXml.Namespace.NS_PREFIX + ":" + elementName, nsmgr);
                else
                    node = xnode.SelectSingleNode(elementName);
                result = GetXmlNodeAttributeBool(node, attributeName, defaultValue);
            }

            return result;
        }

        /// <summary>
        /// Get the value from the attribute passed in for an element, return as int32
        /// </summary>
        /// <param name="xnode"></param>
        /// <param name="elementName"></param>
        /// <param name="attributeName"></param>
        /// <param name="nsmgr"></param>
        /// <returns></returns>
        public static int GetXmlNodeAttributeInt32(XmlNode xnode, string elementName, string attributeName, XmlNamespaceManager nsmgr = null, int defaultValue = 0)
        {
            int result = defaultValue;

            if (xnode != null)
            {
                XmlNode node = null;
                if (nsmgr != null)
                    node = xnode.SelectSingleNode(ConfigXml.Namespace.NS_PREFIX + ":" + elementName, nsmgr);
                else
                    node = xnode.SelectSingleNode(elementName);
                result = GetXmlNodeAttributeInt32(node, attributeName, defaultValue);
            }

            return result;
        }

        /// <summary>
        /// Get the value from the element (InnerText)
        /// </summary>
        /// <param name="xnode"></param>
        /// <param name="elementName"></param>
        /// <param name="nsmgr"></param>
        /// <returns></returns>
        public static string GetXmlNodeValue(XmlNode xnode, string elementName, XmlNamespaceManager nsmgr = null, string defaultValue = null)
        {
            string result = defaultValue;
            if (xnode != null)
            {
                XmlNode node = null;
                if (nsmgr != null)
                    node = xnode.SelectSingleNode(ConfigXml.Namespace.NS_PREFIX + ":" + elementName, nsmgr);
                else
                    node = xnode.SelectSingleNode(elementName);
                result = GetXmlNodeValue(node, defaultValue);
            }
            return result;
        }

        /// <summary>
        /// Get the value from the attribute name passed in from the node
        /// </summary>
        /// <param name="x">XmlNode containing the attribute</param>
        /// <param name="attributeName">Name of the Attribute</param>
        /// <returns></returns>
        public static string GetXmlNodeAttribute(XmlNode x, string attributeName, string defaultValue = null)
        {
            string result = defaultValue;
            if (x != null && x.Attributes[attributeName] != null && !string.IsNullOrEmpty(x.Attributes[attributeName].Value))
                result = x.Attributes[attributeName].Value;
            return result;
        }

        /// <summary>
        /// Get the value from the attribute name passed in from the node, return as boolean type
        /// </summary>
        /// <param name="x">XmlNode containing the attribute</param>
        /// <param name="attributeName">Name of the Attribute</param>
        /// <returns></returns>
        public static bool GetXmlNodeAttributeBool(XmlNode x, string attributeName, bool defaultValue = false)
        {
            bool result = defaultValue;
            if (x != null && x.Attributes[attributeName] != null && !string.IsNullOrEmpty(x.Attributes[attributeName].Value))
                result = Convert.ToBoolean(x.Attributes[attributeName].Value);
            return result;
        }

        /// <summary>
        /// Get the value from the attribute name passed in from the node, return as int32 type
        /// </summary>
        /// <param name="x">XmlNode containing the attribute</param>
        /// <param name="attributeName">Name of the Attribute</param>
        /// <returns></returns>
        public static int GetXmlNodeAttributeInt32(XmlNode x, string attributeName, int defaultValue = 0)
        {
            int result = defaultValue;
            if (x != null && x.Attributes[attributeName] != null && !string.IsNullOrEmpty(x.Attributes[attributeName].Value))
                result = Convert.ToInt32(x.Attributes[attributeName].Value);
            return result;
        }

        /// <summary>
        /// Get the value from the node
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string GetXmlNodeValue(XmlNode x, string defaultValue = null)
        {
            string result = defaultValue;

            if (x != null && !string.IsNullOrEmpty(x.Value))
                result = x.Value;
            else if (x != null && !string.IsNullOrEmpty(x.InnerText))
                result = x.InnerText;

            return result;
        }
        #endregion GetXmlValue

        #region GetXmlNodeList
        /// <summary>
        /// Get a list of XML Nodes for the element
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elementName"></param>
        /// <param name="nsmgr"></param>
        /// <returns></returns>
        public static XmlNodeList GetXmlNodeList(XmlDocument doc, string elementName, XmlNamespaceManager nsmgr = null)
        {
            if (nsmgr != null)
                return doc.SelectNodes("//" + ConfigXml.Namespace.NS_PREFIX + ":" + elementName, nsmgr);
            else
                return doc.SelectNodes("//" + elementName);
        }

        /// <summary>
        /// Get a list of XML Nodes for the element
        /// </summary>
        /// <param name="node"></param>
        /// <param name="elementName"></param>
        /// <param name="nsmgr"></param>
        /// <returns></returns>
        public static XmlNodeList GetXmlNodeList(XmlNode node, string elementName, XmlNamespaceManager nsmgr = null)
        {
            if (nsmgr != null)
                return node.SelectNodes(ConfigXml.Namespace.NS_PREFIX + ":" + elementName, nsmgr);
            else
                return node.SelectNodes(elementName);
        }

        /// <summary>
        /// Get a single XML Node for the element
        /// </summary>
        /// <param name="node"></param>
        /// <param name="elementName"></param>
        /// <param name="nsmgr"></param>
        /// <returns></returns>
        public static XmlNode GetXmlNode(XmlNode node, string elementName, XmlNamespaceManager nsmgr = null)
        {
            if (nsmgr != null)
                return node.SelectSingleNode(ConfigXml.Namespace.NS_PREFIX + ":" + elementName, nsmgr);
            else
                return node.SelectSingleNode(elementName);
        }
        #endregion GetXmlNodeList

        #region LoadGlobalOptionSetXML
        /// <summary>
        /// Load Global Optionset XML definition into an object
        /// </summary>
        /// <param name="xmlFilePath"></param>
        /// <param name="customizationPrefix">The customization prefix</param>
        /// <param name="languageCode">The language code number, should default to 1033 for English (US)</param>
        /// <param name="schemaLowerCase">Determine if the schema name is always set to lower case</param>
        /// <returns></returns>
        public static CrmGlobalOpCollection LoadGlobalOpXml(string xmlFilePath, string customizationPrefix, int languageCode, bool schemaLowerCase = true)
        {
            CrmGlobalOpCollection c = new CrmGlobalOpCollection();

            XmlNamespaceManager nsmgr = null;
            XmlDocument doc = ConfigXmlHelper.LoadXmlDoc(xmlFilePath, out nsmgr, ConfigXml.Namespace.GLOBALOP_XMLNS);
            XmlNodeList nodelist = ConfigXmlHelper.GetXmlNodeList(doc, ConfigXml.GlobalOp.NODE_NAME, nsmgr);

            foreach (XmlNode globalop in nodelist)
            {
                //check if this is a OOTB optionset
                bool isSystem = GetXmlNodeAttributeBool(globalop, ConfigXml.GlobalOp.Attribute.ISSYSTEM);

                CrmGlobalOp crmglobalop = new CrmGlobalOp()
                {
                    Name = isSystem == false ? string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, GetXmlNodeAttribute(globalop, ConfigXml.GlobalOp.Attribute.NAME)) : GetXmlNodeAttribute(globalop, ConfigXml.GlobalOp.Attribute.NAME),
                    DisplayName = GetXmlNodeAttribute(globalop, ConfigXml.GlobalOp.Attribute.DISPLAYNAME),
                    Description = GetXmlNodeValue(globalop, ConfigXml.GlobalOp.Element.DESCRIPTION, nsmgr, string.Empty),
                    IsSystem = isSystem,
                    LanguageCode = languageCode,
                    Options = new CrmOptionCollection(),
                };

                //set schema name to logical name
                crmglobalop.SchemaName = crmglobalop.Name;
                //always set logical name to lower case
                crmglobalop.Name = crmglobalop.Name.ToLower();
                //lower case the schema name if required
                if (schemaLowerCase)
                {
                    crmglobalop.SchemaName = crmglobalop.SchemaName.ToLower();
                }

                XmlNodeList optionslist = ConfigXmlHelper.GetXmlNodeList(GetXmlNode(globalop, ConfigXml.GlobalOp.Element.OPTIONS, nsmgr), ConfigXml.Options.NODE_NAME, nsmgr);
                foreach (XmlNode op in optionslist)
                {
                    CrmOption crmop = new CrmOption()
                    {
                        Value = GetXmlNodeAttributeInt32(op, ConfigXml.Options.Attribute.VALUE),
                        Label = GetXmlNodeAttribute(op, ConfigXml.Options.Attribute.LABEL),
                        Description = GetXmlNodeAttribute(op, ConfigXml.Options.Attribute.DESCRIPTION, string.Empty),
                        Color = GetXmlNodeAttribute(op, ConfigXml.Options.Attribute.COLOR),
                        Order = GetXmlNodeAttributeInt32(op, ConfigXml.Options.Attribute.ORDER),
                        LanguageCode = languageCode
                    };

                    crmglobalop.Options.Add(crmop);
                }

                c.Add(crmglobalop);
            }

            return c;
        }
        #endregion LoadGlobalOptionSetXML

        #region LoadEntityXML
        /// <summary>
        /// Load Entity XML definition into an object
        /// </summary>
        /// <param name="xmlFilePath"></param>
        /// <param name="customizationPrefix">The customization prefix</param>
        /// <param name="languageCode">The language code number, should default to 1033 for English (US)</param>
        /// <param name="schemaLowerCase">Determine if the schema name is always set to lower case</param>
        /// <param name="suffixLookupWithId">Determine if all attribute with Lookup/Customer data type will end with ID</param>
        /// <param name="suffixOptionSetWithId">Determine if all attribute with OptionSet data type will end with ID</param>
        /// <returns></returns>
        public static CrmEntityCollection LoadEntityXml(string xmlFilePath, string customizationPrefix, int languageCode, bool schemaLowerCase = true, bool suffixLookupWithId = true, bool suffixOptionSetWithId = true)
        {
            CrmEntityCollection c = new CrmEntityCollection();

            XmlNamespaceManager nsmgr = null;
            XmlDocument doc = ConfigXmlHelper.LoadXmlDoc(xmlFilePath, out nsmgr, ConfigXml.Namespace.ENTITIES_XMLNS);
            XmlNodeList nodelist = ConfigXmlHelper.GetXmlNodeList(doc, ConfigXml.Entity.NODE_NAME, nsmgr);

            foreach (XmlNode en in nodelist)
            {
                //check if this is a OOTB entity
                bool isSystem = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISSYSTEM);
                bool isPrimaryFieldSystem = isSystem;

                CrmEntity crmentity = new CrmEntity()
                {
                    IsSystem = isSystem,
                    LanguageCode = languageCode,
                    Name = isSystem == false ? string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.NAME)) : GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.NAME),
                    DisplayName = GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.DISPLAYNAME),
                    DisplayPluralName = GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.DISPLAYPLURALNAME),
                    Description = GetXmlNodeValue(en, ConfigXml.Entity.Element.DESCRIPTION, nsmgr, string.Empty),

                    Ownership = GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.OWNERSHIP),
                    IsActivityEntity = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISACTIVITYENTITY),
                    DisplayInActivityMenu = GetXmlNodeAttributeInt32(en, ConfigXml.Entity.Attribute.DISPLAYINACTIVITYMENU),
                    Color = !string.IsNullOrEmpty(GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.COLOR)) ? "#" + GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.COLOR) : null,

                    IsBusinessProcessEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISBUSINESSPROCESSENABLED),

                    IsNotesEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISNOTESENABLED),
                    IsConnectionsEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISCONNECTIONSENABLED),
                    IsActivitiesEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISACTIVITIESENABLED),

                    IsActivityPartyEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISACTIVITYPARTYENABLED),
                    IsMailMergeEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISMAILMERGEENABLED),
                    IsDocumentManagementEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISDOCUMENTMANAGEMENTENABLED),
                    IsAccessTeamsEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISACCESSTEAMSENABLED),
                    IsQueuesEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISQUEUESENABLED),
                    AutoRouteToOwnerQueue = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.AUTOROUTETOOWNERQUEUE),
                    IsKnowledgeManagementEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISKNOWLEDGEMANAGEMENTENABLED),

                    IsQuickCreateEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISQUICKCREATEENABLED),
                    IsDuplicateDetectionEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISDUPLICATEDETECTIONENABLED),
                    IsAuditEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISAUDITENABLED),
                    IsChangeTrackingEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISCHANGETRACKINGENABLED),

                    IsVisibleInPhoneExpress = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISVISIBLEINPHONEEXPRESS),
                    IsVisibleInMobileClient = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISVISIBLEINMOBILECLIENT),
                    ReadOnlyInMobileClient = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.READONLYINMOBILECLIENT),
                    IsAvailableOffline = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISAVAILABLEOFFLINE),

                    IsEntityHelpUrlEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISENTITYHELPURLENABLED),
                    EntityHelpUrl = GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.ENTITYHELPURL),

                    IsDoNotIncludeSubComponents = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Attribute.ISDONOTINCLUDESUBCOMPONENTS),

                    SmallIcon = GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.SMALLICON),
                    MediumIcon = GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.MEDIUMICON),
                    VectorIcon = GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.VECTORICON),

                    PrimaryField = new CrmPrimaryField()
                    {
                        LanguageCode = languageCode,
                        Name = isPrimaryFieldSystem == false ? string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, GetXmlNodeAttribute(en, ConfigXml.Entity.Element.PRIMARYFIELD, ConfigXml.PrimaryField.Attribute.NAME, nsmgr)) : GetXmlNodeAttribute(en, ConfigXml.Entity.Element.PRIMARYFIELD, ConfigXml.PrimaryField.Attribute.NAME, nsmgr),
                        DisplayName = GetXmlNodeAttribute(en, ConfigXml.Entity.Element.PRIMARYFIELD, ConfigXml.PrimaryField.Attribute.DISPLAYNAME, nsmgr),
                        Description = GetXmlNodeAttribute(en, ConfigXml.Entity.Element.PRIMARYFIELD, ConfigXml.PrimaryField.Attribute.DESCRIPTION, nsmgr, string.Empty),
                        Length = GetXmlNodeAttributeInt32(en, ConfigXml.Entity.Element.PRIMARYFIELD, ConfigXml.PrimaryField.Attribute.LENGTH, nsmgr),
                        FieldRequirement = GetXmlNodeAttribute(en, ConfigXml.Entity.Element.PRIMARYFIELD, ConfigXml.PrimaryField.Attribute.FIELDREQUIREMENT, nsmgr),
                        IsValidForAdvancedFind = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Element.PRIMARYFIELD, ConfigXml.PrimaryField.Attribute.ISVALIDFORADVANCEDFIND, nsmgr, true),
                        IsAuditEnabled = GetXmlNodeAttributeBool(en, ConfigXml.Entity.Element.PRIMARYFIELD, ConfigXml.PrimaryField.Attribute.ISAUDITENABLED, nsmgr, true),
                    },
                    Attributes = new CrmAttributeCollection(),
                };

                //check if the entity is declared as an activity, the primary field will always be created as system field with the name subject
                if (crmentity.IsActivityEntity)
                {
                    isPrimaryFieldSystem = true;
                    crmentity.PrimaryField.Name = "subject";
                }

                //validate entity schema name length
                if (crmentity.Name.Length > customizationPrefix.Length + 39)
                throw new Exception("The maximum length of the entity name is 39.");

                //set schema name to logical name
                crmentity.SchemaName = crmentity.Name;
                crmentity.PrimaryField.SchemaName = crmentity.PrimaryField.Name;
                //always set logical name to lower case
                crmentity.Name = crmentity.Name.ToLower();
                crmentity.PrimaryField.Name = crmentity.PrimaryField.Name.ToLower();
                //lower case the schema name if required
                if (schemaLowerCase)
                {
                    crmentity.SchemaName = crmentity.SchemaName.ToLower();
                    crmentity.PrimaryField.SchemaName = crmentity.PrimaryField.SchemaName.ToLower();
                }

                //always add the primary field as the first attribute of the entity
                CrmAttribute primaryField = new CrmAttribute()
                {
                    IsPrimaryField = true,
                    IsSystem = isPrimaryFieldSystem,
                    LanguageCode = crmentity.PrimaryField.LanguageCode,
                    Name = crmentity.PrimaryField.Name,
                    DisplayName = crmentity.PrimaryField.DisplayName,
                    Description = crmentity.PrimaryField.Description,
                    DataType = "SingleText",
                    MaxLength = crmentity.PrimaryField.Length,
                    FieldRequirement = crmentity.PrimaryField.FieldRequirement,
                    IsValidForAdvancedFind = crmentity.PrimaryField.IsValidForAdvancedFind,
                    IsAuditEnabled = crmentity.PrimaryField.IsAuditEnabled,
                };
                //set schema name to logical name
                primaryField.SchemaName = primaryField.Name;
                //always set logical name to lower case
                primaryField.Name = primaryField.Name.ToLower();
                //lower case the schema name if required and when it is not a system entity
                if (schemaLowerCase && !isSystem)
                {
                    primaryField.SchemaName = primaryField.SchemaName.ToLower();
                }
                crmentity.Attributes.Add(primaryField);

                XmlNodeList attributeslist = ConfigXmlHelper.GetXmlNodeList(GetXmlNode(en, ConfigXml.Entity.Element.ATTRIBUTES, nsmgr), ConfigXml.Attrs.NODE_NAME, nsmgr);
                foreach (XmlNode atr in attributeslist)
                {
                    bool isSystemAtr = GetXmlNodeAttributeBool(atr, ConfigXml.Attrs.Attribute.ISSYSTEM);
                    bool isSystemGlobalOp = GetXmlNodeAttributeBool(atr, ConfigXml.Attrs.Attribute.ISSYSTEMGLOBALOPTIONSET);
                    bool isSystemRefEntity = GetXmlNodeAttributeBool(atr, ConfigXml.Attrs.Attribute.ISSYSTEMREFERENCEENTITY);
                    string originalName = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.NAME);

                    CrmAttribute crmatr = new CrmAttribute()
                    {
                        IsSystem = isSystemAtr,
                        LanguageCode = languageCode,
                        Name = isSystemAtr == false ? string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, originalName) : originalName,
                        DisplayName = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.DISPLAYNAME),
                        Description = GetXmlNodeValue(atr, ConfigXml.Attrs.Element.DESCRIPTION, nsmgr, string.Empty),

                        FieldRequirement = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.FIELDREQUIREMENT),
                        IsValidForAdvancedFind = GetXmlNodeAttributeBool(atr, ConfigXml.Attrs.Attribute.ISVALIDFORADVANCEDFIND, true),
                        IsAuditEnabled = GetXmlNodeAttributeBool(atr, ConfigXml.Attrs.Attribute.ISAUDITENABLED, true),
                        IsSecured = GetXmlNodeAttributeBool(atr, ConfigXml.Attrs.Attribute.ISSECURED),

                        DataType = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.DATATYPE),
                        FieldType = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.FIELDTYPE),
                        FormatType = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.FORMATTYPE),
                        MaxLength = GetXmlNodeAttributeInt32(atr, ConfigXml.Attrs.Attribute.LENGTH, 100),
                        MinValue = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.MINVALUE, "NULL"),
                        MaxValue = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.MAXVALUE, "NULL"),

                        TwoOptionsDefault = GetXmlNodeAttributeBool(atr, ConfigXml.Attrs.Attribute.TWOOPTIONSDEFAULT),
                        TrueLabel = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.TRUELABEL, "Yes"),
                        TrueColor = !string.IsNullOrEmpty(GetXmlNodeAttribute(en, ConfigXml.Attrs.Attribute.TRUECOLOR)) ? "#" + GetXmlNodeAttribute(en, ConfigXml.Attrs.Attribute.TRUECOLOR) : null,
                        FalseLabel = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.FALSELABEL, "No"),
                        FalseColor = !string.IsNullOrEmpty(GetXmlNodeAttribute(en, ConfigXml.Attrs.Attribute.FALSECOLOR)) ? "#" + GetXmlNodeAttribute(en, ConfigXml.Attrs.Attribute.FALSECOLOR) : null,

                        OptionSetDefault = GetXmlNodeAttributeInt32(atr, ConfigXml.Attrs.Attribute.OPTIONSETDEFAULT, -1) == -1 ? (int?)null : GetXmlNodeAttributeInt32(atr, ConfigXml.Attrs.Attribute.OPTIONSETDEFAULT),
                        GlobalOptionSetName = isSystemGlobalOp == false ? (GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.GLOBALOPTIONSETNAME) != null ? string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.GLOBALOPTIONSETNAME)) : null) : GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.GLOBALOPTIONSETNAME),
                        IsSystemGlobalOptionSet = isSystemGlobalOp,

                        PrecisionSource = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.PRECISIONSOURCE),
                        Precision = GetXmlNodeAttributeInt32(atr, ConfigXml.Attrs.Attribute.PRECISION),

                        DateTimeBehaviorValue = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.DATETIMEBEHAVIOR),

                        LookupRefEntityName = isSystemRefEntity == false ? (GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPREFERENCEENTITYNAME) != null ? string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPREFERENCEENTITYNAME)) : null) : GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPREFERENCEENTITYNAME),
                        IsSystemRefEntity = isSystemRefEntity,
                        LookupRelationshipName = isSystemAtr == false ? (GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPRELATIONSHIPNAME) != null ? string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPRELATIONSHIPNAME)) : null) : GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPRELATIONSHIPNAME),
                        LookupMenuOrder = GetXmlNodeAttributeInt32(atr, ConfigXml.Attrs.Attribute.LOOKUPMENUORDER, 10000),
                        LookupMenuGroup = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPMENUGROUP),
                        LookupMenuBehavior = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPMENUBEHAVIOR),
                        LookupMenuCustomLabel = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPMENUCUSTOMLABEL),
                        LookupRelationshipBehaviorAssign = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPRELATIONSHIPBEHAVIORASSIGN),
                        LookupRelationshipBehaviorShare = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPRELATIONSHIPBEHAVIORSHARE),
                        LookupRelationshipBehaviorUnshare = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPRELATIONSHIPBEHAVIORUNSHARE),
                        LookupRelationshipBehaviorReparent = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPRELATIONSHIPBEHAVIORREPARENT),
                        LookupRelationshipBehaviorDelete = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPRELATIONSHIPBEHAVIORDELETE),
                        LookupRelationshipBehaviorRollupView = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.LOOKUPRELATIONSHIPBEHAVIORROLLUPVIEW),

                        ImeMode = GetXmlNodeAttribute(atr, ConfigXml.Attrs.Attribute.IMEMODE),
                    };
                    
                    //set the default min and max value based on data type, it will be CRM default values
                    if (crmatr.MinValue == "NULL" || crmatr.MaxValue == "NULL")
                    {
                        switch (crmatr.CrmDataType)
                        {
                            case CrmAttributeDataType.Currency:
                                if (crmatr.MinValue == "NULL")
                                    crmatr.MinValue = "-922337203685477";
                                if (crmatr.MaxValue == "NULL")
                                    crmatr.MaxValue = "922337203685477";
                                break;

                            case CrmAttributeDataType.Float:
                            case CrmAttributeDataType.Decimal:
                                if (crmatr.MinValue == "NULL")
                                    crmatr.MinValue = "-100000000000";
                                if (crmatr.MaxValue == "NULL")
                                    crmatr.MaxValue = "100000000000";
                                break;

                            case CrmAttributeDataType.Int32:
                                if (crmatr.MinValue == "NULL")
                                    crmatr.MinValue = "-2147483648";
                                if (crmatr.MaxValue == "NULL")
                                    crmatr.MaxValue = "2147483647";
                                break;

                            default:
                                if (crmatr.MinValue == "NULL")
                                    crmatr.MinValue = "-2147483648";
                                if (crmatr.MaxValue == "NULL")
                                    crmatr.MaxValue = "2147483647";
                                break;
                        }
                    }

                    //if the attribute is optionset/lookup/customer data type, ensure the name always ends with id unless it is a system attribute
                    if (suffixLookupWithId && (crmatr.CrmDataType == CrmAttributeDataType.Lookup || crmatr.CrmDataType == CrmAttributeDataType.Customer) && !isSystemAtr)
                    {
                        if (!crmatr.Name.ToLower().EndsWith(SUFFIX_ID.ToLower()))
                            crmatr.Name += SUFFIX_ID;
                    }

                    //if the attribute is optionset data type, ensure the name always ends with id unless it is a system attribute
                    if (suffixOptionSetWithId && (crmatr.CrmDataType == CrmAttributeDataType.OptionSet || crmatr.CrmDataType == CrmAttributeDataType.MultiOptionSet) && !isSystemAtr)
                    {
                        if (!crmatr.Name.ToLower().EndsWith(SUFFIX_ID.ToLower()))
                            crmatr.Name += SUFFIX_ID;
                    }

                    //auto-generate the lookup relationship name for custom columns when no value is provided
                    if (crmatr.CrmDataType == CrmAttributeDataType.Lookup && string.IsNullOrEmpty(crmatr.LookupRelationshipName) && !isSystemAtr)
                    {
                        //auto-generate using the current entity name and the reference entity name with the attribute name, need to be shorter than 82
                        string relName = $"{crmatr.LookupRefEntityName}_{crmentity.SchemaName}_{originalName}";
                        
                        //append ID if required
                        if (suffixLookupWithId && !relName.EndsWith(SUFFIX_ID, StringComparison.CurrentCultureIgnoreCase))
                            relName += SUFFIX_ID;

                        //check for length
                        if (relName.Length > 82)
                        {
                            //need a shorter name
                            int extraChar = relName.Length - 82 + 2;
                            int cutBy = (int)Math.Floor((decimal)(extraChar / 2));

                            //cut the table schema names
                            string currentTable = crmentity.SchemaName.Substring(0, crmentity.SchemaName.Length - cutBy);
                            string refTable = crmatr.LookupRefEntityName.Substring(0, crmatr.LookupRefEntityName.Length - cutBy);

                            relName = $"{refTable}_{currentTable}_{originalName}";

                            //append ID if required
                            if (suffixLookupWithId && !relName.EndsWith(SUFFIX_ID, StringComparison.CurrentCultureIgnoreCase))
                                relName += SUFFIX_ID;
                        }

                        //auto-generated name
                        crmatr.LookupRelationshipName = string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, relName);
                    }

                    //if attribute is lookup data type, ensure the relationship name always ends with id unless it is a system attribute
                    if (crmatr.CrmDataType == CrmAttributeDataType.Lookup && suffixLookupWithId && !crmatr.LookupRelationshipName.ToLower().EndsWith(SUFFIX_ID.ToLower()) && !isSystemAtr)
                        crmatr.LookupRelationshipName += SUFFIX_ID;

                    //generate the customer account/contact relationship name unless it is a system attribute
                    if (crmatr.CrmDataType == CrmAttributeDataType.Customer)
                    {
                        if (suffixLookupWithId && !isSystemAtr)
                        {
                            crmatr.CustomerAccountRelationshipName = string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.NAME) + "_Account_" + originalName + SUFFIX_ID);
                            crmatr.CustomerContactRelationshipName = string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.NAME) + "_Contact_" + originalName + SUFFIX_ID);
                        }
                        else
                        {
                            crmatr.CustomerAccountRelationshipName = string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.NAME) + "_Account_" + originalName);
                            crmatr.CustomerContactRelationshipName = string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, GetXmlNodeAttribute(en, ConfigXml.Entity.Attribute.NAME) + "_Contact_" + originalName);
                        }
                    }

                    //if the attribute is an image data type, always replace the schema name to EntityImage
                    if (crmatr.CrmDataType == CrmAttributeDataType.Image)
                    {
                        crmatr.Name = "EntityImage";
                    }

                    //validate attribute schema name and lookup relationship name length
                    if (crmatr.Name.Length > customizationPrefix.Length + 41)
                        throw new Exception("The maximum length of the attribute name is 41.");
                    if (!string.IsNullOrEmpty(crmatr.LookupRelationshipName) && crmatr.LookupRelationshipName.Length > customizationPrefix.Length + 82)
                        throw new Exception("The maximum length of the attribute lookup relationship name is 82.");

                    //set schema name to logical name
                    crmatr.SchemaName = crmatr.Name;
                    //always set logical name to lower case
                    crmatr.Name = crmatr.Name.ToLower();
                    if (!string.IsNullOrEmpty(crmatr.LookupRefEntityName))
                        crmatr.LookupRefEntityName = crmatr.LookupRefEntityName.ToLower();

                    //lower case the lookup relationship name if required
                    if (schemaLowerCase && !string.IsNullOrEmpty(crmatr.LookupRelationshipName))
                        crmatr.LookupRelationshipName = crmatr.LookupRelationshipName.ToLower();

                    //always set customer relationship name to lower case
                    if (!string.IsNullOrEmpty(crmatr.CustomerAccountRelationshipName))
                        crmatr.CustomerAccountRelationshipName = crmatr.CustomerAccountRelationshipName.ToLower();
                    if (!string.IsNullOrEmpty(crmatr.CustomerContactRelationshipName))
                        crmatr.CustomerContactRelationshipName = crmatr.CustomerContactRelationshipName.ToLower();

                    //lower case the schema name if required and when it is not a system attribute
                    if (schemaLowerCase && !isSystemAtr)
                    {
                        crmatr.SchemaName = crmatr.SchemaName.ToLower();
                    }

                    //lower case the global optionset name as well when lower case schema name is in effect and not a system global optionset
                    if (schemaLowerCase && !isSystemGlobalOp && crmatr.IsGlobalOptionSet)
                    {
                        crmatr.GlobalOptionSetName = crmatr.GlobalOptionSetName.ToLower();
                    }

                    //if the attribute is optionset data type and it is a local optionset, retrieve the local options
                    if ((crmatr.CrmDataType == CrmAttributeDataType.OptionSet || crmatr.CrmDataType == CrmAttributeDataType.MultiOptionSet) && !crmatr.IsGlobalOptionSet)
                    {
                        crmatr.Options = new CrmOptionCollection();
                        XmlNodeList optionslist = ConfigXmlHelper.GetXmlNodeList(GetXmlNode(atr, ConfigXml.Attrs.Element.OPTIONS, nsmgr), ConfigXml.Options.NODE_NAME, nsmgr);
                        foreach (XmlNode op in optionslist)
                        {
                            CrmOption crmop = new CrmOption()
                            {
                                Value = GetXmlNodeAttributeInt32(op, ConfigXml.Options.Attribute.VALUE),
                                Label = GetXmlNodeAttribute(op, ConfigXml.Options.Attribute.LABEL),
                                Description = GetXmlNodeAttribute(op, ConfigXml.Options.Attribute.DESCRIPTION, string.Empty),
                                Color = GetXmlNodeAttribute(op, ConfigXml.Options.Attribute.COLOR),
                                Order = GetXmlNodeAttributeInt32(op, ConfigXml.Options.Attribute.ORDER),
                                LanguageCode = languageCode
                            };

                            crmatr.Options.Add(crmop);
                        }
                    }

                    crmentity.Attributes.Add(crmatr);
                }

                c.Add(crmentity);
            }

            return c;
        }
        #endregion LoadEntityXML

        #region LoadNtoNRelationXML
        /// <summary>
        /// Load NtoN Relation XML definition into an object
        /// </summary>
        /// <param name="xmlFilePath"></param>
        /// <param name="customizationPrefix">The customization prefix</param>
        /// <param name="languageCode">The language code number, should default to 1033 for English (US)</param>
        /// <param name="schemaLowerCase">Determine if the schema name is always set to lower case</param>
        /// <returns></returns>
        public static CrmManyToManyRelationCollection LoadNtoNRelationXml(string xmlFilePath, string customizationPrefix, int languageCode, bool schemaLowerCase = true)
        {
            CrmManyToManyRelationCollection c = new CrmManyToManyRelationCollection();

            XmlNamespaceManager nsmgr = null;
            XmlDocument doc = ConfigXmlHelper.LoadXmlDoc(xmlFilePath, out nsmgr, ConfigXml.Namespace.NTONRELATIONS_XMLNS);
            XmlNodeList nodelist = ConfigXmlHelper.GetXmlNodeList(doc, ConfigXml.NtoNRelation.NODE_NAME, nsmgr);

            foreach (XmlNode rel in nodelist)
            {
                //get the entity1 and entity2 node
                XmlNode en1 = GetXmlNode(rel, ConfigXml.NtoNRelation.Element.ENTITY1, nsmgr);
                XmlNode en2 = GetXmlNode(rel, ConfigXml.NtoNRelation.Element.ENTITY2, nsmgr);

                //check if this is a OOTB relationship
                bool isSystem = GetXmlNodeAttributeBool(rel, ConfigXml.NtoNRelation.Attribute.ISSYSTEM);
                bool isEntity1System = GetXmlNodeAttributeBool(en1, ConfigXml.RelationEntity.Attribute.ISSYSTEM);
                bool isEntity2System = GetXmlNodeAttributeBool(en2, ConfigXml.RelationEntity.Attribute.ISSYSTEM);

                CrmManyToManyRelation crmrelation = new CrmManyToManyRelation()
                {
                    IsSystem = isSystem,
                    Name = isSystem == false ? string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, GetXmlNodeAttribute(rel, ConfigXml.NtoNRelation.Attribute.NAME)) : GetXmlNodeAttribute(rel, ConfigXml.NtoNRelation.Attribute.NAME),
                    IsValidForAdvancedFind = GetXmlNodeAttributeBool(rel, ConfigXml.NtoNRelation.Attribute.ISVALIDFORADVANCEDFIND, true),
                    Entity1 = new CrmEntityRelation()
                    {
                        IsSystem = isEntity1System,
                        LanguageCode = languageCode,
                        Name = isEntity1System == false ? (GetXmlNodeAttribute(en1, ConfigXml.RelationEntity.Attribute.NAME) != null ? string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, GetXmlNodeAttribute(en1, ConfigXml.RelationEntity.Attribute.NAME)) : null) : GetXmlNodeAttribute(en1, ConfigXml.RelationEntity.Attribute.NAME),
                        MenuOrder = GetXmlNodeAttributeInt32(en1, ConfigXml.RelationEntity.Attribute.MENUORDER, 10000),
                        MenuGroup = GetXmlNodeAttribute(en1, ConfigXml.RelationEntity.Attribute.MENUGROUP),
                        MenuBehavior = GetXmlNodeAttribute(en1, ConfigXml.RelationEntity.Attribute.MENUBEHAVIOR),
                        CustomLabel = GetXmlNodeAttribute(en1, ConfigXml.RelationEntity.Attribute.MENUBEHAVIOR),
                    },
                    Entity2 = new CrmEntityRelation()
                    {
                        IsSystem = isEntity2System,
                        LanguageCode = languageCode,
                        Name = isEntity2System == false ? (GetXmlNodeAttribute(en2, ConfigXml.RelationEntity.Attribute.NAME) != null ? string.Format(CUSTOM_SCHEMANAME_FORMAT, customizationPrefix, GetXmlNodeAttribute(en2, ConfigXml.RelationEntity.Attribute.NAME)) : null) : GetXmlNodeAttribute(en2, ConfigXml.RelationEntity.Attribute.NAME),
                        MenuOrder = GetXmlNodeAttributeInt32(en2, ConfigXml.RelationEntity.Attribute.MENUORDER, 10000),
                        MenuGroup = GetXmlNodeAttribute(en2, ConfigXml.RelationEntity.Attribute.MENUGROUP),
                        MenuBehavior = GetXmlNodeAttribute(en2, ConfigXml.RelationEntity.Attribute.MENUBEHAVIOR),
                        CustomLabel = GetXmlNodeAttribute(en2, ConfigXml.RelationEntity.Attribute.MENUBEHAVIOR),
                    },
                };

                //validate attribute schema name and lookup relationship name length
                if (crmrelation.Name.Length > customizationPrefix.Length + 41)
                    throw new Exception("The maximum length of the N:N relationship name is 41.");

                //set schema name to logical name
                crmrelation.SchemaName = crmrelation.Name;
                //always set logical name to lower case
                crmrelation.Name = crmrelation.Name.ToLower();
                crmrelation.Entity1.Name = crmrelation.Entity1.Name.ToLower();
                crmrelation.Entity2.Name = crmrelation.Entity2.Name.ToLower();
                //lower case the schema name if required
                if (schemaLowerCase)
                {
                    crmrelation.SchemaName = crmrelation.SchemaName.ToLower();
                }

                c.Add(crmrelation);
            }

            return c;
        }
        #endregion LoadNtoNRelationXML
    }
}