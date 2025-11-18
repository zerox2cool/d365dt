using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZStudio.D365.DeploymentHelper.Core.Base;
using ZStudio.D365.DeploymentHelper.Core.Models.Entities;

namespace ZStudio.D365.DeploymentHelper.Core.DataObjects
{
    public class CrmSystemDataObject
    {
        CrmDataCache<BusinessUnit> _cacheBusinessUnit = new CrmDataCache<BusinessUnit>(BusinessUnit.EntityLogicalName, false);
        CrmDataCache<Team> _cacheTeam = new CrmDataCache<Team>(Team.EntityLogicalName, false);

        public IOrganizationService OrganizationService { get; set; }

        public CrmSystemDataObject(IOrganizationService svc)
        {
            OrganizationService = svc;
        }

        private void LoadBusinessUnitCache()
        {
            _cacheBusinessUnit.GetCache(OrganizationService);
        }

        private void LoadTeamCache()
        {
            _cacheTeam.GetCache(OrganizationService);
        }

        public BusinessUnit GetBusinessUnitById(Guid id)
        {
            return _cacheBusinessUnit.GetById(OrganizationService, id);
        }

        public BusinessUnit GetRootBusinessUnit()
        {
            LoadBusinessUnitCache();
            if (_cacheBusinessUnit?.Cache != null)
                return (from bu in _cacheBusinessUnit.Cache where bu.ParentBusinessUnitId == null select bu).First();
            return null;
        }

        public BusinessUnit GetBusinessUnitByName(string name)
        {
            return _cacheBusinessUnit.GetByStringAttribute(OrganizationService, "name", name);
        }

        public Team GetTeamById(Guid id)
        {
            return _cacheTeam.GetById(OrganizationService, id);
        }

        public Team GetTeamByName(string name)
        {
            return _cacheTeam.GetByStringAttribute(OrganizationService, "name", name);
        }

        public Team GetTeamByNameAndBU(string buName, string name)
        {
            BusinessUnit bu = GetBusinessUnitByName(buName);
            if (bu != null)
            {
                LoadTeamCache();
                return (from t in _cacheTeam.Cache where t.BusinessUnitId != null && t.BusinessUnitId.Id == t.Id && t.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) select t).First();
            }
            return null;
        }
    }
}