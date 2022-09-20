using System;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    #region Enumerations

    public enum FieldDelimiter
    {
        Colon = 1,
        Comma = 2,
        Tab = 3,
        Semicolon = 4
    }

    public enum DataDelimiter
    {
        DoubleQuote = 1,
        None = 2,
        SingleQuote = 3
    }

    #endregion Enumerations

    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(DataImportElement))]
    public class DataImportCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataImportElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataImportElement)element).FileName;
        }

        protected override string ElementName
        {
            get { return "import"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public DataImportElement this[int index]
        {
            get { return (DataImportElement)BaseGet(index); }
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
    public class DataImportElement : ConfigurationElement
    {
        [ConfigurationProperty("fileName", IsKey = true, IsRequired = true)]
        public string FileName
        {
            get { return (string)base["fileName"]; }
            set { base["fileName"] = value; }
        }

        [ConfigurationProperty("entityName", IsRequired = true)]
        public string EntityName
        {
            get { return (string)base["entityName"]; }
            set { base["entityName"] = value; }
        }
        
        [ConfigurationProperty("datamap", DefaultValue = "", IsRequired = false)]
        public string DataMap
        {
            get { return (string)base["datamap"]; }
            set { base["datamap"] = value; }
        }

        [ConfigurationProperty("fieldDelimiter", DefaultValue = FieldDelimiter.Comma, IsRequired = false)]
        public FieldDelimiter FieldDelimiter
        {
            get { return (FieldDelimiter)base["fieldDelimiter"]; }
            set { base["fieldDelimiter"] = value; }
        }

        [ConfigurationProperty("dataDelimiter", DefaultValue = DataDelimiter.DoubleQuote, IsRequired = false)]
        public DataDelimiter DataDelimiter
        {
            get { return (DataDelimiter)base["dataDelimiter"]; }
            set { base["dataDelimiter"] = value; }
        }

        [ConfigurationProperty("owningTeam", IsRequired = false)]
        public string OwningTeam
        {
            get { return (string)base["owningTeam"]; }
            set { base["owningTeam"] = value; }
        }

        [ConfigurationProperty("domainUserName", IsRequired = false)]
        public string DomainUserName
        {
            get { return (string)base["domainUserName"]; }
            set { base["domainUserName"] = value; }
        }

        [ConfigurationProperty("detectDuplicates", DefaultValue = true, IsRequired = false)]
        public bool DetectDuplicates
        {
            get { return (bool)base["detectDuplicates"]; }
            set { base["detectDuplicates"] = value; }
        }

        [ConfigurationProperty("isManyToMany", DefaultValue = false, IsRequired = false)]
        public bool IsManyToMany
        {
            get { return (bool)base["isManyToMany"]; }
            set { base["isManyToMany"] = value; }
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