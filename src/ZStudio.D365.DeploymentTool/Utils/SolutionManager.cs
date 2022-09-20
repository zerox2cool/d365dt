using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using System.Xml;
using ZD365DT.DeploymentTool.Utils.Plugin;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ZD365DT.DeploymentTool.Utils
{
    public static class SolutionManager
    {
        public enum ComponentType
        {
            Entity = 1,
            Attribute = 2,
            Relationship = 3,
            AttributePicklistValue = 4,
            AttributeLookupValue = 5,
            ViewAttribute = 6,
            OptionSet = 9,
            SavedQuery = 26,
            Workflow = 29,
            Report = 31,
            RibbonCustomization = 50,
            SiteMap = 62,
            PluginAssembly = 91,
            SdkMessageProcessingStep = 92,
            SdkMessageProcessingStepImage = 93
        }

        public static Publisher GetPublisherRecord(IOrganizationService svc, Guid publisherId)
        {
            Publisher publisher = (Publisher)svc.Retrieve(Publisher.EntityLogicalName, publisherId, new ColumnSet(true));
            return publisher;
        }

        public static Publisher GetPublisherRecord(IOrganizationService svc, string name)
        {
            Publisher result = null;

            QueryExpression q = new QueryExpression(Publisher.EntityLogicalName);
            q.ColumnSet = new ColumnSet(true);
            q.Criteria = new FilterExpression(LogicalOperator.And);
            q.Criteria.AddCondition(new ConditionExpression("uniquename", ConditionOperator.Equal, name));
            EntityCollection output = svc.RetrieveMultiple(q);
            if (output != null && output.Entities.Count > 0)
            {
                if (output.Entities.Count == 1)
                    result = (Publisher)output[0];
                else
                    throw new Exception(string.Format("There are more than one Publisher with the name {0} in the system.", name));
            }

            return result;
        }

        public static void PublishAllXmlRequest(IOrganizationService svc)
        {
            PublishAllXmlRequest publishRequest = new PublishAllXmlRequest();
            svc.Execute(publishRequest);
        }

        public static Solution GetSolutionRecord(IOrganizationService svc, string name)
        {
            Solution result = null;

            QueryExpression q = new QueryExpression(Solution.EntityLogicalName);
            q.ColumnSet = new ColumnSet(true);
            q.Criteria = new FilterExpression(LogicalOperator.And);
            q.Criteria.AddCondition(new ConditionExpression("uniquename", ConditionOperator.Equal, name));
            EntityCollection output = svc.RetrieveMultiple(q);
            if (output != null && output.Entities.Count > 0)
            {
                if (output.Entities.Count == 1)
                    result = (Solution)output[0];
                else
                    throw new Exception(string.Format("There are more than one Solution with the name {0} in the system.", name));
            }

            return result;
        }

        /// <summary>
        /// Add a component to a CRM Solution
        /// </summary>
        public static void AddSolutionComponent(IOrganizationService service, string solutionName, Guid objectId, ComponentType componentType, bool addRequiredComponents, bool doNotIncludeSubComponent = false)
        {
            AddSolutionComponentRequest req = new AddSolutionComponentRequest();

            //sub-component only applicable for entity
            if (componentType == ComponentType.Entity)
                req.DoNotIncludeSubcomponents = doNotIncludeSubComponent;
            req.AddRequiredComponents = addRequiredComponents;
            req.ComponentId = objectId;
            req.ComponentType = (int)componentType;
            req.SolutionUniqueName = solutionName;
            service.Execute(req);
        }

        /// <summary>
        /// Remove a component from a CRM Solution
        /// </summary>
        public static void RemoveSolutionComponent(IOrganizationService service, string solutionName, Guid objectId, ComponentType componentType)
        {
            RemoveSolutionComponentRequest remove = new RemoveSolutionComponentRequest();
            remove.ComponentId = objectId;
            remove.ComponentType = (int)componentType;
            remove.SolutionUniqueName = solutionName;
            service.Execute(remove);
        }

        public static void UpgradeSolutionCommand( string solutionPath,string uniquename,bool ismanaged, WebServiceUtils utils,ref bool  anyError)
        {
            QueryExpression query = new QueryExpression(SystemUser.EntityLogicalName);
            query.PageInfo = new PagingInfo() { Count = 1, PageNumber = 1 };
            utils.Service.RetrieveMultiple(query);
            
            if (string.IsNullOrEmpty(solutionPath))
            {
                throw new Exception("Solution File details are missing.");
            }
            if (!File.Exists(solutionPath))
            {
                throw new Exception(string.Format("Solution file '{0}' does not exist.", solutionPath));
            }
           
            List<Assembly> assemblies = GetPluginAssemblies(solutionPath);
            bool pluginUpgrade = false;
            foreach (Assembly assemblyDll in assemblies)
            {
                AssemblyName assemblyDllName = assemblyDll.GetName();
                string assemblyName = assemblyDllName.Name;
                
                PluginAssembly oldAssembly = RegisterPlugins.GetPluginAssembly(utils.Service, assemblyName);

                //if assembly versions are same, only update the assembly
                if (oldAssembly != null &&
                    (oldAssembly.Major < assemblyDllName.Version.Major ||
                     oldAssembly.Minor < assemblyDllName.Version.Minor))
                {
                    pluginUpgrade = true;
                    
                    RegisterPlugins.UpgradeAssembly(assemblyDll, oldAssembly, assemblyName, solutionPath, uniquename, ismanaged, utils, ref anyError);
                }
            }
        
            if (!pluginUpgrade)
            {
                utils.ImportSolution(solutionPath, uniquename, ismanaged,ref anyError);
               // SolutionManager.ImportSolution(service, Program.GetFileArg(".zip"));
            }
        }

        /// <summary>
        /// Gets all plugin assemblies in the solution zip file
        /// </summary>
        /// <param name="solutionZipFilepath">Solution zip file path</param>
        /// <returns>List of Assemblies</returns>
        public static List<Assembly> GetPluginAssemblies(string solutionZipFilepath)
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
    }
}