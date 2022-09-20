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

    public class InstallIncrementSolutionNumberAction : InstallAction
    {
        private IncrementSolutionCollection incrementCollection;
        private WebServiceUtils utils;
        private Dictionary<string, AttributeMetadata> cacheAttribute = new Dictionary<string, AttributeMetadata>();
        private int? _ParentLineNumber = null;

        public override string ActionName { get { return "Increment/Set Solution Version"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallIncrementSolutionNumberAction(IncrementSolutionCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils, int parentLineNumber) : base(context)
        {
            incrementCollection = collections;
            utils = webServiceUtils;

            //load the solution collection from the XML file provided, should upgrade all other config to load from external config XML in future
            if (!string.IsNullOrEmpty(collections.SolutionConfig))
            {
                string xmlFilePath = Path.GetFullPath(IOUtils.ReplaceStringTokens(collections.SolutionConfig, Context.Tokens));
                Logger.LogInfo("Loading Increment/Set Version Solution from the file {0}...", xmlFilePath);

                //increment the solutions from the Solution Config XML, generate a new Increment Solution Collection object
                IncrementSolutionCollection sol = new IncrementSolutionCollection(xmlFilePath);

                //copy any attributes config setup on the DeploymentConfig
                sol.SetVersion = incrementCollection.SetVersion;
                sol.SetVersionValue = incrementCollection.SetVersionValue;
                sol.PublishType = incrementCollection.PublishType;

                incrementCollection = sol;
                _ParentLineNumber = parentLineNumber;
            }
        }

        public override int Index
        {
            get
            {
                if (_ParentLineNumber == null)
                    return incrementCollection.ElementInformation.LineNumber;
                else
                    return _ParentLineNumber.Value;
            }
        }

        protected override void RunBackupAction()
        {
            //Do nothing for Backup
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo(" ");
            Logger.LogInfo("Executing Increment/Set Solution Build Number...");
            if (incrementCollection != null)
            {
                bool setVersion = incrementCollection.SetVersion;
                string versionValue = string.Empty;

                bool anyError = false;
                string errorString = string.Empty;

                //when set version, get the version value and validate it
                if (setVersion)
                {
                    versionValue = IOUtils.ReplaceStringTokens(incrementCollection.SetVersionValue, Context.Tokens);
                    if (string.IsNullOrEmpty(versionValue))
                    {
                        Logger.LogError("  Set Version Value is empty, can not update solution version");
                        throw new Exception("Set Version Value is empty, can not update solution version.");
                    }
                    else
                    {
                        Logger.LogInfo("  Updating Solution Version");

                        //validate that it contain 3 segements
                        if (versionValue.Contains(".") && versionValue.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries).Length == 4)
                        {
                            //version is valid
                            Logger.LogInfo("  New solution version: '{0}'", versionValue);
                        }
                        else
                        {
                            Logger.LogError("  Version Value is in an invalid format, example of a valid version is 1.0.20201231.1 or 1.0.0.1");
                            throw new Exception("Version Value is in an invalid format, example of a valid version is 1.0.20201231.1 or 1.0.0.1.");
                        }
                    }
                }
                else
                {
                    Logger.LogInfo("  Incrementing Solution Version");
                }

                foreach (IncrementSolutionElement element in incrementCollection)
                {
                    string solutionName = IOUtils.ReplaceStringTokens(element.SolutionName, Context.Tokens);
                    if (string.IsNullOrEmpty(solutionName))                    
                        Logger.LogError("  Solution Name is empty, can not update solution version");                    
                    else
                    {
                        try
                        {
                            QueryExpression queryCheckForSolution = new QueryExpression
                            {
                                EntityName = Solution.EntityLogicalName,
                                ColumnSet = new ColumnSet(true),
                                Criteria = new FilterExpression()
                            };
                            queryCheckForSolution.Criteria.AddCondition("uniquename", ConditionOperator.Equal, solutionName);

                            //retrieve the solution
                            EntityCollection querySolutionResults = utils.Service.RetrieveMultiple(queryCheckForSolution);
                            Solution solutionResults = null;
                            //check if the solution exists
                            if (querySolutionResults.Entities.Count > 0)
                            {
                                string message = string.Empty;
                                solutionResults = (Solution)querySolutionResults.Entities[0];

                                if (setVersion)
                                {
                                    solutionResults.Version = versionValue;
                                    message = string.Format("  Setting solution version to {0} ", versionValue);
                                }
                                else
                                {
                                    string oldVersion = solutionResults.Version;
                                    string newVersion = string.Empty;
                                    string[] versionSplit = oldVersion.Split(new string[] { "." }, StringSplitOptions.None);

                                    int presentVersion;
                                    bool isNum = int.TryParse(versionSplit[versionSplit.Length - 1], out presentVersion);

                                    //Increment the version
                                    if (isNum)
                                    {
                                        for (int i = 0; i < versionSplit.Length; i++)
                                        {
                                            if (i != (versionSplit.Length - 1))
                                                newVersion += versionSplit[i] + ".";
                                            else
                                                newVersion += (presentVersion + 1);
                                        }
                                        solutionResults.Version = newVersion;

                                        message = string.Format("  Incremented solution version from {0} to {1} ", oldVersion, newVersion);
                                    }
                                }

                                if (!string.IsNullOrEmpty(message))
                                {
                                    utils.Service.Update(solutionResults);
                                    Logger.LogInfo(message);
                                }

                                if (!anyError && incrementCollection.PublishType == PublishType.Separately)
                                {
                                    utils.PublishCustomizations(false, ref anyError);
                                }
                            }                                
                            else                           
                                Logger.LogInfo(string.Format("  The solution {0} is does not exists to increment/set the version", solutionName));  

                        }
                        catch (Exception ex)
                        {
                            anyError = true;
                            errorString += Environment.NewLine + ex.Message;
                        }
                    }
                }
                if (!anyError && incrementCollection.PublishType == PublishType.Batch)
                {
                    utils.PublishCustomizations(false, ref anyError);
                }

                if (anyError)
                    throw new Exception(errorString);
            }
        }

        protected override void RunUninstallAction()
        {
            // throw new NotImplementedException();
        }

        public override string GetExecutionErrorMessage()
        {
            return "Failed to Increment/Set Solution Build Version.";
        }

        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.IncrementSolutionBuildVersion;
        }
    }
}