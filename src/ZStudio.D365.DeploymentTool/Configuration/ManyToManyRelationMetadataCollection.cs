using System;
using System.Configuration;
using System.Xml;
using ZD365DT.DeploymentTool.Utils;

namespace ZD365DT.DeploymentTool.Configuration
{
    /// <summary>
    /// The collection class that will store the list of N:N Relation Definitions to be created/updated.
    /// </summary>
    [ConfigurationCollection(typeof(MetadataElement))]
    public class ManyToManyRelationMetadataCollection : ConfigurationElementCollection
    {
        public ManyToManyRelationMetadataCollection() : base() { }

        public ManyToManyRelationMetadataCollection(string xmlFilePath)
        {
            //parse a configuration XML file
            XmlNamespaceManager nsmgr = null;
            XmlDocument doc = ConfigXmlHelper.LoadXmlDoc(xmlFilePath, out nsmgr, ConfigXml.Namespace.METADATA_XMLNS);
            XmlNodeList nodelist = ConfigXmlHelper.GetXmlNodeList(doc, ConfigXml.Metadata.NODE_NAME, nsmgr);

            foreach (XmlNode x in nodelist)
            {
                MetadataElement e = new MetadataElement();
                e.MetadataFile = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Metadata.Element.METADATAFILE, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Metadata.Element.METADATAFILE, nsmgr) : null;
                e.CustomizationPrefix = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Metadata.Element.CUSTOMIZATIONPREFIX, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Metadata.Element.CUSTOMIZATIONPREFIX, nsmgr) : null;
                e.SolutionName = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Metadata.Element.SOLUTIONNAME, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Metadata.Element.SOLUTIONNAME, nsmgr) : null;
                e.Publish = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Metadata.Element.PUBLISH, nsmgr) != null ? Convert.ToBoolean(ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Metadata.Element.PUBLISH, nsmgr)) : false;
                e.LanguageCode = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Metadata.Element.LANGUAGECODE, nsmgr) != null ? Convert.ToInt32(ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Metadata.Element.LANGUAGECODE, nsmgr)) : 1033;
                base.BaseAdd(e);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new MetadataElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MetadataElement)element).MetadataFile;
        }

        protected override string ElementName
        {
            get { return "metadata"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public MetadataElement this[int index]
        {
            get { return (MetadataElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        [ConfigurationProperty("continueOnError", IsRequired = true, DefaultValue = true)]
        public bool ContinueOnError
        {
            get { return (bool)this["continueOnError"]; }
            set { this["continueOnError"] = value; }
        }

        [ConfigurationProperty("metadataconfig")]
        public string MetadataConfig
        {
            get { return (string)this["metadataconfig"]; }
            set { this["metadataconfig"] = value; }
        }

        [ConfigurationProperty("schemaLowerCase", IsRequired = false, DefaultValue = true)]
        public bool SchemaLowerCase
        {
            get { return (bool)this["schemaLowerCase"]; }
            set { this["schemaLowerCase"] = value; }
        }
    }
}