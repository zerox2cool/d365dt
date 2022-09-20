using System;
using System.IO;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ZD365DT.DeploymentTool
{

    public class InstallDataDeleteAction : InstallAction
    {
        private DataDeleteCollection deleteCollection;
        private WebServiceUtils utils;
        private Dictionary<string, AttributeMetadata> cacheAttribute = new Dictionary<string, AttributeMetadata>();

        public override string ActionName { get { return "Data Delete"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallDataDeleteAction(DataDeleteCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils)
            : base(context)
        {
            deleteCollection = collections;
            utils = webServiceUtils;
        }
        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "DataBackup"); }
        }

        public override int Index
        {
            get { return deleteCollection.ElementInformation.LineNumber; }
        }

        protected override void RunBackupAction()
        {
            //Do nothing for Backup
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo(" ");
            Logger.LogInfo("Executing {0}...", ActionName);
            if (deleteCollection != null)
            {
                bool anyError = false;
                string errorText = string.Empty;
                foreach (DataDeleteElement element in deleteCollection)
                {
                    string entityName = IOUtils.ReplaceStringTokens(element.EntityName, Context.Tokens);
                    if (element.DataBackup)
                    {
                        if (!Directory.Exists(BackupDirectory))
                            Directory.CreateDirectory(BackupDirectory);

                        string fileName = BackupDirectory + "\\" + entityName + "_" + System.DateTime.Now.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture) + ".csv";

                        Logger.LogInfo("Executing Data Backup of entity {0}", entityName);
                        utils.ExportCrmData(entityName, fileName);
                    }

                    try
                    {
                        Logger.LogInfo("Executing Data Delete of entity {0}", entityName);

                        QueryExpression opportunitiesQuery = BuildQuery(entityName);
                        BulkDeleteRequest bulkDeleteRequest = new BulkDeleteRequest();
                        bulkDeleteRequest.JobName = "Bulk Delete " + entityName;
                        bulkDeleteRequest.QuerySet = new QueryExpression[]
                    {
                        opportunitiesQuery             
                    };

                        // Set the start time for the bulk delete.
                        bulkDeleteRequest.StartDateTime = DateTime.Now;

                        // Set the required recurrence pattern.
                        bulkDeleteRequest.RecurrencePattern = String.Empty;

                        // Set email activity properties.
                        bulkDeleteRequest.SendEmailNotification = false;
                        bulkDeleteRequest.ToRecipients = new Guid[] { utils.CurrentOrgContext.CurrentUserId };
                        bulkDeleteRequest.CCRecipients = new Guid[] { };

                        // Submit the bulk delete job.
                        // NOTE: Because this is an asynchronous operation, the response will be immediate.
                        BulkDeleteResponse _bulkDeleteResponse =
                            (BulkDeleteResponse)utils.Service.Execute(bulkDeleteRequest);
                        Logger.LogInfo("The bulk delete operation has been requested.");

                        if (element.WaitForCompletion)
                            CheckSuccess(_bulkDeleteResponse.JobId, element.WaitTimeout,ref anyError,ref errorText);

                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex);
                    }
                }
                if (anyError)
                    throw new Exception(errorText);
            }
        }

        /// <summary>
        /// This method will query for the BulkDeleteOperation until it has been
        /// completed or until the designated time runs out.  It then checks to see if
        /// the operation was successful.
        /// </summary>
        private void CheckSuccess(Guid _asyncOperationId, int WaitTimeout, ref bool anyError,ref string errorText)
        {
            try
            {

                // Query for bulk delete operation and check for status.
                QueryByAttribute bulkQuery = new QueryByAttribute(
                    BulkDeleteOperation.EntityLogicalName);
                bulkQuery.ColumnSet = new ColumnSet(true);

                // NOTE: When the bulk delete operation was submitted, the GUID that was
                // returned was the asyncoperationid, not the bulkdeleteoperationid.
                bulkQuery.Attributes.Add("asyncoperationid");

                bulkQuery.Values.Add(_asyncOperationId);

                // With only the asyncoperationid at this point, a RetrieveMultiple is
                // required to get the
                // bulk delete operation created above.
                EntityCollection entityCollection = utils.Service.RetrieveMultiple(bulkQuery);
                BulkDeleteOperation createdBulkDeleteOperation = null;

                // Monitor the async operation via polling until it is complete or max
                // polling time expires.

                int secondsTicker = WaitTimeout;
                while (secondsTicker > 0)
                {
                    // Make sure the async operation was retrieved.
                    if (entityCollection.Entities.Count > 0)
                    {
                        // Grab the one bulk operation that has been created.
                        createdBulkDeleteOperation = (BulkDeleteOperation)entityCollection.Entities[0];

                        // Check the operation's state.
                        if (createdBulkDeleteOperation.StateCode.Value != BulkDeleteOperationState.Completed)
                        {
                            // The operation has not yet completed.  Wait a second for the
                            // status to change.
                            System.Threading.Thread.Sleep(1000);
                            secondsTicker--;

                            // Retrieve a fresh version of the bulk delete operation.
                            entityCollection = utils.Service.RetrieveMultiple(bulkQuery);
                        }
                        else
                        {
                            // Stop polling as the operation's state is now complete.
                            secondsTicker = 0;
                        }
                    }
                    else
                    {
                        // Wait a second for async operation to activate.
                        System.Threading.Thread.Sleep(1000);
                        secondsTicker--;

                        // Retrieve the entity again.
                        entityCollection = utils.Service.RetrieveMultiple(bulkQuery);
                    }
                }

                // Validate that the operation was completed.
                if (createdBulkDeleteOperation != null)
                {
                    // _bulkDeleteOperationId = createdBulkDeleteOperation.BulkDeleteOperationId;
                    if (createdBulkDeleteOperation.StateCode.Value != BulkDeleteOperationState.Completed)
                    {
                        Logger.LogError(
                            "Polling for the BulkDeleteOperation took longer than allowed ({0} seconds).",
                            WaitTimeout);
                        anyError = true;

                        errorText += Environment.NewLine + string.Format(
                            "Polling for the BulkDeleteOperation took longer than allowed ({0} seconds).",
                            WaitTimeout);
                    }
                    else
                    {
                        Logger.LogInfo("The BulkDeleteOperation succeeded.\r\n  Successes: {0}, Failures: {1}",
                            createdBulkDeleteOperation.SuccessCount,
                            createdBulkDeleteOperation.FailureCount);
                    }
                }
                else
                {

                    Logger.LogError("The BulkDeleteOperation could not be retrieved.");
                    anyError = true;
                    errorText += Environment.NewLine + "The BulkDeleteOperation could not be retrieved.";
                }
            }
            catch (Exception ex)
            {

                Logger.LogException(ex);
            }
        }



        /// <summary>
        /// Builds a query that matches all opportunities that are not in the open state.
        /// </summary>
        private static QueryExpression BuildQuery(string entityName)
        {
            var queryExpression = new QueryExpression();
            queryExpression.EntityName = entityName;
            queryExpression.Distinct = false;
            return queryExpression;
        }



     

        protected override void RunUninstallAction()
        {
            // throw new NotImplementedException();
        }

        public override string GetExecutionErrorMessage()
        {
            return "Failed to Delete Data.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.DeleteDataFailed;
        }
    }
}
