using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Client;

namespace ZD365DT.DeploymentTool.Utils
{
    public class ClientExtensionEditor
    {
        const string RIBBON_SOLUTION_NAME = "ClientExtensionEditor";
        const string CUSTOMIZATIONS_PART = "/customizations.xml";
        const string SOLUTIONS_PART = "/solution.xml";
        const string ATTR_DEFAULT_SOLUTION = "Default";
        const string DEFAULT_SOLUTION_ID = "FD140AAF-4DF4-11DD-BD17-0019B9312238";
        const string SOLUTION_ZIPFILE = "ClientExtensions.zip";
        const string DEFAULT_RIBBON_XML_FILE = ".\\ClientExtensionEditor\\ribbon.xml";
        const string DEFAULT_SITEMAP_XML_FILE = ".\\ClientExtensionEditor\\sitemap.xml";

        bool anyError = false;
        string backupDirectory = ".\\ClientExtensionEditor";
        IOrganizationService _service = null;
        WebServiceUtils _utils = null;
        string _solutionName = string.Empty;
        WebServiceUtils utils = null;
        public ClientExtensionEditor(string solution, WebServiceUtils util, string backupDir)
        {
            SetSolutionName(solution);
            _utils = util;
            _service = util.Service;
            backupDirectory = backupDir;
            utils = util;
        }

        public string SolutionName
        {
            get
            {
                return _solutionName;
            }
            set
            {
                SetSolutionName(value);
            }
        }


        /// Retrieves ribbonxml for entities and saves it to a file
        /// </summary>
        /// <param name="entities">list of entities to retrieve ribbonxml for</param>
        /// <param name="includeAppRibbon">Inlude the application ribbon</param>
        /// <param name="filename">Filename to save to. (if this is null it will default to ribbon.xml)</param>
        public void GetRibbonXml(IEnumerable<string> entities, bool includeAppRibbon, string filename)
        {
            Logger.LogInfo("  Retrieving Ribbon XML from CRM");
            //Publish(entities, includeAppRibbon);
            SetupSolution(entities, includeAppRibbon, false);

            XmlDocument customizations = ExportCustomizations();
            RemoveEverythingButRibbonAndSitemapFromXml(customizations);

            if (string.IsNullOrEmpty(filename))
            {
                filename = DEFAULT_RIBBON_XML_FILE;
            }

            customizations.Save(filename);
            Logger.LogInfo("  Retrieved Ribbon XML from CRM to {0}", filename);
        }

        /// <summary>
        /// Retrievess the sitemap xml and saves it to a file
        /// </summary>
        /// <param name="filename">file to save to (if this is null it will default to sitemap.xml</param>
        public void GetSiteMapXml(string filename)
        {
            Logger.LogInfo("  Retrieving SiteMap XML from CRM");

            // Publish(null, true);
            SetupSolution(null, false, true);

            XmlDocument customizations = ExportCustomizations();
            RemoveEverythingButRibbonAndSitemapFromXml(customizations);

            if (string.IsNullOrEmpty(filename))
            {
                filename = DEFAULT_SITEMAP_XML_FILE;
            }

            customizations.Save(filename);
            Logger.LogInfo("  Retrieved SiteMap XML from CRM to {0}", filename);
        }

        /// <summary>
        /// Uploads a ribbon customizations xml file to the CRM Server
        /// </summary>
        /// <param name="ribbonXmlFileName">Xml file to upload</param>
        public void UploadRibbonXml(string ribbonXmlFileName)
        {
            Logger.LogInfo("  Uploading ribbon xml file to CRM");

            if (string.IsNullOrEmpty(ribbonXmlFileName))
            {
                ribbonXmlFileName = DEFAULT_RIBBON_XML_FILE;
            }

            if (!File.Exists(ribbonXmlFileName))
            {
                throw new Exception("  Ribbon xml file specified does not exist");
            }

            //load the doc
            XmlDocument doc = new XmlDocument();
            doc.Load(ribbonXmlFileName);

            //get the entities from the xml doc
            IEnumerable<string> entities = GetEntitiesFromXml(doc);
            bool includeAppRibbon = XmlInludesAppRibbon(doc);



            //setup the solution
            SetupSolution(entities, includeAppRibbon, false);

            //update the ribbondiffs for each entity in the solution
            UpdateSolution(doc);

            //publish again for changes
            utils.PublishCustomizations(false, ref anyError);

            Logger.LogInfo(" Uploaded ribbon xml file to CRM.");
        }

