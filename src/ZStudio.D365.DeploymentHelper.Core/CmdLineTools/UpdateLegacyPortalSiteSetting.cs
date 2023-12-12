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
    [HelperType(nameof(UpdateLegacyPortalSiteSetting))]
    public class UpdateLegacyPortalSiteSetting : HelperToolBase
    {
        private AdxSiteSetting[] config = null;
        private Entity[] serverData = null;

        public UpdateLegacyPortalSiteSetting(string crmConnectionString, string configJson, Dictionary<string, string> tokens, bool debugMode) : base(crmConnectionString, configJson, tokens, debugMode)
        {
        }

        private int GetServerData()
        {
            XrmQueryExpression query = new XrmQueryExpression("adx_sitesetting");
            serverData = Fetch.RetrieveAllEntityByQuery(query.ToQueryExpression());
            if (serverData?.Length > 0)
                return serverData.Length;
            else
                return 0;
        }

        private Entity IsExist(Guid websiteId, string name)
        {
            if (serverData?.Length > 0)
            {
                var coll = from rec in serverData where rec["adx_websiteid"] != null && ((EntityReference)rec["adx_websiteid"]).Id.Equals(websiteId) && Convert.ToString(rec["adx_name"]).Equals(name, StringComparison.CurrentCultureIgnoreCase) select rec;
                if (coll?.ToArray().Length > 0)
                    return coll?.ToArray()[0];
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
                config = JsonConvert.DeserializeObject<AdxSiteSetting[]>(ConfigJson);
            }
            catch (Exception dex)
            {
                throw new ArgumentException($"The Config JSON is invalid and cannot be deserialise to AdxSiteSetting[]. Exception: {dex.Message}");
            }

            Log(LOG_LINE);
            Log($"Config Parameters (adx_sitesetting to update):");
            Log(LOG_LINE);
            Log($"Settings Count: {config?.Length}");
            foreach (var ss in config)
                Debug($"Settings: WebsiteID: {ss.WebsiteId} - Key: {ss.Name} - Value: {ss.Value}");
            Log(LOG_LINE);
        }

        protected override bool OnRun_Implementation(out string exceptionMessage)
        {
            exceptionMessage = string.Empty;

            //load server data
            int totalServerCount = GetServerData();
            Log($"Site Setting (adx_sitesetting) count on server: {totalServerCount}");
            Log($"Perform Site Settings Create/Update.");
            Log(LOG_SEGMENT);

            int updateCount = 0;
            int createCount = 0;
            foreach (var d in config)
            {
                Log($"Updating Setting Key {d.Name} for WebsiteID '{d.WebsiteId}'.");

                Entity record = IsExist(Guid.Parse(d.WebsiteId), d.Name);
                if (record != null)
                {
                    //data found to update
                    Entity upd = new Entity("adx_sitesetting", record.Id);
                    upd["adx_value"] = d.Value;
                    OrgService.Update(upd);
                    Log($"Updated adx_sitesetting: {record.Id}");
                    updateCount++;

                    Debug($"Setting '{d.Name}' for WebsiteID '{d.WebsiteId}' updated to the value '{d.Value}'.");
                }
                else
                {
                    //insert
                    Log($"adx_sitesetting with name '{d.Name}' not found.");
                    Entity cre = new Entity("adx_sitesetting");
                    cre["adx_websiteid"] = new EntityReference("adx_website", Guid.Parse(d.WebsiteId));
                    cre["adx_name"] = d.Name;
                    cre["adx_value"] = d.Value;
                    Guid id = OrgService.Create(cre);
                    Log($"Created adx_sitesetting: {id}");
                    createCount++;

                    Debug($"Setting '{d.Name}' for WebsiteID '{d.WebsiteId}' with the value '{d.Value}' has been created.");
                }

                Log(LOG_SEGMENT);
            }
            Log($"Site Setting Success Count - Updated: {updateCount}; Created: {createCount};");

            return true;
        }
    }
}