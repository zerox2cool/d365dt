using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
     [ConfigurationCollection(typeof(DataDeleteElement))]
    public class DataDeleteCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataDeleteElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataDeleteElement)element).EntityName;
        }

        protected override string ElementName
        {
            get { return "delete"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public DataDeleteElement this[int index]
        {
            get { return (DataDeleteElement)BaseGet(index); }
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
    public class DataDeleteElement : ConfigurationElement
    {
        

        [ConfigurationProperty("entityName", IsRequired = true)]
        public string EntityName
        {
            get { return (string)base["entityName"]; }
            set { base["entityName"] = value; }
        }

        [ConfigurationProperty("dataBackUp", DefaultValue = true, IsRequired = false)]
        public bool DataBackup
        {
            get { return (bool)base["dataBackUp"]; }
            set { base["dataBackUp"] = value; }
        }

        [ConfigurationProperty("waitForCompletion", DefaultValue = true, IsRequired = false)]
        public bool WaitForCompletion
        {
            get { return (bool)base["waitForCompletion"]; }
            set { base["waitForCompletion"] = value; }
        }


        [ConfigurationProperty("waitTimeout", DefaultValue = 240, IsRequired = false)]
        public int WaitTimeout
        {
            get { return (int)base["waitTimeout"]; }
            set { base["waitTimeout"] = value; }
        }
    }
}
