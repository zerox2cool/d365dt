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
    [HelperType(nameof(UpdatePortalSiteSetting))]
    public class UpdatePortalSiteSetting : HelperToolBase
    {
        private AdxSiteSetting[] config = null;
        private Entity[] serverData = null;

        private string TableName = "adx_sitesetting";
        private string TableWebsiteName = "adx_website";
        private string ColumnName = "adx_name";
        private string ColumnWebsiteId = "adx_websiteid";
        private string ColumnValue = "adx_value";

        public UpdatePortalSiteSetting(string crmConnectionString, string configJson, Dictionary<string, string> tokens, bool portalEnhancedMode, bool debugMode, int debugSleep) : base(crmConnectionString, configJson, tokens, portalEnhancedMode, debugMode, debugSleep)
        {
            //portal mode check
            if (PortalEnhancedMode)
            {
                //update table and column schema for enhanced mode
                TableName = "mspp_sitesetting";
                TableWebsiteName = "mspp_website";
                ColumnName = "mspp_name";
                ColumnWebsiteId = "mspp_websiteid";
                ColumnValue = "mspp_value";
            }
        }

        private int GetServerData()
        {
            XrmQueryExpression query = new XrmQueryExpression(TableName);
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
                var coll = from rec in serverData where rec[ColumnWebsiteId] != null && ((EntityReference)rec[ColumnWebsiteId]).Id.Equals(websiteId) && Convert.ToString(rec[ColumnName]).Equals(name, StringComparison.CurrentCultureIgnoreCase) select rec;
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
            Log($"Config Parameters ({TableName} to update):");
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
            Log($"Site Setting ({TableName}) count on server: {totalServerCount}");
            Log($"Perform Site Settings Create/Update.");
            Log(LOG_SEGMENT);

            int updateCount = 0;
            int createCount = 0;
            int failedCount = 0;
            foreach (var d in config)
            {
                Log($"Updating Setting Key {d.Name} for WebsiteID '{d.WebsiteId}'.");

                try
                {
                    Entity record = IsExist(Guid.Parse(d.WebsiteId), d.Name);
                    if (record != null)
                    {
                        //data found to update
                        Entity upd = new Entity(TableName, record.Id);
                        upd[ColumnValue] = d.Value;
                        OrgService.Update(upd);
                        Log($"Updated {TableName}: {record.Id}");
                        updateCount++;

                        Debug($"Setting '{d.Name}' for WebsiteID '{d.WebsiteId}' updated to the value '{d.Value}'.");
                    }
                    else
                    {
                        //insert
                        Log($"{TableName} with name '{d.Name}' not found.");
                        Entity cre = new Entity(TableName);
                        cre[ColumnWebsiteId] = new EntityReference(TableWebsiteName, Guid.Parse(d.WebsiteId));
                        cre[ColumnName] = d.Name;
                        cre[ColumnValue] = d.Value;
                        Guid id = OrgService.Create(cre);
                        Log($"Created {TableName}: {id}");
                        createCount++;

                        Debug($"Setting '{d.Name}' for WebsiteID '{d.WebsiteId}' with the value '{d.Value}' has been created.");
                    }

                    Log(LOG_SEGMENT);
                }
                catch (Exception ex)
                {
                    Log($"FAILED to update {TableName}: {d.Name}. Exception: {ex.Message}");
                    failedCount++;
                }
            }
            Log($"Site Setting Success Count - Updated: {updateCount}; Created: {createCount};");

            return (failedCount == 0);
        }
    }
}