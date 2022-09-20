using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
   
    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(AssemblyModeElement))]
    public class AssemblyModeCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AssemblyModeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AssemblyModeElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "assemblyMode"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public AssemblyElement this[int index]
        {
            get { return (AssemblyElement)BaseGet(index); }
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
    public class AssemblyModeElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }
        
        [ConfigurationProperty("sandboxed", IsRequired = true, DefaultValue = true)]
        public bool Sandboxed
        {
            get { return (bool)base["sandboxed"]; }
            set { base["sandboxed"] = value; }
        }
    }
}