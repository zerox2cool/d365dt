using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Xml.Serialization;
using ZD365DT.DeploymentTool.Utils.SitemapCustomization;

namespace ZD365DT.DeploymentTool.Utils.SitemapCustomization
{
    [Serializable]
    [XmlType(AnonymousType = true)]
    [XmlRoot("ImportExportXml", Namespace = "", IsNullable = false)]
    public partial class ImportExportXml
    {
        private string entities;
        private string roles;
        private string workflows;
        private SiteMapRoot siteMap;
        private string entityMaps;
        private string entityRelationships;
        private Language languages;
        private string version;
        private string languagecode;
        private string generatedBy;

        [XmlElement("Entities", IsNullable = true)]
        public string Entities
        {
            get { return this.entities; }
            set { this.entities = value; }
        }

        [XmlElement("Roles", IsNullable = true)]
        public string Roles
        {
            get { return this.roles; }
            set { this.roles = value; }
        }

        [XmlElement("Workflows", IsNullable = true)]
        public string Workflows
        {
            get { return this.workflows; }
            set { this.workflows = value; }
        }

        [XmlElement("SiteMap", IsNullable = false)]
        public SiteMapRoot SiteMap
        {
            get { return this.siteMap; }
            set { this.siteMap = value; }
        }

        [XmlElement("EntityMaps", IsNullable = true)]
        public string EntityMaps
        {
            get { return this.entityMaps; }
            set { this.entityMaps = value; }
        }

        [XmlElement("EntityRelationships", IsNullable = true)]
        public string EntityRelationships
        {
            get { return this.entityRelationships; }
            set { this.entityRelationships = value; }
        }

        [XmlElement("Languages", IsNullable = true)]
        public Language Languages
        {
            get { return this.languages; }
            set { this.languages = value; }
        }

        [XmlAttribute("version")]
        public string Version
        {
            get { return this.version; }
            set { this.version = value; }
        }

        [XmlAttribute("languagecode")]
        public string LanguageCode
        {
            get { return this.languagecode; }
            set { this.languagecode = value; }
        }

        [XmlAttribute("generatedBy")]
        public string GeneratedBy
        {
            get { return this.generatedBy; }
            set { this.generatedBy = value; }
        }
    }

    [Serializable()]
    [XmlType(AnonymousType = true)]
    public partial class Language
    {
        private string[] laguages;

