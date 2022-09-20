using System;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(PluginElement))]
    public class PluginCollection : ConfigurationElementCollection
    {
        [ConfigurationProperty("sourceDirectory", IsKey = true, IsRequired = true)]
        public string SourceDirectory
        {
            get { return (string)base["sourceDirectory"]; }
            set { base["sourceDirectory"] = value; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PluginElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PluginElement)element).DefinitionFileName;
        }

        protected override string ElementName
        {
            get { return "plugin"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public PluginElement this[int index]
        {
            get { return (PluginElement)BaseGet(index); }
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
    public class PluginElement : ConfigurationElement
    {
        [ConfigurationProperty("definitionFileName", IsKey = true, IsRequired = true)]
        public string DefinitionFileName
        {
            get { return (string)base["definitionFileName"]; }
            set { base["definitionFileName"] = value; }
        }

        [ConfigurationProperty("solution", IsKey = false, IsRequired = false,DefaultValue="")]
        public string SolutionName
        {
            get { return (string)base["solution"]; }
            set { base["solution"] = value; }
        }

        [ConfigurationProperty("uploadonly", IsKey = false, IsRequired = false, DefaultValue = false)]
        public bool UploadOnly
        {
            get { return (bool)base["uploadonly"]; }
            set { base["uploadonly"] = value; }
        }

        [ConfigurationProperty("unregister", IsKey = false, IsRequired = false, DefaultValue = false)]
        public bool Unregister
        {
            get { return (bool)base["unregister"]; }
            set { base["unregister"] = value; }
        }

        [ConfigurationProperty("unregisteronly", IsKey = false, IsRequired = false, DefaultValue = false)]
        public bool UnregisterOnly
        {
            get { return (bool)base["unregisteronly"]; }
            set { base["unregisteronly"] = value; }
        }
       
    }
   
}