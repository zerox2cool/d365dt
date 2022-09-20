using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    /// <summary>
    /// The collection class that will store the list of CRM theme to be created/updated.
    /// </summary>
    [ConfigurationCollection(typeof(ThemeElement))]
    public class ThemeCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ThemeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ThemeElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "theme"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public ThemeElement this[int index]
        {
            get { return (ThemeElement)BaseGet(index); }
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
    /// The class that holds the theme record to be created/updated.
    /// </summary>
    public class ThemeElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("themeSettingFile", IsRequired = true)]
        public string ThemeSettingFile
        {
            get { return (string)base["themeSettingFile"]; }
            set { base["themeSettingFile"] = value; }
        }

        [ConfigurationProperty("publishTheme", IsRequired = true, DefaultValue = false)]
        public bool PublishTheme
        {
            get { return (bool)base["publishTheme"]; }
            set { base["publishTheme"] = value; }
        }
    }
}