        /// <summary>
        /// Uploads a sitemap customizations xml file to the Crm Server
        /// </summary>
        /// <param name="filename">file to upload</param>
        public void UploadSiteMapXml(string filename)
        {
            Logger.LogInfo("  Uploading sitemap xml file to CRM");

            if (string.IsNullOrEmpty(filename))
            {
                filename = DEFAULT_SITEMAP_XML_FILE;
            }

            if (!File.Exists(filename))
            {
                throw new Exception("  Sitemap xml file specified does not exist");
            }

            //load the doc
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            //get the entities from the xml doc
            if (!XmlInludesSiteMap(doc))
            {
                throw new Exception("Xml file does not have sitemap section");
            }


            //setup the solution
            SetupSolution(null, false, true);

            //update the solution
            UpdateSolution(doc);

            //publish changes
            utils.PublishCustomizations(false, ref anyError);
        }


        #region solutions

        /// <summary>
        /// Exports the solution and extracts the customizations xml 
        /// </summary>
        private XmlDocument ExportCustomizations()
        {
            bool anyError = false;
            string solutionZipFile = Path.Combine(backupDirectory, _solutionName.ToLower() + ".zip");

            utils.ExportSolution(solutionZipFile, _solutionName, false, true, ref anyError);

            //extract the customizations xml
            XmlDocument doc = new XmlDocument();
            using (Package p = ZipPackage.Open(solutionZipFile))
            {
                PackagePartCollection parts = p.GetParts();

                foreach (PackagePart part in parts)
                {
                    if (part.Uri.ToString() == CUSTOMIZATIONS_PART)
                    {
                        Stream stream = part.GetStream();
                        doc.Load(stream);
                        break;
                    }
                }
            }

            return doc;
        }

        /// <summary>
        /// Updates the ribbons and sitemap of a solution without changing anything else in the solution
        /// </summary>
        private void UpdateSolution(XmlDocument ribbonXmlDoc)
        {
            bool anyError = false;
            string solutionZipFile = Path.Combine(backupDirectory, _solutionName.ToLower() + ".zip");

            utils.ExportSolution(solutionZipFile, _solutionName, false, true, ref anyError);

            Logger.LogInfo("  Exported solution file.");

            //extract the customizations xml
            XmlDocument currentXml = new XmlDocument();
            using (Package package = ZipPackage.Open(solutionZipFile))
            {
                Uri customizationsPartUri = new Uri(CUSTOMIZATIONS_PART, UriKind.Relative);

                //get the current xml
                ZipPackagePart part = (ZipPackagePart)package.GetPart(customizationsPartUri);
                using (Stream strm = part.GetStream())
                {
                    currentXml.Load(strm);
                }

                //replace the ribbon sections in the current xml doc with ribbons from the new doc
                ReplaceClientExtensionSections(currentXml, ribbonXmlDoc);

                //replace the xml part with the new one
                package.DeletePart(customizationsPartUri);
                PackagePart newPart = package.CreatePart(customizationsPartUri, System.Net.Mime.MediaTypeNames.Text.Xml);
                using (XmlWriter xw = XmlWriter.Create(newPart.GetStream()))
                {
                    currentXml.WriteTo(xw);
                }
            }

            Logger.LogInfo("  Merged ribbon/sitemap xml into exported solution.");


            Logger.LogInfo("  Importing solution file.");
            utils.ImportSolution(solutionZipFile, _solutionName, false, ref anyError);

        }


