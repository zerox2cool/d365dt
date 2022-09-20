using System;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    public enum CopyType
    {
        directory,
        file
    }

    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(CopyElement))]
    public class CopyCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CopyElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CopyElement)element).Target;
        }

        protected override string ElementName
        {
            get { return "element"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public CopyElement this[int index]
        {
            get { return (CopyElement)BaseGet(index); }
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
    public class CopyElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public CopyType CopyType
        {
            get { return (CopyType)base["type"]; }
            set { base["type"] = value; }
        }

        [ConfigurationProperty("source", IsKey = true, IsRequired = true)]
        public string Source
        {
            get { return (string)base["source"]; }
            set { base["source"] = value; }
        }

        [ConfigurationProperty("target", IsRequired = true)]
        public string Target
        {
            get { return (string)base["target"]; }
            set { base["target"] = value; }
        }

        [ConfigurationProperty("removeOnUninstall", IsRequired = false, DefaultValue = false)]
        public bool RemoveOnUninstall
        {
            get { return (bool)base["removeOnUninstall"]; }
            set { base["removeOnUninstall"] = value; }
        }
    }
}