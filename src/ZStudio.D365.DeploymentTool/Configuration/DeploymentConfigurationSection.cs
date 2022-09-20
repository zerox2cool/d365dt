using System;
using System.Configuration;
using System.Reflection;
using System.IO;

namespace ZD365DT.DeploymentTool.Configuration
{
    /// <summary>
    /// The option for 'action' in the deployment context that specifies
    /// whether an execution should be an install or an uninstall
    /// </summary>
    public enum DeploymentAction
    {
        Install,
        Uninstall
    }

    /// <summary>
    /// The Class that will have the XML config file data loaded into it via the configuration Manager.
    /// </summary>
    public class DeploymentConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("xmlns", IsRequired = false)]
        public string XmlNamespace { get { return (string)base["xmlns"]; } }

        /// <summary>
        /// The value of the property here "action" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("action", DefaultValue = DeploymentAction.Install)]
        public DeploymentAction Action { get { return (DeploymentAction)base["action"]; } }

        /// <summary>
        /// The value of the property here "failOnError" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("failOnError", DefaultValue = false)]
        public bool FailOnError { get { return (bool)base["failOnError"]; } }

        /// <summary>
        /// The value of the property here "loggingDirectory" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("loggingDirectory", IsRequired = true)]
        public string LoggingDirectory { get { return (string)base["loggingDirectory"]; } }

        /// <summary>
        /// Get the configuration specified in the app.config file
        /// </summary>
        /// <returns>instance of DeploymentOrganizationCollection read from the config file</returns>
        [ConfigurationProperty("deploymentOrganizations", IsDefaultCollection = true)]
        public DeploymentOrganizationCollection DeploymentOrganizations { get { return (DeploymentOrganizationCollection)base["deploymentOrganizations"]; } }

        /// <summary>
        /// Get the configuration specified in the app.config file
        /// </summary>
        /// <returns>instance of ExecutionContextsCollection read from the config file</returns>
        [ConfigurationProperty("executionContexts")]
        public ExecutionContextsCollection ExecutionContexts { get { return (ExecutionContextsCollection)base["executionContexts"]; } }

        /// <summary>
        /// Get the current configuration specified in the app.config file
        /// </summary>
        /// <returns>instance of DeploymentConfigurationSection read from the config file</returns>
        public static DeploymentConfigurationSection ReadFromConfigFile(string environment)
        {
            DeploymentConfigurationSection section = null;
            //http://social.msdn.microsoft.com/Forums/en-US/clr/thread/1e14f665-10a3-426b-a75d-4e66354c5522
            //http://social.msdn.microsoft.com/Forums/en/netfxbcl/thread/ec237df4-f05e-4711-a230-fb089c395c73

            if (!string.IsNullOrEmpty(environment))
            {
                string path = Path.GetFullPath(environment);
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = path;
                if (!File.Exists(fileMap.ExeConfigFilename))
                    throw new ArgumentException("Configuration file do not Exists : ", path);

                var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                section = config.GetSection("deploymentConfiguration") as DeploymentConfigurationSection;
            }
            else
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                section = config.GetSection("deploymentConfiguration") as DeploymentConfigurationSection;
            }
            return section;
        }

        /// <summary>
        /// Returns whether a ConfugurationElement has been defined in the configuration file
        /// </summary>
        /// <param name="element">The element to check</param>
        /// <returns><c>True</c> if the element exists, otherwise <c>False</c></returns>
        public static bool IsSectionDefined(ConfigurationElement element)
        {
            return (element != null && element.ElementInformation != null && element.ElementInformation.IsPresent);
        }
    }
}