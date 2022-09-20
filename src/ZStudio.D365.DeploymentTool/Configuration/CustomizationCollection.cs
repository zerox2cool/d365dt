using System;
using System.Configuration;
using System.Xml;
using ZD365DT.DeploymentTool.Utils;

namespace ZD365DT.DeploymentTool.Configuration
{
    public enum PublishType
    {
        Separately,
        Batch,
        None
    }

    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(CustomizationElement))]
    public class CustomizationCollection : ConfigurationElementCollection
    {
        public CustomizationCollection() : base() { }

        public CustomizationCollection(string xmlFilePath)
        {
            //parse a configuration XML file
            XmlNamespaceManager nsmgr = null;
            XmlDocument doc = ConfigXmlHelper.LoadXmlDoc(xmlFilePath, out nsmgr, ConfigXml.Namespace.CUSTOMIZATION_XMLNS);
            XmlNodeList nodelist = ConfigXmlHelper.GetXmlNodeList(doc, ConfigXml.Customization.NODE_NAME, nsmgr);

            foreach (XmlNode x in nodelist)
            {
                CustomizationElement e = new CustomizationElement();
                e.CustomizationFile = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.FILE, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.FILE, nsmgr) : null;
                e.SolutionName = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.SOLUTIONNAME, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.SOLUTIONNAME, nsmgr) : null;
                e.IsManagedText = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.ISMANAGED, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.ISMANAGED, nsmgr) : null;
                e.Action = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.ACTION, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.ACTION, nsmgr) : null;
                e.DeactivateWorkflow = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.DEACTIVATEWORKFLOW, nsmgr) != null ? Convert.ToBoolean(ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.DEACTIVATEWORKFLOW, nsmgr)) : true;
                e.PublishBeforeExport = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.PUBLISHBEFOREEXPORT, nsmgr) != null ? Convert.ToBoolean(ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.PUBLISHBEFOREEXPORT, nsmgr)) : true;
                e.BackupBeforeImport = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.BACKUPBEFOREIMPORT, nsmgr) != null ? Convert.ToBoolean(ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.BACKUPBEFOREIMPORT, nsmgr)) : true;
                e.WaitTimeout = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.WAITTIMEOUT, nsmgr) != null ? Convert.ToInt32(ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.WAITTIMEOUT, nsmgr)) : 1800;
                e.ExportRetryTimeout = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.EXPORTRETRYTIMEOUT, nsmgr) != null ? Convert.ToInt32(ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Customization.Element.EXPORTRETRYTIMEOUT, nsmgr)) : 600;

                base.BaseAdd(e);
            }
        }

        [ConfigurationProperty("customizationconfig", IsRequired = false)]
        public string CustomizationConfig
        {
            get { return (string)base["customizationconfig"]; }
            set { base["customizationconfig"] = value; }
        }

        [ConfigurationProperty("publishWorkflows", DefaultValue = false, IsRequired = false)]
        public bool PublishWorkflows
        {
            get { return (bool)base["publishWorkflows"]; }
            set { base["publishWorkflows"] = value; }
        }

        [ConfigurationProperty("publishType", DefaultValue = PublishType.Batch, IsRequired = false)]
        public PublishType PublishType
        {
            get { return (PublishType)base["publishType"]; }
            set { base["publishType"] = value; }
        }

        [ConfigurationProperty("workflowlist", IsRequired = false, DefaultValue = "")]
        public string WorkflowList
        {
            get { return (string)base["workflowlist"]; }
            set { base["workflowlist"] = value; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CustomizationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CustomizationElement)element).CustomizationFile;
        }

        protected override string ElementName
        {
            get { return "customization"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public CustomizationElement this[int index]
        {
            get { return (CustomizationElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }
    }


    /// <summary>
    /// The class that holds onto each element returned by the configuration manager.
    /// </summary>
    public class CustomizationElement : ConfigurationElement
    {
        [ConfigurationProperty("file", IsKey = true, IsRequired = true)]
        public string CustomizationFile
        {
            get { return (string)base["file"]; }
            set { base["file"] = value; }
        }

        [ConfigurationProperty("solutionName", IsRequired = true)]
        public string SolutionName
        {
            get { return (string)base["solutionName"]; }
            set { base["solutionName"] = value; }
        }

        [ConfigurationProperty("action", IsRequired = true)]
        public string Action
        {
            get { return (string)base["action"]; }
            set { base["action"] = value; }
        }

        [ConfigurationProperty("deactivateworkflow", IsRequired = true, DefaultValue = true)]
        public bool DeactivateWorkflow
        {
            get { return (bool)base["deactivateworkflow"]; }
            set { base["deactivateworkflow"] = value; }
        }

        [ConfigurationProperty("publishBeforeExport", IsRequired = false, DefaultValue = true)]
        public bool PublishBeforeExport
        {
            get { return (bool)base["publishBeforeExport"]; }
            set { base["publishBeforeExport"] = value; }
        }

        [ConfigurationProperty("backupBeforeImport", IsRequired = false, DefaultValue = true)]
        public bool BackupBeforeImport
        {
            get { return (bool)base["backupBeforeImport"]; }
            set { base["backupBeforeImport"] = value; }
        }

        [ConfigurationProperty("waitTimeout", IsRequired = false, DefaultValue = 1800)]
        public int WaitTimeout
        {
            get { return (int)base["waitTimeout"]; }
            set { base["waitTimeout"] = value; }
        }

        [ConfigurationProperty("exportRetryTimeout", IsRequired = false, DefaultValue = 600)]
        public int ExportRetryTimeout
        {
            get { return (int)base["exportRetryTimeout"]; }
            set { base["exportRetryTimeout"] = value; }
        }

        public bool IsManaged
        {
            get;
            set;
        }

        [ConfigurationProperty("ismanaged", IsRequired = true)]
        public string IsManagedText
        {
            get { return (string)base["ismanaged"]; }
            set { base["ismanaged"] = value; }
        }
    }
}