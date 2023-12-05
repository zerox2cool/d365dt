using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZStudio.D365.Shared.Data.Framework.Cmd;
using ZStudio.D365.Shared.Framework.Util;
using System.Diagnostics;
using static ZStudio.D365.Shared.Framework.Util.CrmConnector;
using Microsoft.Xrm.Sdk.Organization;
using ZStudio.D365.Shared.Framework.Core.Query;

namespace ZStudio.D365.DeploymentHelper.Core.CmdLineTools
{
    public class IncrementSolutionVersion
    {
        private Stopwatch WatchTimer = new Stopwatch();

        public bool DebugMode { get; private set; }

        public string CrmConnectionString { get; private set; }

        public Dictionary<string, object> Config { get; private set; }

        public string SolutionName { get; private set; }

        public bool IncrementRevision { get; private set; }
        public bool IncrementBuild { get; private set; }
        public bool IncrementMinor { get; private set; }
        public bool IncrementMajor { get; private set; }

        public CrmConnector CrmConn { get; private set; }

        public IncrementSolutionVersion(string crmConnectionString, Dictionary<string, object> config, bool debugMode = false)
        {
            DebugMode = debugMode;

            CrmConnectionString = crmConnectionString;
            Config = config;
            SolutionName = Convert.ToString(config["SolutionName"]);
            IncrementRevision = Convert.ToBoolean(config["IncrementRevision"]);
            IncrementBuild = Convert.ToBoolean(config["IncrementBuild"]);
            IncrementMinor = Convert.ToBoolean(config["IncrementMinor"]);
            IncrementMajor = Convert.ToBoolean(config["IncrementMajor"]);
        }

        #region Run
        public void Run()
        {
            OnStarted();
            OnRun();
            OnStopped();
        }

        protected void OnStarted()
        {
            WatchTimer.Start();
            ConsoleLog.Info($"{Assembly.GetEntryAssembly().FullName} Starting...");
            ConsoleLog.Info($"Helper: {nameof(IncrementSolutionVersion)}");
            ConsoleLog.Info($"CRM Connection String: {CrmConnectionString}");
            ConsoleLog.Info($"Solution Name: {SolutionName}");
            ConsoleLog.Info($"Increment Revision: {IncrementRevision}");
            ConsoleLog.Info($"Increment Build: {IncrementBuild}");
            ConsoleLog.Info($"Increment Minor: {IncrementMinor}");
            ConsoleLog.Info($"Increment Major: {IncrementMajor}");
            ConsoleLog.Info($"Debug: {DebugMode}");
            ConsoleLog.Info(string.Empty);

            if (DebugMode)
            {
                int sleepTime = 15;
                ConsoleLog.Info($"Running on Debug Mode, you have {sleepTime} seconds to attached the process now for debugging: {Assembly.GetExecutingAssembly()}.");
                Thread.Sleep(sleepTime * 1000);
            }
        }

        protected void OnStopped()
        {
            WatchTimer.Stop();
            ConsoleLog.Info($"Total Time: {WatchTimer.ElapsedMilliseconds / 1000} second(s)");

            //pause to display result for debugging
            if (DebugMode)
                ConsoleLog.Pause();
        }

        private void Log(string log)
        {
            ConsoleLog.Info(log);
            ConsoleLog.Info(string.Empty);
        }

        protected void OnRun()
        {
            try
            {
                CrmConn = new CrmConnector(CrmConnectionString);

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
                    ConsoleLog.Info($"Current Version: {oldVersion}");

                    //calculate new version
                    string[] versionSplit = oldVersion.Split(new string[] { "." }, StringSplitOptions.None);
                    if (versionSplit?.Length == 4)
                    {
                        int major = int.Parse(versionSplit[0]);
                        int minor = int.Parse(versionSplit[1]);
                        int build = int.Parse(versionSplit[2]);
                        int revision = int.Parse(versionSplit[3]);

                        if (IncrementMajor)
                            major++;
                        if (IncrementMinor)
                            minor++;
                        if (IncrementBuild)
                            build++;
                        if (IncrementRevision)
                            revision++;

                        newVersion = $"{major}.{minor}.{build}.{revision}";
                        ConsoleLog.Info($"New Version: {newVersion}");
                    }
                    else
                    {
                        throw new ArgumentException($"The solution '{SolutionName}' version of '{oldVersion}' is invalid.");
                    }

                    //set new version
                    if (!newVersion.Equals(oldVersion))
                    {
                        //update
                        ConsoleLog.Info($"Update the solution '{SolutionName}' to the version '{newVersion}'.");
                        Entity solUpdate = new Entity(sol.LogicalName)
                        {
                            Id = sol.Id,
                        };
                        solUpdate["version"] = newVersion;
                        CrmConn.OrgService.Update(solUpdate);
                    }
                    else
                        ConsoleLog.Info($"No Update, the new version is the same as the current version.");
                }
                else
                {
                    throw new ArgumentException($"The solution name '{SolutionName}' is not found.");
                }
            }
            catch (Exception ex)
            {
                ConsoleLog.Error($"Error: {ex.Message}; Trace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                ConsoleLog.Info($"Run Completed...");
                ConsoleLog.Info($"CRM Connection String: {CrmConnectionString}");
            }
        }
        #endregion Run
    }
}