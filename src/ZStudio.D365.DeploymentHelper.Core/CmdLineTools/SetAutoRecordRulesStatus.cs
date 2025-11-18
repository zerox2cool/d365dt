using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Automation.Peers;
using ZStudio.D365.DeploymentHelper.Core.Base;
using ZStudio.D365.DeploymentHelper.Core.Constants;
using ZStudio.D365.DeploymentHelper.Core.Models;
using ZStudio.D365.DeploymentHelper.Core.Util;
using ZStudio.D365.Shared.Data.Framework.Cmd;
using ZStudio.D365.Shared.Framework.Core.Query;

namespace ZStudio.D365.DeploymentHelper.Core.CmdLineTools
{
    [HelperType(nameof(SetAutoRecordRulesStatus))]
    public class SetAutoRecordRulesStatus : HelperToolBase
    {
        private const string DISPLAYNAME = "Auto Record Rules";
        private const string TABLENAME = "convertrule";
        private const int COMPONENT_TYPEID = (int)SolutionHelper.ComponentType.AutoRecordRules;

        private const int DRAFT_STATE = 0;
        private const int ACTIVE_STATE = 1;
        private const int DRAFT_STATUSCODE = 1;
        private const int ACTIVE_STATUSCODE = 2;

        private Dictionary<string, object> config = null;

        public string SolutionName { get; private set; }

        public bool AllRules { get; private set; }

        public int Status { get; private set; }

        public SetAutoRecordRulesStatus(string crmConnectionString, string configJson, Dictionary<string, string> tokens, bool portalEnhancedMode, bool debugMode, int debugSleep) : base(crmConnectionString, configJson, tokens, portalEnhancedMode, debugMode, debugSleep)
        {
        }

        private Entity[] GetAllComponents()
        {
            XrmQueryExpression query = new XrmQueryExpression(TABLENAME);
            Entity[] components = Fetch.RetrieveAllEntityByQuery(query.ToQueryExpression());
            if (components?.Length > 0)
                return components;
            return null;
        }

        private Entity[] GetComponents(Guid[] ids)
        {
            return Fetch.RetrieveAllEntityByCrmIds(TABLENAME, ids);
        }

        public override void PreExecute_HandlerImplementation()
        {
            try
            {
                //load config JSON
                config = JsonConvert.DeserializeObject<Dictionary<string, object>>(ConfigJson);

                SolutionName = Convert.ToString(config["SolutionName"]);
                AllRules = Convert.ToBoolean(config["AllRules"]);
                Status = Convert.ToInt32(config["Status"]);
            }
            catch (Exception dex)
            {
                throw new ArgumentException($"The Config JSON is invalid and cannot be deserialise to Dictionary<string, object>. Exception: {dex.Message}");
            }

            Log(LOG_LINE);
            Log($"Config Parameters:");
            Log(LOG_LINE);
            Log($"AllRules: {AllRules}");
            Log($"Solution Name: {SolutionName}");
            Log($"Status: {Status}");
            Log(LOG_LINE);
        }

        protected override bool OnRun_Implementation(out string exceptionMessage)
        {
            exceptionMessage = string.Empty;

            Log($"Draft Status: {DRAFT_STATE}");
            Log($"Active Status: {ACTIVE_STATE}");
            Log($"Perform {DISPLAYNAME} Activate/Deactivate.");
            if (Status == DRAFT_STATE)
                Log($"Trying to Deactivate all {DISPLAYNAME} found.");
            else if (Status == ACTIVE_STATE)
                Log($"Trying to Activate all {DISPLAYNAME} found.");
            Log(LOG_SEGMENT);

            Entity[] components2Process = null;
            if (AllRules)
            {
                //to process all rules
                components2Process = GetAllComponents();

                Log($"Process All {DISPLAYNAME}.");
            }
            else if (!string.IsNullOrEmpty(SolutionName))
            {
                //to process all rules within a solution
                //get solution ID
                Guid? solutionId = SolutionHelper.GetSolutionIdByName(OrgService, SolutionName);
                if (solutionId == null)
                    throw new ArgumentException($"The solution name '{SolutionName}' is not found.");

                //uncomment to find component type ID
                //Entity[] components = SolutionHelper.GetSolutionComponents(OrgService, solutionId.Value);
                //var lists = (from en in components where Guid.Parse(Convert.ToString(en["objectid"])).Equals(new Guid("c8fb66db-7c25-ee11-9965-000d3a6a9fdb")) select en).ToList();
                //return false;

                Entity[] solutionComponents = SolutionHelper.GetSolutionComponentsByComponentType(OrgService, solutionId.Value, COMPONENT_TYPEID);
                if (solutionComponents?.Length > 0)
                {
                    Guid[] ids = (from sc in solutionComponents select Guid.Parse(Convert.ToString(sc["objectid"]))).ToArray();
                    components2Process = GetComponents(ids);
                }

                Log($"Process {DISPLAYNAME} in the solution '{SolutionName}' ({solutionId}).");
            }

            //activate or deactivate
            int updateCount = 0;
            int failedCount = 0;
            if (components2Process?.Length > 0)
            {
                //there are components to process
                Log($"{DISPLAYNAME} Count: {components2Process?.Length}");
                Log(LOG_SEGMENT);

                string action = "Activating";
                string actioned = "activated";
                OptionSetValue updateState = null;
                OptionSetValue updateStatusCode = null;
                if (Status == DRAFT_STATE)
                {
                    action = "Deactivating";
                    actioned = "Deactivated";
                    updateState = new OptionSetValue(DRAFT_STATE);
                    updateStatusCode = new OptionSetValue(DRAFT_STATUSCODE);
                }
                else if (Status == ACTIVE_STATE)
                {
                    action = "Activating";
                    actioned = "Activated";
                    updateState = new OptionSetValue(ACTIVE_STATE);
                    updateStatusCode = new OptionSetValue(ACTIVE_STATUSCODE);
                }

                foreach (Entity component in components2Process)
                {
                    if (component.Contains("statecode") && component["statecode"] != null && component["statecode"] is OptionSetValue)
                    {
                        if (((OptionSetValue)component["statecode"]).Value != Status)
                        {
                            StringBuilder sb = new StringBuilder();
                            try
                            {
                                //update the status
                                SetStateRequest req = new SetStateRequest()
                                {
                                    EntityMoniker = component.ToEntityReference(),
                                    State = updateState,
                                    Status = updateStatusCode,
                                };

                                sb.Append($"{action} '{component["name"]}'. ");
                                OrgService.Execute(req);
                                sb.Append($"{actioned} SUCCESS");
                                updateCount++;
                            }
                            catch (Exception ex)
                            {
                                sb.Append($"FAILED. Exception: {ex.Message}");
                                failedCount++;
                            }
                            finally
                            {
                                Log($"{sb.ToString()}");
                            }
                        }
                        else
                        {
                            Log($"'{component["name"]}' is already {actioned}.");
                        }
                    }
                }
            }
            else
            {
                //no rules found to process
                Log($"No {DISPLAYNAME} Found.");
            }

            Log(LOG_SEGMENT);
            Log($"{DISPLAYNAME} Update Count: {updateCount}; Failed Count: {failedCount}");
            ShowInfoWarning($"{Messages.WarningMessages.COMPONENT_WITH_FLOW}");

            return (failedCount == 0);
        }
    }
}