using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using ZStudio.D365.DeploymentHelper.Core.Base;
using ZStudio.D365.Shared.Framework.Core.Query;

namespace ZStudio.D365.DeploymentHelper.Core.CmdLineTools
{
    [HelperType(nameof(DeleteTablePermission))]
    public class DeleteTablePermission : HelperToolBase
    {
        private Dictionary<string, object> config = null;

        public Guid WebsiteId { get; private set; }

        private string TableName = "adx_entitypermission";
        private string ColumnWebsiteId = "adx_websiteid";
        private string ColumnParentPermission = "adx_parententitypermission";

        public DeleteTablePermission(string crmConnectionString, string configJson, Dictionary<string, string> tokens, bool portalEnhancedMode, bool debugMode, int debugSleep) : base(crmConnectionString, configJson, tokens, portalEnhancedMode, debugMode, debugSleep)
        {
            //portal mode check
            if (PortalEnhancedMode)
            {
                //update table and column schema for enhanced mode
                TableName = "mspp_entitypermission";
                ColumnWebsiteId = "mspp_websiteid";
                ColumnParentPermission = "mspp_parententitypermission";
            }
        }

        private Entity[] GetTablePermission(Guid websiteId)
        {
            XrmQueryExpression query = new XrmQueryExpression(TableName)
                .Condition(ColumnWebsiteId, ConditionOperator.Equal, websiteId);
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

            Log($"Deleting Table Permission ({TableName}) for WebsiteId: {WebsiteId}");
            Log($"Table Permission ({TableName}) Count: {count}");
            Log(LOG_SEGMENT);

            //delete data for a website and parent is null
            int deleteCount = 0;
            int failedCount = 0;
            if (tablePermissions?.Length > 0)
            {
                foreach (var d in tablePermissions)
                {
                    //delete parent permissions
                    if (!d.Contains(ColumnParentPermission) || d[ColumnParentPermission] == null)
                    {
                        try
                        {
                            OrgService.Delete(d.LogicalName, d.Id);
                            Log($"Deleted {TableName}: {d.Id}");
                            deleteCount++;
                        }
                        catch (Exception ex)
                        {
                            Log($"FAILED to delete {TableName}: {d.Id}. Exception: {ex.Message}");
                            failedCount++;
                        }
                    }
                }
                Log($"Deleted {deleteCount} row(s) of {TableName} for the website ID {WebsiteId} that are parent permission.");
            }
            else
                Log($"Nothing in {TableName} to delete.");
            
            return (failedCount == 0);
        }
    }
}