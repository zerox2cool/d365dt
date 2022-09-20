// Using the TraceListeners and Writers for logging so have to define the TRACE switch
#define TRACE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Web.Services.Protocols;
using ZD365DT.DeploymentTool.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Deployment;

namespace ZD365DT.DeploymentTool.Utils
{
    public class Logger
    {
        #region Private Constants
        private const string FORMAT_MESSAGE = "{0}|{1}| {2}";
        private const string VERBOSE_MESSAGE = "Verbose";
        private const string INFO_MESSAGE = "   Info";
        private const string WARNING_MESSAGE = "Warning";
        private const string ERROR_MESSAGE = "  Error";
        private const string TIME_FORMAT = "HH:mm:ss";
        private const string DATE_FORMAT = "yyyy-MM-dd HH.mm.ss";
        private static readonly char[] TRIM_CHARS = new char[] { ':', ' ', '@', '"' };
        private static string CONFIG = "config";
        private static readonly string backupDirectory;
        #endregion Private Constants

        #region Variables
        public static StringBuilder AllLogs = new StringBuilder();
        private static bool Verbose = false;
        #endregion Variables

        #region Constructors
        static Logger()
        {
            TextWriterTraceListener consoleWriter = new TextWriterTraceListener(Console.Out);
            StringWriter sw = new StringWriter(AllLogs);
            TextWriterTraceListener sbWriter = new TextWriterTraceListener(sw);

            Trace.AutoFlush = true;
            Trace.Listeners.Clear();
            Trace.Listeners.Add(consoleWriter);
            Trace.Listeners.Add(sbWriter);

            Exception setupException = null;
            try
            {
                try
                {
                    Verbose = DeployCrm.LogVerbose;
                }
                catch (Exception)
                {
                    Verbose = true;
                }

                DeploymentConfigurationSection config = null;
                try
                {
                    config = DeploymentConfigurationSection.ReadFromConfigFile(DeployCrm.ConfigEnvironment);
                }
                catch (Exception)
                {
                    //if reading from DeployCrm failed, pass in null and use the application config
                    config = DeploymentConfigurationSection.ReadFromConfigFile(null);
                }
                //string directoryName = String.Format("{0} {1}", DateTime.Now.ToString(DATE_FORMAT), config.CrmConnection.OrganizationName);
                string directoryName = String.Format("{0} {1}", DateTime.Now.ToString(DATE_FORMAT),string.Empty);
                string backupdir = string.Empty;

                if (config == null || config.LoggingDirectory == null)
                    backupdir = ".";
                else
                    backupdir = config.LoggingDirectory;

                backupDirectory = Path.Combine(backupdir, directoryName);
                if (!Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }
                string logFileName = Path.Combine(BackupDirectory, "Deploy CRM Customizations.log");
                TextWriterTraceListener logWriter = new TextWriterTraceListener(logFileName);
                Trace.Listeners.Add(logWriter);

                Assembly assembly = Assembly.GetExecutingAssembly();
                string appName = "Microsoft Dynamics 365 CE Deployment Tool";
                if (Attribute.IsDefined(assembly, typeof(AssemblyDescriptionAttribute)))
                {
                    AssemblyDescriptionAttribute adAttr = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
                    appName = adAttr.Description;
                }
                LogInfo("{0}", appName);
                LogInfo("  Version          : {0}", assembly.GetName().Version.ToString(4));
                LogInfo("  Log File         : {0}", Path.GetFileName(logFileName));
                LogInfo("  Backup Directory : {0}", Path.GetFullPath(BackupDirectory));
                LogInfo("");
            }
            catch (Exception ex)
            {
                setupException = ex;
            }

            if (setupException != null)
            {
                LogError("There was an error configuring the logger - '{0}'", setupException.Message);
            }
        }
        
        private Logger()
        {
            // Define a blank private constructor to prevent instanciation of this class
        }
        #endregion Constructors

        public static string BackupDirectory { get { return backupDirectory; } }

        /// <summary>
        /// Logs an verbose message using string.Format
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="args">Optional parameters that should included in the message using String.Format</param>
        public static void LogVerbose(string message, params object[] args)
        {
            if (Verbose)
                Log(string.Format(message, args), VERBOSE_MESSAGE);
        }

        /// <summary>
        /// Logs an information message using string.Format
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="args">Optional parameters that should included in the message using String.Format</param>
        public static void LogInfo(string message, params object[] args)
        {
            Log(string.Format(message, args), INFO_MESSAGE);
        }

        /// <summary>
        /// Logs a warning message using string.Format
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="args">Optional parameters that should included in the message using String.Format</param>
        public static void LogWarning(string message, params object[] args)
        {
            Log(string.Format(message, args), WARNING_MESSAGE);
        }

        /// <summary>
        /// Logs an error message using string.Format
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="args">Optional parameters that should included in the message using String.Format</param>
        public static void LogError(string message, params object[] args)
        {
            Log(string.Format(message, args), ERROR_MESSAGE);
        }

