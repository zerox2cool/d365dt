using System;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(ExternalActionElement))]
    public class ExternalActionCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ExternalActionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExternalActionElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "action"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public ExternalActionElement this[int index]
        {
            get { return (ExternalActionElement)BaseGet(index); }
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
    public class ExternalActionElement : ConfigurationElement
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

        [ConfigurationProperty("arguments", IsRequired = true)]
        public string Arguments
        {
            get { return (string)base["arguments"]; }
            set { base["arguments"] = value; }
        }

        [ConfigurationProperty("workingDirectory", IsRequired = false)]
        public string WorkingDirectory
        {
            get { return (string)base["workingDirectory"]; }
            set { base["workingDirectory"] = value; }
        }

        [ConfigurationProperty("captureOutput", IsRequired = false, DefaultValue = true)]
        public bool CaptureOutput
        {
            get { return (bool)base["captureOutput"]; }
            set { base["captureOutput"] = value; }
        }

        [ConfigurationProperty("expectZeroReturnCode", IsRequired = false, DefaultValue = true)]
        public bool ExpectZeroReturnCode
        {
            get { return (bool)base["expectZeroReturnCode"]; }
            set { base["expectZeroReturnCode"] = value; }
        }
    }
}