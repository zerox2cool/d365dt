using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using ZStudio.D365.DeploymentHelper.Core.Base;
using ZStudio.D365.DeploymentHelper.Core.CmdLineTools;
using ZStudio.D365.DeploymentHelper.Core.Util;
using ZStudio.D365.Shared.Data.Framework.Cmd;
using ZStudio.D365.Shared.Framework.Util;

namespace ZStudio.D365.DeploymentHelper
{
    internal class Program
    {
        /// <summary>
        /// Stores all the Helper Tool Type Handlers, only loaded once
        /// </summary>
        internal static Dictionary<string, Type> HelperTypeHandlers = null;

        private const string DELIMITER = ";;";

        static int Main(string[] args)
        {
            ExecutionReturnCode result = ExecutionReturnCode.Success;
            bool debugMode = true;
            int debugSleep = 15;
            try
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                string crmConnectionString = CmdArgsHelper.ReadArgument(args, "connectionString", true, @"Source CRM simplified connection string. e.g. Url=https://d365source.crm6.dynamics.com/; AuthType=OAuth; Username=crmscript@zerostudio.onmicrosoft.com; LoginPrompt=Auto;");
                string helper = CmdArgsHelper.ReadArgument(args, "helper", true, @"Helper to Run, must specify the helper program to run.");
                string configFile = CmdArgsHelper.ReadArgument(args, "config", false, @"The optional config JSON file to be used by the helper program to run, the JSON is a serialised Dictionary<string, object> and can contain tokens (e.g. @token1@) that will be replace by passing in tokey key and data. Pass in NULL or do not include this argument when there is no config.");
                string tokenKey = CmdArgsHelper.ReadArgument(args, "key", false, $"(Optional) Variable Token replacement key separated by double-semicolon ({DELIMITER}), the token data must be provided in the same order as the key. Pass in NULL or do not include this argument when there is no token. e.g. token1{DELIMITER}token2");
                string tokenData = CmdArgsHelper.ReadArgument(args, "data", false, $"(Optional) Variable Token replacement data separated by double-semicolon ({DELIMITER}). Pass in NULL or do not include this argument when there is no token. e.g. data1{DELIMITER}replacethis2");
                string debug = CmdArgsHelper.ReadArgument(args, "debug", false, "Debug Mode, default to false. e.g. true or false");
                string debugSleepInSec = CmdArgsHelper.ReadArgument(args, "sleep", false, "Debug Sleep in seconds, default to 15 seconds.");
                if (string.IsNullOrEmpty(debug))
                    debugMode = false;
                else
                    debugMode = bool.Parse(debug);
                if (string.IsNullOrEmpty(debugSleepInSec))
                    debugSleep = 15;
                else
                    debugSleep = int.Parse(debugSleepInSec);

                if (string.IsNullOrEmpty(configFile) || configFile.Equals("null", StringComparison.CurrentCultureIgnoreCase))
                    configFile = string.Empty;
                if (string.IsNullOrEmpty(tokenKey) || tokenKey.Equals("null", StringComparison.CurrentCultureIgnoreCase))
                    tokenKey = string.Empty;
                if (string.IsNullOrEmpty(tokenData) || tokenData.Equals("null", StringComparison.CurrentCultureIgnoreCase))
                    tokenData = string.Empty;

                ConsoleLog.Info($"Connection String: {ArgsHelper.MaskCrmConnectionString(crmConnectionString)}");
                ConsoleLog.Info($"Helper: {helper}");
                ConsoleLog.Info($"Working Folder Path: {Environment.CurrentDirectory}");
                ConsoleLog.Info(HelperToolBase.LOG_LINE);
                ConsoleLog.Info($"Read Config File ('{configFile}') if available.");
                ConsoleLog.Info($"Token Key Length: {tokenKey.Length}");
                ConsoleLog.Info($"Token Data Length: {tokenData.Length}");
                ConsoleLog.Info(HelperToolBase.LOG_LINE);

                #region SystemTokens
                //always add system tokens from the connected D365 connection string
                Dictionary<string, string> tokens = null;
                CrmConnector conn = new CrmConnector(crmConnectionString);
                if (conn != null)
                {
                    ConsoleLog.Info($"Adding System Token retrieve from D365.");

                    if (tokens == null)
                        tokens = new Dictionary<string, string>();
                    tokens.Add(HelperTokenKey.ORGFRIENDLYNAME_TOKEN.Replace("@", ""), conn.OrganizationFriendlyName);
                    if (conn.OrganizationId != null)
                        tokens.Add(HelperTokenKey.ORGID_TOKEN.Replace("@", ""), conn.OrganizationId.Value.ToString());
                    if (conn.TenantId != null)
                        tokens.Add(HelperTokenKey.ORGTENANTID_TOKEN.Replace("@", ""), conn.TenantId.Value.ToString());
                    tokens.Add(HelperTokenKey.ORGSERVICEURL_TOKEN.Replace("@", ""), conn.OrganizationServiceUrl);
                    tokens.Add(HelperTokenKey.ORGDATAURL_TOKEN.Replace("@", ""), conn.OrganizationDataServiceUrl);
                    tokens.Add(HelperTokenKey.ORGWEBAPPURL_TOKEN.Replace("@", ""), conn.WebApplicationUrl);

                    if (conn.WhoAmI != null)
                    {
                        tokens.Add(HelperTokenKey.LOGONUSERID_TOKEN.Replace("@", ""), conn.WhoAmI.UserId.ToString());
                        tokens.Add(HelperTokenKey.LOGONUSEREMAIL_TOKEN.Replace("@", ""), conn.WhoAmI.Email);
                        tokens.Add(HelperTokenKey.LOGONUSERFULLNAME_TOKEN.Replace("@", ""), conn.WhoAmI.FullName);
                    }
                }
                else
                    throw new ArgumentException($"Unable to connect to the environment using the connection string provided ({crmConnectionString}).");
                #endregion SystemTokens

                #region UserTokens
                //read token data into Dictionary if provided
                if (!string.IsNullOrEmpty(tokenKey) && !string.IsNullOrEmpty(tokenData))
                {
                    string[] keySplit = tokenKey.Split(new string[] { DELIMITER }, StringSplitOptions.None);
                    string[] dataSplit = tokenData.Split(new string[] { DELIMITER }, StringSplitOptions.None);

                    if (keySplit.Length == dataSplit.Length)
                    {
                        for (int i = 0; i < keySplit.Length; i++)
                            tokens.Add(keySplit[i], dataSplit[i]);
                    }
                    else
                        throw new ArgumentException($"The number of token item in Token Key ({keySplit.Length}) and Token Data ({dataSplit.Length}) is different.");
                }
                #endregion UserTokens

                //get token count and value
                ConsoleLog.Info($"Token Count (including system): {tokens.Count}");
                if (tokens?.Count > 0)
                {
                    foreach (var token in tokens)
                        ConsoleLog.Info($"{token.Key}: {token.Value}");
                    ConsoleLog.Info(HelperToolBase.LOG_LINE);
                }

                #region Config
                //read config JSON that is in the file
                string configJson = string.Empty;
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
                    ConsoleLog.Info(HelperToolBase.LOG_LINE);

                    try
                    {
                        configJson = File.ReadAllText(configFile);
                        //replace with token data
                        configJson = ReplaceTokens(configJson, tokens);
                    }
                    catch (Exception dex)
                    {
                        throw new ArgumentException($"The JSON in '{configFile}' is invalid and cannot be de-tokenized. Exception: {dex.Message}");
                    }
                }
                #endregion Config

