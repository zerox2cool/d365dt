using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Automation.Peers;
using ZStudio.D365.DeploymentHelper.Core.Base;
using ZStudio.D365.DeploymentHelper.Core.Models;
using ZStudio.D365.Shared.Data.Framework.Cmd;
using ZStudio.D365.Shared.Framework.Core.Query;

namespace ZStudio.D365.DeploymentHelper.Core.CmdLineTools
{
    [HelperType(nameof(SyncTeams2ColumnSecurityProfile))]
    public class SyncTeams2ColumnSecurityProfile : HelperToolBase
    {
        private TeamColumnSecurityProfile[] config = null;
        private Entity[] serverData = null;

        public SyncTeams2ColumnSecurityProfile(string crmConnectionString, string configJson, Dictionary<string, string> tokens, bool debugMode, int debugSleep) : base(crmConnectionString, configJson, tokens, debugMode, debugSleep)
        {
        }

        public override void PreExecute_HandlerImplementation()
        {
            try
            {
                //load config JSON
                config = JsonConvert.DeserializeObject<TeamColumnSecurityProfile[]>(ConfigJson);
            }
            catch (Exception dex)
            {
                throw new ArgumentException($"The Config JSON is invalid and cannot be deserialise to TeamColumnSecurityProfile[]. Exception: {dex.Message}");
            }

            Log(LOG_LINE);
            Log($"Config Parameters (teamprofiles to update):");
            Log(LOG_LINE);
            Log($"Team Profiles Count: {config?.Length}");
            foreach (var cfg in config)
                Debug($"Profile: Name: {cfg.ProfileName} - Teams: {cfg.Teams}");
            Log(LOG_LINE);
        }

        protected override bool OnRun_Implementation(out string exceptionMessage)
        {
            exceptionMessage = string.Empty;
            int failedCount = 0;

            return (failedCount == 0);
        }
    }
}