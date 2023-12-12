using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using ZStudio.D365.DeploymentHelper.Core.Util;
using ZStudio.D365.Shared.Data.Framework.Cmd;
using ZStudio.D365.Shared.Framework.Util;

namespace ZStudio.D365.DeploymentHelper.Core.Base
{
    public abstract class HelperToolBase : IHelperToolLogic, IDisposable
    {
        private const int CRM_TEXT_MAX_LENGTH = 1048576;
        public const string LOG_LINE = "----------------------------------------------------------------------------------";
        public const string LOG_SEGMENT = "------";

        #region Variables
        private Stopwatch _watchTimer = new Stopwatch();
        private string _crmConnString = null;
        private string _configJson = null;
        private CrmConnector _crmConn = null;
        private Dictionary<string, string> _tokens = null;
        private bool _isSuccess = false;
        #endregion Variables

        #region Properties
        /// <summary>
        /// The helper name.
        /// </summary>
        public string HelperName { get; set; }

        /// <summary>
        /// Indicate the program is running in debug mode.
        /// </summary>
        public bool DebugMode { get; set; }

        /// <summary>
        /// CRM connection string
        /// </summary>
        public string CrmConnectionString
        {
            get
            {
                return _crmConnString;
            }
        }

        /// <summary>
        /// Configuration in serialized JSON.
        /// </summary>
        public string ConfigJson
        {
            get
            {
                return _configJson;
            }
        }

        /// <summary>
        /// CRM Connector
        /// </summary>
        public CrmConnector CrmConn
        {
            get
            {
                return _crmConn;
            }
        }

        /// <summary>
        /// The <see cref="IOrganizationService"/> to D365.
        /// </summary>
        public IOrganizationService OrgService
        {
            get
            {
                return CrmConn?.OrgService;
            }
        }

        /// <summary>
        /// The Fetch Helper.
        /// </summary>
        public FetchHelper Fetch { get; set; }

        /// <summary>
        /// The logger storage.
        /// </summary>
        public StringBuilder Logger { get; set; }

        /// <summary>
        /// Variable Token replacement value for the data.
        /// </summary>
        public Dictionary<string, string> Tokens
        {
            get
            {
                return _tokens;
            }
        }

        /// <summary>
        /// Logs from the execution logger <see cref="Logger"/>.
        /// </summary>
        public string Logs
        {
            get
            {
                string log = GetLog();
                //the maximum length in CRM for a Multiple Line of Text is 1048576.
                if (!string.IsNullOrEmpty(log) && log.Length > CRM_TEXT_MAX_LENGTH)
                    log = log.Substring(0, CRM_TEXT_MAX_LENGTH);
                return log;
            }
        }

        /// <summary>
        /// The result.
        /// </summary>
        public bool? IsSuccess
        {
            get
            {
                return _isSuccess;
            }
        }
        #endregion Properties

        #region Consstructor
        /// <summary>
        /// Initialize the Helper Tool
        /// </summary>
        /// <param name="crmConnectionString"></param>
        /// <param name="config"></param>
        /// <param name="debugMode"></param>
        public HelperToolBase(string crmConnectionString, string configJson, Dictionary<string, string> tokens, bool debugMode = false)
        {
            if (string.IsNullOrEmpty(crmConnectionString))
                throw new ArgumentNullException("CRM connection string is required.");

            DebugMode = debugMode;

            _crmConnString = crmConnectionString;
            _configJson = configJson;
            _tokens = tokens;

            _crmConn = new CrmConnector(CrmConnectionString);

            Logger = new StringBuilder();
            Fetch = new FetchHelper(OrgService);
        }
        #endregion Consstructor

