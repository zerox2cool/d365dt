using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{


    [ConfigurationCollection(typeof(DataExportElement))]
    public class DataExportCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataExportElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataExportElement)element).EntityName;
        }

        protected override string ElementName
        {
            get { return "export"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public DataExportElement this[int index]
        {
            get { return (DataExportElement)BaseGet(index); }
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
    public class DataExportElement : ConfigurationElement
    {


        [ConfigurationProperty("entityName", IsRequired = true)]
        public string EntityName
        {
            get { return (string)base["entityName"]; }
            set { base["entityName"] = value; }
        }

        [ConfigurationProperty("fileName", IsKey = true, IsRequired = true)]
        public string FileName
        {
            get { return (string)base["fileName"]; }
            set { base["fileName"] = value; }
        }
    }
}
