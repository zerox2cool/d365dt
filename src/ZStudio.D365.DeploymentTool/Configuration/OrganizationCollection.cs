using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{

    public enum Action
    {
        create,
        delete,
        enable,
        disable
    }


    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(OrganizationElement))]
    public class OrganizationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new OrganizationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OrganizationElement)element).UniqueName;
        }

        protected override string ElementName
        {
            get { return "organization"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public OrganizationElement this[int index]
        {
            get { return (OrganizationElement)BaseGet(index); }
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
    public class OrganizationElement : ConfigurationElement
    {
        [ConfigurationProperty("uniquename", IsRequired = true)]
        public string UniqueName
        {
            get { return (string)base["uniquename"]; }
            set { base["uniquename"] = value; }
        }

        [ConfigurationProperty("friendlyname", IsRequired = true)]
        public string FriendlyName
        {
            get { return (string)base["friendlyname"]; }
            set { base["friendlyname"] = value; }
        }

        [ConfigurationProperty("sqlservername", IsRequired = true)]
        public string SqlServerName
        {
            get { return (string)base["sqlservername"]; }
            set { base["sqlservername"] = value; }
        }

        [ConfigurationProperty("srsurl", IsRequired = true)]
        public string SrsUrl
        {
            get { return (string)base["srsurl"]; }
            set { base["srsurl"] = value; }
        }

        [ConfigurationProperty("basecurrencycode", IsRequired = true)]
        public string BaseCurrencyCode
        {
            get { return (string)base["basecurrencycode"]; }
            set { base["basecurrencycode"] = value; }
        }

        [ConfigurationProperty("basecurrencyname", IsRequired = true)]
        public string BaseCurrencyName
        {
            get { return (string)base["basecurrencyname"]; }
            set { base["basecurrencyname"] = value; }
        }

        [ConfigurationProperty("basecurrencysymbol", IsRequired = true)]
        public string BaseCurrencySymbol
        {
            get { return (string)base["basecurrencysymbol"]; }
            set { base["basecurrencysymbol"] = value; }
        }

        [ConfigurationProperty("basecurrencyprecision", IsRequired = true)]
        public int BaseCurrencyPrecision
        {
            get { return (int)base["basecurrencyprecision"]; }
            set { base["basecurrencyprecision"] = value; }
        }


        [ConfigurationProperty("action", IsKey = true, IsRequired = true)]
        public Action Action
        {
            get { return (Action)base["action"]; }
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
