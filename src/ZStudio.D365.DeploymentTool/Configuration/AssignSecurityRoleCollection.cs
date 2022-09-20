using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    [ConfigurationCollection(typeof(AssignSecurityRoleElement))]
   public class AssignSecurityRoleCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AssignSecurityRoleElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AssignSecurityRoleElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "element"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public AssignSecurityRoleElement this[int index]
        {
            get { return (AssignSecurityRoleElement)BaseGet(index); }
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
    public class AssignSecurityRoleElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("securityrole", IsRequired = true)]
        public string SecurityRole
        {
            get { return (string)base["securityrole"]; }
            set { base["securityrole"] = value; }
        }

        [ConfigurationProperty("domainusername", DefaultValue = "", IsRequired = false)]
        public string DomainUserName
        {
            get { return (string)base["domainusername"]; }
            set { base["domainusername"] = value; }
        }

        [ConfigurationProperty("teamname", DefaultValue = "", IsRequired = false)]
        public string TeamName
        {
            get { return (string)base["teamname"]; }
            set { base["teamname"] = value; }
        }

    }
}
