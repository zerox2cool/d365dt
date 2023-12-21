using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Automation.Peers;
using ZStudio.D365.DeploymentHelper.Core.Base;
using ZStudio.D365.DeploymentHelper.Core.Models;
using ZStudio.D365.DeploymentHelper.Core.Util;
using ZStudio.D365.Shared.Data.Framework.Cmd;
using ZStudio.D365.Shared.Framework.Core.Query;

namespace ZStudio.D365.DeploymentHelper.Core.CmdLineTools
{
    [HelperType(nameof(SetAutoRecordRulesStatus))]
    public class SetAutoRecordRulesStatus : HelperToolBase
    {
        private const string ARC_TABLENAME = "convertrule";
        private const int DRAFT_STATE = 0;
        private const int ACTIVE_STATE = 1;
        private const int DRAFT_STATUSCODE = 1;
        private const int ACTIVE_STATUSCODE = 2;
        private const int AUTO_RECORD_RULES_COMPONENT_TYPEID = 154;

        private Dictionary<string, object> config = null;

        public string SolutionName { get; private set; }

        public bool AllRules { get; private set; }

        public int Status { get; private set; }

        public SetAutoRecordRulesStatus(string crmConnectionString, string configJson, Dictionary<string, string> tokens, bool debugMode, int debugSleep) : base(crmConnectionString, configJson, tokens, debugMode, debugSleep)
        {
        }

        private Entity[] GetAllRules()
        {
            XrmQueryExpression query = new XrmQueryExpression(ARC_TABLENAME);
            Entity[] rules = Fetch.RetrieveAllEntityByQuery(query.ToQueryExpression());
            if (rules?.Length > 0)
                return rules;
            return null;
        }

        private Entity[] GetRules(Guid[] ids)
        {
            return Fetch.RetrieveAllEntityByCrmIds(ARC_TABLENAME, ids);
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
            Log($"Perform Auto Record Rules Activate/Deactivate.");
            if (Status == DRAFT_STATE)
                Log($"Trying to Deactivate all Auto Record Rules found.");
            else if (Status == ACTIVE_STATE)
                Log($"Trying to Activate all Auto Record Rules found.");
            Log(LOG_SEGMENT);

            Entity[] arcComponents = null;
            if (AllRules)
            {
                //to process all rules
                arcComponents = GetAllRules();

                Log($"Process All Rules.");
            }
            else if (!string.IsNullOrEmpty(SolutionName))
            {
                //to process all rules within a solution
                //get solution ID
                Guid? solutionId = SolutionHelper.GetSolutionIdByName(OrgService, SolutionName);
                if (solutionId == null)
                    throw new ArgumentException($"The solution name '{SolutionName}' is not found.");

                //Entity[] components = SolutionHelper.GetSolutionComponents(OrgService, solutionId.Value);
                //var lists = (from en in components where Guid.Parse(Convert.ToString(en["objectid"])).Equals(new Guid("c8fb66db-7c25-ee11-9965-000d3a6a9fdb")) select en).ToList();
                Entity[] solutionComponents = SolutionHelper.GetSolutionComponentsByComponentType(OrgService, solutionId.Value, AUTO_RECORD_RULES_COMPONENT_TYPEID);
                if (solutionComponents?.Length > 0)
                {
                    Guid[] ids = (from sc in solutionComponents select Guid.Parse(Convert.ToString(sc["objectid"]))).ToArray();
                    arcComponents = GetRules(ids);
                }

                Log($"Process Rules in the solution '{SolutionName}' ({solutionId}).");
            }

            //activate or deactivate
            int updateCount = 0;
            if (arcComponents?.Length > 0)
            {
                //there are rules to process
                Log($"Rule Count: {arcComponents?.Length}");
                Log(LOG_SEGMENT);

                string action = "Activating";
                string currentState = "activated";
                OptionSetValue updateState = null;
                OptionSetValue updateStatusCode = null;
                if (Status == DRAFT_STATE)
                {
                    action = "Deactivating";
                    currentState = "deactivated";
                    updateState = new OptionSetValue(DRAFT_STATE);
                    updateStatusCode = new OptionSetValue(DRAFT_STATUSCODE);
                }
                else if (Status == ACTIVE_STATE)
                {
                    action = "Activating";
                    currentState = "activated";
                    updateState = new OptionSetValue(ACTIVE_STATE);
                    updateStatusCode = new OptionSetValue(ACTIVE_STATUSCODE);
                }

                foreach (Entity arc in arcComponents)
                {
                    if (arc.Contains("statecode") && arc["statecode"] != null && arc["statecode"] is OptionSetValue)
                    {
                        if (((OptionSetValue)arc["statecode"]).Value != Status)
                        {
                            //update the status
                            SetStateRequest req = new SetStateRequest()
                            {
                                EntityMoniker = arc.ToEntityReference(),
                                State = updateState,
                                Status = updateStatusCode,
                            };

                            Log($"{action} '{arc["name"]}'.");
                            OrgService.Execute(req);
                            updateCount++;
                        }
                        else
                        {
                            Log($"'{arc["name"]}' is already {currentState}.");
                        }
                    }
                }
            }
            else
            {
                //no rules found to process
                Log($"No Rules Found.");
            }

            Log(LOG_SEGMENT);
            Log($"Auto Records Rules Update Count: {updateCount};");

            return true;
        }
    }
}