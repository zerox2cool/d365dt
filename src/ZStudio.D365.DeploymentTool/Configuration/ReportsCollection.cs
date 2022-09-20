using System;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    public enum ReportInstallBehaviour
    {
        RemoveAndRecreate = 0,
        UpdateExisting = 1,
        LeaveExisting = 2
    }

    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(ReportElement))]
    public class ReportsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ReportElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ReportElement)element).ReportName;
        }

        protected override string ElementName
        {
            get { return "report"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public ReportElement this[int index]
        {
            get { return (ReportElement)BaseGet(index); }
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
    public class ReportElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string ReportName
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("parentName", IsRequired = false)]
        public string ParentReportName
        {
            get { return (string)base["parentName"]; }
            set { base["parentName"] = value; }
        }

        [ConfigurationProperty("fileName", IsRequired = true)]
        public string FileName
        {
            get { return (string)base["fileName"]; }
            set { base["fileName"] = value; }
        }

        [ConfigurationProperty("description", IsRequired = false)]
        public string Description
        {
            get { return (string)base["description"]; }
            set { base["description"] = value; }
        }

        [ConfigurationProperty("languageCode", DefaultValue = "1033", IsRequired = false)]
        public int LanguageCode
        {
            get { return (int)base["languageCode"]; }
            set { base["languageCode"] = value; }
        }

        [ConfigurationProperty("relatedRecords", IsRequired = false)]
        public string RelatedRecords
        {
            get { return (string)base["relatedRecords"]; }
            set { base["relatedRecords"] = value; }
        }

        [ConfigurationProperty("displayAreas", IsRequired = false)]
        public string DispalyAreas
        {
            get { return (string)base["displayAreas"]; }
            set { base["displayAreas"] = value; }
        }

        [ConfigurationProperty("categoryCodes", IsRequired = false)]
        public string CategoryCodes
        {
            get { return (string)base["categoryCodes"]; }
            set { base["categoryCodes"] = value; }
        }

        [ConfigurationProperty("publishExternal", DefaultValue = false, IsRequired = false)]
        public bool PublishExternal
        {
            get { return (bool)base["publishExternal"]; }
            set { base["publishExternal"] = value; }
        }

        [ConfigurationProperty("installBehaviour", DefaultValue = ReportInstallBehaviour.RemoveAndRecreate, IsRequired = false)]
        public ReportInstallBehaviour InstallBehaviour
        {
            get { return (ReportInstallBehaviour)base["installBehaviour"]; }
            set { base["installBehaviour"] = value; }
        }
    }
}