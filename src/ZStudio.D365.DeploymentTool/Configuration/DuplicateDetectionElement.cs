using System;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    [ConfigurationCollection(typeof(DuplicateDetectionElement))]
    public class DuplicateDetectionCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DuplicateDetectionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DuplicateDetectionElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "duplicateDetection"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public DuplicateDetectionElement this[int index]
        {
            get { return (DuplicateDetectionElement)BaseGet(index); }
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
    public class DuplicateDetectionElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("sourceFile", IsKey = false, IsRequired = true)]
        public string SourceFile
        {
            get { return (string)base["sourceFile"]; }
            set { base["sourceFile"] = value; }
        }

        [ConfigurationProperty("action", IsKey = false, IsRequired = true)]
        public string Action
        {
            get { return (string)base["action"]; }
            set { base["action"] = value; }
        }
    }
}