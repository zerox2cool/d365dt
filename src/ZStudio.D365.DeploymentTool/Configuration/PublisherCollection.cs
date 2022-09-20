using System;
using System.Configuration;
using System.Xml;
using ZD365DT.DeploymentTool.Utils;

namespace ZD365DT.DeploymentTool.Configuration
{
    /// <summary>
    /// The collection class that will store the list of Publisher to be created/updated.
    /// </summary>
    [ConfigurationCollection(typeof(PublisherElement))]
    public class PublisherCollection : ConfigurationElementCollection
    {
        public PublisherCollection() : base() { }

        public PublisherCollection(string xmlFilePath)
        {
            //parse a configuration XML file
            XmlNamespaceManager nsmgr = null;
            XmlDocument doc = ConfigXmlHelper.LoadXmlDoc(xmlFilePath, out nsmgr, ConfigXml.Namespace.PUBLISHER_XMLNS);
            XmlNodeList nodelist = ConfigXmlHelper.GetXmlNodeList(doc, ConfigXml.Publisher.NODE_NAME, nsmgr);

            foreach (XmlNode x in nodelist)
            {
                PublisherElement e = new PublisherElement();
                e.Name = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.NAME, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.NAME, nsmgr) : null;
                e.DisplayName = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.DISPLAYNAME, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.DISPLAYNAME, nsmgr) : null;
                e.Description = ConfigXmlHelper.GetXmlNodeValue(x, ConfigXml.Publisher.Element.DESCRIPTION, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeValue(x, ConfigXml.Publisher.Element.DESCRIPTION, nsmgr) : null;
                e.Prefix = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.PREFIX, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.PREFIX, nsmgr) : null;
                e.OptionValuePrefix = Convert.ToInt32(ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.OPTONVALUEPREFIX, nsmgr));
                e.Email = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.EMAIL, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.EMAIL, nsmgr) : null;
                e.Phone = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.PHONE, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.PHONE, nsmgr) : null;
                e.Website = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.WEBSITE, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.WEBSITE, nsmgr) : null;
                e.AddressLine1 = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.ADDRESS_LINE1, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.ADDRESS_LINE1, nsmgr) : null;
                e.AddressLine2 = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.ADDRESS_LINE2, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.ADDRESS_LINE2, nsmgr) : null;
                e.AddressCity = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.ADDRESS_CITY, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.ADDRESS_CITY, nsmgr) : null;
                e.AddressState = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.ADDRESS_STATE, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.ADDRESS_STATE, nsmgr) : null;
                e.AddressPostalCode = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.ADDRESS_POSTALCODE, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.ADDRESS_POSTALCODE, nsmgr) : null;
                e.AddressCountry = ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.ADDRESS_COUNTRY, nsmgr) != null ? ConfigXmlHelper.GetXmlNodeAttributeValue(x, ConfigXml.Publisher.Element.ADDRESS_COUNTRY, nsmgr) : null;

                base.BaseAdd(e);    
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PublisherElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PublisherElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "publisher"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public PublisherElement this[int index]
        {
            get { return (PublisherElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        [ConfigurationProperty("publisherconfig")]
        public string PublisherConfig
        {
            get { return (string)this["publisherconfig"]; }
            set { this["publisherconfig"] = value; }
        }
    }

    /// <summary>
    /// The class that holds the publisher record to be created/updated.
    /// </summary>
    public class PublisherElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("displayname", IsRequired = true)]
        public string DisplayName
        {
            get { return (string)base["displayname"]; }
            set { base["displayname"] = value; }
        }

        [ConfigurationProperty("description", IsRequired = false)]
        public string Description
        {
            get { return (string)base["description"]; }
            set { base["description"] = value; }
        }

        [ConfigurationProperty("prefix", IsRequired = true)]
        public string Prefix
        {
            get { return (string)base["prefix"]; }
            set { base["prefix"] = value; }
        }

        [ConfigurationProperty("optionvalueprefix", IsRequired = true)]
        public int OptionValuePrefix
        {
            get { return (int)base["optionvalueprefix"]; }
            set { base["optionvalueprefix"] = value; }
        }

        [ConfigurationProperty("website", IsRequired = false)]
        public string Website
        {
            get { return (string)base["website"]; }
            set { base["website"] = value; }
        }

        [ConfigurationProperty("email", IsRequired = false)]
        public string Email
        {
            get { return (string)base["email"]; }
            set { base["email"] = value; }
        }

        [ConfigurationProperty("phone", IsRequired = false)]
        public string Phone
        {
            get { return (string)base["phone"]; }
            set { base["phone"] = value; }
        }

        [ConfigurationProperty("addressline1", IsRequired = false)]
        public string AddressLine1
        {
            get { return (string)base["addressline1"]; }
            set { base["addressline1"] = value; }
        }

        [ConfigurationProperty("addressline2", IsRequired = false)]
        public string AddressLine2
        {
            get { return (string)base["addressline2"]; }
            set { base["addressline2"] = value; }
        }

        [ConfigurationProperty("addresscity", IsRequired = false)]
        public string AddressCity
        {
            get { return (string)base["addresscity"]; }
            set { base["addresscity"] = value; }
        }

        [ConfigurationProperty("addressstate", IsRequired = false)]
        public string AddressState
        {
            get { return (string)base["addressstate"]; }
            set { base["addressstate"] = value; }
        }

        [ConfigurationProperty("addresspostalcode", IsRequired = false)]
        public string AddressPostalCode
        {
            get { return (string)base["addresspostalcode"]; }
            set { base["addresspostalcode"] = value; }
        }

        [ConfigurationProperty("addresscountry", IsRequired = false)]
        public string AddressCountry
        {
            get { return (string)base["addresscountry"]; }
            set { base["addresscountry"] = value; }
        }
    }
}