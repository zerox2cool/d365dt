using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using ZStudio.D365.DeploymentHelper.Core.Base;
using ZStudio.D365.Shared.Framework.Core.Query;

namespace ZStudio.D365.DeploymentHelper.Core.CmdLineTools
{
    [HelperType(nameof(DeleteTablePermission))]
    public class DeleteTablePermission : HelperToolBase
    {
        private Dictionary<string, object> config = null;

        public Guid WebsiteId { get; private set; }

        public DeleteTablePermission(string crmConnectionString, string configJson, Dictionary<string, string> tokens, bool debugMode, int debugSleep) : base(crmConnectionString, configJson, tokens, debugMode, debugSleep)
        {
        }

        private Entity[] GetTablePermission(Guid websiteId)
        {
            XrmQueryExpression query = new XrmQueryExpression("adx_entitypermission")
                .Condition("adx_websiteid", ConditionOperator.Equal, websiteId);
            Entity[] coll = Fetch.RetrieveAllEntityByQuery(query.ToQueryExpression());
            if (coll?.Length > 0)
                return coll;
            return null;
        }

        public override void PreExecute_HandlerImplementation()
        {
            try
            {
                //load config JSON
                config = JsonConvert.DeserializeObject<Dictionary<string, object>>(ConfigJson);

                WebsiteId = Guid.Parse(Convert.ToString(config["WebsiteId"]));
            }
            catch (Exception dex)
            {
                throw new ArgumentException($"The Config JSON is invalid and cannot be deserialise to Dictionary<string, object>. Exception: {dex.Message}");
            }

            Log(LOG_LINE);
            Log($"Config Parameters:");
            Log(LOG_LINE);
            Log($"WebsiteId: {WebsiteId}");
            Log(LOG_LINE);
        }

        protected override bool OnRun_Implementation(out string exceptionMessage)
        {
            exceptionMessage = string.Empty;

            //load data
            Entity[] tablePermissions = GetTablePermission(WebsiteId);
            int count = 0;
            if (tablePermissions?.Length > 0)
                count = tablePermissions.Length;

            Log($"Deleting Table Permission (adx_entitypermission) for WebsiteId: {WebsiteId}");
            Log($"Table Permission (adx_entitypermission) Count: {count}");
            Log(LOG_SEGMENT);

            //delete data for a website and parent is null
            int deleteCount = 0;
            int failedCount = 0;
            if (tablePermissions?.Length > 0)
            {
                foreach (var d in tablePermissions)
                {
                    //delete parent permissions
                    if (!d.Contains("adx_parententitypermission") || d["adx_parententitypermission"] == null)
                    {
                        try
                        {
                            OrgService.Delete(d.LogicalName, d.Id);
                            Log($"Deleted adx_entitypermission: {d.Id}");
                            deleteCount++;
                        }
                        catch (Exception ex)
                        {
                            Log($"FAILED to delete adx_entitypermission: {d.Id}. Exception: {ex.Message}");
                            failedCount++;
                        }
                    }
                }
                Log($"Deleted {deleteCount} row(s) of adx_entitypermission for the website ID {WebsiteId} that are parent permission.");
            }
            else
                Log($"Nothing in adx_entitypermission to delete.");
            
            return (failedCount == 0);
        }
    }
}