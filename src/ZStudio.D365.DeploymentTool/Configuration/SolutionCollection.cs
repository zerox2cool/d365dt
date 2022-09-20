using System;
using System.Configuration;
using System.Xml;
using ZD365DT.DeploymentTool.Utils;

namespace ZD365DT.DeploymentTool.Configuration
{
    /// <summary>
    /// The collection class that will store the list of Solutions to be created/updated.
    /// </summary>
    [ConfigurationCollection(typeof(SolutionElement))]
    public class SolutionCollection : ConfigurationElementCollection
    {
        public SolutionCollection() : base() { }

        public SolutionCollection(string xmlFilePath)
        {
            //parse a configuration XML file
            XmlNamespaceManager nsmgr = null;
            XmlDocument doc = ConfigXmlHelper.LoadXmlDoc(xmlFilePath, out nsmgr, ConfigXml.Namespace.SOLUTION_XMLNS);
            XmlNodeList nodelist = ConfigXmlHelper.GetXmlNodeList(doc, ConfigXml.Solution.NODE_NAME, nsmgr);

            foreach (XmlNode x in nodelist)
            {
                SolutionElement e = new SolutionElement();
                e.Name = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Solution.Element.NAME, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Solution.Element.NAME, nsmgr) : null;
                e.DisplayName = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Solution.Element.DISPLAYNAME, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Solution.Element.DISPLAYNAME, nsmgr) : null;
                e.Description = ConfigXmlHelper.GetXmlNodeValue(x, ConfigXml.Solution.Element.DESCRIPTION, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeValue(x, ConfigXml.Solution.Element.DESCRIPTION, nsmgr) : null;
                e.PublisherUniqueName = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Solution.Element.PUBLISHERNAME, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Solution.Element.PUBLISHERNAME, nsmgr) : null;
                e.Version = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Solution.Element.VERSION, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Solution.Element.VERSION, nsmgr) : null;
                base.BaseAdd(e);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SolutionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SolutionElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "solution"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public SolutionElement this[int index]
        {
            get { return (SolutionElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        [ConfigurationProperty("solutionconfig")]
        public string SolutionConfig
        {
            get { return (string)this["solutionconfig"]; }
            set { this["solutionconfig"] = value; }
        }
    }

    /// <summary>
    /// The class that holds the solution record to be created/updated.
    /// </summary>
    public class SolutionElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("displayname", IsRequired = true)]
        public string DisplayName
        {
            get { return (string)base["displayname"]; }
            set { base["displayname"] = value; }
        }

        [ConfigurationProperty("publisheruniquename", IsRequired = true)]
        public string PublisherUniqueName
        {
            get { return (string)base["publisheruniquename"]; }
            set { base["publisheruniquename"] = value; }
        }

        [ConfigurationProperty("version", IsRequired = false)]
        public string Version
        {
            get { return (string)base["version"]; }
            set { base["version"] = value; }
        }

        [ConfigurationProperty("description", IsRequired = false)]
        public string Description
        {
            get { return (string)base["description"]; }
            set { base["description"] = value; }
        }
    }
}