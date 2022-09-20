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
    [ConfigurationCollection(typeof(ActivateWorkflowsElement))]
    public class ActivateWorkflowsCollection : ConfigurationElementCollection
    {
        [ConfigurationProperty("allWorkflows", DefaultValue = true, IsRequired = false)]
        public bool AllWorkflows
        {
            get { return (bool)base["allWorkflows"]; }
            set { base["allWorkflows"] = value; }
        }        

        protected override ConfigurationElement CreateNewElement()
        {
            return new ActivateWorkflowsElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ActivateWorkflowsElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "workflow"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public ActivateWorkflowsElement this[int index]
        {
            get { return (ActivateWorkflowsElement)BaseGet(index); }
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
    public class ActivateWorkflowsElement : ConfigurationElement
    {
       

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

       


    }
}
