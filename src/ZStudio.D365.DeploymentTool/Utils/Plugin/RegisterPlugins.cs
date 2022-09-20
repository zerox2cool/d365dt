using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Client;
using ZD365DT.DeploymentTool.Configuration;

/// <summary>
/// (WINSON) NOTE: THIS CLASS NEED TO BE REFACTOR, BAD CODING
/// </summary>
namespace ZD365DT.DeploymentTool.Utils.Plugin
{
    /// <summary>
    /// Plugin registration
    /// </summary>
    public class RegisterPlugins
    {
        #region constants
        //picklist values used for registering plugins
        const int SOURCETYPE_DB = 0;
        const int SOURCETYPE_DISK = 1;
        const int SOURCETYPE_GAC = 2;
        const int ISOMODE_NONE = 1;
        const int ISOMODE_SANDBOX = 2;
        const int MODE_SYNC = 0;
        const int MODE_ASYNC = 1;
        const int STAGE_PREOUTSIDE = 10;
        const int STAGE_PREINSIDE = 20;
        const int STAGE_POSTINSIDE = 40;
        const int STAGE_POSTOUTSIDE = 50;
        const int IMAGE_PRE = 0;
        const int IMAGE_POST = 1;
        const int IMAGE_BOTH = 2;
        const int DEPLOYMENT_SERVERONLY = 0;
        const int DEPLOYMENT_OFFLINEONLY = 1;
        const int DEPLOYMENT_BOTH = 2;

        const string XML_FILE = "register.xml";

        //XML element and attribute names
        const string ELEMENT_ASSEMBLY = "assembly";
        const string ATTR_SOLUTION = "solution";
        const string ATTR_SRC = "src";
        const string ATTR_SANDBOX = "sandbox";
        const string ATTR_LOCATION = "location";
        const string ATTR_LOCATION_DATABASE = "database";
        const string ATTR_LOCATION_GAC = "gac";
        const string ATTR_LOCATION_ONDISK = "on-disk";

        const string ELEMENT_TYPE = "type";
        const string ATTR_TYPE_NAME = "name";
        const string ATTR_TYPE_DESCRIPTION = "description";
        const string ATTR_TYPE_DISPLAY_NAME = "displayname";
        const string ATTR_TYPE_WORKFLOW_GROUP = "workflowgroup";

        const string ELEMENT_STEP = "step";
        const string ATTR_ENTITY = "entity";
        const string ATTR_STAGE = "stage";
        const string ATTR_MESSAGE = "message";
        const string ATTR_CONFIGURATION = "configuration";
        const string ATTR_RANK = "rank";
        const string ATTR_STAGE_PREVALIDATION = "prevalidation";
        const string ATTR_STAGE_PRE = "pre";
        const string ATTR_STAGE_POST = "post";
        const string ATTR_STAGE_ASYNC = "async";
        const string ATTR_FILTERATTRIBUTES = "filterattributes";
        const string ATTR_ASYNCAUTODELETE = "asyncautodelete";
        const string ATTR_SUPPORTEDDEPLOYMENT = "supporteddeployment";
        const string ATTR_SUPPORTEDDEPLOYMENT_SERVER = "server";
        const string ATTR_SUPPORTEDDEPLOYMENT_OFFLINE = "offline";
        const string ATTR_SUPPORTEDDEPLOYMENT_BOTH = "both";
        const string ATTR_IMPERSONATINGUSERID = "impersonatinguserid";

        const string ELEMENT_IMAGE = "image";
        const string ATTR_IMAGE_NAME = "name";
        const string ATTR_IMAGE_TYPE = "type";
        const string ATTR_IMAGE_TYPE_PRE = "pre";
        const string ATTR_IMAGE_TYPE_POST = "post";
        const string ATTR_IMAGE_TYPE_BOTH = "both";
        const string ATTR_MESSAGE_PROPERTY_NAME_TYPE = "messagepropertyname";
        const string ATTR_ENTITYALIAS = "entityalias";
        const string ATTR_IMAGE_MESSAGE_PROPERTY_NAME_TARGET = "Target";
        const string ATTR_IMAGE_MESSAGE_PROPERTY_NAME_ID = "Id";
        const string ATTR_IMAGE_MESSAGE_PROPERTY_NAME_EmailId = "EmailId";
        const string ATTR_IMAGE_MESSAGE_PROPERTY_NAME_EntityMoniker = "EntityMoniker";
        const string ATTR_IMAGE_MESSAGE_PROPERTY_NAME_SubordinateId = "SubordinateId";
        const string IMG_NAME_PREIMAGE = "PreImage";
        const string IMG_NAME_POSTIMAGE = "PostImage";
        const string IMG_NAME_BOTHIMAGE = "PrePostImage";
        const string ATTR_ATTRIBUTES = "attributes";

        const string UPDATED = "UPDATED";
        const string PARAM_UNREGISTER = "/unregister";
        const string PARAM_UNREGISTERONLY = "/unregisteronly";

        const string ATTR_DEFAULT_SOLUTION = "Default";
        const string DEFAULT_SOLUTION_ID = "FD140AAF-4DF4-11DD-BD17-0019B9312238";

        #endregion

        static WebServiceUtils _utils = null;
        static string _solutionName = string.Empty;
        static Dictionary<string, Guid> _sdkMessageCache = new Dictionary<string, Guid>();

        static Dictionary<string, PluginType> _pluginTypeData = new Dictionary<string, PluginType>();
        static Dictionary<string, SdkMessageProcessingStep> _pluginStepData = new Dictionary<string, SdkMessageProcessingStep>();
        static Dictionary<string, SdkMessageProcessingStepImage> _imageData = new Dictionary<string, SdkMessageProcessingStepImage>();

        private static string _xmlFilePath = null;

