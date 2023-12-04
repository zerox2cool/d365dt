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

namespace ZStudio.D365.DeploymentHelper.CmdLineTools
{
    public class TestConnection
    {
        private Stopwatch WatchTimer = new Stopwatch();

        public bool DebugMode { get; private set; }

        public string CrmConnectionString { get; private set; }
        
        public CrmConnector CrmConn { get; private set; }

        public TestConnection(string crmConnectionString, bool debugMode)
        {
            DebugMode = debugMode;

            CrmConnectionString = crmConnectionString;
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
            ConsoleLog.Info("{0} Starting...", Assembly.GetEntryAssembly().FullName);
            //hide the password
            ConsoleLog.Info("CRM Connection String: {0}", CrmConnectionString);
            ConsoleLog.Info("Debug: {0}", DebugMode);
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
            ConsoleLog.Info("Total Time: {0} second(s)", WatchTimer.ElapsedMilliseconds / 1000);

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
                ConsoleLog.Error("Error: {0}; Trace: {1}", ex.Message, ex.StackTrace);
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