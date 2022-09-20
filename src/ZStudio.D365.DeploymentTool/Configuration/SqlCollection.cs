using System;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(SqlElement))]
    public class SqlCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SqlElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SqlElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "script"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public SqlElement this[int index]
        {
            get { return (SqlElement)BaseGet(index); }
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
    public class SqlElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("executeActionOn", IsRequired = false, DefaultValue = ExecuteActionOn.Intall)]
        public ExecuteActionOn ExectuteActionOn
        {
            get { return (ExecuteActionOn)base["executeActionOn"]; }
            set { base["executeActionOn"] = value; }
        }

        [ConfigurationProperty("connectionString", IsRequired = true)]
        public string ConnectionString
        {
            get { return (string)base["connectionString"]; }
            set { base["connectionString"] = value; }
        }

        [ConfigurationProperty("sourceFile", IsRequired = false)]
        public string SourceFile
        {
            get { return (string)base["sourceFile"]; }
            set { base["sourceFile"] = value; }
        }

        [ConfigurationProperty("sourceDirectory", IsRequired = false)]
        public string SourceDirectory
        {
            get { return (string)base["sourceDirectory"]; }
            set { base["sourceDirectory"] = value; }
        }

        [ConfigurationProperty("executeInTransaction", IsRequired = false, DefaultValue = true)]
        public bool ExecuteInTransaction
        {
            get { return (bool)base["executeInTransaction"]; }
            set { base["executeInTransaction"] = value; }
        }
    }
}