        #region Tokens
        /// <summary>
        /// Replace values in JSON string with token variable values. The token key is case-sensitive.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private string ReplaceTokens(string json)
        {
            string result = json;

            if (!string.IsNullOrEmpty(json) && Tokens?.Count > 0)
            {
                foreach (var token in Tokens)
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
        protected string GetToken(string key)
        {
            string result = null;

            if (!string.IsNullOrEmpty(key) && Tokens?.Count > 0)
            {
                if (Tokens.ContainsKey(key))
                    result = Tokens[key];
            }

            return result;
        }
        #endregion Tokens

        #region Run
        /// <summary>
        /// Set the Success result.
        /// </summary>
        /// <param name="result"></param>
        private void SetIsSuccess(bool result)
        {
            _isSuccess = result;
        }

        /// <summary>
        /// This main method to execute the helper operation to be implemented by the base class.
        /// </summary>
        /// <returns>Returns the execution result</returns>
        public bool Run()
        {
            OnStarted();
            bool result = OnRun();
            OnStopped();

            return result;
        }

        protected void OnStarted()
        {
            _watchTimer.Start();
            ConsoleLog.Info($"{Assembly.GetEntryAssembly().FullName} Starting...");
            ConsoleLog.Info($"Helper: {HelperName}");
            ConsoleLog.Info($"CRM Connection String: {ArgsHelper.MaskCrmConnectionString(CrmConnectionString)}");
            ConsoleLog.Info($"Debug: {DebugMode}");
            ConsoleLog.Info(LOG_LINE);

            if (DebugMode)
            {
                int sleepTime = 15;
                ConsoleLog.Info($"Running on Debug Mode, you have {sleepTime} seconds to attached the process now for debugging: {Assembly.GetExecutingAssembly()}.");
                ConsoleLog.Info(LOG_LINE);
                Thread.Sleep(sleepTime * 1000);
            }
        }

        protected void OnStopped()
        {
            _watchTimer.Stop();
            ConsoleLog.Info($"Total Time: {_watchTimer.ElapsedMilliseconds / 1000} second(s)");
            ConsoleLog.Info(LOG_LINE);

            //pause to display result for debugging
            if (DebugMode)
            {
                ConsoleLog.Info($"On Debug Mode...");
                ConsoleLog.Pause();
            }
        }

        protected bool OnRun()
        {
            try
            {
                ConsoleLog.Info($"Helper Default Diagnostic...");
                CrmConnector.CrmConnectionWhoAmIUser whoAmI = CrmConn.WhoAmI;
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
                ConsoleLog.Info(LOG_LINE);

                //pre-execution
                PreExecute_HandlerImplementation();

                //run logic
                ConsoleLog.Info(string.Empty);
                ConsoleLog.Info($"Execute OnRun_Implementation...");
                ConsoleLog.Info(LOG_LINE);

                bool result = OnRun_Implementation(out string exceptionMessage);
                ConsoleLog.Info($"OnRun_Implementation Result: {result}");

                //set the result
                SetIsSuccess(result);

                if (!result)
                    ConsoleLog.Info($"OnRun_Implementation Exception: {exceptionMessage}");

                //run post execution process for success/failure
                if (result)
                    OnSuccess_HandlerImplementation();
                else
                    OnFailure_HandlerImplementation();

                //post-execution
                PostExecute_HandlerImplementation();

                //display log
                ConsoleLog.Info($"Logs:");
                ConsoleLog.Info($"{GetLog()}");
                ConsoleLog.Info(LOG_LINE);

                return result;
            }
            catch (Exception ex)
            {
                ConsoleLog.Info(LOG_LINE);
                ConsoleLog.Error($"Error: {ex.Message}; Trace: {ex.StackTrace}");
                ConsoleLog.Info(LOG_LINE);

                //handle exception and the result will be failure, the on-failure handler will not run, only the exception handler is executed
                SetIsSuccess(false);
                OnException_HandlerImplementation(ex);

                throw;
            }
            finally
            {
                ConsoleLog.Info($"Run Completed...");
                ConsoleLog.Info($"CRM Connection String: {ArgsHelper.MaskCrmConnectionString(CrmConnectionString)}");
                ConsoleLog.Info(LOG_LINE);
            }
        }

        #region RunImplementation
        /// <summary>
        /// The customizeable implementation method for business logic before execution starts to be written on the implementation class. This will run when the before the execute starts.
        /// </summary>
        public virtual void PreExecute_HandlerImplementation()
        {
            Log($"Pre-Execution: Base Implementation");
        }

        /// <summary>
        /// The customizeable implementation method for business logic after execution starts to be written on the implementation class. This will run when at the end of the execute.
        /// </summary>
        public virtual void PostExecute_HandlerImplementation()
        {
            Log($"Post-Execution: Base Implementation");
        }

        /// <summary>
        /// The implementation method where the execution business logic is to be written on the implementation class.
        /// </summary>
        /// <returns>Returns the execution result</returns>
        protected abstract bool OnRun_Implementation(out string exceptionMessage);

        /// <summary>
        /// The customizeable implementation method for on-success business logic to be written on the implementation class. This will run when the execute result is success.
        /// </summary>
        public virtual void OnSuccess_HandlerImplementation()
        {
            Log($"OnSuccess: Base Result TRUE.");
        }

        /// <summary>
        /// The customizeable implementation method for on-failure business logic to be written on the implementation class. This will run when the execute result is failure.
        /// </summary>
        public virtual void OnFailure_HandlerImplementation()
        {
            Log($"OnFailure: Base Result FALSE.");
        }

        /// <summary>
        /// The customizeable implementation method for exception handler to be written on the implementation class. This will run when the execute hits an unhandled exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public virtual void OnException_HandlerImplementation(Exception ex)
        {
            Log($"OnException: {ex.Message}");
        }
        #endregion RunImplementation
        #endregion Run

        #region Logs
        /// <summary>
        /// Get the Logs from the Logger storage.
        /// </summary>
        /// <param name="isClear"></param>
        public string GetLog(bool isClear = false)
        {
            if (isClear)
            {
                string logs = Logger.ToString();
                Logger.Clear();
                return logs;
            }
            else
                return Logger.ToString();
        }

        /// <summary>
        /// Log a text to the Logger storage.
        /// </summary>
        /// <param name="text"></param>
        public void Log(string text)
        {
            Logger.AppendLine(text);
        }

        /// <summary>
        /// Log a formatted text to the Logger storage.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="p"></param>
        public void Log(string format, params object[] p)
        {
            Logger.AppendLine(string.Format(format, p));
        }

        /// <summary>
        /// Log a debug text to the Logger storage.
        /// </summary>
        /// <param name="text"></param>
        public void Debug(string text)
        {
            if (DebugMode)
                Logger.AppendLine(text);
        }
        #endregion Logs

        #region Dispose
        /// <summary>
        /// Disposes the resources.
        /// </summary>
        public void Dispose()
        {
            _crmConn = null;
            Logger = null;
        }
        #endregion Dispose
    }
}