        public static void Register(string registerXml, WebServiceUtils util, StringDictionary tokens, PluginElement element)
        {
            string pluginDefinitionXml = IOUtils.ReadFileReplaceTokens(registerXml, tokens);

            //Load xml file
            XmlDocument doc = null;
            doc = new XmlDocument();
            doc.LoadXml(pluginDefinitionXml);
            _xmlFilePath = registerXml;

            if (doc == null)
            {
                doc = new XmlDocument();
                doc.Load(XML_FILE);
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace(ConfigXml.Namespace.NS_PREFIX, ConfigXml.Namespace.PLUGIN_XMLNS);

            _utils = util;

            //We MUST do a retrieveMultiple BEFORE loading the plugin assemblies to pre-populate 
            //the entity types in the AppDomainBasedKnownProxyTypesProvider class inside the Microsoft.Xrm.Sdk.dll
            //if we dont do this then it will actually try to load the entity types defined INSIDE the plugin assembly and we'll get type mismatch errors.
            QueryExpression query = new QueryExpression(SystemUser.EntityLogicalName);
            query.PageInfo = new PagingInfo() { Count = 1, PageNumber = 1 };
            _utils.Service.RetrieveMultiple(query);

            XmlNodeList assemblyNodes = doc.SelectNodes("//" + ConfigXml.Namespace.NS_PREFIX + ":" + ELEMENT_ASSEMBLY, nsmgr);
            if (assemblyNodes.Count == 0)
                assemblyNodes = doc.GetElementsByTagName(ELEMENT_ASSEMBLY);

            if (assemblyNodes.Count == 0)
            {
                Logger.LogInfo("No Assembly Nodes found for any Plugins and Workflows in the file {0}...", pluginDefinitionXml);
            }

            foreach (XmlNode assemblyNode in assemblyNodes)
            {
                RegisterAssembly(assemblyNode, element, tokens, nsmgr);
            }
        }

        #region Get Plugins
        private static EntityCollection GetPluginAssembliesFromSolution(string solutionName, out List<Guid> pluginStepsGuids)
        {
            EntityCollection pluginAssemblyCollection = new EntityCollection();
            pluginStepsGuids = new List<Guid>();

            Guid solutionId = Guid.Empty;
            if (solutionName == ATTR_DEFAULT_SOLUTION)
            {
                solutionId = new Guid(DEFAULT_SOLUTION_ID);
            }
            else
            {
                QueryByAttribute query = new QueryByAttribute(Solution.EntityLogicalName);
                query.ColumnSet = new ColumnSet("solutionid");
                query.Attributes.AddRange("uniquename");
                query.Values.AddRange(solutionName);

                EntityCollection solutionRetrieved = _utils.Service.RetrieveMultiple(query);

                if (solutionRetrieved.Entities.Count > 0)
                {
                    solutionId = solutionRetrieved.Entities[0].Id;
                }
            }

            if (solutionId != Guid.Empty)
            {
                QueryExpression solutionComponentQuery = new QueryExpression(SolutionComponent.EntityLogicalName);
                solutionComponentQuery.ColumnSet = new ColumnSet("objectid", "componenttype");

                solutionComponentQuery.Criteria.AddCondition("solutionid", ConditionOperator.Equal, solutionId);

                solutionComponentQuery.Criteria.AddCondition("componenttype", ConditionOperator.In,
                                                             new object[] { 91, 92 });
                solutionComponentQuery.Criteria.AddFilter(LogicalOperator.And);

                EntityCollection solutionComponentsCollection = _utils.Service.RetrieveMultiple(solutionComponentQuery);


                List<Guid> pluginAssemblyGuids = new List<Guid>();
                foreach (var entity in solutionComponentsCollection.Entities)
                {
                    SolutionComponent component = (SolutionComponent)entity;


                    if (component.ComponentType.Value == 91)
                    {
                        pluginAssemblyGuids.Add(component.ObjectId.Value);
                    }
                    else if (component.ComponentType.Value == 92)
                    {
                        pluginStepsGuids.Add(component.ObjectId.Value);
                    }

                }

                if (pluginAssemblyGuids.Count > 0)
                {
                    QueryExpression pluginAssemblyQuery = new QueryExpression(PluginAssembly.EntityLogicalName);
                    pluginAssemblyQuery.ColumnSet = new ColumnSet("pluginassemblyid", "name", "isolationmode",
                                                                  "sourcetype");

                    object[] values = new object[pluginAssemblyGuids.Count];
                    for (int i = 0; i < pluginAssemblyGuids.Count; i++)
                    {
                        values[i] = pluginAssemblyGuids[i];
                    }

                    pluginAssemblyQuery.Criteria.AddCondition("pluginassemblyid", ConditionOperator.In, values);
                    pluginAssemblyQuery.Criteria.AddFilter(LogicalOperator.And);

                    pluginAssemblyCollection = _utils.Service.RetrieveMultiple(pluginAssemblyQuery);
                }
            }

            return pluginAssemblyCollection;
        }

        /// <summary>
        /// Get plugin assemblies based on solution
        /// </summary>
        public static void GetPlugins(string solutionName, WebServiceUtils util, string absoluteFilePath)
        {
            _utils = util;
            _solutionName = solutionName;
            if (string.IsNullOrEmpty(_solutionName))
            {
                _solutionName = "Default";
            }

            Logger.LogInfo("Warning!: Information related to Secured Configuration setting of registered steps will not get added to the generated XML");
            Logger.LogInfo(string.Format("Retrieving registered assemblies information for solution: {0}", _solutionName));
            List<Guid> pluginStepsGuids;
            EntityCollection pluginAssemblyCollection = GetPluginAssembliesFromSolution(_solutionName, out pluginStepsGuids);

            if (pluginAssemblyCollection == null)
            {
                ShowErrorMessage(string.Format("Please check the solution name, no plugins found registered for solution: {0}", _solutionName));
                return;
            }


            if (pluginAssemblyCollection.Entities.Count > 0)
            {
                Logger.LogInfo(string.Format("Total Plugin Assemblies found: {0}", pluginAssemblyCollection.Entities.Count));
                WriteRegisterPluginXMLFile(pluginAssemblyCollection, pluginStepsGuids, absoluteFilePath);
            }
            else
            {
                ShowErrorMessage(string.Format("No Plugins found in solution: {0}", _solutionName));
                return;
            }
        }

        /// <summary>
        /// Generates register.xml file for /getplugins parameter passed to the application
        /// </summary>
        private static void WriteRegisterPluginXMLFile(EntityCollection pluginAssemblyCollection, List<Guid> pluginStepsGuids, string absoluteFilePath)
        {

            Logger.LogInfo("Generating register.xml file....");
            XmlDocument doc = new XmlDocument();

            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", "utf-8", null);

            XmlElement rootElement = doc.DocumentElement;
            doc.InsertBefore(xmldecl, rootElement);

            XmlElement deployElement = (XmlElement)doc.AppendChild(doc.CreateElement("deploy"));
            deployElement.SetAttribute("xmlns", ConfigXml.Namespace.PLUGIN_XMLNS);
            
            foreach (var entity in pluginAssemblyCollection.Entities)
            {
                PluginAssembly assembly = (PluginAssembly)entity;
                XmlElement assemblyElement = (XmlElement)deployElement.AppendChild(doc.CreateElement(ELEMENT_ASSEMBLY));
                assemblyElement.SetAttribute(ATTR_SRC, assembly.Name + ".dll");

                if (assembly.SourceType.Value == SOURCETYPE_DB)
                    assemblyElement.SetAttribute(ATTR_LOCATION, ATTR_LOCATION_DATABASE);
                else if (assembly.SourceType.Value == SOURCETYPE_DISK)
                    assemblyElement.SetAttribute(ATTR_LOCATION, ATTR_LOCATION_ONDISK);
                else if (assembly.SourceType.Value == SOURCETYPE_GAC)
                    assemblyElement.SetAttribute(ATTR_LOCATION, ATTR_LOCATION_GAC);
                
                assemblyElement.SetAttribute(ATTR_SANDBOX, assembly.IsolationMode.Value == 2 ? "true" : "false");

                List<PluginType> pluginTypes = GetAllPluginTypes(assembly);
                List<SdkMessageProcessingStep> pluginSteps = GetStepsInPluginTypes(pluginTypes);

                Logger.LogInfo(string.Format("Total Plugin found in {0} : {1}....", assembly.Name, pluginTypes.Count));

                foreach (var pluginType in pluginTypes)
                {
                    XmlElement pluginTypeElement = (XmlElement)assemblyElement.AppendChild(doc.CreateElement(ELEMENT_TYPE));
                    pluginTypeElement.SetAttribute(ATTR_TYPE_NAME, pluginType.TypeName);
                    pluginTypeElement.SetAttribute(ATTR_TYPE_DISPLAY_NAME, pluginType.Name);
                    pluginTypeElement.SetAttribute(ATTR_TYPE_DESCRIPTION, pluginType.Description);
                    if (!string.IsNullOrEmpty(pluginType.WorkflowActivityGroupName))
                        pluginTypeElement.SetAttribute(ATTR_TYPE_WORKFLOW_GROUP, pluginType.WorkflowActivityGroupName);

                    List<SdkMessageProcessingStep> steps = pluginSteps.Where(x => pluginType.PluginTypeId != null && (x.PluginTypeId.Id == pluginType.PluginTypeId.Value)).ToList();
                    foreach (var step in steps)
                    {
                        XmlElement stepElement = (XmlElement)pluginTypeElement.AppendChild(doc.CreateElement("step"));
                        if (step.SdkMessageFilterId != null)
                        {
                            // Query to get Entity name for which step is registered
                            SdkMessageFilter sdkMessageFilter =
                                (SdkMessageFilter)
                                _utils.Service.Retrieve(SdkMessageFilter.EntityLogicalName,
                                                       step.SdkMessageFilterId.Id,
                                                       new ColumnSet("primaryobjecttypecode"));

                            stepElement.SetAttribute(ATTR_ENTITY,
                                                     sdkMessageFilter.PrimaryObjectTypeCode != null
                                                         ? sdkMessageFilter.PrimaryObjectTypeCode.ToString()
                                                         : "");
                        }
                        else
                        {
                            stepElement.SetAttribute(ATTR_ENTITY, "none");
                        }
                        stepElement.SetAttribute(ATTR_MESSAGE,
                                                 step.SdkMessageId != null
                                                     ? step.SdkMessageId.Name.ToLower().ToString()
                                                     : "");
                        int stageNumber = step.Stage.Value;
                        switch (stageNumber)
                        {
                            case STAGE_PREOUTSIDE:
                                stepElement.SetAttribute(ATTR_STAGE, ATTR_STAGE_PREVALIDATION);
                                break;
                            case STAGE_PREINSIDE:
                                stepElement.SetAttribute(ATTR_STAGE, ATTR_STAGE_PRE);
                                break;
                            case STAGE_POSTINSIDE:
                                stepElement.SetAttribute(ATTR_STAGE, ATTR_STAGE_POST);
                                break;
                            default:
                                stepElement.SetAttribute(ATTR_STAGE, ATTR_STAGE_ASYNC);
                                break;
                        }

                        if (step.Rank != null)
                        {
                            stepElement.SetAttribute(ATTR_RANK, step.Rank.Value.ToString());
                        }

                        if (!string.IsNullOrEmpty(step.FilteringAttributes))
                        {
                            //set the attributes as a comma separated list, when all attribtues are selected, it will NULL and not required
                            stepElement.SetAttribute(ATTR_FILTERATTRIBUTES, step.FilteringAttributes);
                        }

                        stepElement.SetAttribute(ATTR_ASYNCAUTODELETE, (step.AsyncAutoDelete != null && step.AsyncAutoDelete.Value).ToString().ToLower());

                        if (step.SupportedDeployment.Value == DEPLOYMENT_SERVERONLY)
                        {
                            stepElement.SetAttribute(ATTR_SUPPORTEDDEPLOYMENT, ATTR_SUPPORTEDDEPLOYMENT_SERVER);
                        }
                        else if (step.SupportedDeployment.Value == DEPLOYMENT_OFFLINEONLY)
                        {
                            stepElement.SetAttribute(ATTR_SUPPORTEDDEPLOYMENT, ATTR_SUPPORTEDDEPLOYMENT_OFFLINE);
                        }
                        else if (step.SupportedDeployment.Value == DEPLOYMENT_OFFLINEONLY)
                        {
                            stepElement.SetAttribute(ATTR_SUPPORTEDDEPLOYMENT, ATTR_SUPPORTEDDEPLOYMENT_BOTH);
                        }

                        if (step.ImpersonatingUserId != null)
                        {
                            stepElement.SetAttribute(ATTR_IMPERSONATINGUSERID, step.ImpersonatingUserId.Id.ToString());
                        }
                        //unsecure parameters
                        if (!String.IsNullOrEmpty(step.Configuration))
                        {
                            stepElement.SetAttribute(ATTR_CONFIGURATION, step.Configuration.ToString());
                        }

                        List<SdkMessageProcessingStepImage> images =
                            GetAllImagesInSteps(steps).Where(
                                x =>
                                step.SdkMessageProcessingStepId != null &&
                                x.SdkMessageProcessingStepId.Id == step.SdkMessageProcessingStepId.Value).
                                ToList();

                        foreach (var image in images)
                        {
                            XmlElement imageElement = (XmlElement)stepElement.AppendChild(doc.CreateElement(ELEMENT_IMAGE));

                            //set name
                            imageElement.SetAttribute(ATTR_IMAGE_NAME, image.Name);

                            //set the image type
                            int imageTypeNumber = image.ImageType.Value;
                            switch (imageTypeNumber)
                            {
                                case IMAGE_POST:
                                    imageElement.SetAttribute(ATTR_IMAGE_TYPE, ATTR_IMAGE_TYPE_POST);
                                    break;
                                case IMAGE_PRE:
                                    imageElement.SetAttribute(ATTR_IMAGE_TYPE, ATTR_IMAGE_TYPE_PRE);
                                    break;
                                case IMAGE_BOTH:
                                    imageElement.SetAttribute(ATTR_IMAGE_TYPE, ATTR_IMAGE_TYPE_BOTH);
                                    break;
                                default:
                                    imageElement.SetAttribute(ATTR_IMAGE_TYPE, "error");
                                    break;
                            }

                            //set the image alias that is used to fetch from the context
                            imageElement.SetAttribute(ATTR_ENTITYALIAS, image.EntityAlias);

                            //set the attributes as a comma separated list, when all attribtues are selected, it will NULL and not required
                            if (!string.IsNullOrEmpty(image.Attributes1))
                                imageElement.SetAttribute(ATTR_ATTRIBUTES, image.Attributes1);
                        }
                    }
                }
            }
            //string absoluteFilePath = null;
            //foreach (string arg in Program.GetArgs())
            //{
            //    if (arg.EndsWith(".xml", StringComparison.CurrentCultureIgnoreCase))
            //    {
            //        absoluteFilePath = arg;
            //    }
            //}

            if (string.IsNullOrEmpty(absoluteFilePath))
            {
                absoluteFilePath = XML_FILE;
            }

            // Checks whether file already exist with path and name provided,
            //if yes then append numeric character based on file count at the end of file name
            int fileCount = 0;
            string newFileName = absoluteFilePath;
            while (File.Exists(newFileName))
            {
                fileCount = fileCount + 1;
                newFileName = absoluteFilePath.Substring(0, absoluteFilePath.IndexOf(".xml")) + fileCount.ToString() + ".xml";
            }
            doc.Save(newFileName);
            Logger.LogInfo(string.Format("XML file created and saved by name {0}....", newFileName));
        }
        #endregion

        #region UpdateAssemblyIsolationMode
        public static bool UpdateAssemblyIsolationMode(string name, bool isolationMode, WebServiceUtils util, bool ignoreIfNotFound = false)
        {
            if (_utils == null)
                _utils = util;

            QueryExpression query = new QueryExpression();
            query.EntityName = PluginAssembly.EntityLogicalName;
            query.ColumnSet = new ColumnSet(new string[] { "pluginassemblyid", "isolationmode" });
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("name", ConditionOperator.Equal, new object[] { name });
            EntityCollection result = _utils.Service.RetrieveMultiple(query);

            if (result != null && result.Entities.Count > 0)
            {
                //found, update the isolation mode
                PluginAssembly assembly = (PluginAssembly)result.Entities[0];

                //display current isolation mode
                bool currentIsolationMode = false;
                if (assembly.IsolationMode != null)
                {
                    Logger.LogInfo("{0} current IsolationMode: {1}", name, assembly.IsolationMode.Value == ISOMODE_NONE ? "None" : "Sandbox");
                    currentIsolationMode = (assembly.IsolationMode.Value == ISOMODE_SANDBOX);
                }

                if (currentIsolationMode != isolationMode)
                {
                    if (isolationMode)
                        assembly.IsolationMode = new OptionSetValue(ISOMODE_SANDBOX);
                    else
                        assembly.IsolationMode = new OptionSetValue(ISOMODE_NONE);
                    Logger.LogInfo("Updating {0} Isolation Mode from {1} to {2}.", name, currentIsolationMode, isolationMode);
                    _utils.Service.Update(assembly);
                }
                else
                {
                    Logger.LogInfo("No change to Isolation Mode, no update.");
                }

                return true;
            }
            else
            {
                //not found
                if (!ignoreIfNotFound)
                {
                    throw new Exception(string.Format("The plugin assembly with the name '{0}' does not exist.", name));
                }
                else
                {
                    //do nothing and ignore
                }
                return false;
            }
        }
        #endregion UpdateAssemblyIsolationMode

        /// <summary>
        /// Registers a plugin assembly
        /// </summary>
        private static void RegisterAssembly(XmlNode assemblyNode, PluginElement element, StringDictionary tokens, XmlNamespaceManager nsmgr)
        {
            RequireAttribute(ATTR_SRC, assemblyNode);

            string solutionName = IOUtils.ReplaceStringTokens(element.SolutionName, tokens);
            bool uploadonly = element.UploadOnly;
            bool unregister = element.Unregister;
            bool unregisteronly = element.UnregisterOnly;

            string assemblyFileName = assemblyNode.Attributes[ATTR_SRC].Value;
            string src = Path.GetFullPath(assemblyFileName);
            _solutionName = solutionName;
            if (assemblyNode.Attributes[ATTR_SOLUTION] != null)
            {
                _solutionName = assemblyNode.Attributes[ATTR_SOLUTION].Value;
            }
            if (string.IsNullOrEmpty(_solutionName))
            {
                Logger.LogWarning("Warning: Plugins will not be registered to any solution");
            }

            PluginAssembly assembly = new PluginAssembly();

            //if the given path to the DLL is not an absolute path then try looking in the same dir as the xml file
            if (!File.Exists(src))
            {
                if (!string.IsNullOrEmpty(_xmlFilePath))
                {
                    string newPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(_xmlFilePath), assemblyFileName));
                    if (File.Exists(newPath))
                    {
                        src = newPath;
                        Logger.LogInfo("Found assembly in " + src);
                    }
                    else
                    {
                        ShowErrorMessage("Assembly not Found");
                    }
                }
            }

