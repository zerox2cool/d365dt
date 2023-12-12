using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using ZStudio.D365.DeploymentHelper.Core.Base;
using ZStudio.D365.Shared.Framework.Core.Query;

namespace ZStudio.D365.DeploymentHelper.Core.CmdLineTools
{
    [HelperType(nameof(CalculateSolutionVersion))]
    public class CalculateSolutionVersion : HelperToolBase
    {
        public string SolutionName { get; private set; }

        public string OutputTextFile { get; private set; }

        public bool IncrementRevision { get; private set; }
        public bool IncrementBuild { get; private set; }
        public bool IncrementMinor { get; private set; }
        public bool IncrementMajor { get; private set; }

        public CalculateSolutionVersion(string crmConnectionString, Dictionary<string, object> config, Dictionary<string, string> tokens, bool debugMode) : base(crmConnectionString, config, tokens, debugMode)
        {
            SolutionName = Convert.ToString(config["SolutionName"]);
            OutputTextFile = Convert.ToString(config["OutputTextFile"]);
            IncrementRevision = Convert.ToBoolean(config["IncrementRevision"]);
            IncrementBuild = Convert.ToBoolean(config["IncrementBuild"]);
            IncrementMinor = Convert.ToBoolean(config["IncrementMinor"]);
            IncrementMajor = Convert.ToBoolean(config["IncrementMajor"]);
        }

        public override void PreExecute_HandlerImplementation()
        {
            Log(LOG_LINE);
            Log($"Config Parameters:");
            Log(LOG_LINE);
            Log($"Solution Name: {SolutionName}");
            Log($"Output Text File: {OutputTextFile}");
            Log($"Increment Revision: {IncrementRevision}");
            Log($"Increment Build: {IncrementBuild}");
            Log($"Increment Minor: {IncrementMinor}");
            Log($"Increment Major: {IncrementMajor}");
            Log(string.Empty);
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

                    //write to output
                    if (!string.IsNullOrEmpty(OutputTextFile))
                    {
                        if (File.Exists(OutputTextFile))
                        {
                            File.Delete(OutputTextFile);
                        }
                        else
                        {
                            //try to combine with current working folder path
                            OutputTextFile = Path.Combine(Environment.CurrentDirectory, OutputTextFile);
                            if (File.Exists(OutputTextFile))
                            {
                                File.Delete(OutputTextFile);
                            }
                        }

                        //write to output file
                        Log($"New Version output to '{OutputTextFile}'.");
                        File.WriteAllText(OutputTextFile, newVersion);
                    }

                    result = true;
                }
                else
                {
                    throw new ArgumentException($"The solution '{SolutionName}' version of '{oldVersion}' is invalid.");
                }
            }
            else
            {
                throw new ArgumentException($"The solution name '{SolutionName}' is not found.");
            }

            return result;
        }
    }
}