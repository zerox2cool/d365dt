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
    [HelperType(nameof(SetCloudFlowStatus))]
    public class SetCloudFlowStatus : HelperToolBase
    {
        private const string DISPLAYNAME = "Cloud Flow";
        private const string TABLENAME = "workflow";
        private const int COMPONENT_TYPEID = (int)SolutionHelper.ComponentType.CloudFlow;
        private const int MODERN_FLOWID = 5;
        private const int MAX_RETRY = 7;

        private const int DRAFT_STATE = 0;
        private const int ACTIVE_STATE = 1;
        private const int DRAFT_STATUSCODE = 1;
        private const int ACTIVE_STATUSCODE = 2;

        private Dictionary<string, object> config = null;

        public string SolutionName { get; private set; }

        public bool AllCustomFlow { get; private set; }

        public int Status { get; private set; }

        public SetCloudFlowStatus(string crmConnectionString, string configJson, Dictionary<string, string> tokens, bool portalEnhancedMode, bool debugMode, int debugSleep) : base(crmConnectionString, configJson, tokens, portalEnhancedMode, debugMode, debugSleep)
        {
        }

        private bool IsFiltreredOutSystem(Entity comp)
        {
            bool isSkip = false;
            if (comp.Contains("primaryentity") && comp["primaryentity"] != null && Convert.ToString(comp["primaryentity"]).StartsWith("msdyn", StringComparison.CurrentCultureIgnoreCase))
                isSkip = true;
            if (!isSkip && comp.Contains("createdby") && comp["createdby"] != null && comp["createdby"] is EntityReference && !string.IsNullOrEmpty(((EntityReference)comp["createdby"]).Name) && ((EntityReference)comp["createdby"]).Name.Equals("SYSTEM", StringComparison.CurrentCultureIgnoreCase))
                isSkip = true;
            return isSkip;
        }

        private bool IsFiltreredOutCertainPrefixInName(Entity comp)
        {
            bool isSkip = false;
            if (comp.Contains("name") && comp["name"] != null)
            {
                string name = Convert.ToString(comp["name"]);
                if (name.StartsWith("ZZDEL", StringComparison.CurrentCultureIgnoreCase))
                    isSkip = true;
                else if (name.StartsWith("ARC:", StringComparison.CurrentCultureIgnoreCase))
                    isSkip = true;
                else if (name.StartsWith("ServiceLevelAgreement_ActionFlow", StringComparison.CurrentCultureIgnoreCase))
                    isSkip = true;
            }
            return isSkip;
        }

        private Entity[] GetAllComponents()
        {
            List<Entity> result = new List<Entity>();

            XrmQueryExpression query = new XrmQueryExpression(TABLENAME)
                .Condition("category", ConditionOperator.Equal, 5);
            Entity[] components = Fetch.RetrieveAllEntityByQuery(query.ToQueryExpression());
            if (components?.Length > 0)
            {
                //filter out system flows and Microsoft flows
                foreach (var comp in components)
                {
                    bool isSkip = IsFiltreredOutSystem(comp);
                    
                    //add logic to skip flow with certain prefix
                    if (!isSkip)
                        isSkip = IsFiltreredOutCertainPrefixInName(comp);

                    if (!isSkip)
                        result.Add(comp);
                }
            }

            if (result?.Count > 0)
                return result.ToArray();
            return null;
        }

        private Entity[] GetComponents(Guid[] ids)
        {
            List<Entity> result = new List<Entity>();

            Entity[] components = Fetch.RetrieveAllEntityByCrmIds(TABLENAME, ids);
            if (components?.Length > 0)
            {
                //filter out system flows and Microsoft flows
                foreach (var comp in components)
                {
                    bool isSkip = IsFiltreredOutSystem(comp);

                    //add logic to skip flow with certain prefix
                    if (!isSkip)
                        isSkip = IsFiltreredOutCertainPrefixInName(comp);

                    if (!isSkip)
                        result.Add(comp);
                }
            }

            if (result?.Count > 0)
                return result.ToArray();
            return null;
        }

        public override void PreExecute_HandlerImplementation()
        {
            try
            {
                //load config JSON
                config = JsonConvert.DeserializeObject<Dictionary<string, object>>(ConfigJson);

                SolutionName = Convert.ToString(config["SolutionName"]);
                AllCustomFlow = Convert.ToBoolean(config["AllCustomFlow"]);
                Status = Convert.ToInt32(config["Status"]);
            }
            catch (Exception dex)
            {
                throw new ArgumentException($"The Config JSON is invalid and cannot be deserialise to Dictionary<string, object>. Exception: {dex.Message}");
            }

            Log(LOG_LINE);
            Log($"Config Parameters:");
            Log(LOG_LINE);
            Log($"AllCustomFlow: {AllCustomFlow}");
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
            if (AllCustomFlow)
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
                //var lists = (from en in components where Guid.Parse(Convert.ToString(en["objectid"])).Equals(new Guid("583e58cf-0141-ee11-bdf4-000d3a6ad7cb")) select en).ToList();
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
                string actioned = "Activated";
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

                #region InitialRun
                Dictionary<Guid, Entity> failedComponents2Retry = new Dictionary<Guid, Entity>();
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

                                failedComponents2Retry.Add(component.Id, component);
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
                #endregion InitialRun

                #region RetryRun
                //perform retry until max retry count, as the data retrieve has no relation to parent items not being activated, we will retry up to 7 times until all flows are activated
                int retryCount = 0;
                bool isRetryRequired = (failedComponents2Retry.Count > 0);
                while (isRetryRequired)
                {
                    retryCount++;
                    Log(LOG_SEGMENT);
                    Log($"Retry Run: {retryCount}; Number of Failed Items: {failedComponents2Retry.Count};");
                    Log(LOG_SEGMENT);

                    List<Guid> successIds = new List<Guid>();
                    foreach (var component in failedComponents2Retry)
                    {
                        if (component.Value.Contains("statecode") && component.Value["statecode"] != null && component.Value["statecode"] is OptionSetValue)
                        {
                            if (((OptionSetValue)component.Value["statecode"]).Value != Status)
                            {
                                StringBuilder sb = new StringBuilder();
                                try
                                {
                                    //update the status
                                    SetStateRequest req = new SetStateRequest()
                                    {
                                        EntityMoniker = component.Value.ToEntityReference(),
                                        State = updateState,
                                        Status = updateStatusCode,
                                    };

                                    sb.Append($"{action} '{component.Value["name"]}'. ");
                                    OrgService.Execute(req);
                                    sb.Append($"{actioned} SUCCESS");

                                    successIds.Add(component.Key);
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
                                Log($"'{component.Value["name"]}' is already {actioned}.");
                            }
                        }
                    }

                    //remove successful object from the fail container
                    if (successIds.Count > 0)
                    {
                        foreach (var id in successIds)
                        {
                            if (failedComponents2Retry.ContainsKey(id))
                                failedComponents2Retry.Remove(id);
                        }
                    }

                    //check if we need to retry again
                    if (failedComponents2Retry.Count == 0 || retryCount >= MAX_RETRY)
                        isRetryRequired = false;
                }
                #endregion RetryRun

                //set fail count to be the remaining component in fail container
                failedCount = failedComponents2Retry.Count;

                Log(LOG_SEGMENT);
                Log($"Total Retry: {retryCount}");
            }
            else
            {
                //no rules found to process
                Log($"No {DISPLAYNAME} Found.");
            }

            Log(LOG_SEGMENT);
            Log($"{DISPLAYNAME} Update Count: {updateCount}; Failed Count: {failedCount}");

            return (failedCount == 0);
        }
    }
}