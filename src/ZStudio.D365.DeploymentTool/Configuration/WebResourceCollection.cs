using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    public enum SourceType
    {
        configxml,
        directory
    }

    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(CustomizationElement))]
    public class WebResourceCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new WebResourceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WebResourceElement)element).Source;
        }

        protected override string ElementName
        {
            get { return "webresource"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public WebResourceElement this[int index]
        {
            get { return (WebResourceElement)BaseGet(index); }
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
    public class WebResourceElement : ConfigurationElement
    {
        [ConfigurationProperty("source", IsKey = true, IsRequired = true)]
        public string Source
        {
            get { return (string)base["source"]; }
            set { base["source"] = value; }
        }

        [ConfigurationProperty("sourcetype", IsRequired = true, DefaultValue = SourceType.directory)]
        public SourceType sType
        {
            get { return (SourceType)base["sourcetype"]; }
            set { base["sourcetype"] = value; }
        }

        [ConfigurationProperty("includesubfolders", IsRequired = false, DefaultValue = true)]
        public bool IncludeSubfolders
        {
            get { return (bool)base["includesubfolders"]; }
            set { base["includesubfolders"] = value; }
        }

        [ConfigurationProperty("usepathinname", IsRequired = false, DefaultValue = true)]
        public bool UsePathInName
        {
            get { return (bool)base["usepathinname"]; }
            set { base["usepathinname"] = value; }
        }

        [ConfigurationProperty("usecustomizationprefixindisplayname", IsRequired = false, DefaultValue = false)]
        public bool UseCustomizationPrefixInDisplayName
        {
            get { return (bool)base["usecustomizationprefixindisplayname"]; }
            set { base["usecustomizationprefixindisplayname"] = value; }
        }

        [ConfigurationProperty("useextensionasprefix", IsRequired = false, DefaultValue = false)]
        public bool UseExtensionAsPrefix
        {
            get { return (bool)base["useextensionasprefix"]; }
            set { base["useextensionasprefix"] = value; }
        }

        [ConfigurationProperty("updatedescription", IsRequired = false, DefaultValue = true)]
        public bool UpdateDescription
        {
            get { return (bool)base["updatedescription"]; }
            set { base["updatedescription"] = value; }
        }

        [ConfigurationProperty("useextensioninname", IsRequired = false, DefaultValue = true)]
        public bool UseExtensionInName
        {
            get { return (bool)base["useextensioninname"]; }
            set { base["useextensioninname"] = value; }
        }

        [ConfigurationProperty("filename", IsRequired = false, DefaultValue = "")]
        public string FileName
        {
            get { return (string)base["filename"]; }
            set { base["filename"] = value; }
        }

        [ConfigurationProperty("solution", IsRequired = false, DefaultValue = "")]
        public string Solution
        {
            get { return (string)base["solution"]; }
            set { base["solution"] = value; }
        }

        [ConfigurationProperty("customizationPrefix", IsRequired = false, DefaultValue = "")]
        public string CustomizationPrefix
        {
            get { return (string)base["customizationPrefix"]; }
            set { base["customizationPrefix"] = value; }
        }

        [ConfigurationProperty("removeOnUninstall", IsRequired = false, DefaultValue = false)]
        public bool RemoveOnUninstall
        {
            get { return (bool)base["removeOnUninstall"]; }
            set { base["removeOnUninstall"] = value; }
        }
    }
}