        /// <summary>
        /// Sets up a solution that contains only the entities given.
        /// Note: this will always re-use the same solution and remove or add entities as needed.
        /// </summary>
        private void SetupSolution(IEnumerable<string> entities, bool includeAppRibbon, bool includeSiteMap)
        {
            if (entities == null)
            {
                entities = new string[0];
            }

            QueryExpression query = new QueryExpression(Solution.EntityLogicalName);
            query.Criteria.Conditions.Add(new ConditionExpression("uniquename", ConditionOperator.Equal, _solutionName));
            EntityCollection solutions = _service.RetrieveMultiple(query);

            List<string> entitiesAlreadyInSolution = new List<string>();
            bool appRibbonAlreadyInSolution = false;
            bool sitemapAlreadyInSolution = false;

            if (solutions.Entities.Count == 0)
            {
                //create a new blank solution 
                Solution sol = new Solution();
                sol.UniqueName = _solutionName;
                sol.FriendlyName = _solutionName;
                sol.PublisherId = new EntityReference(Publisher.EntityLogicalName, RetrievePublisherId());
                sol.Version = "1.0.0.0";
                _service.Create(sol);
                Logger.LogInfo("  Created new solution for managing ribbon and sitemap");
            }
            else
            {
                Solution solution = (Solution)solutions.Entities[0];

                //Get the list of entities that are already in the solution
                QueryExpression getComponents = new QueryExpression(SolutionComponent.EntityLogicalName);
                getComponents.ColumnSet.AllColumns = true;
                getComponents.Criteria.Conditions.Add(new ConditionExpression("solutionid", ConditionOperator.Equal, solution.SolutionId.Value));
                EntityCollection components = _service.RetrieveMultiple(getComponents);


                //Any entity that is in the solution but not in our list of entities should be removed from the solution
                foreach (SolutionComponent comp in components.Entities)
                {
                    //remove entity if its not in our list
                    if (comp.ComponentType.Value == (int)SolutionManager.ComponentType.Entity)
                    {
                        string entity = Metadata.GetEntityLogicalName(_utils, comp.ObjectId.Value);

                        if (_solutionName == RIBBON_SOLUTION_NAME && !entities.Contains(entity, StringComparer.CurrentCultureIgnoreCase))
                        {
                            SolutionManager.RemoveSolutionComponent(_service, RIBBON_SOLUTION_NAME, comp.ObjectId.Value, SolutionManager.ComponentType.Entity);
                        }

                        entitiesAlreadyInSolution.Add(entity);
                    }
                    else if (comp.ComponentType.Value == (int)SolutionManager.ComponentType.RibbonCustomization)
                    {
                        appRibbonAlreadyInSolution = true;

                        //remove app ribbbon if not included 
                        if (_solutionName == RIBBON_SOLUTION_NAME && !includeAppRibbon)
                        {
                            SolutionManager.RemoveSolutionComponent(_service, RIBBON_SOLUTION_NAME, comp.ObjectId.Value, SolutionManager.ComponentType.RibbonCustomization);
                        }

                    }
                    else if (comp.ComponentType.Value == (int)SolutionManager.ComponentType.SiteMap)
                    {
                        sitemapAlreadyInSolution = true;

                        //remove the sitemap if not included 
                        if (_solutionName == RIBBON_SOLUTION_NAME && !includeSiteMap)
                        {
                            SolutionManager.RemoveSolutionComponent(_service, RIBBON_SOLUTION_NAME, comp.ObjectId.Value, SolutionManager.ComponentType.SiteMap);
                        }

                    }
                }
            }

            //add entities to the solution if they are not already added
            foreach (string entity in entities)
            {
                if (!entitiesAlreadyInSolution.Contains(entity, StringComparer.CurrentCultureIgnoreCase))
                {
                    Guid componentId = Metadata.GetMetadataId(_utils, entity).Value;
                    SolutionManager.AddSolutionComponent(_service, _solutionName, componentId, SolutionManager.ComponentType.Entity, false);
                    Logger.LogInfo("  Added entity '" + entity + "' to solution");
                }
            }

            if (includeAppRibbon && !appRibbonAlreadyInSolution)
            {
                //add the app ribbon to the solution
                QueryExpression getAppRibbonId = new QueryExpression(RibbonCustomization.EntityLogicalName);
                getAppRibbonId.Criteria.Conditions.Add(new ConditionExpression("entity", ConditionOperator.Null));
                getAppRibbonId.Criteria.Conditions.Add(new ConditionExpression("ismanaged", ConditionOperator.Equal, false));
                EntityCollection ribbons = _service.RetrieveMultiple(getAppRibbonId);
                if (ribbons.Entities.Count > 0)
                {
                    SolutionManager.AddSolutionComponent(_service, _solutionName, ribbons[0].Id, SolutionManager.ComponentType.RibbonCustomization, false);
                    Logger.LogInfo("  Added application ribbon to solution");
                }
            }

            if (includeSiteMap && !sitemapAlreadyInSolution)
            {
                //add the sitemap to the solution
                QueryExpression getSiteMapId = new QueryExpression(SiteMap.EntityLogicalName);
                EntityCollection sitemaps = _service.RetrieveMultiple(getSiteMapId);
                if (sitemaps.Entities.Count > 0)
                {
                    SolutionManager.AddSolutionComponent(_service, _solutionName, sitemaps[0].Id, SolutionManager.ComponentType.SiteMap, false);
                    Logger.LogInfo("  Added sitemap to solution");
                }
            }
        }


