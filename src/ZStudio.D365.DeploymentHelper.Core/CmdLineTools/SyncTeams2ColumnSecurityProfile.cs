using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Description;
using System.Windows.Automation.Peers;
using ZStudio.D365.DeploymentHelper.Core.Base;
using ZStudio.D365.DeploymentHelper.Core.DataObjects;
using ZStudio.D365.DeploymentHelper.Core.Models;
using ZStudio.D365.DeploymentHelper.Core.Models.Entities;
using ZStudio.D365.Shared.Data.Framework.Cmd;
using ZStudio.D365.Shared.Framework.Core.Query;

namespace ZStudio.D365.DeploymentHelper.Core.CmdLineTools
{
    [HelperType(nameof(SyncTeams2ColumnSecurityProfile))]
    public class SyncTeams2ColumnSecurityProfile : HelperToolBase
    {
        private CrmSystemDataObject csdo = null;
        private Teams2ColumnSecurityProfile config = null;
        private TeamProfiles[] serverData = null;

        private string TableName = TeamProfiles.EntityLogicalName;

        public SyncTeams2ColumnSecurityProfile(string crmConnectionString, string configJson, Dictionary<string, string> tokens, bool portalEnhancedMode, bool debugMode, int debugSleep) : base(crmConnectionString, configJson, tokens, portalEnhancedMode, debugMode, debugSleep)
        {
        }

        private int GetServerData()
        {
            XrmQueryExpression query = new XrmQueryExpression(TableName);
            serverData = Fetch.RetrieveAllEntityByQuery<TeamProfiles>(query.ToQueryExpression());
            if (serverData?.Length > 0)
                return serverData.Length;
            else
                return 0;
        }

        private TeamProfiles IsExist(Guid profileId, Guid teamId)
        {
            if (serverData?.Length > 0)
            {
                var coll = from rec in serverData where rec.FieldSecurityProfileId != null && rec.TeamId != null && rec.FieldSecurityProfileId.Equals(profileId) && rec.TeamId.Equals(teamId) select rec;
                if (coll?.ToArray().Length > 0)
                    return coll?.ToArray()[0];
                else
                    return null;
            }
            else
                return null;
        }

        private TeamProfiles[] GetTeamsInProfile(Guid profileId)
        {
            if (serverData?.Length > 0)
            {
                var coll = from rec in serverData where rec.FieldSecurityProfileId != null && rec.TeamId != null && rec.FieldSecurityProfileId.Equals(profileId) select rec;
                if (coll?.ToArray().Length > 0)
                    return coll?.ToArray();
                else
                    return null;
            }
            else
                return null;
        }

        public override void PreExecute_HandlerImplementation()
        {
            try
            {
                //load config JSON
                config = JsonConvert.DeserializeObject<Teams2ColumnSecurityProfile>(ConfigJson);
            }
            catch (Exception dex)
            {
                throw new ArgumentException($"The Config JSON is invalid and cannot be deserialise to Teams2ColumnSecurityProfile. Exception: {dex.Message}");
            }

            try
            {
                //init data object
                csdo = new CrmSystemDataObject(OrgService);

                //fetch system data
                FetchConfigurationData();
            }
            catch (Exception dex)
            {
                throw new ArgumentException($"The Config JSON contain invalid data that cannot be fetch from the target system. Exception: {dex.Message}");
            }

            Log(LOG_LINE);
            Log($"Config Parameters (teamprofiles_association to update):");
            Log($"IsSync: {config?.Settings?.IsSync}");
            Log(LOG_LINE);
            Log($"Team Profiles Count: {config?.ColumnSecurityProfiles?.Length}");
            foreach (var cfg in config?.ColumnSecurityProfiles)
            {
                Debug($"Profile: Name: {cfg.ProfileName} ({cfg.ProfileId}) - Team Count: {cfg.BusinessUnitTeams?.Count}");
                foreach (var t in cfg.BusinessUnitTeams)
                    Debug($"Team Name: {t.TeamName} ({t.BusinessUnitName}) - {t.TeamId} ({t.BusinessUnitId})");
            }
            Log(LOG_LINE);
        }

