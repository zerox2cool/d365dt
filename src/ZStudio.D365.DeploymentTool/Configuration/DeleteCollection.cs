using System;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    public enum DeleteType
    {
        directory,
        file
    }
    public enum TargetType
    {
        subfolder,
        files,
        allcontent,
        all
    }

    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(DeleteElement))]
    public class DeleteCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DeleteElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DeleteElement)element).Target;
        }

        protected override string ElementName
        {
            get { return "element"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public DeleteElement this[int index]
        {
            get { return (DeleteElement)BaseGet(index); }
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
    public class DeleteElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public DeleteType DeleteType
        {
            get { return (DeleteType)base["type"]; }
            set { base["type"] = value; }
        }
       

        [ConfigurationProperty("target", IsRequired = true)]
        public string Target
        {
            get { return (string)base["target"]; }
            set { base["target"] = value; }
        }

        [ConfigurationProperty("targettype", IsRequired = false, DefaultValue =TargetType.all)]
        public TargetType TargetType
        {
            get { return (TargetType)base["targettype"]; }
            set { base["targettype"] = value; }
        }



        [ConfigurationProperty("removeOnUninstall", IsRequired = false, DefaultValue = false)]
        public bool RemoveOnUninstall
        {
            get { return (bool)base["removeOnUninstall"]; }
            set { base["removeOnUninstall"] = value; }
        }
    }
}