        /// <summary>
        /// retrieves the first available publsiherid
        /// </summary>
        private Guid RetrievePublisherId()
        {
            QueryExpression query = new QueryExpression(Publisher.EntityLogicalName);
            query.ColumnSet.AllColumns = true;
            query.PageInfo = new PagingInfo() { Count = 1, PageNumber = 1 };
            query.Criteria.Conditions.Add(new ConditionExpression("uniquename", ConditionOperator.NotEqual, "MicrosoftCorporation"));
            query.Orders.Add(new OrderExpression("createdon", OrderType.Ascending));
            EntityCollection publishers = _service.RetrieveMultiple(query);
            return publishers.Entities[0].Id;
        }

        /// <summary>
        /// Publishes a collection of entities
        /// </summary>
        public void Publish(IEnumerable<string> entities, bool publishAll)
        {
            Logger.LogInfo("  Publishing customizations... ");

            if (publishAll)
            {
                _service.Execute(new PublishAllXmlRequest());
            }
            else
            {
                PublishXmlRequest publish = new PublishXmlRequest();
                StringBuilder sb = new StringBuilder("<importexportxml><entities>");
                foreach (string entity in entities)
                {
                    sb.Append("<entity>");
                    sb.Append(entity.ToLowerInvariant());
                    sb.Append("</entity>");
                }
                sb.Append("</entities><optionsets /><webresources></webresources></importexportxml>");
                publish.ParameterXml = sb.ToString();
                _service.Execute(publish);
            }

            Logger.LogInfo("  Done.");
        }

        #endregion

        /// <summary>
        /// Gets all plugin assemblies in the solution zip file
        /// </summary>
        /// <param name="solutionZipFilepath">Solution zip file path</param>
        /// <returns>List of Assemblies</returns>
        public List<Assembly> GetPluginAssemblies(string solutionZipFilepath)
        {
            List<Assembly> pluginAssemblyCollection = new List<Assembly>();
            //extract the solutions xml
            XmlDocument doc = new XmlDocument();

            using (Package p = ZipPackage.Open(solutionZipFilepath))
            {
                PackagePartCollection parts = p.GetParts();

                foreach (PackagePart part in parts)
                {
                    if (part.Uri.ToString().StartsWith("/PluginAssemblies"))
                    {

                        Stream stream = part.GetStream();
                        byte[] assemblyByteArray = new byte[stream.Length];
                        stream.Read(assemblyByteArray, 0, assemblyByteArray.Length);
                        Assembly assemblyDll = Assembly.Load(assemblyByteArray);
                        pluginAssemblyCollection.Add(assemblyDll);
                    }
                }
            }
            return pluginAssemblyCollection;
        }

