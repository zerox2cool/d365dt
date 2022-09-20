/**********************************************************
Copyright © Zero.Studio 2022
Created By: winson
Created On: 11 aug 2016
Updated By: winson
Updated On: 05 may 2020
**********************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using ZD365DT.DeploymentTool.Utils;

namespace ZD365DT.DeploymentTool.Context
{
    /// <summary>
    /// A class maintaining the current deployment organization context information such as OrgId, UserId, OrgName
    /// </summary>
    public class OrganizationContext
    {
        //context properties
        public Guid CurrentUserId { private set; get; }
        public Guid CurrentUserBusinessUnitId { private set; get; }
        public string UserName { private set; get; }
        public string UserFullName { private set; get; }
        public Guid OrganizationId { private set; get; }
        public bool OrganizationAuditEnabled { private set; get; }
        public string OrganizationName { private set; get; }
        public Version InitialVersion { private set; get; }
        public string CrmVersionYear { private set; get; }
        public int MajorVersion { private set; get; }

        public OrganizationContext(IOrganizationService svc)
        {
            SetContext(svc);
        }

        private void SetContext(IOrganizationService svc)
        {
            //who am i request
            WhoAmIResponse who = (WhoAmIResponse)svc.Execute(new WhoAmIRequest());
            Guid orgId = who.OrganizationId;
            Guid userId = who.UserId;

            //retrieve the organization record
            Organization org = svc.Retrieve(Organization.EntityLogicalName, orgId, new ColumnSet(true)) as Organization;

            //retrieve the systemuser record
            SystemUser su = svc.Retrieve(SystemUser.EntityLogicalName, userId, new ColumnSet(true)) as SystemUser;

            //set variable values
            CurrentUserId = userId;
            CurrentUserBusinessUnitId = su.BusinessUnitId.Id;
            UserName = su.DomainName;
            UserFullName = su.FullName;
            OrganizationId = orgId;
            OrganizationName = org.Name;
            OrganizationAuditEnabled = org.IsAuditEnabled.Value;
            InitialVersion = new Version(org.InitialVersion);
            SetCrmYearVersion(InitialVersion.Major, InitialVersion.Minor);
        }

        private void SetCrmYearVersion(int majorVersion, int minorVersion)
        {
            MajorVersion = majorVersion;
            switch (majorVersion)
            {
                case 4:
                    CrmVersionYear = "4.0";
                    break;

                case 5:
                    CrmVersionYear = "2011";
                    break;

                case 6:
                    CrmVersionYear = "2013";
                    break;

                case 7:
                    CrmVersionYear = "2015";
                    break;

                case 8:
                    if (minorVersion < 2)
                        CrmVersionYear = "2016";
                    else
                        CrmVersionYear = "D365";
                    break;

                case 9:
                    CrmVersionYear = "D365";
                    break;

                default:
                    CrmVersionYear = "Version Unknown";
                    break;
            }
        }

        public void DisplayCurrentContext()
        {
            //print summary as info message
            Logger.LogInfo("   Current UserId: {0}", CurrentUserId.ToString());
            Logger.LogInfo("    BusinesUnitId: {0}", CurrentUserBusinessUnitId.ToString());
            Logger.LogInfo("         Username: {0}", UserName);
            Logger.LogInfo("         Fullname: {0}", UserFullName);
            Logger.LogInfo("   OrganizationId: {0}", OrganizationId.ToString());
            Logger.LogInfo("Organization Name: {0}", OrganizationName);
            Logger.LogInfo("    Audit Enabled: {0}", OrganizationAuditEnabled);
            Logger.LogInfo("          Version: {0}", InitialVersion.ToString());
            Logger.LogInfo("              CRM: {0}", CrmVersionYear);
            Logger.LogInfo(" ");
        }
    }
}