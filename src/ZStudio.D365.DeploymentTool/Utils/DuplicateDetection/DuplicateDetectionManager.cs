using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Services.Protocols;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace ZD365DT.DeploymentTool.Utils.DuplicateDetection
{
    public class DuplicateDetectionManager
    {
        #region Local Variables

      

        private static WebServiceUtils utils;
        public static WebServiceUtils Utils
        {
            set { utils = value; }
        }


        private enum DuplicateDetectionStatusCode
        {
            Unpublished = 0,
            Publishing = 1,
            Published = 2
        }

        #endregion Local Variables

        #region Constructor





        static DuplicateDetectionManager()
        {
            // utils = WebServiceUtils.Instance;


        }

        #endregion Constructor

        public static bool Publish(DuplicateDetectionRule[] rules, ref bool anyError)
        {
            bool result = Remove(rules, ref anyError);
            List<Guid> crmAsyncOperationIds = new List<Guid>();
            if (result)
            {
                foreach (DuplicateDetectionRule rule in rules)
                {
                    Logger.LogInfo("  Adding Duplicate Detection Rule '{0}'", rule.Name);
                    Guid JobId = Guid.Empty;
                    result = AddDuplicateRule(rule, out JobId);
                    if (!result)
                    {
                        Logger.LogError("Failed to add Duplicate Detection Rule '{0}'", rule.Name);
                        anyError = true;
                    }
                    else
                    {
                        if(JobId != Guid.Empty)
                        {
                            crmAsyncOperationIds.Add(JobId);
                        }
                    }
                }
                Logger.LogInfo("Wait for the Duplicate Detection Rules to be Published");
                //wait for the Duplicate detection rules to be published
                WaitForAsyncJobCompletion(crmAsyncOperationIds);
                Logger.LogInfo("Duplicate detection rules are Published");
            }
            return result;
        }

        public static bool Remove(DuplicateDetectionRule[] rules, ref bool anyError)
        {
            bool result = true;

            foreach (DuplicateDetectionRule rule in rules)
            {
                Logger.LogInfo("  Removing Duplicate Detection Rule '{0}'", rule.Name);
                result = RemoveDuplicateRuleNamed(rule.Name);
                if (!result)
                {
                    Logger.LogError("Failed to remove Duplicate Detection Rule '{0}'", rule.Name);
                    anyError = true;
                    break;
                }
            }

            return result;
        }

        public static bool DeleteAllRules()
        {
            bool result = true;

            Logger.LogInfo("  Retrieving all Duplicate Detection Rules...");
            EntityCollection rules = GetAllRules();
            if (rules != null)
            {
                foreach (DuplicateRule rule in rules.Entities)
                {
                    Logger.LogInfo("    Deleting Rule '{0}'...", rule.Name);
                    DeleteRule(rule.DuplicateRuleId.Value);
                }
            }

            return result;
        }

        public static DuplicateDetection RetrieveAllRules()
        {
            DuplicateDetection result = new DuplicateDetection();

            Logger.LogInfo("  Retrieving all Duplicate Detection Rules...");
            EntityCollection rules = GetAllRules();
            if (rules != null)
            {
                List<DuplicateDetectionRule> ruleList = new List<DuplicateDetectionRule>();

                foreach (DuplicateRule rule in rules.Entities)
                {
                    DuplicateDetectionRule dupDetectionRule = new DuplicateDetectionRule(rule);

                    Logger.LogInfo("    Retrieving all Conditions for Rule '{0}'...", rule.Name);
                    EntityCollection conditions = GetRuleCondionsFor(rule.DuplicateRuleId.Value);

                    List<DuplicateDetectionRulesCondition> dupDetectionConditions = new List<DuplicateDetectionRulesCondition>();

                    foreach (DuplicateRuleCondition condition in conditions.Entities)
                    {
                        dupDetectionConditions.Add(new DuplicateDetectionRulesCondition(condition));
                    }
                    dupDetectionRule.Conditions = dupDetectionConditions.ToArray();
                    ruleList.Add(dupDetectionRule);
                }
                result.DuplicateDetectionRules = ruleList.ToArray();
            }
            return result;
        }

        #region Add Duplicate Detection Rules
        private static bool AddDuplicateRule(DuplicateDetectionRule rule, out Guid JobId)
        {
            bool result = true;
            Guid _jobId = Guid.Empty;
            try
            {
                DuplicateRule duplicateRule = rule.GetDuplicateRule();
                //duplicateRule.MatchingEntityTypeCode = new OptionSetValue( (int)entityMetaData[duplicateRule.MatchingEntityName]);
                //duplicateRule.MatchingEntityName = duplicateRule.MatchingEntityName;

                Logger.LogInfo("    Creating rule '{0}'", rule.Name);
              Guid  _ruleId = utils.Service.Create(duplicateRule);
                
                if (rule.Conditions != null && rule.Conditions.Length > 0)
                {
                    for (int i = 0; i < rule.Conditions.Length; i++)
                    {
                        DuplicateRuleCondition conditions = rule.Conditions[i].GetDuplicateRule();
                        Logger.LogInfo("    Creating rule '{0}' Condition {1}", rule.Name, conditions.BaseAttributeName + " match " + conditions.MatchingAttributeName);
                        DuplicateRuleCondition accountDupCondition = new DuplicateRuleCondition
                        {
                            BaseAttributeName = conditions.BaseAttributeName,
                            MatchingAttributeName = conditions.MatchingAttributeName,
                            OperatorCode = new OptionSetValue(0), // Exact Match.
                            RegardingObjectId = new EntityReference(DuplicateRule.EntityLogicalName, _ruleId)
                        };
                        Guid conditionId = utils.Service.Create(accountDupCondition);
                    }

                    Logger.LogInfo("    Publishing rule '{0}'", rule.Name);
                    result = PublishRule(_ruleId, out _jobId);
                }
                else
                {
                    Logger.LogError("  No conditions defined for '{0}'", rule.Name);
                    result = false;
                }
            }
            catch (SoapException soapEx)
            {
                Logger.LogException(soapEx);
                JobId = _jobId;
                result = false;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                JobId = _jobId;
                result = false;
            }
            JobId = _jobId;
            return result;
        }

        private static bool PublishRule(Guid duplicateRuleId, out Guid JobId)
        {
            bool result = true;
            Guid _jobId = Guid.Empty;
            PublishDuplicateRuleRequest request = new PublishDuplicateRuleRequest();
            request.DuplicateRuleId = duplicateRuleId;

            try
            {
               var response = (PublishDuplicateRuleResponse) utils.Service.Execute(request);
               _jobId=response.JobId;
            }
            catch (SoapException soapEx)
            {
                Logger.LogException(soapEx);
                result = false;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                result = false;
            }
            JobId = _jobId;
            return result;
        }
        #endregion Add Duplicate Detection Rules

        #region Remove Duplicate Detection Rules
        private static bool RemoveDuplicateRuleNamed(string name)
        {
            bool result = true;
            DuplicateRule rule = GetRuleNamed(name);
            if (rule != null)
            {
                if (rule.StatusCode.Value == (int)DuplicateDetectionStatusCode.Published ||
                    rule.StatusCode.Value == (int)DuplicateDetectionStatusCode.Publishing)
                {
                    Logger.LogInfo("    Un-publishing rule '{0}'", name);
                    result = UnpublishRule(rule.DuplicateRuleId.Value);
                }
                if (result)
                {
                    Logger.LogInfo("    Deleting rule conditions for '{0}'", name);
                    result = DeleteRuleConditions(rule.DuplicateRuleId.Value);
                }
                if (result)
                {
                    Logger.LogInfo("    Deleting rule '{0}'", name);
                    result = DeleteRule(rule.DuplicateRuleId.Value);
                }
            }
            else
            {
                Logger.LogWarning("    Duplicate Rule '{0}' NOT FOUND, nothing to remove", name);
            }
            return result;
        }

        private static bool DeleteRule(Guid duplicateRuleId)
        {
            bool result = true;
            try
            {
                utils.Service.Delete(DuplicateRule.EntityLogicalName.ToString(), duplicateRuleId);
            }
            catch (SoapException soapEx)
            {
                Logger.LogException(soapEx);
                result = false;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                result = false;
            }
            return result;
        }

        private static bool DeleteRuleConditions(Guid duplicateRuleId)
        {
            bool result = true;
            try
            {
                EntityCollection entities = GetRuleCondionsFor(duplicateRuleId);
                if (entities == null)
                {
                    result = false;
                }
                else
                {
                    foreach (DuplicateRuleCondition condition in entities.Entities)
                    {
                        utils.Service.Delete(DuplicateRuleCondition.EntityLogicalName.ToString(), condition.DuplicateRuleConditionId.Value);
                    }
                }
            }
            catch (SoapException soapEx)
            {
                Logger.LogException(soapEx);
                result = false;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                result = false;
            }
            return result;
        }

        private static bool UnpublishRule(Guid duplicateRuleId)
        {
            bool result = true;
            UnpublishDuplicateRuleRequest request = new UnpublishDuplicateRuleRequest();
            request.DuplicateRuleId = duplicateRuleId;

            try
            {
                utils.Service.Execute(request);
            }
            catch (SoapException soapEx)
            {
                Logger.LogException(soapEx);
                result = false;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                result = false;
            }
            return result;
        }
        #endregion Remove Duplicate Detection Rules

        #region Get Rules and Conditions

        private static DuplicateRule GetRuleNamed(string name)
        {
            DuplicateRule result = null;
            ColumnSet cols = new ColumnSet(new string[] { "duplicateruleid", "statuscode" });

            QueryByAttribute query = new QueryByAttribute();
            query.ColumnSet = cols;
            query.EntityName = DuplicateRule.EntityLogicalName.ToString();
            query.Attributes.Add("name");
            query.Values.Add(name);

            try
            {
                EntityCollection entities = utils.Service.RetrieveMultiple(query);
                if (entities.Entities.Count > 0)
                {
                    result = entities.Entities[0] as DuplicateRule;
                }
            }
            catch (SoapException soapEx)
            {
                Logger.LogException(soapEx);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return result;
        }

        private static EntityCollection GetAllRules()
        {
            EntityCollection result = null;

            QueryExpression query = new QueryExpression();
            query.ColumnSet = new ColumnSet(true);
            query.EntityName = DuplicateRule.EntityLogicalName.ToString();

            try
            {
                result = utils.Service.RetrieveMultiple(query);
            }
            catch (SoapException soapEx)
            {
                Logger.LogException(soapEx);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return result;
        }

        private static EntityCollection GetRuleCondionsFor(Guid duplicateRuleId)
        {
            EntityCollection result = null;

            QueryByAttribute query = new QueryByAttribute();
            query.ColumnSet = new ColumnSet(true);
            query.EntityName = DuplicateRuleCondition.EntityLogicalName.ToString();
            query.Attributes.Add("regardingobjectid");
            query.Values.Add(duplicateRuleId);

            try
            {
                result = utils.Service.RetrieveMultiple(query);
            }
            catch (SoapException soapEx)
            {
                Logger.LogException(soapEx);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return result;
        }

        #endregion Get Rules and Conditions

        #region Wait for the rules ot be published
        /// <summary>
        /// Waits for async job to complete
        /// </summary>
        /// <param name="asyncJobId"></param>
        public static void WaitForAsyncJobCompletion(IEnumerable<Guid> asyncJobIds)
        {
            List<Guid> asyncJobList = new List<Guid>(asyncJobIds);
            ColumnSet cs = new ColumnSet("statecode", "asyncoperationid");
            int retryCount = 100;

            while (asyncJobList.Count != 0 && retryCount > 0)
            {
                // Retrieve the async operations based on the ids
                var crmAsyncJobs = utils.Service.RetrieveMultiple(
                    new QueryExpression("asyncoperation")
                    {
                        ColumnSet = cs,
                        Criteria = new FilterExpression
                        {
                            Conditions = 
                            {
                                new ConditionExpression("asyncoperationid",
                                    ConditionOperator.In, asyncJobList.ToArray())
                            }
                        }
                    });

                // Check to see if the operations are completed and if so remove them from the Async Guid list
                foreach (var item in crmAsyncJobs.Entities)
                {
                    var crmAsyncJob = item.ToEntity<AsyncOperation>();
                    if (crmAsyncJob.StateCode.HasValue &&
                        crmAsyncJob.StateCode.Value == AsyncOperationState.Completed)
                        asyncJobList.Remove(crmAsyncJob.AsyncOperationId.Value);

                    Console.WriteLine(String.Concat("Async operation state is ",
                        crmAsyncJob.StateCode.Value.ToString(),
                        ", async operation id: ", crmAsyncJob.AsyncOperationId.Value.ToString()));
                }

                // If there are still jobs remaining, sleep the thread.
                if (asyncJobList.Count > 0)
                    System.Threading.Thread.Sleep(2000);

                retryCount--;
            }

            if (retryCount == 0 && asyncJobList.Count > 0)
            {
                for (int i = 0; i < asyncJobList.Count; i++)
                {
                    Console.WriteLine(String.Concat(
                        "The following async operation has not completed: ",
                        asyncJobList[i].ToString()));
                }
            }
        }
        #endregion
    }
}
