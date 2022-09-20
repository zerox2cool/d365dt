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
    [ConfigurationCollection(typeof(StopWindowsServiceElement))]
    public class StopWindowsServiceCollection : ConfigurationElementCollection
    {
       

        protected override ConfigurationElement CreateNewElement()
        {
            return new StopWindowsServiceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((StopWindowsServiceElement)element).ServiceName;
        }

        protected override string ElementName
        {
            get { return "service"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public StopWindowsServiceElement this[int index]
        {
            get { return (StopWindowsServiceElement)BaseGet(index); }
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
    public class StopWindowsServiceElement : ConfigurationElement
    {
        [ConfigurationProperty("server", IsRequired = true)]
        public string ServerName
        {
            get { return (string)base["server"]; }
            set { base["server"] = value; }
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string ServiceName
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

    }
}