        public EntityCollection GetPluginAssembliesFromSolution(string solutionName, out List<Guid> pluginStepsGuids)
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

                EntityCollection solutionRetrieved = _service.RetrieveMultiple(query);

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
                solutionComponentQuery.Criteria.AddCondition("componenttype", ConditionOperator.In, new object[] { (int)SolutionManager.ComponentType.PluginAssembly, (int)SolutionManager.ComponentType.SdkMessageProcessingStep });
                solutionComponentQuery.Criteria.AddFilter(LogicalOperator.And);
                EntityCollection solutionComponentsCollection = _service.RetrieveMultiple(solutionComponentQuery);

                List<Guid> pluginAssemblyGuids = new List<Guid>();
                foreach (var entity in solutionComponentsCollection.Entities)
                {
                    SolutionComponent component = (SolutionComponent)entity;
                    if (component.ComponentType.Value == (int)SolutionManager.ComponentType.PluginAssembly)
                    {
                        pluginAssemblyGuids.Add(component.ObjectId.Value);
                    }
                    else if (component.ComponentType.Value == (int)SolutionManager.ComponentType.SdkMessageProcessingStep)
                    {
                        pluginStepsGuids.Add(component.ObjectId.Value);
                    }
                }

                if (pluginAssemblyGuids.Count > 0)
                {
                    QueryExpression pluginAssemblyQuery = new QueryExpression(PluginAssembly.EntityLogicalName);
                    pluginAssemblyQuery.ColumnSet = new ColumnSet("pluginassemblyid", "name", "isolationmode", "sourcetype");

                    object[] values = new object[pluginAssemblyGuids.Count];
                    for (int i = 0; i < pluginAssemblyGuids.Count; i++)
                    {
                        values[i] = pluginAssemblyGuids[i];
                    }

                    pluginAssemblyQuery.Criteria.AddCondition("pluginassemblyid", ConditionOperator.In, values);
                    pluginAssemblyQuery.Criteria.AddFilter(LogicalOperator.And);
                    pluginAssemblyCollection = _service.RetrieveMultiple(pluginAssemblyQuery);
                }
            }

