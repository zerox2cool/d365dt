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
    [ConfigurationCollection(typeof(AssemblyElement))]
    public class AssemblyCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AssemblyElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AssemblyElement)element).Source;
        }

        protected override string ElementName
        {
            get { return "workflowassembly"; }
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
    public class AssemblyElement : ConfigurationElement
    {
        

        [ConfigurationProperty("source", IsKey = true, IsRequired = true)]
        public string Source
        {
            get { return (string)base["source"]; }
            set { base["source"] = value; }
        }


        [ConfigurationProperty("solution", IsRequired = false, DefaultValue ="")]
        public string Solution
        {
            get { return (string)base["solution"]; }
            set { base["solution"] = value; }
        }

        [ConfigurationProperty("removeOnUninstall", IsRequired = false, DefaultValue = false)]
        public bool RemoveOnUninstall
        {
            get { return (bool)base["removeOnUninstall"]; }
            set { base["removeOnUninstall"] = value; }
        }
    }
}
