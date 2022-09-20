using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using ZD365DT.DeploymentTool.Utils.SitemapCustomization;

namespace ZD365DT.DeploymentTool.Utils.SitemapCustomization
{
    [Serializable]
    public abstract class HasTitlesAndDesciption
    {
        private SiteMapTitle[] titles;
        private SiteMapDescription[] descriptions;

        [XmlArrayItem("Title", IsNullable = false)]
        public virtual SiteMapTitle[] Titles
        {
            get { return this.titles; }
            set { this.titles = value; }
        }

        [XmlArrayItem("Description", IsNullable = false)]
        public virtual SiteMapDescription[] Descriptions
        {
            get { return this.descriptions; }
            set { this.descriptions = value; }
        }

        #region Merge Titles

        public void MergeTitles(SiteMapTitle[] toMerge)
        {
            if (toMerge != null)
            {
                foreach (SiteMapTitle mergeTitle in toMerge)
                {
                    bool titleFound = false;
                    if (Titles != null)
                    {
                        foreach (SiteMapTitle souceTitle in Titles)
                        {
                            if (mergeTitle.LCID == souceTitle.LCID)
                            {
                                titleFound = true;
                                souceTitle.Title = mergeTitle.Title;
                                break;
                            }
                        }
                    }
                    if (!titleFound)
                    {
                        Add(mergeTitle);
                    }
                }
            }
        }

        public void Add(SiteMapTitle newTitle)
        {
            List<SiteMapTitle> list = new List<SiteMapTitle>();
            list.AddRange(Titles);
            list.Add(newTitle);

            Titles = list.ToArray();
        }

        #endregion Merge Titles

        #region Merge Descriptions

        public void MergeDescriptions(SiteMapDescription[] toMerge)
        {
            if (toMerge != null)
            {
                foreach (SiteMapDescription mergeDesc in toMerge)
                {
                    bool descFound = false;
                    if (Descriptions != null)
                    {
                        foreach (SiteMapDescription souceDesc in Descriptions)
                        {
                            if (mergeDesc.LCID == souceDesc.LCID)
                            {
                                descFound = true;
                                souceDesc.Description = mergeDesc.Description;
                                break;
                            }
                        }
                    }
                    if (!descFound)
                    {
                        Add(mergeDesc);
                    }
                }
            }
        }

        public void Add(SiteMapDescription newDescription)
        {
            List<SiteMapDescription> list = new List<SiteMapDescription>();
            list.AddRange(Descriptions);
            list.Add(newDescription);

            Descriptions = list.ToArray();
        }

        #endregion Merge Descriptions
    }

    public partial class SiteMap
    {
        public SiteMapArea[] GetAreas()
        {
            List<SiteMapArea> list = new List<SiteMapArea>();
            if (Areas != null)
            {
                list.AddRange(Areas);
            }
            return list.ToArray();
        }

        public SiteMapArea GetAreaWithId(string id)
        {
            SiteMapArea result = null;
            foreach (SiteMapArea area in GetAreas())
            {
                if (area.Id == id)
                {
                    result = area;
                    break;
                }
            }
            return result;
        }

        public void Add(SiteMapArea area)
        {
            List<SiteMapArea> list = new List<SiteMapArea>();
            list.AddRange(GetAreas());
            list.Add(area);

            Areas = list.ToArray();
        }
    }


    public partial class SiteMapArea
    {
        public SiteMapAreaGroup[] GetGroups()
        {
            List<SiteMapAreaGroup> list = new List<SiteMapAreaGroup>();
            if (Groups != null)
            {
                list.AddRange(Groups);
            }
            return list.ToArray();
        }

        public SiteMapAreaGroup GetGroupWithId(string id)
        {
            SiteMapAreaGroup result = null;
            foreach (SiteMapAreaGroup group in GetGroups())
            {
                if (group.Id == id)
                {
                    result = group;
                    break;
                }
            }
            return result;
        }

        public void Add(SiteMapAreaGroup group)
        {
            List<SiteMapAreaGroup> list = new List<SiteMapAreaGroup>();
            list.AddRange(GetGroups());
            list.Add(group);

            Groups = list.ToArray();
        }

