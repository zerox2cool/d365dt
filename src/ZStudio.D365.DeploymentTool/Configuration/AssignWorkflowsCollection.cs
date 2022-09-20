using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    [ConfigurationCollection(typeof(AssignWorkflowsElement))]
    public class AssignWorkflowsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AssignWorkflowsElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AssignWorkflowsElement)element).WorkflowName;
        }

        protected override string ElementName
        {
            get { return "element"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public AssignWorkflowsElement this[int index]
        {
            get { return (AssignWorkflowsElement)BaseGet(index); }
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
    public class AssignWorkflowsElement : ConfigurationElement
    {
        
        [ConfigurationProperty("workflow", IsRequired = true)]
        public string WorkflowName
        {
            get { return (string)base["workflow"]; }
            set { base["workflow"] = value; }
        }

        [ConfigurationProperty("domainusername", DefaultValue = "", IsRequired = false)]
        public string DomainUserName
        {
            get { return (string)base["domainusername"]; }
            set { base["domainusername"] = value; }
        }


    }
}