        private void FetchConfigurationData()
        {
            foreach (var cfg in config?.ColumnSecurityProfiles)
            {
                Entity profile = csdo.GetFieldSecurityProfileByName(cfg.ProfileName);
                if (profile == null)
                    throw new ArgumentException($"The Field Security Profile '{cfg.ProfileName}' cannot be found in the target system.");
                cfg.ProfileId = profile.Id;

                foreach (var t in cfg.BusinessUnitTeams)
                {
                    Team team = csdo.GetTeamByNameAndBU(t.BusinessUnitName, t.TeamName);
                    if (team == null)
                        throw new ArgumentException($"The Team '{t.TeamName}' in Business Unit '{t.BusinessUnitName}' cannot be found in the target system.");
                    t.BusinessUnitId = team.BusinessUnitId.Id;
                    t.TeamId = team.Id;
                }
            }
        }

        protected override bool OnRun_Implementation(out string exceptionMessage)
        {
            exceptionMessage = string.Empty;

            //load server data
            int totalServerCount = GetServerData();
            Log($"Team Profile ({TableName}) count on server: {totalServerCount}");
            Log($"Perform Team Profile Update.");
            Log(LOG_SEGMENT);

            int deleteCount = 0;
            int createCount = 0;
            int existCount = 0;
            int failedCount = 0;
            foreach (var cfg in config?.ColumnSecurityProfiles)
            {
                Log($"Syncing Profile: Name: {cfg.ProfileName} ({cfg.ProfileId}) - Team Count: {cfg.BusinessUnitTeams?.Count}");

                //scan for teams to remove from profile
                List<TeamProfiles> toDeleteFromTeamProfiles = new List<TeamProfiles>();
                TeamProfiles[] teamsInProfile = GetTeamsInProfile(cfg.ProfileId.Value);
                if (teamsInProfile?.Length > 0)
                {
                    foreach (var tip in teamsInProfile)
                    {
                        bool isFound = false;
                        foreach (var t in cfg.BusinessUnitTeams)
                        {
                            if (tip.TeamId != null && t.TeamId != null && tip.TeamId.Equals(t.TeamId.Value))
                            {
                                //found, no need to delete
                                isFound = true;
                            }
                        }

                        //not found, need to delete
                        if (!isFound)
                            toDeleteFromTeamProfiles.Add(tip);
                    }

                    if (toDeleteFromTeamProfiles?.Count > 0)
                    {
                        foreach (var td in toDeleteFromTeamProfiles)
                        {
                            //delete association
                            DisassociateRequest disassociateReq = new DisassociateRequest
                            {
                                Target = new EntityReference("fieldsecurityprofile", cfg.ProfileId.Value),
                                RelatedEntities = new EntityReferenceCollection() { new EntityReference(Team.EntityLogicalName, td.TeamId.Value) },
                                Relationship = new Relationship("teamprofiles_association"),
                            };
                            OrgService.Execute(disassociateReq);

                            deleteCount++;
                            Log($"DELETE: Disassociate team {td.Id} from the profile {cfg.ProfileName}.");

                        }
                    }
                }

                //associate teams to profile
                foreach (var t in cfg.BusinessUnitTeams)
                {
                    Entity record = IsExist(cfg.ProfileId.Value, t.TeamId.Value);
                    if (record == null)
                    {
                        //no team association found to profile, create new
                        AssociateRequest associateReq = new AssociateRequest
                        {
                            Target = new EntityReference("fieldsecurityprofile", cfg.ProfileId.Value),
                            RelatedEntities = new EntityReferenceCollection() { new EntityReference(Team.EntityLogicalName, t.TeamId.Value) },
                            Relationship = new Relationship("teamprofiles_association"),
                        };
                        OrgService.Execute(associateReq);

                        createCount++;
                        Log($"CREATE: Associate team {t.TeamName} ({t.BusinessUnitName}) to the profile {cfg.ProfileName}.");
                    }
                    else
                    {
                        //already associated
                        existCount++;
                        Log($"EXIST: Team {t.TeamName} ({t.BusinessUnitName}) is already associated to the profile {cfg.ProfileName}.");
                    }
                }
            }

            Log($"Team Profile Success Count - No Change: {existCount}; Created: {createCount}; Deleted: {deleteCount};");

            return (failedCount == 0);
        }
    }
}