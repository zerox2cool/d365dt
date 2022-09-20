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
    [ConfigurationCollection(typeof(SitemapElement))]
    public class SitemapCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SitemapElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SitemapElement)element).FileName;
        }

        protected override string ElementName
        {
            get { return "sitemapelement"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public SitemapElement this[int index]
        {
            get { return (SitemapElement)BaseGet(index); }
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
    public class SitemapElement : ConfigurationElement
    {


        [ConfigurationProperty("filename", IsKey = true, IsRequired = true)]
        public string FileName
        {
            get { return (string)base["filename"]; }
            set { base["filename"] = value; }
        }

      

        [ConfigurationProperty("solution", IsRequired = false, DefaultValue = "")]
        public string Solution
        {
            get { return (string)base["solution"]; }
            set { base["solution"] = value; }
        }

        [ConfigurationProperty("action", IsRequired = false, DefaultValue = "export")]
        public string Action
        {
            get { return (string)base["action"]; }
            set { base["action"] = value; }
        }

       
    }
}