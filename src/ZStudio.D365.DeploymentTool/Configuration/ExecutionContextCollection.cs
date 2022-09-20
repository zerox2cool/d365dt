using System;
using System.Configuration;
using System.Xml;
using ZD365DT.DeploymentTool.Utils;

namespace ZD365DT.DeploymentTool.Configuration
{
    /// <summary>
    /// The collection class that will store the groups of Execution Context collection that is used.
    /// </summary>
    [ConfigurationCollection(typeof(ExecutionContextCollection))]
    public class ExecutionContextsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ExecutionContextCollection();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExecutionContextCollection)element).ContextFile;
        }

        protected override string ElementName
        {
            get { return "executionContext"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public ExecutionContextCollection this[int index]
        {
            get { return (ExecutionContextCollection)BaseGet(index); }
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
    /// The collection class that will store the list of Execution Context values that is used.
    /// </summary>
    [ConfigurationCollection(typeof(NameValueConfigurationElement))]
    public class ExecutionContextCollection : ConfigurationElementCollection
    {
        public ExecutionContextCollection() : base() { }

        public ExecutionContextCollection(string xmlFilePath)
        {
            //parse a configuration XML file
            XmlNamespaceManager nsmgr = null;
            XmlDocument doc = ConfigXmlHelper.LoadXmlDoc(xmlFilePath, out nsmgr, ConfigXml.Namespace.EXECCONTEXT_XMLNS);
            XmlNodeList nodelist = ConfigXmlHelper.GetXmlNodeList(doc, ConfigXml.ExecutionContext.NODE_NAME, nsmgr);

            foreach (XmlNode x in nodelist)
            {
                ExecutionContextElement e = new ExecutionContextElement();
                e.Name = ConfigXmlHelper.GetXmlNodeAttribute(x, ConfigXml.ATTR_NAME);
                e.Value = ConfigXmlHelper.GetXmlNodeAttribute(x, ConfigXml.ATTR_VALUE);

                base.BaseAdd(e);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ExecutionContextElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExecutionContextElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "add"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public ExecutionContextElement this[int index]
        {
            get { return (ExecutionContextElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("contextFile")]
        public string ContextFile
        {
            get { return (string)this["contextFile"]; }
            set { this["contextFile"] = value; }
        }
    }

    /// <summary>
    /// The class that holds the execution context value.
    /// </summary>
    public class ExecutionContextElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get { return (string)base["value"]; }
            set { base["value"] = value; }
        }
    }
}