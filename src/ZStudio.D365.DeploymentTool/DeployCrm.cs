using ZD365DT.DeploymentTool.Configuration;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Utils.CrmMetadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace ZD365DT.DeploymentTool
{
    public class DeployCrm
    {
        private static readonly IDeploymentContext context;
        private static readonly DeploymentConfigurationSection deploymentConfig;
        public static string ConfigEnvironment = string.Empty;
        private static bool Debugging = false;
        public static bool Stopwatch = false;
        public static bool LogVerbose = false;
        private static int SleepMiliseconds = 10000;
        private static readonly char[] TRIM_CHARS = new char[] { ':', ' ', '@', '"' };
        private static string CONFIG = "config";
        private static string SLEEP = "/sleep";
        private static string STOPWATCH = "/stopwatch";
        private static string VERBOSE = "/v";

        static DeployCrm()
        {
            Console.Title = "Microsoft Dynamics 365 CE Deployment Tool";

            try
            {
                //gets the configuration file to be used from the Command Arguments
                //e.g. "config:DeployD365Customizations_SIT.config /sleep:10 /stopwatch /v"
                Dictionary<string, object> args = null;
                GetConfigSectionFromCommandArguments(out args);
                //set command line values
                if (args.ContainsKey(CONFIG))
                    ConfigEnvironment = (string)args[CONFIG];
                else
                    throw new FormatException("The config command-line argument is required.");
                if (args.ContainsKey(SLEEP))
                {
                    SleepMiliseconds = (int)args[SLEEP];
                    if ((int)args[SLEEP] > 0)
                        Debugging = true;
                }
                if (args.ContainsKey(STOPWATCH))
                    Stopwatch = (bool)args[STOPWATCH];
                if (args.ContainsKey(VERBOSE))
                    LogVerbose = (bool)args[VERBOSE];

                if (Debugging)
                    Thread.Sleep(SleepMiliseconds);

                if (!string.IsNullOrEmpty(ConfigEnvironment))
                    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", ConfigEnvironment);
                       
                deploymentConfig = DeploymentConfigurationSection.ReadFromConfigFile(ConfigEnvironment);
                context = new DeploymentContext(deploymentConfig);
            }
            catch (FormatException fex)
            {
                ExecutionReturnCode exitCode = ExecutionReturnCode.InvalidCommandline;
                Logger.LogError("The command line/configuration file is invalid. Message: '{0}'", fex.Message);
                Logger.LogInfo("Deploy CRM Customization failed. Status: 0x{0} ({1})", exitCode.ToString("X"), exitCode.ToString("G"));

                Environment.Exit((int)exitCode);
            }
            catch (Exception ex)
            {
                ExecutionReturnCode exitCode = ExecutionReturnCode.InvalidConfiguration;
                Logger.LogError("The command line/configuration file is invalid. Message: '{0}'", ex.Message);
                Logger.LogInfo("Deploy CRM Customization failed. Status: 0x{0} ({1})", exitCode.ToString("X"), exitCode.ToString("G"));

                Environment.Exit((int)exitCode);
            }
        }

        internal static void GetConfigSectionFromCommandArguments(out Dictionary<string, object> args)
        {
            args = new Dictionary<string, object>();
            List<string> commandLineArgs = new List<string>();
            commandLineArgs.AddRange(Environment.GetCommandLineArgs());
            commandLineArgs.RemoveAt(0); // The first argument is the program name

            if (commandLineArgs.Count > 0)
            {
                string str = string.Empty;
                for (int i = 0; i < commandLineArgs.Count; i++)
                {
                    str = commandLineArgs[i].ToString();

                    int splitIndex = 0;
                    string tokenString = null;
                    string valueString = null;
                    if (str.IndexOf(":") <= 0)
                    {
                        tokenString = str.Trim(TRIM_CHARS);
                    }
                    else
                    {
                        splitIndex = str.IndexOf(":");
                        tokenString = str.Substring(0, splitIndex).Trim(TRIM_CHARS);
                        valueString = str.Substring(splitIndex, str.Length - splitIndex).Trim(TRIM_CHARS);
                    }
                    
                    if (tokenString.ToLower() == CONFIG)
                        args[CONFIG] = valueString;
                    else if (tokenString.ToLower() == SLEEP)
                    {
                        try
                        {
                            args[SLEEP] = Convert.ToInt32(valueString);
                        }
                        catch
                        {
                            //set default
                            args[SLEEP] = 0;
                        }
                    }
                    else if (tokenString.ToLower() == STOPWATCH)
                    {
                        args[STOPWATCH] = true;
                    }
                    else if (tokenString.ToLower() == VERBOSE)
                    {
                        args[VERBOSE] = true;
                    }
                }
            }
        }

        private void LogExecuteAction(InstallAction action, ref ExecutionReturnCode executionResult)
        {
            if (!deploymentConfig.FailOnError || executionResult == ExecutionReturnCode.Success)
            {
                ExecutionReturnCode actionResult = action.Execute();
                if (actionResult != ExecutionReturnCode.Success)
                {
                    Logger.LogError(action.GetExecutionErrorMessage());
                }
                executionResult |= actionResult;
            }
        }

        private ExecutionReturnCode Execute()
        {
            long grandTotalElapsedMiliseconds = 0;
            ExecutionReturnCode result = ExecutionReturnCode.Success;
            foreach (DeploymentOrganizationElement deploymentOrganization in deploymentConfig.DeploymentOrganizations)
            {
                //replace the token on enabled attribute
                deploymentOrganization.Enabled = IOUtils.ReplaceStringTokens(deploymentOrganization.Enabled, context.Tokens);

                //exeucte when enabled
                if (deploymentOrganization.IsEnabled)
                {
                    //Get the orgnizationService
                    string connectionString = deploymentOrganization.CrmConnection.CrmConnectionString;
                    string orgName = deploymentOrganization.CrmConnection.OrganizationName;
                    string enforceTls12 = deploymentOrganization.CrmConnection.EnforceTls12;
                    deploymentOrganization.CrmConnection.SetAuthenticationType(IOUtils.ReplaceStringTokens(deploymentOrganization.CrmConnection.AuthenticationTypeText, context.Tokens));

                    CrmAuthenticationType authType = deploymentOrganization.CrmConnection.AuthenticationType;
                    CrmDeploymentServiceElement deployServiceElement = deploymentOrganization.CrmDeployment;

                    //replace tokens in input strings
                    connectionString = IOUtils.ReplaceStringTokens(connectionString, context.Tokens);
                    orgName = IOUtils.ReplaceStringTokens(orgName, context.Tokens);
                    enforceTls12 = IOUtils.ReplaceStringTokens(enforceTls12, context.Tokens);
                    //ensure enforceTls12 is a boolean value
                    if (string.IsNullOrEmpty(enforceTls12))
                        enforceTls12 = "false";
                    try
                    {
                        enforceTls12 = bool.Parse(enforceTls12).ToString();
                    }
                    catch
                    {
                        enforceTls12 = false.ToString();
                    }

                    Logger.LogInfo(" ");
                    Logger.LogInfo("Starting {0} ({1})... for Organization: {2}", deploymentConfig.Action, deploymentOrganization.Name, orgName);

                    //create a new util instance for all the deployment org. 
                    WebServiceUtils util = new WebServiceUtils(connectionString, orgName, authType, deployServiceElement, context.Tokens, bool.Parse(enforceTls12));

                    //display current CRM connection context
                    util.CurrentOrgContext.DisplayCurrentContext();

                    //set context to entity collection
                    CrmEntityCollection.CurrentOrgContext = util.CurrentOrgContext;

                    //set current version to Metadata helper
                    Metadata.SetCurrentCrmVersion(util.CurrentOrgContext.InitialVersion);

                    #region Add the Deployment Config Sections
                    List<InstallAction> installActions = new List<InstallAction>();
                    if (DeploymentOrganizationElement.IsSectionDefined(deploymentOrganization.OrganizationCreateDelete))
                    {
                        installActions.Add(new InstallOrganizationAction(deploymentOrganization.OrganizationCreateDelete, context, util));
                    }

                    if (DeploymentOrganizationElement.IsSectionDefined(deploymentOrganization.Assemblies))
                    {
                        installActions.Add(new InstallAssemblyAction(deploymentOrganization.Assemblies, context, util));
                    }

                    if (DeploymentOrganizationElement.IsSectionDefined(deploymentOrganization.WebResources))
                    {
                        installActions.Add(new InstallWebResourceAction(deploymentOrganization.WebResources, context, util));
                    }

                    if (DeploymentOrganizationElement.IsSectionDefined(deploymentOrganization.Customizations))
                    {
                        installActions.Add(new InstallCustomizationsAction(deploymentOrganization.Customizations, context, util, deploymentOrganization.Customizations.ElementInformation.LineNumber));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.Copy))
                    {
                        installActions.Add(new InstallCopyAction(deploymentOrganization.Copy, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.Delete))
                    {
                        installActions.Add(new InstallDeleteAction(deploymentOrganization.Delete, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.DuplicateDetection))
                    {
                        installActions.Add(new InstallDuplicateDetectionAction(deploymentOrganization.DuplicateDetection, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.Plugins))
                    {
                        installActions.Add(new InstallPluginsAction(deploymentOrganization.Plugins, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.AssemblyModes))
                    {
                        installActions.Add(new InstallAssemblyModeAction(deploymentOrganization.AssemblyModes, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.Reports))
                    {
                        installActions.Add(new InstallReportsAction(deploymentOrganization.Reports, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.ExternalActions))
                    {
                        installActions.Add(new InstallExternalAction(deploymentOrganization.ExternalActions, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.SqlScripts))
                    {
                        installActions.Add(new InstallSqlAction(deploymentOrganization.SqlScripts, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.Websites))
                    {
                        installActions.Add(new InstallWebsiteAction(deploymentOrganization.Websites, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.WindowsServices))
                    {
                        installActions.Add(new InstallWindowsServiceAction(deploymentOrganization.WindowsServices, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.DataMap))
                    {
                        installActions.Add(new InstallDataMapAction(deploymentOrganization.DataMap, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.DataImport))
                    {
                        installActions.Add(new InstallDataImportAction(deploymentOrganization.DataImport, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.DataDelete))
                    {
                        installActions.Add(new InstallDataDeleteAction(deploymentOrganization.DataDelete, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.DataExport))
                    {
                        installActions.Add(new InstallDataExportAction(deploymentOrganization.DataExport, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.IncrementSolutionBuildNumber))
                    {
                        installActions.Add(new InstallIncrementSolutionNumberAction(deploymentOrganization.IncrementSolutionBuildNumber, context, util, deploymentOrganization.IncrementSolutionBuildNumber.ElementInformation.LineNumber));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.AssignSecurityRole))
                    {
                        installActions.Add(new InstallAssignSecurityRoleAction(deploymentOrganization.AssignSecurityRole, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.AssignWorkflow))
                    {
                        installActions.Add(new InstallAssignWorkflowAction(deploymentOrganization.AssignWorkflow, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.ActivateWorkflows))
                    {
                        installActions.Add(new InstallActivateWorkflowsAction(deploymentOrganization.ActivateWorkflows, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.StopWindowsService))
                    {
                        installActions.Add(new InstallStopWindowsServiceAction(deploymentOrganization.StopWindowsService, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.StartWindowsService))
                    {
                        installActions.Add(new InstallStartWindowsServiceAction(deploymentOrganization.StartWindowsService, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.GetPluginXml))
                    {
                        installActions.Add(new InstallPluginXmlExport(deploymentOrganization.GetPluginXml, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.RibbonExportImport))
                    {
                        installActions.Add(new InstallRibbonAction(deploymentOrganization.RibbonExportImport, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.SitemapExportImport))
                    {
                        installActions.Add(new InstallSitemapAction(deploymentOrganization.SitemapExportImport, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.CrmTheme))
                    {
                        installActions.Add(new InstallThemeAction(deploymentOrganization.CrmTheme, context, util));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.DefaultPublishers))
                    {
                        installActions.Add(new InstallDefaultPublisherAction(deploymentOrganization.DefaultPublishers, context, util, deploymentOrganization.DefaultPublishers.ElementInformation.LineNumber));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.Publishers))
                    {
                        installActions.Add(new InstallPublisherAction(deploymentOrganization.Publishers, context, util, deploymentOrganization.Publishers.ElementInformation.LineNumber));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.Solutions))
                    {
                        installActions.Add(new InstallSolutionAction(deploymentOrganization.Solutions, context, util, deploymentOrganization.Solutions.ElementInformation.LineNumber));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.GlobalOpMetadatas))
                    {
                        installActions.Add(new InstallGlobalOpDefAction(deploymentOrganization.GlobalOpMetadatas, context, util, deploymentOrganization.GlobalOpMetadatas.ElementInformation.LineNumber));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.EntityMetadatas))
                    {
                        installActions.Add(new InstallEntityDefAction(deploymentOrganization.EntityMetadatas, context, util, deploymentOrganization.EntityMetadatas.ElementInformation.LineNumber));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.ManyToManyRelationMetadatas))
                    {
                        installActions.Add(new InstallManyToManyRelationDefAction(deploymentOrganization.ManyToManyRelationMetadatas, context, util, deploymentOrganization.ManyToManyRelationMetadatas.ElementInformation.LineNumber));
                    }

                    if (DeploymentConfigurationSection.IsSectionDefined(deploymentOrganization.PatchCustomizationXml))
                    {
                        installActions.Add(new InstallPatchCustomizationXmlAction(deploymentOrganization.PatchCustomizationXml, context, util, deploymentOrganization.ManyToManyRelationMetadatas.ElementInformation.LineNumber));
                    }
                    #endregion Add the Deployment Config Sections

                    // Sort the collection based on the line-number in the config file
                    installActions.Sort();

                    // If uninstalling, execute the actions in reverse order
                    if (deploymentConfig.Action == DeploymentAction.Uninstall)
                    {
                        installActions.Reverse();
                    }

                    WatchSummary summary = new WatchSummary();
                    foreach (InstallAction action in installActions)
                    {
                        //start timing to a stopwatch
                        TimerWatch tw = new TimerWatch(action.ActionName, action.ActionType, !action.EnableStopwatchDetail, summary);
                        tw.Start();

                        //execute the action
                        LogExecuteAction(action, ref result);

                        //end stopwatch and log the time
                        grandTotalElapsedMiliseconds += tw.Stop();
                    }
                    summary.DisplaySummary();

                    Logger.LogInfo("{0} completed for Organization {1}. Status: 0x{2} ({3})", deploymentConfig.Action, deploymentOrganization.Name, result.ToString("X"), result.ToString("G"));
                }
                else
                {
                    //deployment configuration disabled for this
                    string orgName = deploymentOrganization.CrmConnection.OrganizationName;
                    Logger.LogInfo(" ");
                    Logger.LogInfo("{0} ({1}) is disabled... for Organization: {2}", deploymentConfig.Action, deploymentOrganization.Name, orgName);
                }
            }

            Logger.LogInfo("{0} completed. Status: 0x{1} ({2})", deploymentConfig.Action, result.ToString("X"), result.ToString("G"));
            Logger.LogInfo("Deployment Total Time: {0} seconds", (decimal)grandTotalElapsedMiliseconds / 1000);
            Logger.LogInfo(" ");
            return result;
        }

        [STAThread]
        public static int Main(string[] args)
        {
            ExecutionReturnCode result = ExecutionReturnCode.UnknownFailure;
            try
            {
                //Set the working folder to be the location of the exe
                Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                DeployCrm deployCrm = new DeployCrm();
                result = deployCrm.Execute();
                Environment.Exit((int)result);
                return (int)result;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                Environment.Exit((int)ExecutionReturnCode.UnknownFailure);
                return (int)ExecutionReturnCode.UnknownFailure;
            }
        }
    }
}