        /// <summary>
        /// Logs an Exception message and stack trace as an error
        /// </summary>
        /// <param name="ex">The exception to log</param>
        public static void LogException(Exception ex)
        {
            if (ex is TimeoutException)
            {
                LogException((TimeoutException)ex);
            }
            else if (ex is SoapException)
            {
                LogException((SoapException)ex);
            }
            else if (ex is FaultException<OrganizationServiceFault>)
            {
                LogException((FaultException<OrganizationServiceFault>)ex);
            }
            else if (ex is FaultException<DeploymentServiceFault>)
            {
                LogException((FaultException<DeploymentServiceFault>)ex);
            }
            else
            {
                LogError("Exception: {0}{1}{2}{3}{4}",
                            ex.Message, Environment.NewLine,
                            ex.InnerException != null ? "Has Inner Exception:" + ex.InnerException.ToString() : "No Inner Exception", Environment.NewLine,
                            ex.StackTrace);
            }
        }


        /// <summary>
        /// Logs a Deployment Fault Exception message and stack trace as an error
        /// </summary>
        /// <param name="ex">The exception to log</param>
        public static void LogException(FaultException<DeploymentServiceFault> ex)
        {
            string errText = "";

            if (ex != null && ex.Detail != null && ex.Detail.ErrorDetails != null && ex.Detail.ErrorDetails.Count > 0)
            {
                foreach (KeyValuePair<string, object> errDetails in ex.Detail.ErrorDetails)
                {
                    errText += String.Format("Extra Error ({0}): {1}{2}", errDetails.Key.ToString(), errDetails.Value.ToString(), Environment.NewLine);
                }

            }

            LogError("Deployment Service Fault: {0}{1}{2}{3}{4}{5}{6}{7}{8}",
                        ex.Message, Environment.NewLine,
                        ex.Detail.InnerFault != null ? "Has Inner Fault:" + ex.Detail.InnerFault.ToString() : "No Inner Fault", Environment.NewLine,
                        ex.InnerException != null ? "Has Inner Exception:" + ex.InnerException.ToString() : "No Inner Exception", Environment.NewLine,
                        errText.Length > 0 ? errText : "No Other Error Details", Environment.NewLine,
                        ex.StackTrace);
        }


        /// <summary>
        /// Logs a Fault Exception message and stack trace as an error
        /// </summary>
        /// <param name="ex">The exception to log</param>
        public static void LogException(FaultException<OrganizationServiceFault> ex)
        {
            string errText = "";

            if (ex != null && ex.Detail != null && ex.Detail.ErrorDetails != null && ex.Detail.ErrorDetails.Count > 0)
            {
                foreach (KeyValuePair<string, object> errDetails in ex.Detail.ErrorDetails)
                {
                    errText = String.Format("Extra Error ({0}): {1}{2}", errDetails.Key.ToString(), errDetails.Value.ToString(), Environment.NewLine);
                }

            }

            LogError("Organization Service Fault: {0}{1}{2}{3}{4}{5}{6}{7}{8}", 
                        ex.Message, Environment.NewLine,
                        ex.Detail.InnerFault != null ? "Has Inner Fault:" + ex.Detail.InnerFault.ToString() : "No Inner Fault", Environment.NewLine,
                        ex.InnerException != null ? "Has Inner Exception:" + ex.InnerException.ToString() : "No Inner Exception", Environment.NewLine, 
                        errText.Length > 0 ? errText : "No Other Error Details", Environment.NewLine, 
                        ex.StackTrace);
        }

        /// <summary>
        /// Logs a Timeout Exception message and stack trace as an error
        /// </summary>
        /// <param name="ex">The exception to log</param>
        public static void LogException(TimeoutException ex)
        {
   
            LogError("Timeout Exception: {0}{1}{2}{3}{4}",
                        ex.Message, Environment.NewLine,
                        ex.InnerException != null ? "Has Inner Exception:" + ex.InnerException.ToString() : "No Inner Exception", Environment.NewLine,
                        ex.StackTrace);
        }


        /// <summary>
        /// Logs a Soap Exception message and stack trace as an error
        /// </summary>
        /// <param name="ex">The exception to log</param>
        public static void LogException(SoapException ex)
        {
            LogError("SoapException: {0}{1}{2}{3}{4}",
                        ex.Message, Environment.NewLine,
                        ex.Detail != null ? "Has Inner Exception:" + ex.Detail.InnerText : "No Inner Exception", Environment.NewLine,
                        ex.StackTrace);
         }

        
        /// <summary>
        /// Logs and formats a message in the passed in category
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="category">The category of the message. One of: Error, Info, Warning</param>
        private static void Log(string message, string category)
        {
            Trace.WriteLine(String.Format(FORMAT_MESSAGE, DateTime.Now.ToString(TIME_FORMAT), category, message));
        }
    }
}