        public void UpdateAttributes(SiteMapArea toMerge)
        {
            this.title = toMerge.title;
            this.resourceId = toMerge.resourceId;
            this.icon = toMerge.icon;
            this.url = toMerge.url;
            this.showGroups = toMerge.showGroups;
            this.license = toMerge.license;
            this.description = toMerge.description;
            this.descriptionResourceId = toMerge.descriptionResourceId;

            MergeDescriptions(toMerge.Descriptions);
            MergeTitles(toMerge.Titles);
        }
    }


    public partial class SiteMapAreaGroup
    {
        public SiteMapAreaGroupSubArea[] GetSubAreas()
        {
            List<SiteMapAreaGroupSubArea> list = new List<SiteMapAreaGroupSubArea>();
            if (SubAreas != null)
            {
                list.AddRange(SubAreas);
            }
            return list.ToArray();
        }

        public SiteMapAreaGroupSubArea GetSubAreaWithId(string id)
        {
            SiteMapAreaGroupSubArea result = null;
            foreach (SiteMapAreaGroupSubArea subArea in GetSubAreas())
            {
                if (subArea.Id == id)
                {
                    result = subArea;
                    break;
                }
            }
            return result;
        }

        public void Add(SiteMapAreaGroupSubArea subArea)
        {
            List<SiteMapAreaGroupSubArea> list = new List<SiteMapAreaGroupSubArea>();
            list.AddRange(GetSubAreas());
            list.Add(subArea);

            SubAreas = list.ToArray();
        }

        public void UpdateAttributes(SiteMapAreaGroup toMerge)
        {
            this.title = toMerge.title;
            this.icon = toMerge.icon;
            this.url = toMerge.url;
            this.resourceId = toMerge.resourceId;
            this.isProfile = toMerge.isProfile;
            this.license = toMerge.license;
            this.description = toMerge.description;
            this.descriptionResourceId = toMerge.descriptionResourceId;

            MergeDescriptions(toMerge.Descriptions);
            MergeTitles(toMerge.Titles);
        }
    }

    public partial class SiteMapAreaGroupSubArea
    {
        public SiteMapAreaGroupSubAreaPrivilege[] GetPrivileges()
        {
            List<SiteMapAreaGroupSubAreaPrivilege> list = new List<SiteMapAreaGroupSubAreaPrivilege>();
            if (Privileges != null)
            {
                list.AddRange(Privileges);
            }
            return list.ToArray();
        }

        public SiteMapAreaGroupSubAreaPrivilege GetPrivilegeForEntity(string entity)
        {
            SiteMapAreaGroupSubAreaPrivilege result = null;
            foreach (SiteMapAreaGroupSubAreaPrivilege privilege in GetPrivileges())
            {
                if (privilege.Entity == entity)
                {
                    result = privilege;
                    break;
                }
            }
            return result;
        }
        
        public void Add(SiteMapAreaGroupSubAreaPrivilege privilege)
        {
            List<SiteMapAreaGroupSubAreaPrivilege> privs = new List<SiteMapAreaGroupSubAreaPrivilege>();
            privs.AddRange(GetPrivileges());
            privs.Add(privilege);

            Privileges = privs.ToArray();
        }

        public void UpdateAttributes(SiteMapAreaGroupSubArea toMerge)
        {
            this.title = toMerge.title;
            this.resourceId = toMerge.resourceId;
            this.icon = toMerge.icon;
            this.outlookShortcutIcon = toMerge.outlookShortcutIcon;
            this.url = toMerge.url;
            this.passParams = toMerge.passParams;
            this.client = toMerge.client;
            this.availableOffline = toMerge.availableOffline;
            this.license = toMerge.license;
            this.entity = toMerge.entity;
            this.description = toMerge.description;
            this.descriptionResourceId = toMerge.DescriptionResourceId;

            MergeDescriptions(toMerge.Descriptions);
            MergeTitles(toMerge.Titles);
        }
    }
}
