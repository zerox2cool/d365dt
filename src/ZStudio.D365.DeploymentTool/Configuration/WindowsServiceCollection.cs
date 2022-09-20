using System;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(ReportElement))]
    public class WindowsServiceCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new WindowsServiceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WindowsServiceElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "windowsService"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public WindowsServiceElement this[int index]
        {
            get { return (WindowsServiceElement)BaseGet(index); }
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
    public class WindowsServiceElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("executable", IsRequired = true)]
        public string Executable
        {
            get { return (string)base["executable"]; }
            set { base["executable"] = value; }
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
    }
}