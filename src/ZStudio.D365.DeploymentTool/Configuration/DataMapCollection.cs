using System.Configuration;

namespace ZD365DT.DeploymentTool
{
    public enum Action
    {
        import,
        export,
        delete
    }

    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(DataMapElement))]
    public class DataMapCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DataMapElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataMapElement)element).FileName;
        }

        protected override string ElementName
        {
            get { return "map"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public DataMapElement this[int index]
        {
            get { return (DataMapElement)BaseGet(index); }
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
    public class DataMapElement : ConfigurationElement
    {
        [ConfigurationProperty("fileName", IsKey = true, IsRequired = true)]
        public string FileName
        {
            get { return (string)base["fileName"]; }
            set { base["fileName"] = value; }
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string DataMapName
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("action", DefaultValue = Action.import, IsRequired = true)]
        public Action Action
        {
            get { return (Action)base["action"]; }
            set { base["action"] = value; }
        }
    }
}
