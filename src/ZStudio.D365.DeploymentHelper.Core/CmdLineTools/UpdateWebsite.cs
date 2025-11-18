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
    [HelperType(nameof(UpdateWebsite))]
    public class UpdateWebsite : HelperToolBase
    {
        private AdxWebsite[] config = null;
        private Entity[] serverData = null;

        private string TableName = "adx_website";
        private string ColumnPrimaryDomainName = "adx_primarydomainname";

        public UpdateWebsite(string crmConnectionString, string configJson, Dictionary<string, string> tokens, bool portalEnhancedMode, bool debugMode, int debugSleep) : base(crmConnectionString, configJson, tokens, portalEnhancedMode, debugMode, debugSleep)
        {
            //portal mode check
            if (PortalEnhancedMode)
            {
                //update table and column schema for enhanced mode
                TableName = "mspp_website";
                ColumnPrimaryDomainName = "mspp_primarydomainname";
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

        private Entity IsExist(Guid websiteId)
        {
            if (serverData?.Length > 0)
            {
                var coll = from rec in serverData where rec.Id.Equals(websiteId) select rec;
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
                config = JsonConvert.DeserializeObject<AdxWebsite[]>(ConfigJson);
            }
            catch (Exception dex)
            {
                throw new ArgumentException($"The Config JSON is invalid and cannot be deserialise to AdxWebsite[]. Exception: {dex.Message}");
            }

            Log(LOG_LINE);
            Log($"Config Parameters ({TableName} to update):");
            Log(LOG_LINE);
            Log($"Website Count: {config?.Length}");
            foreach (var ss in config)
                Debug($"Website: WebsiteID: {ss.WebsiteId} - DomainName: {ss.PrimaryDomainName}");
            Log(LOG_LINE);
        }

        protected override bool OnRun_Implementation(out string exceptionMessage)
        {
            exceptionMessage = string.Empty;

            //load server data
            int totalServerCount = GetServerData();
            Log($"Website ({TableName}) count on server: {totalServerCount}");
            Log($"Perform Website Update.");
            Log(LOG_SEGMENT);

            int updateCount = 0;
            int createCount = 0;
            int failedCount = 0;
            foreach (var d in config)
            {
                Log($"Updating Domain Name for WebsiteID '{d.WebsiteId}'.");

                try
                {
                    Entity record = IsExist(Guid.Parse(d.WebsiteId));
                    if (record != null)
                    {
                        //data found to update
                        Entity upd = new Entity(TableName, record.Id);
                        upd[ColumnPrimaryDomainName] = d.PrimaryDomainName;
                        OrgService.Update(upd);
                        Log($"Updated {TableName} domain name for WebsiteID: {record.Id}");
                        updateCount++;

                        Debug($"Website for WebsiteID '{d.WebsiteId}' updated to with the domain '{d.PrimaryDomainName}'.");
                    }
                    else
                    {
                        //insert not supported for website, only update
                        Log($"{TableName} with ID '{d.WebsiteId}' not found.");
                    }

                    Log(LOG_SEGMENT);
                }
                catch (Exception ex)
                {
                    Log($"FAILED to update {TableName}: {d.WebsiteId}. Exception: {ex.Message}");
                    failedCount++;
                }
            }
            Log($"Website Success Count - Updated: {updateCount}; Created: {createCount};");

            return (failedCount == 0);
        }
    }
}