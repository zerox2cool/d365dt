using System;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(ReportElement))]
    public class WebsiteCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new WebsiteElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WebsiteElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "website"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public WebsiteElement this[int index]
        {
            get { return (WebsiteElement)BaseGet(index); }
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
    public class WebsiteElement : ConfigurationElementCollection
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("virtualDirectory", IsRequired = true)]
        public string VirtualDirectory
        {
            get { return (string)base["virtualDirectory"]; }
            set { base["virtualDirectory"] = value; }
        }

        [ConfigurationProperty("applicationPool", IsRequired = true)]
        public string ApplicationPool
        {
            get { return (string)base["applicationPool"]; }
            set { base["applicationPool"] = value; }
        }

        [ConfigurationProperty("sourceDirectory", IsRequired = true)]
        public string SourceDirectory
        {
            get { return (string)base["sourceDirectory"]; }
            set { base["sourceDirectory"] = value; }
        }

        [ConfigurationProperty("targetDirectory", IsRequired = true)]
        public string TargetDirectory
        {
            get { return (string)base["targetDirectory"]; }
            set { base["targetDirectory"] = value; }
        }

        [ConfigurationProperty("allowAnonymousAccess", IsRequired = false, DefaultValue = false)]
        public bool AllowAnonymousAccess
        {
            get { return (bool)base["allowAnonymousAccess"]; }
            set { base["allowAnonymousAccess"] = value; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WebsiteMimeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WebsiteMimeElement)element).Extension;
        }

        protected override string ElementName
        {
            get { return "mime"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public WebsiteMimeElement this[int index]
        {
            get { return (WebsiteMimeElement)BaseGet(index); }
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
    public class WebsiteMimeElement : ConfigurationElement
    {
        [ConfigurationProperty("extension", IsKey = true, IsRequired = true)]
        public string Extension
        {
            get { return (string)base["extension"]; }
            set { base["extension"] = value; }
        }

        [ConfigurationProperty("type", IsKey = true, IsRequired = true)]
        public string Type
        {
            get { return (string)base["type"]; }
            set { base["type"] = value; }
        }
    }
}