            return pluginAssemblyCollection;
        }

        public void SetSolutionName(string solutionName)
        {
            if (!string.IsNullOrEmpty(solutionName))
            {
                _solutionName = solutionName;
            }
            else
            {
                _solutionName = RIBBON_SOLUTION_NAME;
            }
        }

        #region xml helpers

        /// <summary>
        /// Replaces all the ribbonxml and sitemap nodes in one xml file with the matching ribbonxml and sitemap nodes from another xml file
        /// </summary>
        private void ReplaceClientExtensionSections(XmlDocument currentXml, XmlDocument newXml)
        {
            XmlNodeList entityNodes = currentXml.SelectNodes("//Entity");
            foreach (XmlNode entityNode in entityNodes)
            {
                //get the name of the entity
                string entityName = entityNode.SelectSingleNode("Name").InnerText;

                //find the ribbondiff node
                XmlNode ribbonDiffXmlNode = entityNode.SelectSingleNode("RibbonDiffXml");

                //find the matching ribbon diff node in the new xml file
                XmlNode newEntityNode = newXml.SelectSingleNode(string.Format("//Entity[Name='{0}']", entityName));
                //added a null check for a scenario where a entity is present in the current XML but not in the new (modified XML).
                //.e.g. consider a scenario where ribbon customization is being exported from one solution which does not have Account entity, and now somebody 
                //is trying to import the customization into a new solution which contains account entity in it.
                //in that case the accoutn entity will not be available into the new modified ribbon customization. 
                if (newEntityNode != null)
                {
                    XmlNode newRibbonDiffXmlNode = newEntityNode.SelectSingleNode("RibbonDiffXml");

                    //copy the new ribbondiff into the current xml file (overwriting the existing ribbondiff)
                    XmlNode importedNode = currentXml.ImportNode(newRibbonDiffXmlNode, true);
                    entityNode.InsertBefore(importedNode, ribbonDiffXmlNode);
                    entityNode.RemoveChild(ribbonDiffXmlNode);
                }
            }

            //replace the application ribbon xml 
            XmlNode appRibbonNode = currentXml.SelectSingleNode("ImportExportXml/RibbonDiffXml");
            XmlNode newAppRibbonNode = newXml.SelectSingleNode("ImportExportXml/RibbonDiffXml");
            if (appRibbonNode != null && newAppRibbonNode != null)
            {
                XmlNode importedAppRibbonNode = currentXml.ImportNode(newAppRibbonNode, true);
                currentXml.DocumentElement.InsertBefore(importedAppRibbonNode, appRibbonNode);
                currentXml.DocumentElement.RemoveChild(appRibbonNode);
            }

            //replace the sitemap xml 
            XmlNode sitemapNode = currentXml.SelectSingleNode("ImportExportXml/SiteMap");
            XmlNode newSitemapNode = newXml.SelectSingleNode("ImportExportXml/SiteMap");
            if (sitemapNode != null && newSitemapNode != null)
            {
                XmlNode importedSiteMapNode = currentXml.ImportNode(newSitemapNode, true);
                currentXml.DocumentElement.InsertBefore(importedSiteMapNode, sitemapNode);
                currentXml.DocumentElement.RemoveChild(sitemapNode);
            }
        }


        /// <summary>
        /// Removes all non-ribbon xml from the xml file
        /// </summary>
        private void RemoveEverythingButRibbonAndSitemapFromXml(XmlDocument doc)
        {
            RemoveNode(doc, "//FormXml");
            RemoveNode(doc, "//Roles");
            RemoveNode(doc, "//Workflows");
            RemoveNode(doc, "//FieldSecurityProfiles");
            RemoveNode(doc, "//Templates");
            RemoveNode(doc, "//EntityMaps");
            RemoveNode(doc, "//EntityRelationships");
            RemoveNode(doc, "//OrganizationSettings");
            RemoveNode(doc, "//optionsets");
            RemoveNode(doc, "//Languages");
            RemoveNode(doc, "//EntityInfo");
            RemoveNode(doc, "//SavedQueries");
        }

        /// <summary>
        /// Removes nodes that match the xpath query from the xml document
        /// </summary>
        private void RemoveNode(XmlDocument doc, string xpath)
        {
            XmlNodeList list = doc.SelectNodes(xpath);
            foreach (XmlNode node in list)
            {
                node.ParentNode.RemoveChild(node);
            }
        }

        /// <summary>
        /// Gets a list of the entities that are included in the customizations xml file
        /// </summary>
        private IEnumerable<string> GetEntitiesFromXml(XmlDocument doc)
        {
            List<string> list = new List<string>();

            XmlNodeList entityNodes = doc.SelectNodes("//Entity");
            foreach (XmlNode entityNode in entityNodes)
            {
                //get the name of the entity
                string entityName = entityNode.SelectSingleNode("Name").InnerText;
                list.Add(entityName);
            }

            return list;
        }

        private bool XmlInludesAppRibbon(XmlDocument doc)
        {
            XmlNode appRibbonNode = doc.SelectSingleNode("ImportExportXml/RibbonDiffXml");
            return appRibbonNode != null;
        }

        private bool XmlInludesSiteMap(XmlDocument doc)
        {
            XmlNode siteMapNode = doc.SelectSingleNode("ImportExportXml/SiteMap");
            return siteMapNode != null;
        }

        #endregion
    }
}
