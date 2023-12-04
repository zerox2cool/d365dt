using ZStudio.D365.Shared.Data.Framework.Cmd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZStudio.D365.Shared.Framework.Util;
using static ZStudio.D365.Shared.Framework.Util.CrmConnector;
using ZStudio.D365.DeploymentHelper.CmdLineTools;

namespace ZStudio.D365.DeploymentHelper
{
    internal class Program
    {
        static int Main(string[] args)
        {
            ExecutionReturnCode result = ExecutionReturnCode.Success;
            bool debugMode = true;
            try
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                string crmConnectionString = CmdArgsHelper.ReadArgument(args, "connectionString", true, @"Source CRM simplified connection string. e.g. Url=https://d365source.crm6.dynamics.com/; AuthType=OAuth; Username=crmscript@zerostudio.onmicrosoft.com; LoginPrompt=Auto;");
                string helper = CmdArgsHelper.ReadArgument(args, "helper", true, @"Helper to Run, must specify the helper program to run.");
                string configFile = CmdArgsHelper.ReadArgument(args, "config", false, @"The optional config JSON file to be used by the helper program to run. Pass in NULL when there is no config.");
                string debug = CmdArgsHelper.ReadArgument(args, "debug", false, "Debug Mode, default to false. e.g. true or false");
                if (string.IsNullOrEmpty(debug))
                    debugMode = false;
                else
                    debugMode = bool.Parse(debug);

                if (!string.IsNullOrEmpty(configFile) && helper.Equals("null", StringComparison.CurrentCultureIgnoreCase))
                    configFile = string.Empty;
                string sourceCrmUrl = CmdArgsHelper.GetCrmUrlFromConnectionString(crmConnectionString);
                
                ConsoleLog.Info($"Connection String: {sourceCrmUrl}");
                ConsoleLog.Info($"Helper: {helper}");
                ConsoleLog.Info($"Config File: {configFile}");
                ConsoleLog.Info($"Working Folder Path: {Environment.CurrentDirectory}");

                switch (helper.ToUpper())
                {
                    case "TESTCONNECTION":
                        TestConnection tc = new TestConnection(crmConnectionString, debugMode);
                        tc.Run();
                        result = ExecutionReturnCode.Success;
                        break;

                    default:
                        ConsoleLog.Error($"Helper '{helper}' is NOT SUPPORTED.");
                        result = ExecutionReturnCode.Failed;
                        break;
                }
            }
            catch (InvalidProgramException ipex)
            {
                ConsoleLog.Error(ipex);
                if (debugMode)
                    ConsoleLog.Pause();

                result = ExecutionReturnCode.Failed;
            }
            catch (Exception ex)
            {
                ConsoleLog.Error(ex);
                result = ExecutionReturnCode.Failed;
            }
            finally
            {
                Environment.Exit((int)result);
            }

            return (int)result;
        }

        [Flags]
        public enum ExecutionReturnCode : int
        {
            Success = 0,
            Failed = 1,
        }
    }
}