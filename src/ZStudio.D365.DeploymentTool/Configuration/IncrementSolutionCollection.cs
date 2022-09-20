using System;
using System.Configuration;
using System.Xml;
using ZD365DT.DeploymentTool.Utils;

namespace ZD365DT.DeploymentTool.Configuration
{
    [ConfigurationCollection(typeof(IncrementSolutionElement))]
    public class IncrementSolutionCollection : ConfigurationElementCollection
    {
        public IncrementSolutionCollection() : base() { }

        public IncrementSolutionCollection(string xmlFilePath)
        {
            //parse a configuration XML file
            XmlNamespaceManager nsmgr = null;
            XmlDocument doc = ConfigXmlHelper.LoadXmlDoc(xmlFilePath, out nsmgr, ConfigXml.Namespace.SOLUTION_XMLNS);
            XmlNodeList nodelist = ConfigXmlHelper.GetXmlNodeList(doc, ConfigXml.Solution.NODE_NAME, nsmgr);

            foreach (XmlNode x in nodelist)
            {
                IncrementSolutionElement e = new IncrementSolutionElement();
                e.SolutionName = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Solution.Element.NAME, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Solution.Element.NAME, nsmgr) : null;
                
                base.BaseAdd(e);
            }
        }

        [ConfigurationProperty("publishType", DefaultValue = PublishType.Batch, IsRequired = false)]
        public PublishType PublishType
        {
            get { return (PublishType)base["publishType"]; }
            set { base["publishType"] = value; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new IncrementSolutionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IncrementSolutionElement)element).SolutionName;
        }
        protected override string ElementName
        {
            get { return "solution"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public IncrementSolutionElement this[int index]
        {
            get { return (IncrementSolutionElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        [ConfigurationProperty("solutionconfig", IsRequired = false)]
        public string SolutionConfig
        {
            get { return (string)this["solutionconfig"]; }
            set { this["solutionconfig"] = value; }
        }

        [ConfigurationProperty("setversion", IsRequired = false, DefaultValue = false)]
        public bool SetVersion
        {
            get { return (bool)this["setversion"]; }
            set { this["setversion"] = value; }
        }

        [ConfigurationProperty("setversionvalue", IsRequired = false)]
        public string SetVersionValue
        {
            get { return (string)this["setversionvalue"]; }
            set { this["setversionvalue"] = value; }
        }
    }

    /// <summary>
    /// The class that holds onto each element returned by the configuration manager.
    /// </summary>
    public class IncrementSolutionElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string SolutionName
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }
    }
}
