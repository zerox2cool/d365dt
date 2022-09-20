using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    [ConfigurationCollection(typeof(DeploymentOrganizationElement))]
    public class DeploymentOrganizationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DeploymentOrganizationElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DeploymentOrganizationElement)element).Name;
        }
        protected override string ElementName
        {
            get
            {
                return "deploymentOrganization";
            }
        }
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
    }

    public class DeploymentOrganizationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = "true")]
        public string Enabled
        {
            get { return (string)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// Parse the value in <see cref="Enabled"/>. Return <see cref="true"/> by default.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                if (string.IsNullOrEmpty(Enabled))
                    return true;
                else
                {
                    if (bool.TryParse(Enabled, out bool result))
                        return result;
                    else
                        return true;
                }
            }
        }

        /// <summary>
        /// The value of the property here "crm" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("crm", IsRequired = true)]
        public CrmConnectionElement CrmConnection { get { return base["crm"] as CrmConnectionElement; } }

        /// <summary>
        /// The value of the property here "crmdeployment" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("crmdeployment", IsRequired = false)]
        public CrmDeploymentServiceElement CrmDeployment { get { return base["crmdeployment"] as CrmDeploymentServiceElement; } }

        /// <summary>
        /// The value of the property here "assemblies" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("workflowassemblies")]
        public AssemblyCollection Assemblies { get { return base["workflowassemblies"] as AssemblyCollection; } }

        /// <summary>
        /// The value of the property here "webresources" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("webresources")]
        public WebResourceCollection WebResources { get { return base["webresources"] as WebResourceCollection; } }

        /// <summary>
        /// The value of the property here "reports" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("reports")]
        public ReportsCollection Reports { get { return base["reports"] as ReportsCollection; } }

        /// <summary>
        /// The value of the property here "website" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("websites")]
        public WebsiteCollection Websites { get { return base["websites"] as WebsiteCollection; } }

        /// <summary>
        /// The value of the property here "windowsServices" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("windowsServices")]
        public WindowsServiceCollection WindowsServices { get { return base["windowsServices"] as WindowsServiceCollection; } }
        
        /// <summary>
        /// The value of the property here "plugin" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("plugins")]
        public PluginCollection Plugins { get { return base["plugins"] as PluginCollection; } }

        /// <summary>
        /// The value of the property here "assemblyMode" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("assemblyModes")]
        public AssemblyModeCollection AssemblyModes { get { return base["assemblyModes"] as AssemblyModeCollection; } }

        /// <summary>
        /// The value of the property here "duplicateDetectionRules" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("duplicateDetectionRules")]
        public DuplicateDetectionCollection DuplicateDetection { get { return base["duplicateDetectionRules"] as DuplicateDetectionCollection; } }

        /// <summary>
        /// The value of the property here "customization" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("customizations")]
        public CustomizationCollection Customizations { get { return base["customizations"] as CustomizationCollection; } }

        /// <summary>
        /// The value of the property here "copy" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("copy")]
        public CopyCollection Copy { get { return base["copy"] as CopyCollection; } }

        /// <summary>
        /// The value of the property here "delete" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("delete")]
        public DeleteCollection Delete { get { return base["delete"] as DeleteCollection; } }

        /// <summary>
        /// The value of the property here "sql" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("sql")]
        public SqlCollection SqlScripts { get { return base["sql"] as SqlCollection; } }

        /// <summary>
        /// The value of the property here "external" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("external")]
        public ExternalActionCollection ExternalActions { get { return base["external"] as ExternalActionCollection; } }

        /// <summary>
        /// The value of the property here "dataMap" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("dataMap")]
        public DataMapCollection DataMap { get { return base["dataMap"] as DataMapCollection; } }

        /// <summary>
        /// The value of the property here "dataImport" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("dataImport")]
        public DataImportCollection DataImport { get { return base["dataImport"] as DataImportCollection; } }

        /// <summary>
        /// The value of the property here "dataImport" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("dataDelete")]
        public DataDeleteCollection DataDelete { get { return base["dataDelete"] as DataDeleteCollection; } }

        /// <summary>
        /// The value of the property here "dataImport" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("dataExport")]
        public DataExportCollection DataExport { get { return base["dataExport"] as DataExportCollection; } }

        /// <summary>
        /// The value of the property here "incrementSolutionBuildNumber" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("incrementSolutionBuildNumber")]
        public IncrementSolutionCollection IncrementSolutionBuildNumber { get { return base["incrementSolutionBuildNumber"] as IncrementSolutionCollection; } }

        /// <summary>
        /// The value of the property here "assignSecurityRole" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("assignSecurityRole")]
        public AssignSecurityRoleCollection AssignSecurityRole { get { return base["assignSecurityRole"] as AssignSecurityRoleCollection; } }

        /// <summary>
        /// The value of the property here "assignWorkflowToUser" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("assignWorkflowToUser")]
        public AssignWorkflowsCollection AssignWorkflow { get { return base["assignWorkflowToUser"] as AssignWorkflowsCollection; } }

        /// <summary>
        /// The value of the property here "assignWorkflowToUser" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("activateWorkflows")]
        public ActivateWorkflowsCollection ActivateWorkflows { get { return base["activateWorkflows"] as ActivateWorkflowsCollection; } }

        /// <summary>
        /// The value of the property here "stopWindowsService" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("stopWindowsService")]
        public StopWindowsServiceCollection StopWindowsService { get { return base["stopWindowsService"] as StopWindowsServiceCollection; } }

        /// <summary>
        /// The value of the property here "startWindowsService" needs to match that of the config file element
        /// </summary>
        [ConfigurationProperty("startWindowsService")]
        public StartWindowsServiceCollection StartWindowsService { get { return base["startWindowsService"] as StartWindowsServiceCollection; } }

        /// <summary>
        /// The value of the property here "getpluginxml" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("getpluginxml")]
        public PluginXmlExportElement GetPluginXml { get { return base["getpluginxml"] as PluginXmlExportElement; } }

        /// <summary>
        /// The value of the property here "ribbon" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("ribbon")]
        public RibbonCollection RibbonExportImport { get { return base["ribbon"] as RibbonCollection; } }

        /// <summary>
        /// The value of the property here "sitemap" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("sitemap")]
        public SitemapCollection SitemapExportImport { get { return base["sitemap"] as SitemapCollection; } }

        /// <summary>
        /// The value of the property here "organizations" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("organizations")]
        public OrganizationCollection OrganizationCreateDelete { get { return base["organizations"] as OrganizationCollection; } }

        /// <summary>
        /// The value of the property here "theme" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("crmthemes")]
        public ThemeCollection CrmTheme { get { return base["crmthemes"] as ThemeCollection; } }

        /// <summary>
        /// The value of the property here "defaultpublishers" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("defaultpublishers")]
        public DefaultPublisherCollection DefaultPublishers { get { return base["defaultpublishers"] as DefaultPublisherCollection; } }

        /// <summary>
        /// The value of the property here "publishers" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("publishers")]
        public PublisherCollection Publishers { get { return base["publishers"] as PublisherCollection; } }

        /// <summary>
        /// The value of the property here "solutions" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("solutions")]
        public SolutionCollection Solutions { get { return base["solutions"] as SolutionCollection; } }

        /// <summary>
        /// The value of the property here "globaloptionsetmetadatas" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("globaloptionsetmetadatas")]
        public GlobalOpMetadataCollection GlobalOpMetadatas { get { return base["globaloptionsetmetadatas"] as GlobalOpMetadataCollection; } }

        /// <summary>
        /// The value of the property here "entitymetadatas" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("entitymetadatas")]
        public EntMetadataCollection EntityMetadatas { get { return base["entitymetadatas"] as EntMetadataCollection; } }

        /// <summary>
        /// The value of the property here "manytomanyrelationmetadatas" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("manytomanyrelationmetadatas")]
        public ManyToManyRelationMetadataCollection ManyToManyRelationMetadatas { get { return base["manytomanyrelationmetadatas"] as ManyToManyRelationMetadataCollection; } }

        /// <summary>
        /// The value of the property here "patchCustomizationXmls" needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("patchCustomizationXmls")]
        public PatchCustomizationXmlCollection PatchCustomizationXml { get { return base["patchCustomizationXmls"] as PatchCustomizationXmlCollection; } }

        public static bool IsSectionDefined(ConfigurationElement element)
        {
            return (element != null && element.ElementInformation != null && element.ElementInformation.IsPresent);
        }
    }
}