        [XmlElement("Language")]
        public string[] Languages
        {
            get { return this.laguages; }
            set { this.laguages = value; }
        }
    }


    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class SiteMapRoot
    {
        private SiteMap siteMap;

        [XmlElement("SiteMap")]
        public SiteMap SiteMap
        {
            get { return this.siteMap; }
            set { this.siteMap = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class SiteMap
    {
        private SiteMapArea[] areas;
        private string url;

        [XmlElement("Area")]
        public SiteMapArea[] Areas
        {
            get { return this.areas; }
            set { this.areas = value; }
        }

        [XmlAttribute]
        public string Url
        {
            get { return this.url; }
            set { this.url = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class SiteMapArea : HasTitlesAndDesciption
    {
        private SiteMapAreaGroup[] group;
        private string id;
        private string title;
        private string resourceId;
        private string icon;
        private string url;
        private bool? showGroups;
        private string license;
        private string description;
        private string descriptionResourceId;

        [XmlElement("Group")]
        public SiteMapAreaGroup[] Groups
        {
            get { return this.group; }
            set { this.group = value; }
        }

        [XmlAttribute]
        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        [XmlAttribute]
        public string Title
        {
            get { return this.title; }
            set { this.title = value; }
        }

        [XmlAttribute]
        public string ResourceId
        {
            get { return this.resourceId; }
            set { this.resourceId = value; }
        }

        [XmlAttribute]
        public string Icon
        {
            get { return this.icon; }
            set { this.icon = value; }
        }

        [XmlAttribute]
        public string Url
        {
            get { return this.url; }
            set { this.url = value; }
        }

        [XmlAttribute]
        public bool ShowGroups
        {
            get { return ShowGroupsSpecified ? this.showGroups.Value : default(bool); }
            set { this.showGroups = value; }
        }

        [XmlIgnore]
        public bool ShowGroupsSpecified { get { return this.showGroups.HasValue; } }

        [XmlAttribute]
        public string License
        {
            get { return this.license; }
            set { this.license = value; }
        }

        [XmlAttribute]
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        [XmlAttribute]
        public string DescriptionResourceId
        {
            get { return this.descriptionResourceId; }
            set { this.descriptionResourceId = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class SiteMapTitle
    {
        private decimal lCID;
        private string title;

        [XmlAttribute]
        public decimal LCID
        {
            get { return this.lCID; }
            set { this.lCID = value; }
        }

        [XmlAttribute]
        public string Title
        {
            get { return this.title; }
            set { this.title = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class SiteMapDescription
    {
        private decimal lCID;
        private string description;

        [XmlAttribute]
        public decimal LCID
        {
            get { return this.lCID; }
            set { this.lCID = value; }
        }

        [XmlAttribute]
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class SiteMapAreaGroup : HasTitlesAndDesciption
    {
        private SiteMapAreaGroupSubArea[] subAreas;
        private string id;
        private string title;
        private string icon;
        private string url;
        private string resourceId;
        private bool? isProfile;
        private string license;
        private string description;
        private string descriptionResourceId;

        [XmlElement("SubArea")]
        public SiteMapAreaGroupSubArea[] SubAreas
        {
            get { return this.subAreas; }
            set { this.subAreas = value; }
        }

        [XmlAttribute]
        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        [XmlAttribute]
        public string Title
        {
            get { return this.title; }
            set { this.title = value; }
        }

        [XmlAttribute]
        public string Icon
        {
            get { return this.icon; }
            set { this.icon = value; }
        }

        [XmlAttribute]
        public string Url
        {
            get { return this.url; }
            set { this.url = value; }
        }

        [XmlAttribute]
        public string ResourceId
        {
            get { return this.resourceId; }
            set { this.resourceId = value; }
        }

        [XmlAttribute]
        public bool IsProfile
        {
            get { return IsProfileSpecified ? this.isProfile.Value : default(bool); }
            set { this.isProfile = value; }
        }

        [XmlIgnore]
        public bool IsProfileSpecified { get { return this.isProfile.HasValue; } }

        [XmlAttribute]
        public string License
        {
            get { return this.license; }
            set { this.license = value; }
        }

        [XmlAttribute]
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        [XmlAttribute]
        public string DescriptionResourceId
        {
            get { return this.descriptionResourceId; }
            set { this.descriptionResourceId = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class SiteMapAreaGroupSubArea : HasTitlesAndDesciption
    {
        private SiteMapAreaGroupSubAreaPrivilege[] privileges;
        private string id;
        private string title;
        private string resourceId;
        private string icon;
        private string outlookShortcutIcon;
        private string url;
        private bool? passParams;
        private string client;
        private bool? availableOffline;
        private string license;
        private string entity;
        private string description;
        private string descriptionResourceId;

        [XmlElement("Privilege")]
        public SiteMapAreaGroupSubAreaPrivilege[] Privileges
        {
            get { return this.privileges; }
            set { this.privileges = value; }
        }

        [XmlAttribute]
        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        [XmlAttribute]
        public string Title
        {
            get { return this.title; }
            set { this.title = value; }
        }

        [XmlAttribute]
        public string ResourceId
        {
            get { return this.resourceId; }
            set { this.resourceId = value; }
        }

        [XmlAttribute]
        public string Icon
        {
            get { return this.icon; }
            set { this.icon = value; }
        }

        [XmlAttribute]
        public string OutlookShortcutIcon
        {
            get { return this.outlookShortcutIcon; }
            set { this.outlookShortcutIcon = value; }
        }

        [XmlAttribute]
        public string Url
        {
            get { return this.url; }
            set { this.url = value; }
        }

        [XmlAttribute]
        public bool PassParams
        {
            get { return PassParamsSpecified ? this.passParams.Value : default(bool); }
            set { this.passParams = value; }
        }

        [XmlIgnore]
        public bool PassParamsSpecified { get { return this.passParams.HasValue; } }

        [XmlAttribute]
        public string Client
        {
            get { return this.client; }
            set { this.client = value; }
        }

        [XmlAttribute]
        public bool AvailableOffline
        {
            get { return AvailableOfflineSpecified ? this.availableOffline.Value : default(bool); }
            set { this.availableOffline = value; }
        }

        [XmlIgnore]
        public bool AvailableOfflineSpecified { get { return this.availableOffline.HasValue; } }

        [XmlAttribute]
        public string License
        {
            get { return this.license; }
            set { this.license = value; }
        }

        [XmlAttribute]
        public string Entity
        {
            get { return this.entity; }
            set { this.entity = value; }
        }

        [XmlAttribute]
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        [XmlAttribute]
        public string DescriptionResourceId
        {
            get { return this.descriptionResourceId; }
            set { this.descriptionResourceId = value; }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true)]
    public partial class SiteMapAreaGroupSubAreaPrivilege
    {
        private string entity;
        private string privilege;

        [XmlAttribute]
        public string Entity
        {
            get { return this.entity; }
            set { this.entity = value; }
        }

        [XmlAttribute]
        public string Privilege
        {
            get { return this.privilege; }
            set { this.privilege = value; }
        }
    }
}