            FileInfo fi = new FileInfo(src);
            string path = fi.FullName;
            Assembly assemblyDll = Assembly.LoadFile(path); //TODO: is there a better way to get assembly info?
            AssemblyName assemblyDllName = assemblyDll.GetName();
            assembly.Culture = assemblyDllName.CultureInfo.Name == string.Empty ? "neutral" : assemblyDllName.CultureInfo.Name;
            assembly.Name = assemblyDllName.Name;
            assembly.PublicKeyToken = BitConverter.ToString(assemblyDllName.GetPublicKeyToken());
            assembly.Version = assemblyDllName.Version.ToString();
            assembly.Path = fi.Name;

            //default not sandboxed. sandbox mode can be enabled with sandbox='true' in assembly element or /sandbox input parameter
            assembly.IsolationMode = new OptionSetValue(ISOMODE_NONE);
            if ((assemblyNode.Attributes[ATTR_SANDBOX] != null && assemblyNode.Attributes[ATTR_SANDBOX].Value.ToLower() == true.ToString().ToLower()))
            {
                assembly.IsolationMode = new OptionSetValue(ISOMODE_SANDBOX);
            }

            //default sourcetype to disk. other locations can be set with location='database' in assembly element or /location:database input parameter
            assembly.SourceType = new OptionSetValue(SOURCETYPE_DISK);
            if ((assemblyNode.Attributes[ATTR_LOCATION] != null && assemblyNode.Attributes[ATTR_LOCATION].Value.ToLower() == ATTR_LOCATION_DATABASE))
            {
                assembly.SourceType = new OptionSetValue(SOURCETYPE_DB);
                byte[] bytes = File.ReadAllBytes(path);
                assembly.Content = Convert.ToBase64String(bytes);
            }
            if ((assemblyNode.Attributes[ATTR_LOCATION] != null && assemblyNode.Attributes[ATTR_LOCATION].Value.ToLower() == ATTR_LOCATION_GAC))
            {
                assembly.SourceType = new OptionSetValue(SOURCETYPE_GAC);
            }

            bool uploadOnly = assembly.SourceType.Value == SOURCETYPE_DB && uploadonly;

            //find out if this assembly is already registered, having same name and major and minor version
            QueryExpression query = new QueryExpression(PluginAssembly.EntityLogicalName);
            query.ColumnSet = new ColumnSet("name", "major", "minor");
            query.Criteria.Conditions.Add(new ConditionExpression("name", ConditionOperator.Equal, assembly.Name));
            EntityCollection currentAssemblies = _utils.Service.RetrieveMultiple(query);
            if (currentAssemblies.Entities.Count > 0)
            {
                //need to remove the current assembly before registering new version
                Logger.LogInfo("Found assembly already registered");

                if (!uploadOnly)
                {
                    #region without upload only command

                    //bool unregister = false;
                    ////Program.HasArgument(PARAM_UNREGISTER);
                    //bool unregisteronly = false;
                    ////Program.HasArgument(PARAM_UNREGISTERONLY);
                    if (unregister || unregisteronly)
                    {
                        // Loop to check number of assemblies registered having same name
                        //but having different major and minor version, if found delete all previously registered assemblies
                        for (int currentAssemblyIndex = 0; currentAssemblyIndex < currentAssemblies.Entities.Count; currentAssemblyIndex++)
                        {
                            DeleteAssembly(currentAssemblies.Entities[currentAssemblyIndex].Id, false);
                            currentAssemblies.Entities.RemoveAt(currentAssemblyIndex);
                        }
                        if (unregisteronly)
                        {
                            return;
                        }
                    }
                    else
                    {
                        GetPluginDetails(currentAssemblies.Entities[0].Id);
                    }
                    #endregion
                }
            }

            if (unregisteronly)
            {
                return;
            }

