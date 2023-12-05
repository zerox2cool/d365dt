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

namespace ZStudio.D365.DeploymentHelper.Core.CmdLineTools
{
    public class TestConnection
    {
        private Stopwatch WatchTimer = new Stopwatch();

        public bool DebugMode { get; private set; }

        public string CrmConnectionString { get; private set; }

        public Dictionary<string, object> Config { get; private set; }

        public CrmConnector CrmConn { get; private set; }

        public TestConnection(string crmConnectionString, Dictionary<string, object> config, bool debugMode)
        {
            DebugMode = debugMode;

            CrmConnectionString = crmConnectionString;
            Config = config;
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
            ConsoleLog.Info($"Helper: {nameof(TestConnection)}");
            ConsoleLog.Info($"CRM Connection String: {CrmConnectionString}");
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

                CrmConnectionWhoAmIUser whoAmI = CrmConn.WhoAmI;
                if (whoAmI != null)
                {
                    ConsoleLog.Info($"WhoAmI: {whoAmI.UserId}");
                    ConsoleLog.Info($"FullName: {whoAmI.FullName}");
                }

                if (CrmConn != null)
                {
                    ConsoleLog.Info($"OrganizationDataServiceUrl: {CrmConn.OrganizationDataServiceUrl}");
                    ConsoleLog.Info($"OrganizationFriendlyName: {CrmConn.OrganizationFriendlyName}");
                    ConsoleLog.Info($"OrganizationId: {CrmConn.OrganizationId}");
                    ConsoleLog.Info($"OrganizationServiceUrl: {CrmConn.OrganizationServiceUrl}");
                    ConsoleLog.Info($"OrganizationUniqueName: {CrmConn.OrganizationUniqueName}");
                    ConsoleLog.Info($"TenantId: {CrmConn.TenantId}");
                    ConsoleLog.Info($"WebApplicationUrl: {CrmConn.WebApplicationUrl}");
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