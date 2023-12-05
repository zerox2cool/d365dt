using Newtonsoft.Json;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using ZStudio.D365.DeploymentHelper.Core.CmdLineTools;
using ZStudio.D365.Shared.Data.Framework.Cmd;

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
                string configFile = CmdArgsHelper.ReadArgument(args, "config", false, @"The optional config JSON file to be used by the helper program to run, the JSON is a serialised Dictionary<string, object>. Pass in NULL when there is no config.");
                string debug = CmdArgsHelper.ReadArgument(args, "debug", false, "Debug Mode, default to false. e.g. true or false");
                if (string.IsNullOrEmpty(debug))
                    debugMode = false;
                else
                    debugMode = bool.Parse(debug);

                if (!string.IsNullOrEmpty(configFile) && configFile.Equals("null", StringComparison.CurrentCultureIgnoreCase))
                    configFile = string.Empty;
                string sourceCrmUrl = CmdArgsHelper.GetCrmUrlFromConnectionString(crmConnectionString);
                
                ConsoleLog.Info($"Connection String: {sourceCrmUrl}");
                ConsoleLog.Info($"Helper: {helper}");
                ConsoleLog.Info($"Working Folder Path: {Environment.CurrentDirectory}");
                ConsoleLog.Info(string.Empty);
                ConsoleLog.Info($"Read Config File ('{configFile}') if available.");

                //read config JSON that is in
                Dictionary<string, object> config = null;
                if (!string.IsNullOrEmpty(configFile))
                {
                    if (!File.Exists(configFile))
                    {
                        //try to combine with current working folder path
                        configFile = Path.Combine(Environment.CurrentDirectory, configFile);
                        if (!File.Exists(configFile))
                        {
                            throw new FileNotFoundException($"The config file '{configFile}' is not found.");
                        }
                    }
                    ConsoleLog.Info($"Config File: {configFile}");

                    try
                    {
                        config = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(configFile));
                    }
                    catch (Exception dex)
                    {
                        throw new ArgumentException($"The JSON in '{configFile}' is invalid and cannot be deserialise to Dictionary<string, object>. Exception: {dex.Message}");
                    }

                    if (config == null)
                        throw new ArgumentException($"The JSON in '{configFile}' is invalid and cannot be deserialise to Dictionary<string, object>.");
                    else
                    {
                        //log all config value
                        ConsoleLog.Info($"Config Count: {config.Count}");
                        foreach (var kvp in config)
                        {
                            ConsoleLog.Info($"{kvp.Key} = {kvp.Value}");
                        }
                    }
                }
                ConsoleLog.Info(string.Empty);
                ConsoleLog.Info($"Start Helper...");
                ConsoleLog.Info(string.Empty);

                switch (helper.ToUpper())
                {
                    case "TESTCONNECTION":
                        TestConnection tc = new TestConnection(crmConnectionString, config, debugMode);
                        tc.Run();
                        result = ExecutionReturnCode.Success;
                        break;

                    case "CALCULATESOLUTIONVERSION":
                        CalculateSolutionVersion csv = new CalculateSolutionVersion(crmConnectionString, config, debugMode);
                        csv.Run();
                        result = ExecutionReturnCode.Success;
                        break;

                    case "INCREMENTSOLUTIONVERSION":
                        IncrementSolutionVersion isv = new IncrementSolutionVersion(crmConnectionString, config, debugMode);
                        isv.Run();
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