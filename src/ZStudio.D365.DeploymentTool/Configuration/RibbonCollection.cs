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
    [ConfigurationCollection(typeof(RibbonElement))]
    public class RibbonCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new RibbonElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RibbonElement)element).FileName;
        }

        protected override string ElementName
        {
            get { return "ribbonelement"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public RibbonElement this[int index]
        {
            get { return (RibbonElement)BaseGet(index); }
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
    public class RibbonElement : ConfigurationElement
    {


        [ConfigurationProperty("filename", IsKey = true, IsRequired = true)]
        public string FileName
        {
            get { return (string)base["filename"]; }
            set { base["filename"] = value; }
        }      
        
        [ConfigurationProperty("entities", IsRequired = true)]        
        public string Entities
        {
            get { return (string)base["entities"]; }
            set { base["entities"] = value; }
        }

        [ConfigurationProperty("solution", IsRequired = false, DefaultValue = "")]
        public string Solution
        {
            get { return (string)base["solution"]; }
            set { base["solution"] = value; }
        }

        [ConfigurationProperty("includeappribbon", IsRequired = false,DefaultValue=false)]
        public bool IncludeAppRibbon
        {
            get { return (bool)base["includeappribbon"]; }
            set { base["includeappribbon"] = value; }
        }

        [ConfigurationProperty("action", IsRequired = false, DefaultValue = "export")]
        public string Action
        {
            get { return (string)base["action"]; }
            set { base["action"] = value; }
        }

        [ConfigurationProperty("removeOnUninstall", IsRequired = false, DefaultValue = false)]
        public bool RemoveOnUninstall
        {
            get { return (bool)base["removeOnUninstall"]; }
            set { base["removeOnUninstall"] = value; }
        }
    }
}