                #region LoadHelperTypes
                //load all helper type from Core DLL
                string coreDLL = Path.Combine(Environment.CurrentDirectory, "ZStudio.D365.DeploymentHelper.Core.dll");
                if (debugMode)
                {
                    ConsoleLog.Info($"Core DLL: {coreDLL}");
                    ConsoleLog.Info($"Core DLL Type Count: {Assembly.LoadFile(coreDLL).GetTypes().Length}");
                }

                Type coreDLLType = null;
                foreach (var t in Assembly.LoadFile(coreDLL).GetTypes())
                {
                    if (t.FullName.StartsWith("ZStudio.D365.DeploymentHelper.Core"))
                    {
                        coreDLLType = t;
                        break;
                    }
                }
                LoadHelperTypes(coreDLLType);
                
                //log available helper types
                if (debugMode)
                {
                    ConsoleLog.Info($"{GetHelperToolTypeList()}");
                    ConsoleLog.Info(HelperToolBase.LOG_LINE);
                }
                #endregion LoadHelperTypes

                //find helper to instantiate and execute it
                if (HelperTypeHandlers.ContainsKey(helper.ToUpper()))
                {
                    ConsoleLog.Info($"The helper for '{helper}' has been found, instantiate the helper and run.");
                    IHelperToolLogic logic = Activator.CreateInstance(HelperTypeHandlers[helper.ToUpper()], new object[] { crmConnectionString, configJson, tokens, debugMode, debugSleep }) as IHelperToolLogic;
                    if (logic != null)
                    {
                        logic.HelperName = HelperTypeHandlers[helper.ToUpper()].Name;
                        ConsoleLog.Info($"Executing Helper '{HelperTypeHandlers[helper.ToUpper()].Name}'...");
                        ConsoleLog.Info(string.Empty);

                        //execute
                        bool runResult = logic.Run(out string exceptionMessage);
                        if (runResult)
                            result = ExecutionReturnCode.Success;
                        else
                            throw new Exception(exceptionMessage);

                        ConsoleLog.Info($"Helper '{HelperTypeHandlers[helper.ToUpper()].Name}' Run Completed with Success Result...");
                    }
                }
                else
                {
                    ConsoleLog.Error($"Helper '{helper}' is NOT SUPPORTED.");
                    result = ExecutionReturnCode.Failed;
                }
            }
            catch (InvalidProgramException ipex)
            {
                ConsoleLog.Info($"Helper Run with Error...");
                ConsoleLog.Error(ipex);
                if (debugMode)
                    ConsoleLog.Pause();

                result = ExecutionReturnCode.Failed;
            }
            catch (Exception ex)
            {
                ConsoleLog.Info($"Helper Run with Error...");
                ConsoleLog.Error(ex);
                result = ExecutionReturnCode.Failed;
            }
            finally
            {
                ConsoleLog.Info($"Helper Run Completed...");
                Environment.Exit((int)result);
            }

