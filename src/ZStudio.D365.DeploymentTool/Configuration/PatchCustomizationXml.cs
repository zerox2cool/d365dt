using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    public enum PatchType
    {
        CustomControlDefaultConfigs
    }

    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(CustomizationElement))]
    public class PatchCustomizationXmlCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new PatchCustomizationXmlElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PatchCustomizationXmlElement)element).File;
        }

        protected override string ElementName
        {
            get { return "patchCustomizationXml"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public PatchCustomizationXmlElement this[int index]
        {
            get { return (PatchCustomizationXmlElement)BaseGet(index); }
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
    public class PatchCustomizationXmlElement : ConfigurationElement
    {
        [ConfigurationProperty("file", IsRequired = true)]
        public string File
        {
            get { return (string)base["file"]; }
            set { base["file"] = value; }
        }

        [ConfigurationProperty("patchType", DefaultValue = PatchType.CustomControlDefaultConfigs, IsRequired = true)]
        public PatchType PatchType
        {
            get { return (PatchType)base["patchType"]; }
            set { base["patchType"] = value; }
        }
    }
}