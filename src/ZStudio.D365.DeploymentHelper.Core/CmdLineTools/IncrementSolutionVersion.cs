using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ZStudio.D365.DeploymentHelper.Core.Base;
using ZStudio.D365.Shared.Framework.Core.Query;

namespace ZStudio.D365.DeploymentHelper.Core.CmdLineTools
{
    [HelperType(nameof(IncrementSolutionVersion))]
    public class IncrementSolutionVersion : HelperToolBase
    {
        private Dictionary<string, object> config = null;

        public string SolutionName { get; private set; }

        public bool IncrementRevision { get; private set; }
        public bool IncrementBuild { get; private set; }
        public bool IncrementMinor { get; private set; }
        public bool IncrementMajor { get; private set; }

        public IncrementSolutionVersion(string crmConnectionString, string configJson, Dictionary<string, string> tokens, bool debugMode, int debugSleep) : base(crmConnectionString, configJson, tokens, debugMode, debugSleep)
        {
        }

        public override void PreExecute_HandlerImplementation()
        {
            try
            {
                //load config JSON
                config = JsonConvert.DeserializeObject<Dictionary<string, object>>(ConfigJson);

                SolutionName = Convert.ToString(config["SolutionName"]);
                IncrementRevision = Convert.ToBoolean(config["IncrementRevision"]);
                IncrementBuild = Convert.ToBoolean(config["IncrementBuild"]);
                IncrementMinor = Convert.ToBoolean(config["IncrementMinor"]);
                IncrementMajor = Convert.ToBoolean(config["IncrementMajor"]);
            }
            catch (Exception dex)
            {
                throw new ArgumentException($"The Config JSON is invalid and cannot be deserialise to Dictionary<string, object>. Exception: {dex.Message}");
            }

            Log(LOG_LINE);
            Log($"Config Parameters:");
            Log(LOG_LINE);
            Log($"Solution Name: {SolutionName}");
            Log($"Increment Revision: {IncrementRevision}");
            Log($"Increment Build: {IncrementBuild}");
            Log($"Increment Minor: {IncrementMinor}");
            Log($"Increment Major: {IncrementMajor}");
            Log(LOG_LINE);
        }

        protected override bool OnRun_Implementation(out string exceptionMessage)
        {
            exceptionMessage = string.Empty;
            bool result = false;

            //get solution
            XrmQueryExpression query = new XrmQueryExpression("solution")
                .Condition("uniquename", ConditionOperator.Equal, SolutionName);
            EntityCollection coll = CrmConn.OrgService.RetrieveMultiple(query.ToQueryExpression());
            if (coll?.Entities?.Count > 0)
            {
                Entity sol = coll.Entities[0];
                string newVersion = string.Empty;

                //get version
                string oldVersion = Convert.ToString(sol["version"]);
                Log($"Current Version: {oldVersion}");

                //calculate new version
                string[] versionSplit = oldVersion.Split(new string[] { "." }, StringSplitOptions.None);
                if (versionSplit?.Length == 4)
                {
                    int major = int.Parse(versionSplit[0]);
                    int minor = int.Parse(versionSplit[1]);
                    int build = int.Parse(versionSplit[2]);
                    int revision = int.Parse(versionSplit[3]);

                    if (IncrementMajor)
                    {
                        major++;
                        minor = 0;
                        build = 0;
                        revision = 0;
                    }
                    if (IncrementMinor)
                    {
                        minor++;
                        build = 0;
                        revision = 0;
                    }
                    if (IncrementBuild)
                    {
                        build++;
                        revision = 0;
                    }
                    if (IncrementRevision)
                    {
                        revision++;
                    }

                    newVersion = $"{major}.{minor}.{build}.{revision}";
                    Log($"New Version: {newVersion}");
                }
                else
                {
                    exceptionMessage = $"The solution '{SolutionName}' version of '{oldVersion}' is invalid.";
                    throw new ArgumentException(exceptionMessage);
                }

                //set new version
                if (!newVersion.Equals(oldVersion))
                {
                    //update
                    Log($"Update the solution '{SolutionName}' to the version '{newVersion}'.");
                    Entity solUpdate = new Entity(sol.LogicalName)
                    {
                        Id = sol.Id,
                    };
                    solUpdate["version"] = newVersion;
                    CrmConn.OrgService.Update(solUpdate);
                }
                else
                    Log($"No Update, the new version is the same as the current version.");
                result = true;
            }
            else
            {
                exceptionMessage = $"The solution name '{SolutionName}' is not found.";
                throw new ArgumentException(exceptionMessage);
            }

            return result;
        }
    }
}