            Logger.LogInfo(" ");
            Logger.LogInfo("Registering...");
            Logger.LogInfo("Assembly: " + assembly.Name);
            bool isVersionDifferent = false;
            if (uploadOnly && currentAssemblies.Entities.Count > 0)
            {
                // Linq to check the if currenAssemblies Entity Collection contains assembly having
                // same major and minor version, if found update the same else create/register a new one
                var currentAssembly = currentAssemblies.Entities.Where(x => x.Attributes["major"].ToString() == assemblyDllName.Version.Major.ToString() && x.Attributes["minor"].ToString() == assemblyDllName.Version.Minor.ToString()).ToArray();
                if (currentAssembly.Length > 0)
                {
                    Logger.LogInfo("Updating assembly contents");
                    assembly.PluginAssemblyId = currentAssembly[0].Id;
                    _utils.Service.Update(assembly);
                }
                else
                {
                    ShowErrorMessage(@"Plugin cannot be updated as Major\Minor version has been upgraded, Refer help document for further details");
                    return;
                }
            }
            else
            {
                //create or update the assembly
                Guid assemblyId = Guid.Empty;
                if (currentAssemblies.Entities.Count > 0)
                {
                    // Linq to check the if currenAssemblies Entity Collection contains assembly having
                    // same major and minor version, if found update the same else create/register a new one
                    var currentAssembly = currentAssemblies.Entities.Where(x => x.Attributes["major"].ToString() == assemblyDllName.Version.Major.ToString() && x.Attributes["minor"].ToString() == assemblyDllName.Version.Minor.ToString()).ToArray();

                    if (currentAssembly.Length > 0)
                    {
                        assembly.PluginAssemblyId = currentAssembly[0].Id;
                        _utils.Service.Update(assembly);
                        assemblyId = currentAssembly[0].Id;
                        Logger.LogInfo("Update Assembly: " + assembly.Name);
                    }
                    else
                    {
                        Logger.LogInfo(@"Major\Minor version of plugin is differrent,Creating new assembly");
                        isVersionDifferent = true;
                        assemblyId = _utils.Service.Create(assembly);
                        Logger.LogInfo("Created Assembly: " + assembly.Name);
                    }
                }
                else
                {
                    assemblyId = _utils.Service.Create(assembly);
                    Logger.LogInfo("Created Assembly: " + assembly.Name);
                }

                if (!string.IsNullOrEmpty(_solutionName))
                {
                   SolutionManager.AddSolutionComponent(_utils.Service, _solutionName, assemblyId, SolutionManager.ComponentType.PluginAssembly, false);
                }

                XmlNodeList typeNodes = assemblyNode.SelectNodes(ConfigXml.Namespace.NS_PREFIX + ":" + ELEMENT_TYPE, nsmgr);
                if (typeNodes.Count == 0)
                    Logger.LogInfo(@"No plugin type found.");

                foreach (XmlNode typeNode in typeNodes)
                {
                    RegisterPluginType(assemblyId, typeNode, nsmgr);
                }

                if (!isVersionDifferent)
                {
                    //Delete plugin images, steps, and types that are registered on the server 
                    //but are not found in the register.xml file. (anything that doesnt have the 'UPDATED' flag set)
                    foreach (var image in _imageData.Values)
                    {
                        if (!image.Contains(UPDATED))
                        {
                            _utils.Service.Delete(SdkMessageProcessingStepImage.EntityLogicalName, image.Id);
                            Logger.LogInfo("Deleting plugin Image: " + image.Name);
                        }
                    }
                    foreach (var step in _pluginStepData.Values)
                    {
                        if (!step.Contains(UPDATED))
                        {
                            _utils.Service.Delete(SdkMessageProcessingStep.EntityLogicalName, step.Id);
                            Logger.LogInfo("Deleting plugin Step: " + step.Name);
                        }
                    }
                    foreach (var pluginType in _pluginTypeData.Values)
                    {
                        if (!pluginType.Contains(UPDATED))
                        {
                            _utils.Service.Delete(PluginType.EntityLogicalName, pluginType.Id);
                            Logger.LogInfo("Deleting plugin Type: " + pluginType.Name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add a component to a CRM Solution
        /// </summary>
        public static void AddSolutionComponent(IOrganizationService service, string solutionName, Guid objectId, int componentType, bool addRequiredComponents)
        {
            AddSolutionComponentRequest req = new AddSolutionComponentRequest();
            req.AddRequiredComponents = addRequiredComponents;
            req.ComponentId = objectId;
            req.ComponentType = componentType;
            req.SolutionUniqueName = solutionName;
            service.Execute(req);
        }

        /// <summary>
        /// Registers a plugin type
        /// </summary>
        private static void RegisterPluginType(Guid assemblyId, XmlNode typeNode, XmlNamespaceManager nsmgr)
        {
            RequireAttribute(ATTR_TYPE_NAME, typeNode);

            PluginType plugin = new PluginType();

            string fullTypeName = typeNode.Attributes[ATTR_TYPE_NAME].Value;
            string typeName = fullTypeName.Substring(fullTypeName.LastIndexOf(".") + 1);

            string displayName = typeNode.Attributes[ATTR_TYPE_DISPLAY_NAME]?.Value;
            string description = typeNode.Attributes[ATTR_TYPE_DESCRIPTION]?.Value;
            string workflowGroup = typeNode.Attributes[ATTR_TYPE_WORKFLOW_GROUP]?.Value;

            plugin.PluginAssemblyId = new EntityReference(PluginAssembly.EntityLogicalName, assemblyId);
            plugin.TypeName = fullTypeName;
            //as per CRM OOB functionality passing GUID value in Friendly Name.
            plugin.FriendlyName = Guid.NewGuid().ToString();
            plugin.Description = string.IsNullOrEmpty(description) ? typeName : description;
            plugin.Name = string.IsNullOrEmpty(displayName) ? fullTypeName : displayName;
            if (!string.IsNullOrEmpty(workflowGroup))
                plugin.WorkflowActivityGroupName = workflowGroup;

            //create or update the pluginType
            Guid pluginTypeId = Guid.Empty;
            string matchCode = GetPluginTypeMatchCode(plugin);
            if (_pluginTypeData.ContainsKey(matchCode))
            {
                //update existing pluginType
                plugin.Id = _pluginTypeData[matchCode].Id;
                _pluginTypeData[matchCode][UPDATED] = true;
                pluginTypeId = plugin.Id;

                if (!PluginTypesMatch(plugin, _pluginTypeData[matchCode]))
                {
                    _utils.Service.Update(plugin);
                    Console.ForegroundColor = ConsoleColor.White;
                    Logger.LogInfo("  Updated PluginType: " + plugin.Name);
                    Console.ResetColor();
                }
                else
                {
                    Logger.LogInfo("  No Change on PluginType: " + plugin.Name);
                }
            }
            else
            {
                //create new pluginType
                pluginTypeId = _utils.Service.Create(plugin);
                Logger.LogInfo("  Created PluginType: " + plugin.Name);
            }

            XmlNodeList stepNodes = typeNode.SelectNodes(ConfigXml.Namespace.NS_PREFIX + ":" + ELEMENT_STEP, nsmgr);
            foreach (XmlNode stepNode in stepNodes)
            {
                RegisterStep(pluginTypeId, typeName, stepNode, nsmgr);
            }
        }


        /// <summary>
        /// Registers a plugin step
        /// </summary>
        private static void RegisterStep(Guid pluginTypeId, string pluginTypeName, XmlNode stepNode, XmlNamespaceManager nsmgr)
        {
            RequireAttribute(ATTR_STAGE, stepNode);
            RequireAttribute(ATTR_MESSAGE, stepNode);

            string entity = null;
            if (stepNode.Attributes[ATTR_ENTITY] != null)
            {
                entity = stepNode.Attributes[ATTR_ENTITY].Value;
            }
            string stage = stepNode.Attributes[ATTR_STAGE].Value;
            string message = stepNode.Attributes[ATTR_MESSAGE].Value;
            string modeName = stage.ToLower().StartsWith(ATTR_STAGE_ASYNC) ? "async" : "sync";
            int rank = 1;
            string unsecureConfig = null;

            if (stepNode.Attributes[ATTR_RANK] != null)
            {
                rank = Convert.ToInt32(stepNode.Attributes[ATTR_RANK].Value);
            }
            if (stepNode.Attributes[ATTR_CONFIGURATION] != null)
            {
                unsecureConfig = Convert.ToString(stepNode.Attributes[ATTR_CONFIGURATION].Value);
            }

            SdkMessageProcessingStep sdkStep = new SdkMessageProcessingStep();
            sdkStep.EventHandler = new EntityReference(PluginType.EntityLogicalName, pluginTypeId);
            sdkStep.Name = entity + " step " + rank.ToString() + " " + stage + "-" + message + " as " + modeName;
            sdkStep.Mode = new OptionSetValue(stage.ToLower().StartsWith(ATTR_STAGE_ASYNC) ? MODE_ASYNC : MODE_SYNC);
            sdkStep.Rank = rank;
            if (!string.IsNullOrEmpty(unsecureConfig))
                sdkStep.Configuration = unsecureConfig;
            else
                sdkStep.Configuration = null;

            if (stepNode.Attributes[ATTR_IMPERSONATINGUSERID] != null)
            {
                string impersonatinguseridString = stepNode.Attributes[ATTR_IMPERSONATINGUSERID].Value;
                Guid impersonateUserId = Guid.Empty;
                if (Guid.TryParse(impersonatinguseridString, out impersonateUserId))
                {
                    sdkStep.ImpersonatingUserId = new EntityReference(SystemUser.EntityLogicalName, impersonateUserId);
                }
                else
                {
                    Logger.LogInfo(string.Format("WARNING: attribute '{0}' is not a valid guid, so it has been ignored.", ATTR_IMPERSONATINGUSERID));
                }
            }

            //retrieve the id of the sdkmessage (look up based on message name) 
            sdkStep.SdkMessageId = new EntityReference(SdkMessage.EntityLogicalName, GetSdkMessageId(message));

            //retrieve the id of the SdkMessageFilter (look up based on entity and message name)
            int otc = Metadata.GetEntityOTC(_utils, entity);
            if (otc == 0 && message == "associate")
            {
                //when entity code is not found and the message is for associate for a N:N relationship, set the SdkMessageFilterId to NULL
                sdkStep.SdkMessageFilterId = null;
            }
            else
            {
                QueryExpression getFilter = new QueryExpression(SdkMessageFilter.EntityLogicalName);
                getFilter.Criteria.Conditions.Add(new ConditionExpression("primaryobjecttypecode", ConditionOperator.Equal, otc));
                getFilter.Criteria.Conditions.Add(new ConditionExpression("sdkmessageid", ConditionOperator.Equal, sdkStep.SdkMessageId.Id));
                EntityCollection filters = _utils.Service.RetrieveMultiple(getFilter);
                if (filters.Entities.Count > 0)
                {
                    SdkMessageFilter filter = (SdkMessageFilter)filters.Entities[0];
                    sdkStep.SdkMessageFilterId = new EntityReference(SdkMessageFilter.EntityLogicalName, filter.SdkMessageFilterId.Value);
                }
                else
                {
                    throw new Exception("You can't register the message " + message + " on entity " + entity);
                }
            }

            int stageNumber = 0;
            stage = stage.ToLower();
            if (stage == ATTR_STAGE_PREVALIDATION)
                stageNumber = STAGE_PREOUTSIDE;
            else if (stage == ATTR_STAGE_PRE)
                stageNumber = STAGE_PREINSIDE;
            else if (stage == ATTR_STAGE_POST || stage == ATTR_STAGE_ASYNC)
                stageNumber = STAGE_POSTINSIDE;
            else
                throw new Exception(string.Format("Invalid stage. Use '{0}', '{1}', '{2}', or '{3}'.", ATTR_STAGE_PRE, ATTR_STAGE_PREVALIDATION, ATTR_STAGE_POST, ATTR_STAGE_ASYNC));
            sdkStep.Stage = new OptionSetValue(stageNumber);

            sdkStep.SupportedDeployment = new OptionSetValue(DEPLOYMENT_SERVERONLY);
            if (stepNode.Attributes[ATTR_SUPPORTEDDEPLOYMENT] != null)
            {
                string supportedDeploymentAttrText = stepNode.Attributes[ATTR_SUPPORTEDDEPLOYMENT].Value;
                if (string.Compare(supportedDeploymentAttrText, ATTR_SUPPORTEDDEPLOYMENT_OFFLINE, true) == 0)
                {
                    sdkStep.SupportedDeployment = new OptionSetValue(DEPLOYMENT_OFFLINEONLY);
                }
                else if (string.Compare(supportedDeploymentAttrText, ATTR_SUPPORTEDDEPLOYMENT_BOTH, true) == 0)
                {
                    sdkStep.SupportedDeployment = new OptionSetValue(DEPLOYMENT_BOTH);
                }
                else if (string.Compare(supportedDeploymentAttrText, ATTR_SUPPORTEDDEPLOYMENT_SERVER, true) == 0)
                {
                    sdkStep.SupportedDeployment = new OptionSetValue(DEPLOYMENT_SERVERONLY);
                }
                else
                {
                    Logger.LogInfo(string.Format("WARNING: Invalid value for '{0}' attribute. Valid values are '{1}', '{2}', or '{3}'.",
                        ATTR_SUPPORTEDDEPLOYMENT, ATTR_SUPPORTEDDEPLOYMENT_SERVER, ATTR_SUPPORTEDDEPLOYMENT_OFFLINE, ATTR_SUPPORTEDDEPLOYMENT_BOTH));
                }
            }

            if (stepNode.Attributes[ATTR_FILTERATTRIBUTES] != null)
            {
                string filteredAttrs = stepNode.Attributes[ATTR_FILTERATTRIBUTES].Value;

                //clean up and clear all spaces
                if (!string.IsNullOrEmpty(filteredAttrs))
                    filteredAttrs = filteredAttrs.Replace(" ", "");

                sdkStep.FilteringAttributes = filteredAttrs;

                //set filtering attributes into the description
                string description = string.Format("Filtering Attribute(s): {0}", filteredAttrs);
                if (description.Length > 256)
                    description = description.Substring(0, 256);
                sdkStep.Description = description;
            }
            else
            {
                //set to no filter
                sdkStep.FilteringAttributes = string.Empty;
                sdkStep.Description = string.Empty;
            }

            sdkStep.AsyncAutoDelete = false;
            if (stepNode.Attributes[ATTR_ASYNCAUTODELETE] != null)
            {
                string asyncautodelete = stepNode.Attributes[ATTR_ASYNCAUTODELETE].Value.ToLower();
                if (bool.TryParse(asyncautodelete, out bool asyncautodeleteBoolean))
                {
                    sdkStep.AsyncAutoDelete = asyncautodeleteBoolean;
                }
                else
                {
                    //default to false
                    sdkStep.AsyncAutoDelete = false;
                }
            }

            //create or update the plugin step
            Guid stepId = Guid.Empty;
            string matchCode = GetPluginStepMatchCode(sdkStep);
            if (_pluginStepData.ContainsKey(matchCode))
            {
                //update existing step
                sdkStep.Id = _pluginStepData[matchCode].Id;

                _pluginStepData[matchCode][UPDATED] = true;
                stepId = sdkStep.Id;

                if (!PluginStepsMatch(sdkStep, _pluginStepData[matchCode]))
                {
                    _utils.Service.Update(sdkStep);
                    Logger.LogInfo("    Updated Step: " + entity + " " + stage + " " + message);
                }
                else
                {
                    Logger.LogInfo("    No Change in Step: " + entity + " " + stage + " " + message);
                }
            }
            else
            {
                //create new step
                stepId = _utils.Service.Create(sdkStep);
                Logger.LogInfo("    Created Step: " + entity + " " + stage + " " + message);
            }

            if (!string.IsNullOrEmpty(_solutionName))
            {
                SolutionManager.AddSolutionComponent(_utils.Service, _solutionName, stepId, SolutionManager.ComponentType.SdkMessageProcessingStep, false);
            }

            XmlNodeList imageNodes = stepNode.SelectNodes(ConfigXml.Namespace.NS_PREFIX + ":" + ELEMENT_IMAGE, nsmgr);
            foreach (XmlNode imageNode in imageNodes)
            {
                RegisterImage(stepId, imageNode, message.ToLower());
            }
        }


        /// <summary>
        /// Register a plugin image
        /// </summary>
        private static void RegisterImage(Guid stepId, XmlNode imageNode, string messageName)
        {
            SdkMessageProcessingStepImage image = new SdkMessageProcessingStepImage();

            image.SdkMessageProcessingStepId = new EntityReference(SdkMessageProcessingStep.EntityLogicalName, stepId);

            string type = ATTR_IMAGE_TYPE_PRE;
            if (imageNode.Attributes[ATTR_IMAGE_TYPE] != null)
            {
                type = imageNode.Attributes[ATTR_IMAGE_TYPE].Value.ToLower();
            }
            int imageTypeNumber = IMAGE_PRE;
            if (type == ATTR_IMAGE_TYPE_POST)
                imageTypeNumber = IMAGE_POST;
            else if (type == ATTR_IMAGE_TYPE_BOTH)
                imageTypeNumber = IMAGE_BOTH;
            else if (type != ATTR_IMAGE_TYPE_PRE)
            {
                throw new Exception(string.Format("Invalid value for attribute '{0}'. Valid values are '{1}', '{2}', or '{3}'.",
                    ATTR_IMAGE_TYPE, ATTR_IMAGE_TYPE_PRE, ATTR_IMAGE_TYPE_POST, ATTR_IMAGE_TYPE_BOTH));
            }

            if (imageNode.Attributes[ATTR_ENTITYALIAS] == null)
            {
                throw new Exception("plugin image registration error: image requires attribute '" + ATTR_ENTITYALIAS + "'");
            }

            image.ImageType = new OptionSetValue(imageTypeNumber);

            image.MessagePropertyName = SetImageMessagePropertyName(messageName, imageNode);

            if (imageNode.Attributes[ATTR_IMAGE_NAME] != null)
                image.Name = imageNode.Attributes[ATTR_IMAGE_NAME].Value;
            else
            {
                if (imageTypeNumber == IMAGE_BOTH)
                    image.Name = IMG_NAME_BOTHIMAGE;
                else if (imageTypeNumber == IMAGE_PRE)
                    image.Name = IMG_NAME_PREIMAGE;
                else if (imageTypeNumber == IMAGE_POST)
                    image.Name = IMG_NAME_POSTIMAGE;
                else
                    image.Name = imageNode.Attributes[ATTR_ENTITYALIAS].Value;
            }

            image.EntityAlias = imageNode.Attributes[ATTR_ENTITYALIAS].Value;

            if (imageNode.Attributes[ATTR_ATTRIBUTES] != null)
            {
                string attrs = imageNode.Attributes[ATTR_ATTRIBUTES].Value;
                //clean up and clear all spaces
                if (!string.IsNullOrEmpty(attrs))
                    attrs = attrs.Replace(" ", "");

                image.Attributes1 = attrs;
            }

            //create or update the plugin step
            Guid imageId = Guid.Empty;
            string matchCode = GetPluginImageMatchCode(image);
            if (_imageData.ContainsKey(matchCode))
            {
                //update existing step
                image.Id = _imageData[matchCode].Id;
                _imageData[matchCode][UPDATED] = true;
                imageId = image.Id;

                if (!ImagesMatch(image, _imageData[matchCode]))
                {
                    _utils.Service.Update(image);
                    Logger.LogInfo("      Updated Image: " + image.Name + " " + image.EntityAlias);
                }
                else
                {
                    Logger.LogInfo("      No change in Image: " + image.Name + " " + image.EntityAlias);
                }
            }
            else
            {
                //create new step
                imageId = _utils.Service.Create(image);
                Console.ForegroundColor = ConsoleColor.White;
                Logger.LogInfo("      Created Image: " + image.Name + " " + image.EntityAlias);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// throws an exception if the attribute is not present on the element
        /// </summary>
        private static void RequireAttribute(string attributeName, XmlNode node)
        {
            if (node.Attributes[attributeName] == null)
            {
                throw new Exception(string.Format("Missing required attribute '{0}' in element '{1}'.", attributeName, node.Name));
            }
        }


        public static void DeleteAssembly(string assemblyName)
        {
            //find out if this assembly is already registered
            QueryExpression query = new QueryExpression(PluginAssembly.EntityLogicalName);
            query.ColumnSet = new ColumnSet("name");
            query.Criteria.Conditions.Add(new ConditionExpression("name", ConditionOperator.Equal, assemblyName));
            EntityCollection currentAssemblies = _utils.Service.RetrieveMultiple(query);
            if (currentAssemblies.Entities.Count > 0)
            {
                PluginAssembly currentAssembly = (PluginAssembly)currentAssemblies[0];
                DeleteAssembly(currentAssembly.PluginAssemblyId.Value, false);
            }
            else
            {
                throw new Exception("No assembly found with name " + assemblyName);
            }
        }



        /// <summary>
        /// gets a matchcode for determining if two plugintypes are the same
        /// </summary>
        private static string GetPluginTypeMatchCode(PluginType pluginType)
        {
            return pluginType.PluginAssemblyId.Id.ToString() + "_" + pluginType.TypeName;
        }

        /// <summary>
        /// determines if two pluginTypes are exactly the same
        /// (used to determine if the type has changed at all since the last plugin deployment)
        /// </summary>
        private static bool PluginTypesMatch(PluginType a, PluginType b)
        {
            return !(a.PluginAssemblyId.Id != b.PluginAssemblyId.Id ||
                a.TypeName != b.TypeName ||
                a.Description != b.Description ||
                a.Name != b.Name);
        }

        /// <summary>
        /// gets a matchcode for determining if two plugin steps are the same
        /// </summary>
        private static string GetPluginStepMatchCode(SdkMessageProcessingStep step)
        {
            if (step.SdkMessageFilterId != null)
                return step.SdkMessageFilterId.Id.ToString() + "_" + step.SdkMessageId.Id.ToString() + "_" + step.Stage.Value.ToString() + "_" + step.Mode.Value.ToString() + "_" + step.EventHandler.Id.ToString();
            else
                return Guid.Empty.ToString() + "_" + step.SdkMessageId.Id.ToString() + "_" + step.Stage.Value.ToString() + "_" + step.Mode.Value.ToString() + "_" + step.EventHandler.Id.ToString();
        }

        /// <summary>
        /// determines if two plugin steps are exactly the same
        /// (used to determine if the step has changed at all since the last plugin deployment)
        /// </summary>
        private static bool PluginStepsMatch(SdkMessageProcessingStep a, SdkMessageProcessingStep b)
        {
            return !(a.Configuration != b.Configuration ||
                a.EventHandler.Id != b.EventHandler.Id ||
                a.Name != b.Name ||
                a.Mode.Value != b.Mode.Value ||
                a.Rank != b.Rank ||
                (a.ImpersonatingUserId != null && b.ImpersonatingUserId != null && a.ImpersonatingUserId.Id != b.ImpersonatingUserId.Id) ||
                a.SdkMessageId.Id != b.SdkMessageId.Id ||
                a.SdkMessageFilterId?.Id != b.SdkMessageFilterId?.Id ||
                a.Stage.Value != b.Stage.Value ||
                a.SupportedDeployment.Value != b.SupportedDeployment.Value ||
                ((!string.IsNullOrEmpty(a.FilteringAttributes) || !string.IsNullOrEmpty(b.FilteringAttributes)) && a.FilteringAttributes != b.FilteringAttributes) ||
                a.AsyncAutoDelete != b.AsyncAutoDelete);
        }

        /// <summary>
        /// Checks if the plugin step is already the part of imported solution. The key to determine the uniquness is on step name and step's message id
        /// Assumption is that step name would be a combination of plugintypename + entityname + messagename + stage. From pluginregistration tool, CRMDeployment
        /// when a step is registered the name consist of plugintypename + entityname + messagename. In case the solution being upgraded has the name 
        /// different,a duplicate step would be created. We are ignoring the other fields like impersonatinguserid,
        /// configuration,mode,rank,messagefilterid,stage,type of deplyoment and attributes because user might have changed these fileds in the upgraded
        ///  version,so we are avoiding creating a duplicate step by considering only name, plugintypename and messagname.
        /// </summary>
        /// <param name="stepCollection">collection of steps in the plugin type</param>
        /// <param name="step">new step to register</param>
        /// <returns></returns>
        private static bool PluginStepExists(List<SdkMessageProcessingStep> stepCollection, SdkMessageProcessingStep step)
        {
            int count = stepCollection.Where(a => a.Name == step.Name &&
                                      a.SdkMessageId.Id == step.SdkMessageId.Id).ToList().Count();
            if (count > 0) return true;
            return false;
        }

        /// <summary>
        ///  gets a matchcode for determining if two plugin images are the same
        /// </summary>
        private static string GetPluginImageMatchCode(SdkMessageProcessingStepImage image)
        {
            return image.SdkMessageProcessingStepId.Id.ToString() + "_" + image.MessagePropertyName + "_" + image.EntityAlias;
        }

        /// <summary>
        /// determines if two plugin images are exactly the same
        /// (used to determine if the image has changed at all since the last plugin deployment)
        /// </summary>
        private static bool ImagesMatch(SdkMessageProcessingStepImage a, SdkMessageProcessingStepImage b)
        {
            return !(a.SdkMessageProcessingStepId.Id != b.SdkMessageProcessingStepId.Id ||
                a.ImageType.Value != b.ImageType.Value ||
                a.MessagePropertyName != b.MessagePropertyName ||
                a.Name != b.Name ||
                a.EntityAlias != b.EntityAlias ||
                a.Attributes1 != b.Attributes1);
        }

        /// <summary>
        /// Retrieve the data for all plugin types, steps, and images for the plugin assembly.
        /// </summary>
        private static void GetPluginDetails(Guid assemblyid)
        {
            List<Guid> typeids = new List<Guid>();
            QueryExpression getTypes = new QueryExpression(PluginType.EntityLogicalName);
            getTypes.ColumnSet.AllColumns = true;
            getTypes.Criteria.Conditions.Add(new ConditionExpression("pluginassemblyid", ConditionOperator.Equal, assemblyid));
            EntityCollection types = _utils.Service.RetrieveMultiple(getTypes);
            foreach (PluginType t in types.Entities)
            {
                typeids.Add(t.PluginTypeId.Value);
                _pluginTypeData[GetPluginTypeMatchCode(t)] = t;

            }

            List<Guid> stepIds = new List<Guid>();
            if (typeids.Count > 0)
            {
                QueryExpression getSteps = new QueryExpression(SdkMessageProcessingStep.EntityLogicalName);
                getSteps.ColumnSet.AllColumns = true;
                getSteps.Criteria.Conditions.Add(new ConditionExpression("plugintypeid", ConditionOperator.In, typeids));
                EntityCollection steps = _utils.Service.RetrieveMultiple(getSteps);
                foreach (SdkMessageProcessingStep s in steps.Entities)
                {
                    stepIds.Add(s.SdkMessageProcessingStepId.Value);
                    _pluginStepData[GetPluginStepMatchCode(s)] = s;
                }
            }

            if (stepIds.Count > 0)
            {
                QueryExpression getImages = new QueryExpression(SdkMessageProcessingStepImage.EntityLogicalName);
                getImages.ColumnSet.AllColumns = true;
                getImages.Criteria.Conditions.Add(new ConditionExpression("sdkmessageprocessingstepid", ConditionOperator.In, stepIds));
                EntityCollection images = _utils.Service.RetrieveMultiple(getImages);
                foreach (SdkMessageProcessingStepImage img in images.Entities)
                {
                    _imageData[GetPluginImageMatchCode(img)] = img;
                }
            }
        }

        /// <summary>
        /// delete a plugin assembly and all its types, steps, and images.
        /// </summary>
        private static void DeleteAssembly(Guid assemblyid, bool isUpgrade)
        {
            if (isUpgrade)
            {
                Logger.LogInfo("Unregistering previous assembly and all plugins/steps");
            }
            else
            {
                Logger.LogInfo("Deleting assembly and all plugins/steps");
            }
            List<Guid> typeids = new List<Guid>();
            QueryExpression getTypes = new QueryExpression(PluginType.EntityLogicalName);
            getTypes.Criteria.Conditions.Add(new ConditionExpression("pluginassemblyid", ConditionOperator.Equal, assemblyid));
            EntityCollection types = _utils.Service.RetrieveMultiple(getTypes);
            foreach (PluginType t in types.Entities)
            {
                typeids.Add(t.PluginTypeId.Value);
            }

            List<Guid> stepIds = new List<Guid>();
            if (typeids.Count > 0)
            {
                QueryExpression getSteps = new QueryExpression(SdkMessageProcessingStep.EntityLogicalName);
                getSteps.Criteria.Conditions.Add(new ConditionExpression("plugintypeid", ConditionOperator.In, typeids));
                EntityCollection steps = _utils.Service.RetrieveMultiple(getSteps);
                foreach (SdkMessageProcessingStep s in steps.Entities)
                {
                    stepIds.Add(s.SdkMessageProcessingStepId.Value);
                }
            }

            if (stepIds.Count > 0)
            {
                QueryExpression getImages = new QueryExpression(SdkMessageProcessingStepImage.EntityLogicalName);
                getImages.Criteria.Conditions.Add(new ConditionExpression("sdkmessageprocessingstepid", ConditionOperator.In, stepIds));
                EntityCollection images = _utils.Service.RetrieveMultiple(getImages);
                foreach (SdkMessageProcessingStepImage img in images.Entities)
                {
                    Console.Write(".");
                    _utils.Service.Delete(SdkMessageProcessingStepImage.EntityLogicalName, img.SdkMessageProcessingStepImageId.Value);
                }
            }

            foreach (Guid id in stepIds)
            {
                Console.Write(".");
                _utils.Service.Delete(SdkMessageProcessingStep.EntityLogicalName, id);
            }
            foreach (Guid id in typeids)
            {
                Console.Write(".");
                _utils.Service.Delete(PluginType.EntityLogicalName, id);
            }

            Console.Write(".");
            _utils.Service.Delete(PluginAssembly.EntityLogicalName, assemblyid);
            Console.WriteLine(".");

            Logger.LogInfo(" Done.");
        }

        /// <summary>
        /// Returns the id of an SDK message.
        /// Note: this method will cache ids in a static collection to reduce un-needed lookups
        /// It also has the most commonly used sdkmessages IDs hard-coded.
        /// </summary>
        private static Guid GetSdkMessageId(string message)
        {
            message = message.ToLowerInvariant();

            //Since CRM always uses the same GUIDs for sdk messages, we can save lookup time 
            //by keeping a hard-coded list of the most commonly used SDK messages.
            if (_sdkMessageCache.Count == 0)
            {
                _sdkMessageCache.Add("create", new Guid("9EBDBB1B-EA3E-DB11-86A7-000A3A5473E8"));
                _sdkMessageCache.Add("delete", new Guid("A1BDBB1B-EA3E-DB11-86A7-000A3A5473E8"));
                _sdkMessageCache.Add("execute", new Guid("A6BDBB1B-EA3E-DB11-86A7-000A3A5473E8"));
                _sdkMessageCache.Add("setstate", new Guid("1CBEBB1B-EA3E-DB11-86A7-000A3A5473E8"));
                _sdkMessageCache.Add("setstatedynamicentity", new Guid("1DBEBB1B-EA3E-DB11-86A7-000A3A5473E8"));
                _sdkMessageCache.Add("update", new Guid("20BEBB1B-EA3E-DB11-86A7-000A3A5473E8"));
            }

            if (_sdkMessageCache.ContainsKey(message))
            {
                return _sdkMessageCache[message];
            }
            else
            {
                QueryExpression getSdkMessage = new QueryExpression(SdkMessage.EntityLogicalName);
                getSdkMessage.Criteria.Conditions.Add(new ConditionExpression("name", ConditionOperator.Equal, message));
                EntityCollection sdkMessages = _utils.Service.RetrieveMultiple(getSdkMessage);
                if (sdkMessages.Entities.Count > 0)
                {
                    SdkMessage sdkMessage = (SdkMessage)sdkMessages[0];
                    _sdkMessageCache[message] = sdkMessage.SdkMessageId.Value;
                    return sdkMessage.SdkMessageId.Value;
                }
                else
                {
                    throw new Exception(message + " is not a valid sdk message");
                }
            }
        }
        #region Upgrade

        /// <summary>
        /// Upgrades plugin assembly
        /// </summary>
        /// <param name="service">Iorganizationservice</param>
        /// <param name="assemblyDll">assembly dll</param>
        /// <param name="oldAssembly">old plugin assembly</param>
        /// <param name="assemblyName">assembly name</param>
        public static void UpgradeAssembly(Assembly assemblyDll, PluginAssembly oldAssembly, string assemblyName, string solutionPath, string uniquename, bool ismanaged, WebServiceUtils utils, ref bool anyError)
        {
            _utils = utils;
            Logger.LogInfo("Taking backup of all plugin steps of {0}....", assemblyName);
            List<PluginType> pluginTypes = GetAllPluginTypes(oldAssembly);
            List<SdkMessageProcessingStep> steps = GetStepsInPluginTypes(pluginTypes);
            List<SdkMessageProcessingStepImage> images = GetAllImagesInSteps(steps);

            DeleteAssembly(oldAssembly.Id, true);
            try
            {
                Logger.LogInfo("Importing Solution " + uniquename);
                utils.ImportSolution(solutionPath, uniquename, ismanaged, ref anyError);
                if (!anyError)
                {

                    Logger.LogInfo("Fetching all new assemblies ,plugins");
                    PluginAssembly newAssembly = GetPluginAssembly(_utils.Service, assemblyName);
                    List<PluginType> newPluginTypes = GetAllPluginTypes(newAssembly);
                    try
                    {
                        Logger.LogInfo("Registering Plugins");
                        RegisterPluginComponents(pluginTypes, newPluginTypes, steps, images);
                    }

                    catch (Exception ex)
                    {
                        //TODO: Need to store the plugin backup in xml format, so that it can be used  to register plugins incase of any exception during upgrade
                        ShowErrorMessage(ex.Message);
                        Logger.LogInfo("Restoring BackUp");
                        DeleteAssembly(newAssembly.Id, false);
                        RestoreBackUp(oldAssembly, pluginTypes, steps, images);
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
                Logger.LogInfo("Restoring BackUp");
                RestoreBackUp(oldAssembly, pluginTypes, steps, images);
                return;
            }

        }

        /// <summary>
        /// Gets plugin assembly by name
        /// </summary>
        /// <param name="service">Iorganizationservice</param>
        /// <param name="assemblyName">assembly name</param>
        /// <returns>plugin assembly</returns>
        public static PluginAssembly GetPluginAssembly(IOrganizationService service, string assemblyName)
        {
            QueryExpression query = new QueryExpression(PluginAssembly.EntityLogicalName);
            query.ColumnSet.Columns.Add("name");
            query.ColumnSet.Columns.Add("major");
            query.ColumnSet.Columns.Add("minor");
            query.ColumnSet.Columns.Add("content");
            query.Criteria.Conditions.Add(new ConditionExpression("name", ConditionOperator.Equal, assemblyName));
            EntityCollection pluginassemblys = service.RetrieveMultiple(query);
            if (pluginassemblys.Entities.Count > 0)
            {
                return (PluginAssembly)pluginassemblys.Entities[0];
            }
            return null;

        }

        /// <summary>
        /// Gets all the plugin types in the assembly
        /// </summary>
        /// <param name="assembly">PluginAssembly</param>
        /// <returns>List of PluginTypes</returns>
        private static List<PluginType> GetAllPluginTypes(PluginAssembly assembly)
        {
            QueryExpression query = new QueryExpression(PluginType.EntityLogicalName);
            query.ColumnSet.AllColumns = true;
            query.Criteria.Conditions.Add(new ConditionExpression("pluginassemblyid", ConditionOperator.Equal, assembly.Id));
            EntityCollection pTypes = _utils.Service.RetrieveMultiple(query);
            return pTypes.Entities.Cast<PluginType>().ToList();
        }


        /// <summary>
        /// Gets all the steps in the given plugin types
        /// </summary>
        /// <param name="pluginTypes">collection of plugintypes</param>
        /// <returns>collection of messageprocessingstep</returns>
        private static List<SdkMessageProcessingStep> GetStepsInPluginTypes(List<PluginType> pluginTypes)
        {
            if (pluginTypes.Count > 0)
            {
                QueryExpression query = new QueryExpression(SdkMessageProcessingStep.EntityLogicalName);
                query.ColumnSet.AllColumns = true;
                query.Criteria.Conditions.Add(new ConditionExpression("plugintypeid", ConditionOperator.In,
                                                                      pluginTypes.Select(x => x.Id).ToList()));
                //query.Criteria.Conditions.Add(new ConditionExpression("ismanaged", ConditionOperator.Equal, false));
                EntityCollection entityCollection = _utils.Service.RetrieveMultiple(query);
                return entityCollection.Entities.Cast<SdkMessageProcessingStep>().ToList();
            }

            return null;

        }

        /// <summary>
        /// Gets all the images resgistered in the given steps
        /// </summary>
        /// <param name="steps">collection of steps</param>
        /// <returns>collection of stepimages</returns>
        private static List<SdkMessageProcessingStepImage> GetAllImagesInSteps(List<SdkMessageProcessingStep> steps)
        {
            if (steps.Count > 0)
            {
                QueryExpression getImages = new QueryExpression(SdkMessageProcessingStepImage.EntityLogicalName);
                getImages.ColumnSet.AllColumns = true;
                getImages.Criteria.Conditions.Add(new ConditionExpression("sdkmessageprocessingstepid", ConditionOperator.In,
                                                                          steps.Select(x => x.Id).ToList()));

                EntityCollection images = _utils.Service.RetrieveMultiple(getImages);
                return images.Entities.Cast<SdkMessageProcessingStepImage>().ToList();
            }

            return null;
        }

        /// <summary>
        /// Registers plugin steps and images
        /// </summary>
        /// <param name="oldPluginTypes">old plugin types</param>
        /// <param name="newPluginTypes">new plugin types</param>
        /// <param name="steps">plugin steps</param>
        /// <param name="images">plugins images</param>
        private static void RegisterPluginComponents(List<PluginType> oldPluginTypes, List<PluginType> newPluginTypes, List<SdkMessageProcessingStep> steps, List<SdkMessageProcessingStepImage> images)
        {
            StringBuilder errorMessage = new StringBuilder();
            bool invalid = false;
            errorMessage.AppendLine("Following plugin types have not been registerd ");

            Logger.LogInfo("Registering PluginSteps/Images");

            //Get the list of steps registered after importing solution
            List<SdkMessageProcessingStep> newSteps = GetStepsInPluginTypes(newPluginTypes);
            foreach (PluginType pluginType in oldPluginTypes)
            {

                PluginType newPluginType = newPluginTypes.Where(x => x.TypeName == pluginType.TypeName).First();
                if (newPluginType != null)
                {
                    List<SdkMessageProcessingStep> stepsToRegister = steps.Where(x => x.PluginTypeId.Id == pluginType.Id && x.IsManaged == false).ToList();
                    List<SdkMessageProcessingStep> stepsRegistered = newSteps.Where(x => x.PluginTypeId.Id == newPluginType.Id && x.IsManaged == false).ToList();

                    if (stepsToRegister.Count() > 0)
                    {
                        #region Steps

                        foreach (SdkMessageProcessingStep sdkMessageProcessingStep in stepsToRegister)
                        {
                            Guid stepId = Guid.NewGuid();
                            List<SdkMessageProcessingStepImage> oldImages =
                                images.Where(x => x.SdkMessageProcessingStepId.Id == sdkMessageProcessingStep.Id).ToList();

                            sdkMessageProcessingStep.PluginTypeId = new EntityReference(PluginType.EntityLogicalName,
                                                                                        newPluginType.Id);
                            sdkMessageProcessingStep.EventHandler = new EntityReference(PluginType.EntityLogicalName,
                                                                                        newPluginType.Id);
                            sdkMessageProcessingStep.Id = stepId;
                            //TODO: Registering disabled steps throws crm exception, hence need to set it explicitly to active. This will be documented as known issue, since disabled steps will get enabled after doing a solutionupgrade
                            sdkMessageProcessingStep.StatusCode = new OptionSetValue(1);
                            //register the step and image only if the step is not already registered after solution import
                            if (!PluginStepExists(stepsRegistered, sdkMessageProcessingStep))
                            {
                                _utils.Service.Create(sdkMessageProcessingStep);
                                #region Images
                                foreach (SdkMessageProcessingStepImage sdkMessageProcessingStepImage in oldImages)
                                {
                                    sdkMessageProcessingStepImage.Id = Guid.NewGuid();
                                    sdkMessageProcessingStepImage.SdkMessageProcessingStepId =
                                        new EntityReference(SdkMessageProcessingStep.EntityLogicalName, stepId);
                                    _utils.Service.Create(sdkMessageProcessingStepImage);
                                }
                                #endregion
                                Console.Write(".");
                            }




                        }
                        #endregion
                    }
                }
                else
                {
                    invalid = true;
                    errorMessage.AppendLine(pluginType.TypeName);
                }

                if (invalid)
                {
                    ShowErrorMessage(errorMessage.ToString());
                }
            }
            Logger.LogInfo("Done");

        }

        /// <summary>
        /// Register the older plugin steps and other components
        /// </summary>
        /// <param name="oldAssembly">old plugin assembly</param>
        /// <param name="oldPluginTypes">old plugin types</param>
        /// <param name="steps">old plugin steps</param>
        /// <param name="images">old plugin images</param>
        private static void RestoreBackUp(PluginAssembly oldAssembly, List<PluginType> oldPluginTypes, List<SdkMessageProcessingStep> steps, List<SdkMessageProcessingStepImage> images)
        {
            Logger.LogInfo("Registering plugin assembly {0} {1}", oldAssembly.Path, oldAssembly.Version);
            _utils.Service.Create(oldAssembly);


            StringBuilder errorMessage = new StringBuilder();
            errorMessage.AppendLine("Following plugin types have not been registerd ");
            Console.Write("Registering PluginSteps/Images");

            foreach (PluginType pluginType in oldPluginTypes)
            {
                _utils.Service.Create(pluginType);
                Console.Write(".");
                List<SdkMessageProcessingStep> stepsToRegister = steps.Where(x => x.PluginTypeId.Id == pluginType.Id).ToList();

                if (stepsToRegister.Count() > 0)
                {
                    #region Steps
                    foreach (SdkMessageProcessingStep sdkMessageProcessingStep in stepsToRegister)
                    {

                        List<SdkMessageProcessingStepImage> oldImages =
                            images.Where(x => x.SdkMessageProcessingStepId.Id == sdkMessageProcessingStep.Id).ToList();

                        //TODO: Registering disabled steps throws crm exception, hence need to set it explicitly to active. This will be documented as known issue, since disabled steps will get enabled after doing a solutionupgrade
                        sdkMessageProcessingStep.StatusCode = new OptionSetValue(1);
                        _utils.Service.Create(sdkMessageProcessingStep);
                        Console.Write(".");

                        #region Images
                        foreach (SdkMessageProcessingStepImage sdkMessageProcessingStepImage in oldImages)
                        {
                            _utils.Service.Create(sdkMessageProcessingStepImage);
                        }
                        #endregion

                    }
                    #endregion
                }

            }
            Logger.LogInfo("Done");
        }

        /// <summary>
        /// Show error message
        /// </summary>
        /// <param name="message">message</param>
        private static void ShowErrorMessage(string message)
        {
          //  Console.ForegroundColor = ConsoleColor.Red;            
            Logger.LogError(message);
            //throw new Exception(message);
            //Console.ForegroundColor = ConsoleColor.Gray;
        }
        #endregion

        /// <summary>
        /// Returns Plugin Image MassageName Property based on Steps message on which plugin gets invoked
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="imageNode"></param>
        /// <returns></returns>
        private static string SetImageMessagePropertyName(string messageName, XmlNode imageNode)
        {
            string imageMessagePropertyName = string.Empty;
            switch (messageName)
            {
                case "assign":
                    imageMessagePropertyName = ATTR_IMAGE_MESSAGE_PROPERTY_NAME_TARGET;
                    break;
                case "create":
                    imageMessagePropertyName = ATTR_IMAGE_MESSAGE_PROPERTY_NAME_ID;
                    break;
                case "delete":
                    imageMessagePropertyName = ATTR_IMAGE_MESSAGE_PROPERTY_NAME_TARGET;
                    break;
                case "deliverincoming":
                    imageMessagePropertyName = ATTR_IMAGE_MESSAGE_PROPERTY_NAME_EmailId;
                    break;
                case "deliverpromote":
                    imageMessagePropertyName = ATTR_IMAGE_MESSAGE_PROPERTY_NAME_EmailId;
                    break;
                case "executeworkflow":
                    imageMessagePropertyName = ATTR_IMAGE_MESSAGE_PROPERTY_NAME_TARGET;
                    break;
                case "merge":
                    if (imageNode.Attributes[ATTR_MESSAGE_PROPERTY_NAME_TYPE] != null)
                    {
                        if (imageNode.Attributes[ATTR_MESSAGE_PROPERTY_NAME_TYPE].Value == ATTR_IMAGE_MESSAGE_PROPERTY_NAME_TARGET)
                            imageMessagePropertyName = ATTR_IMAGE_MESSAGE_PROPERTY_NAME_TARGET;
                        else if (imageNode.Attributes[ATTR_MESSAGE_PROPERTY_NAME_TYPE].Value == ATTR_IMAGE_MESSAGE_PROPERTY_NAME_SubordinateId)
                            imageMessagePropertyName = ATTR_IMAGE_MESSAGE_PROPERTY_NAME_SubordinateId;
                        else
                        {
                            throw new Exception("Invalid Image Message Name Property value for Merge message....");
                        }
                    }
                    else
                    {
                        throw new Exception("imagemessagepropertyname property not found for an Assembly step to be registered on 'Merge' message");
                    }
                    break;
                case "route":
                    imageMessagePropertyName = ATTR_IMAGE_MESSAGE_PROPERTY_NAME_TARGET;
                    break;
                case "send":
                    //This is only applicable for Send message when the entity is e-mail. If the entity is template
                    //or fax, then the parameter should be ParameterName.FaxId or ParameterName.TemplateId
                    imageMessagePropertyName = ATTR_IMAGE_MESSAGE_PROPERTY_NAME_EmailId;
                    break;
                case "setstate":
                    imageMessagePropertyName = ATTR_IMAGE_MESSAGE_PROPERTY_NAME_EntityMoniker;
                    break;
                case "setstatedynamicentity":
                    imageMessagePropertyName = ATTR_IMAGE_MESSAGE_PROPERTY_NAME_EntityMoniker;
                    break;
                case "update":
                    imageMessagePropertyName = ATTR_IMAGE_MESSAGE_PROPERTY_NAME_TARGET;
                    break;
                default:
                    //There are no valid message property names for images for any other messages
                    break;
            }
            return imageMessagePropertyName;
        }

    }
}