            return (int)result;
        }

        #region Tokens
        /// <summary>
        /// Replace values in JSON string with token variable values. The token key is case-sensitive.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static string ReplaceTokens(string json, Dictionary<string, string> tokens)
        {
            string result = json;

            if (!string.IsNullOrEmpty(json) && tokens?.Count > 0)
            {
                foreach (var token in tokens)
                {
                    result = result.Replace($"@{token.Key}@", token.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Return the token value for of key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected static string GetToken(string key, Dictionary<string, string> tokens)
        {
            string result = null;

            if (!string.IsNullOrEmpty(key) && tokens?.Count > 0)
            {
                if (tokens.ContainsKey(key))
                    result = tokens[key];
            }

            return result;
        }
        #endregion Tokens

        #region LoadHelperTypes
        /// <summary>
        /// Load the collection of Helper Type handlers in the assembly.
        /// </summary>
        private static void LoadHelperTypes(Type typeContainingTheHelpers)
        {
            //load the collection of action extensions if it has not been loaded
            if (HelperTypeHandlers == null || HelperTypeHandlers.Count > 0)
            {
                ConsoleLog.Info($"Loading HelperTypeHandlers.");

                Type[] declaredExtensionInWorkflow = Type.EmptyTypes;
                try
                {
                    ConsoleLog.Info($"Loading Assembly.");

                    declaredExtensionInWorkflow = Assembly.GetAssembly(typeContainingTheHelpers).GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    declaredExtensionInWorkflow = ex.Types;
                }
                catch
                {
                    //do nothing
                }

                //add applicable handler into a dictionary
                try
                {
                    HelperTypeHandlers = declaredExtensionInWorkflow.Where(handler => handler.GetCustomAttribute<HelperTypeAttribute>() != null).ToDictionary(key => key.GetCustomAttribute<HelperTypeAttribute>().HelperType);
                }
                catch (ArgumentException aex)
                {
                    throw new ArgumentException(string.Format("There are duplicate helper types defined. Exception: {0}", aex.Message));
                }

                //construct assembly info
                StringBuilder sb = new StringBuilder();
                if (HelperTypeHandlers == null || HelperTypeHandlers.Count == 0)
                {
                    sb.AppendLine(string.Format("PassInAssembly CodeBase: {0}", Assembly.GetAssembly(typeContainingTheHelpers).CodeBase));
                    sb.AppendLine(string.Format("PassInAssembly FullName: {0}", Assembly.GetAssembly(typeContainingTheHelpers).FullName));
                    sb.AppendLine(string.Format("CallingAssembly CodeBase: {0}", Assembly.GetCallingAssembly().CodeBase));
                    sb.AppendLine(string.Format("CallingAssembly FullName: {0}", Assembly.GetCallingAssembly().FullName));
                }

                //if no handler, throw exception
                if (HelperTypeHandlers == null || HelperTypeHandlers.Count == 0)
                    throw new Exception(string.Format("No Helper Type Extensions found. AssemblyInfo: {0}", sb.ToString()));
            }
            else
            {
                ConsoleLog.Info($"HelperTypeHandlers already loaded. Count: {HelperTypeHandlers.Count}");
            }
        }

        private static string GetHelperToolTypeList()
        {
            //log all the loaded handler
            StringBuilder sb = new StringBuilder();
            if (HelperTypeHandlers != null)
            {
                foreach (KeyValuePair<string, Type> h in HelperTypeHandlers)
                {
                    sb.AppendLine(string.Format("Key: {0}; Type: {1};", h.Key, h.Value.FullName));
                }
            }
            return sb.ToString();
        }
        #endregion LoadHelperTypes

        #region ExecutionReturnCode
        [Flags]
        public enum ExecutionReturnCode : int
        {
            Success = 0,
            Failed = 1,
        }
        #endregion ExecutionReturnCode
    }
}