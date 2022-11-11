using ZD365DT.DeploymentTool.Configuration;
using ZD365DT.DeploymentTool.Context;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Linq;

namespace ZD365DT.DeploymentTool.Utils
{
    public class WebServiceUtils
    {
        private const int MAX_EXPORT_RETRY = 3;

        #region Properties
        private static CrmAuthenticationType _authenticationType;

        public CrmAuthenticationType authenticationType
        {
            get { return _authenticationType; }
            set { _authenticationType = value; }
        }

        private IOrganizationService _service;
        public IOrganizationService Service
        {
            get { return _service; }
        }

        private DeploymentServiceClient _deployservice;
        public DeploymentServiceClient DeployService
        {
            get { return _deployservice; }
        }

        public bool EnforceTls12 { get; private set; }

        public string OrgName { get; private set; }

        public string CrmConnectionString { get; private set; }

        public CrmDeploymentServiceElement DeployElement { get; private set; }

        public StringDictionary StringTokens { get; private set; }

        public OrganizationContext CurrentOrgContext { get; private set; }
        #endregion Properties

        #region Variables
        enum ImportSolutionStatus
        {
            success = 0,
            warning = 1,
            error = 2,
            couldnotparse = 3
        }

        int timeoutMinutes = 30;
        int cacheTimeout = 2;
        private Dictionary<string, AttributeMetadata> cacheAttribute = new Dictionary<string, AttributeMetadata>();
        #endregion Variables

        #region Constructors
        /// <summary>
        /// Constructs a WebServiceUtils in the current Orgnization and user's context
        /// </summary>
        /// <param name="crmConnectionString">connection string for the org</param>
        /// <param name="orgName">org name</param>
        /// <param name="authType">authentication type</param>
        public WebServiceUtils(string crmConnectionString, string orgName, CrmAuthenticationType authType, bool enforceTls12 = false)
        {
            //ignore any SSL certificate error
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(OnCertValidationCallback);

            CrmConnectionString = crmConnectionString;
            OrgName = orgName;
            EnforceTls12 = enforceTls12;
            _authenticationType = authType;
            DeployElement = null;
            StringTokens = null;

            SetupConnection();
        }

        /// <summary>
        /// Constructs a WebServiceUtils in the current Orgnization and user's context
        /// </summary>
        /// <param name="crmConnectionString">connection string for the org</param>
        /// <param name="orgName">org name</param>
        /// <param name="authType">authentication type</param>
        public WebServiceUtils(string crmConnectionString, string orgName, CrmAuthenticationType authType, CrmDeploymentServiceElement deployElement, StringDictionary tokens, bool enforceTls12 = false)
        {
            //ignore any SSL certificate error
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(OnCertValidationCallback);

            CrmConnectionString = crmConnectionString;
            OrgName = orgName;
            EnforceTls12 = enforceTls12;
            _authenticationType = authType;
            DeployElement = deployElement;
            StringTokens = tokens;

            SetupConnection();
        }

        private void SetupConnection(bool noCache = false)
        {
            _service = GetCrmService(CrmConnectionString, OrgName, DeployElement, StringTokens, noCache);

            //set context
            CurrentOrgContext = new OrganizationContext(_service);
        }
        #endregion Constructors

        #region Crm Service
        /// <summary>
        /// returns a CRM service
        /// </summary>
        /// <param name="crmConnectionString"></param>
        /// <param name="orgName"></param>
        /// <returns></returns>
        public IOrganizationService GetCrmService(string crmConnectionString, string orgName, CrmDeploymentServiceElement createDeployService, StringDictionary tokens, bool noCache = false)
        {
            object cachedService = CrmCacheManager.GetExisting(orgName);
            if (noCache)
                _service = null;
            else
                _service = (cachedService == null) ? null : (IOrganizationService)cachedService;

            if (_service == null)
            {
                //read the timoutMinutes and cacheTimeout from the config file
                GetAppSettingsSectionFromCommandArguments();

                if (!crmConnectionString.ToLower().Contains("url=") || !crmConnectionString.ToLower().Contains("authtype="))
                    throw new Exception("URL and AuthType is required on a D365 CE connection string.");

                //use the XRM tooling on the new CRM SDK v9 and deprecate the use of office365 and change to oauth
                //https://docs.microsoft.com/en-us/powerapps/developer/common-data-service/authenticate-office365-deprecation
                if (crmConnectionString.ToLower().Contains(".dynamics.com"))
                {
                    //validate when connecting to the cloud, from 4th Feb 2020 onwards, connection strings need to be updated to use OAuth and contain the AppId (recommended to add a new AppId on AAD)
                    if (crmConnectionString.ToLower().Contains("authtype=office365"))
                        throw new Exception("Cannot connect to D365 online CDS with the Office365 authentication type, it has been deprecated effective 4th Feb 2020.");
                    if (crmConnectionString.ToLower().Contains("authtype=oauth") && !crmConnectionString.ToLower().Contains("appid="))
                        throw new Exception("Cannot connect to D365 online CDS without an Application ID, please register an application on your AAD or use Microsoft default one.");
                    if (crmConnectionString.ToLower().Contains("authtype=oauth") && !crmConnectionString.ToLower().Contains("redirecturi=app://"))
                        throw new Exception("Cannot connect to D365 online CDS without a Redirect Uri.");

                    if (crmConnectionString.ToLower().Contains("authtype=clientsecret") && (!crmConnectionString.ToLower().Contains("clientid=") || !crmConnectionString.ToLower().Contains("clientsecret=")))
                        throw new Exception("Cannot connect to D365 online CDS without a Client ID or Client Secret.");
                    if (crmConnectionString.ToLower().Contains("authtype=certificate") && (!crmConnectionString.ToLower().Contains("clientid=") || !crmConnectionString.ToLower().Contains("thumbprint=")))
                        throw new Exception("Cannot connect to D365 online CDS without a Client ID or Certificate Thumbprint.");
                }

                //for latest D365 (post Dec-2017), bug in CRM requiring the use of TLS12 instead of the default SSL3, this will not be require for .NET 4.6.2
                if (crmConnectionString.ToLower().Contains("authtype=oauth") && crmConnectionString.ToLower().Contains(".dynamics.com"))
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                else if (crmConnectionString.ToLower().Contains("https:") && EnforceTls12)
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //set the connection timeout to the new static property as OrganizationServiceProxy has been deprecated in Apr 2020
                CrmServiceClient.MaxConnectionTimeout = new TimeSpan(0, timeoutMinutes, 0);

                //use the XRM tooling on the new CRM SDK v8 onwards
                //https://msdn.microsoft.com/en-us/library/mt608573.aspx
                CrmServiceClient crmSvc = new CrmServiceClient(crmConnectionString);
                if (crmSvc.IsReady)
                {
                    //set the connection timeout to the inner channel as well if possible
                    if (crmSvc.OrganizationWebProxyClient != null)
                        crmSvc.OrganizationWebProxyClient.InnerChannel.OperationTimeout = new TimeSpan(0, timeoutMinutes, 0);

                    //set the Organization Service
                    _service = crmSvc;

                    CrmCacheManager.AddOrReplaceExisting(orgName, _service, cacheTimeout.ToString());
                }
                else
                {
                    if (!string.IsNullOrEmpty(crmSvc.LastCrmError))
                        throw new Exception(string.Format("Fail to connect to D365 CE. {0}", crmSvc.LastCrmError));
                    else
                        throw new Exception("Fail to connect to D365 CE.");
                }
            }

            if (createDeployService != null && !string.IsNullOrEmpty(createDeployService.ServiceUrl) && !string.IsNullOrEmpty(createDeployService.Username) &&
                !string.IsNullOrEmpty(createDeployService.Password) && !string.IsNullOrEmpty(createDeployService.Domain))
            {
                string serviceUrl = IOUtils.ReplaceStringTokens(createDeployService.ServiceUrl, tokens);
                string userName = IOUtils.ReplaceStringTokens(createDeployService.Username, tokens);
                string password = IOUtils.ReplaceStringTokens(createDeployService.Password, tokens);
                string domain = IOUtils.ReplaceStringTokens(createDeployService.Domain, tokens);

                object cachedDeployService = CrmCacheManager.GetExisting(serviceUrl + userName + "_deployment");
                _deployservice = (cachedDeployService == null) ? null : (DeploymentServiceClient)cachedDeployService;
                if (_deployservice == null)
                {
                    _deployservice =
                       Microsoft.Xrm.Sdk.Deployment.Proxy.ProxyClientHelper.CreateClient(
                          new Uri(serviceUrl + "/XRMDeployment/2011/Deployment.svc"));

                    _deployservice.ClientCredentials.Windows.ClientCredential =
                        new System.Net.NetworkCredential(userName,
                        password, domain);

                    _deployservice.Endpoint.Binding.SendTimeout = new TimeSpan(0, 0, 600);
                    CrmCacheManager.AddOrReplaceExisting(serviceUrl + userName + "_deployment", _deployservice, cacheTimeout.ToString());
                }
            }
            return _service;

        }

        /// <summary>
        /// Sets the timoutMinutes and cacheTimeout from the config file
        /// </summary>
        private void GetAppSettingsSectionFromCommandArguments()
        {
            List<string> commandLineArgs = new List<string>();
            commandLineArgs.AddRange(Environment.GetCommandLineArgs());
            commandLineArgs.RemoveAt(0); // The first argument is the program name
            string ConfigEnvironment = string.Empty;
            char[] TRIM_CHARS = new char[] { ':', ' ', '@', '"' };
            string CONFIG = "config";
            AppSettingsSection section = null;
            if (commandLineArgs.Count > 0)
            {
                for (int i = 0; i < commandLineArgs.Count; i++)
                {
                    ConfigEnvironment = commandLineArgs[i].ToString();
                    if (ConfigEnvironment.IndexOf(":") <= 0)
                    {
                        throw new ArgumentException("Invalid Commandline Token", ConfigEnvironment);
                    }
                    int splitIndex = ConfigEnvironment.IndexOf(":");
                    string tokenString = ConfigEnvironment.Substring(0, splitIndex).Trim(TRIM_CHARS);
                    string valueString = ConfigEnvironment.Substring(splitIndex, ConfigEnvironment.Length - splitIndex).Trim(TRIM_CHARS);
                    if (tokenString.ToLower() == CONFIG)
                    {
                        ConfigEnvironment = valueString;
                        break;
                    }
                }

                //if there is a configfile sepcfied in the command line aurgments get the appsettings section from that config file
                if (!string.IsNullOrEmpty(ConfigEnvironment))
                {
                    string path = Path.GetFullPath(ConfigEnvironment);
                    ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                    fileMap.ExeConfigFilename = path;
                    if (!File.Exists(fileMap.ExeConfigFilename))
                        throw new ArgumentException("Configuration file do not Exists : ", path);

                    var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                    section = config.GetSection("appSettings") as AppSettingsSection;
                }
                //get the appsetings section from the default config file
                else
                {
                    var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    if (config != null)
                        section = config.GetSection("appSettings") as AppSettingsSection;
                }

                // set the values if the section is not null, else default values will be taken
                if (section != null)
                {

                    if (section.Settings["CrmServiceTimeoutMinutes"] != null && !string.IsNullOrEmpty((string)section.Settings["CrmServiceTimeoutMinutes"].Value))
                        timeoutMinutes = int.Parse(section.Settings["CrmServiceTimeoutMinutes"].Value);

                    if (section.Settings["CacheExpirationHours"] != null && !string.IsNullOrEmpty((string)section.Settings["CacheExpirationHours"].Value))
                        cacheTimeout = int.Parse(section.Settings["CacheExpirationHours"].Value);
                }
            }
        }
        #endregion Crm Service

        #region OnCertValidationCallback
        public static bool OnCertValidationCallback(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            //ignore any SSL certificate error
            return true;
        }
        #endregion OnCertValidationCallback

        #region Report Methods
        /// <summary>
        /// publish report to the org
        /// </summary>
        /// <param name="reportElement"></param>
        /// <param name="tokens"></param>
        internal void PublishReport(ReportElement reportElement, StringDictionary tokens)
        {
            string fileName = Path.GetFileName(reportElement.FileName);
            bool isUpdate = false;

            Report rep = GetReportFileNamed(reportElement.ReportName, fileName);
            if (rep != null)
            {
                if (reportElement.InstallBehaviour == ReportInstallBehaviour.LeaveExisting)
                {
                    Logger.LogInfo("  Report '{0}' already exists and is being left unchanged.", reportElement.ReportName);
                    return;
                }
                else if (reportElement.InstallBehaviour == ReportInstallBehaviour.RemoveAndRecreate)
                {
                    DeleteReportNamed(reportElement.ReportName, fileName);
                    rep = new Report();
                }
                else
                {
                    Logger.LogInfo("  Report '{0}' already exists and will be updated.", reportElement.ReportName);
                    isUpdate = true;
                }
            }
            else
            {
                rep = new Report();
            }

            rep.Name = reportElement.ReportName;
            rep.FileName = fileName;
            if (!String.IsNullOrEmpty(reportElement.Description))
            {
                rep.Description = reportElement.Description;
            }
            rep.ReportTypeCode = new OptionSetValue(1); //ReportingServices
            rep.LanguageCode = new int?(reportElement.LanguageCode);
            rep.IsPersonal = new bool?(false);

            if (!String.IsNullOrEmpty(reportElement.ParentReportName))
            {
                Guid parentid = GetReportNamed(reportElement.ParentReportName);
                if (parentid != Guid.Empty)
                {
                    rep.ParentReportId = new EntityReference(Report.EntityLogicalName.ToString(), parentid);
                    Logger.LogInfo("    Setting report Parent to {0}", reportElement.ParentReportName);
                }
                else
                {
                    Logger.LogWarning("  Unable to find parent report '{0}' for report '{1}'", reportElement.ParentReportName, reportElement.ReportName);
                }
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(IOUtils.ReadFileReplaceTokens(Path.GetFullPath(reportElement.FileName), tokens));
            rep.BodyText = doc.ChildNodes[1].OuterXml;

            Logger.LogInfo("  Creating report {0} from file {1}", reportElement.ReportName, fileName);

            Guid reportId = Guid.Empty;

            if (isUpdate)
            {
                _service.Update(rep);
                reportId = rep.ReportId.Value;
            }
            else
            {
                reportId = _service.Create(rep);
            }

            SetDisplayAreasFor(reportId, reportElement.DispalyAreas);
            SetRelatedEntitiesFor(reportId, reportElement.RelatedRecords);
            SetReportCategroriesFor(reportId, reportElement.CategoryCodes);

            if (reportElement.PublishExternal)
            {
                //    Logger.LogInfo("  Making report '{0}' available to external users", reportElement.ReportName);
                //    PublishExternalRequest request = new PublishExternalRequest();
                //    request.ReportId = reportId;
                //    _service.Execute(request);
                //
            }
        }


        /// <summary>
        /// Get a report By Name
        /// </summary>
        /// <param name="parentreport">The name of the report</param>
        /// <returns>The report identifier</returns>
        private Guid GetReportNamed(string reportName)
        {
            Guid parentid = Guid.Empty;
            if (!String.IsNullOrEmpty(reportName))
            {
                ColumnSet cols = new ColumnSet(new string[] { "reportid" });

                QueryByAttribute query = new QueryByAttribute();
                query.EntityName = Report.EntityLogicalName.ToString();
                query.ColumnSet = cols;
                query.Attributes.Add("name");
                query.Values.Add(reportName);

                EntityCollection bec = _service.RetrieveMultiple(query);
                if (bec.Entities != null && bec.Entities.Count > 0)
                {
                    parentid = (bec.Entities[0] as Report).ReportId.Value;
                }
            }
            return parentid;
        }


        /// <summary>
        /// Returns a report held in the CRM Systems which uses the specified
        /// filename
        /// </summary>
        /// <param name="name">The name of the report</param>
        /// <param name="filename">The filename of the report</param>
        /// <returns>A report definition</returns>
        internal Report GetReportFileNamed(string name, string filename)
        {
            return GetReportFileNamed(name, filename, new ColumnSet(true));
        }


        /// <summary>
        /// Deletes the report from the CRM system with the specified filename
        /// </summary>
        /// <param name="filename">The name of the report</param>
        /// <param name="filename">The filename of the report</param>
        internal void DeleteReportNamed(string name, string filename)
        {
            ColumnSet columns = new ColumnSet(new string[] { "reportid" });
            Report rep = GetReportFileNamed(name, filename, columns);

            if (rep != null)
            {
                Logger.LogInfo("  Removing report {0}", filename);
                DeleteRelatedEntitiesFor(rep.ReportId.Value);
                DeleteReportCategoriesFor(rep.ReportId.Value);
                DeleteReportDisplayAreasFor(rep.ReportId.Value);
                _service.Delete(Report.EntityLogicalName.ToString(), rep.ReportId.Value);
            }
        }


        /// <summary>
        /// Returns a report held in the CRM Systems which uses the specified
        /// filename
        /// </summary>
        /// <param name="name">The name of the report</param>
        /// <param name="filename">The filename of the report</param>
        /// <param name="cols">The columns to return </param>
        /// <returns>A report definition</returns>
        private Report GetReportFileNamed(string name, string filename, ColumnSet cols)
        {
            Report result = null;

            string filenameWithoutPath = Path.GetFileName(filename);

            FilterExpression filter = new FilterExpression();
            filter.FilterOperator = LogicalOperator.And;

            QueryExpression query = new QueryExpression();
            query.EntityName = Report.EntityLogicalName.ToString();
            query.ColumnSet = cols;
            query.Criteria = filter;
            query.Criteria.AddCondition("name", ConditionOperator.Equal, name);
            query.Criteria.AddCondition("filename", ConditionOperator.Equal, filenameWithoutPath);

            EntityCollection entities = _service.RetrieveMultiple(query);
            if (entities.Entities.Count > 0)
            {
                result = entities.Entities[0] as Report;
            }
            return result;
        }


        private void SetReportCategroriesFor(Guid reportId, string categories)
        {
            DeleteReportCategoriesFor(reportId);

            if (!String.IsNullOrEmpty(categories))
            {
                string[] cats = categories.Split(',');
                foreach (string category in cats)
                {
                    ReportCategory entity = new ReportCategory();
                    entity.ReportId = new EntityReference(Report.EntityLogicalName.ToString(), reportId);
                    entity.CategoryCode = new OptionSetValue(int.Parse(category));

                    Logger.LogInfo("    Registering report in category {0}", category);
                    _service.Create(entity);
                }
            }
        }

        private void DeleteReportCategoriesFor(Guid reportId)
        {
            ColumnSet cols = new ColumnSet(new string[] { "reportcategoryid" });

            QueryByAttribute query = new QueryByAttribute();
            query.Attributes.Add("reportid");
            query.Values.Add(reportId);
            query.EntityName = ReportCategory.EntityLogicalName.ToString();
            query.ColumnSet = cols;

            EntityCollection entities = _service.RetrieveMultiple(query);

            foreach (ReportCategory entity in entities.Entities)
            {
                _service.Delete(ReportCategory.EntityLogicalName.ToString(), entity.ReportCategoryId.Value);
            }
        }


        private void SetRelatedEntitiesFor(Guid reportId, string relatedEntities)
        {
            DeleteRelatedEntitiesFor(reportId);

            if (!String.IsNullOrEmpty(relatedEntities))
            {
                string[] entities = relatedEntities.Split(',');
                foreach (string relatedEntity in entities)
                {
                    ReportEntity entity = new ReportEntity();
                    entity.ReportId = new EntityReference(Report.EntityLogicalName.ToString(), reportId);
                    entity.ObjectTypeCode = relatedEntity;

                    Logger.LogInfo("    Registering report against entity {0}", relatedEntity);
                    _service.Create(entity);
                }
            }
        }


        private void DeleteRelatedEntitiesFor(Guid reportId)
        {
            ColumnSet cols = new ColumnSet(new string[] { "reportentityid" });

            QueryByAttribute query = new QueryByAttribute();
            query.Attributes.Add("reportid");
            query.Values.Add(reportId);
            query.EntityName = ReportEntity.EntityLogicalName.ToString();
            query.ColumnSet = cols;

            EntityCollection entities = _service.RetrieveMultiple(query);

            foreach (ReportEntity entity in entities.Entities)
            {
                _service.Delete(ReportEntity.EntityLogicalName.ToString(), entity.ReportEntityId.Value);
            }
        }


        private void SetDisplayAreasFor(Guid reportId, string displayAreas)
        {
            DeleteReportDisplayAreasFor(reportId);

            if (!String.IsNullOrEmpty(displayAreas))
            {
                string[] areas = displayAreas.Split(',');
                foreach (string area in areas)
                {
                    ReportVisibility entity = new ReportVisibility();
                    entity.ReportId = new EntityReference(Report.EntityLogicalName.ToString(), reportId);
                    entity.VisibilityCode = new OptionSetValue(int.Parse(area));

                    Logger.LogInfo("    Setting report to display in area {0}", area);
                    _service.Create(entity);
                }
            }
        }


        private void DeleteReportDisplayAreasFor(Guid reportId)
        {
            ColumnSet cols = new ColumnSet(new string[] { "reportvisibilityid" });

            QueryByAttribute query = new QueryByAttribute();
            query.Attributes.Add("reportid");
            query.Values.Add(reportId);
            query.EntityName = ReportVisibility.EntityLogicalName.ToString();
            query.ColumnSet = cols;

            EntityCollection entities = _service.RetrieveMultiple(query);

            foreach (ReportVisibility entity in entities.Entities)
            {
                _service.Delete(ReportVisibility.EntityLogicalName.ToString(), entity.ReportVisibilityId.Value);
            }
        }

        #endregion Report Methods

        #region Workflow Methods

        public void PublishAllWorkflows(string[] workflowList)
        {
            ColumnSet cols = new ColumnSet(new string[] { "workflowid", "name" });

            FilterExpression filter = new FilterExpression();
            filter.FilterOperator = LogicalOperator.And;

            ConditionExpression orgCondition = new ConditionExpression();
            orgCondition.AttributeName = "scope";
            orgCondition.Operator = ConditionOperator.Equal;
            orgCondition.Values.Add(4);

            ConditionExpression draftCondition = new ConditionExpression();
            draftCondition.AttributeName = "statecode";
            draftCondition.Operator = ConditionOperator.Equal;
            draftCondition.Values.Add(0);

            ConditionExpression pluginTypeNullCondition = new ConditionExpression();
            pluginTypeNullCondition.AttributeName = "plugintypeid";
            pluginTypeNullCondition.Operator = ConditionOperator.Null;

            ConditionExpression typeCondition = new ConditionExpression();
            typeCondition.AttributeName = "type";
            typeCondition.Operator = ConditionOperator.Equal;
            typeCondition.Values.Add(1);

            QueryExpression query = new QueryExpression();
            query.EntityName = "workflow";
            query.ColumnSet = cols;
            query.Criteria = filter;
            query.Criteria.AddCondition(orgCondition);
            query.Criteria.AddCondition(draftCondition);
            query.Criteria.AddCondition(pluginTypeNullCondition);
            query.Criteria.AddCondition(typeCondition);

            EntityCollection entities = _service.RetrieveMultiple(query);

            Exception errorEx = null;
            string errorMessage = string.Empty;
            foreach (Entity wf in entities.Entities)
            {
                try
                {
                    //activate all workflow or workflow that is in the list
                    if (workflowList == null || (workflowList != null && CommonHelper.StringInArray((string)wf.Attributes["name"], workflowList)))
                    {
                        Logger.LogInfo("Publishing '{0}'.", wf.Attributes["name"]);
                        SetStateRequest request = new SetStateRequest();
                        request.EntityMoniker = new EntityReference("workflow", wf.Id);
                        //set the status and state correctly when activating workflow
                        request.Status = new OptionSetValue((int)workflow_statuscode.Activated);
                        request.State = new OptionSetValue((int)WorkflowState.Activated);
                        _service.Execute(request);
                    }
                }
                catch (Exception ex)
                {
                    //capture all error, but continue to publish the rest of the workflows
                    errorMessage += string.Format("Error Activating Workflow: {0}, Error: {1}{2}", wf.Attributes["name"], ex.Message, Environment.NewLine);
                    //capture the last exception
                    errorEx = ex;
                }
            }

            //check if there is error
            if (!string.IsNullOrEmpty(errorMessage))
                throw new Exception(errorMessage, errorEx);
        }

        public void AssignAndDeactivateWorkflowsOnly(Guid userId, string[] workflowList)
        {
            ColumnSet cols = new ColumnSet(true);

            FilterExpression filter = new FilterExpression();
            filter.FilterOperator = LogicalOperator.And;

            //ConditionExpression draftCondition = new ConditionExpression();
            //draftCondition.AttributeName = "statecode";
            //draftCondition.Operator = ConditionOperator.Equal;
            //draftCondition.Values.Add(1);

            ConditionExpression typeCondition = new ConditionExpression();
            typeCondition.AttributeName = "type";
            typeCondition.Operator = ConditionOperator.Equal;
            typeCondition.Values.Add(1);

            ConditionExpression categoryCondition = new ConditionExpression();
            categoryCondition.AttributeName = "category";
            categoryCondition.Operator = ConditionOperator.Equal;
            categoryCondition.Values.Add(0); //0 = workflow; 2 = business rule; 3 = action

            QueryExpression query = new QueryExpression();
            query.EntityName = Workflow.EntityLogicalName;
            query.ColumnSet = cols;
            query.Criteria = filter;

            query.Criteria.AddCondition(typeCondition);
            //only filter by category from CRM2015 onwards
            if (CurrentOrgContext.MajorVersion >= 7)
                query.Criteria.AddCondition(categoryCondition);
            EntityCollection entities = _service.RetrieveMultiple(query);

            foreach (Entity wf in entities.Entities)
            {
                try
                {
                    //assign and deactivate all workflows or workflow that is in the list
                    if (workflowList == null || (workflowList != null && CommonHelper.StringInArray((string)wf.Attributes["name"], workflowList)))
                    {
                        Logger.LogInfo("  Assign workflow '{0}' to user '{1}'.", wf.Attributes["name"], userId);
                        AssignRequest assign = new AssignRequest
                        {
                            Assignee = new EntityReference(SystemUser.EntityLogicalName,
                                        userId),
                            Target = new EntityReference(Workflow.EntityLogicalName,
                                        wf.Id)
                        };

                        _service.Execute(assign);

                        //deactivate workflow only when it is activated
                        if (wf["statecode"] != null && ((OptionSetValue)wf["statecode"]).Value == (int)WorkflowState.Activated)
                        {
                            Logger.LogInfo("  Deactivate workflow '{0}'.", wf.Attributes["name"]);
                            SetStateRequest request = new SetStateRequest();
                            request.EntityMoniker = new EntityReference("workflow", wf.Id);
                            request.State = new OptionSetValue((int)WorkflowState.Draft);
                            request.Status = new Microsoft.Xrm.Sdk.OptionSetValue((int)workflow_statuscode.Draft);

                            _service.Execute(request);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
        }

        public void ActivateWorkflows(Guid userId, string workflowName, bool allworkflows, ref bool anyError, ref string errorString)
        {
            ColumnSet cols = new ColumnSet(true);

            FilterExpression filter = new FilterExpression();
            filter.FilterOperator = LogicalOperator.And;

            ConditionExpression draftCondition = new ConditionExpression();
            draftCondition.AttributeName = "statecode";
            draftCondition.Operator = ConditionOperator.Equal;
            draftCondition.Values.Add(0);

            ConditionExpression typeCondition = new ConditionExpression();
            typeCondition.AttributeName = "type";
            typeCondition.Operator = ConditionOperator.Equal;
            typeCondition.Values.Add(1);

            ConditionExpression nameCondition = new ConditionExpression();
            nameCondition.AttributeName = "name";
            nameCondition.Operator = ConditionOperator.Equal;
            nameCondition.Values.Add(workflowName);

            QueryExpression query = new QueryExpression();
            query.EntityName = Workflow.EntityLogicalName;// "workflow";
            query.ColumnSet = cols;
            query.Criteria = filter;

            if (!allworkflows)
                query.Criteria.AddCondition(nameCondition);

            query.Criteria.AddCondition(draftCondition);
            query.Criteria.AddCondition(typeCondition);
            EntityCollection entities = _service.RetrieveMultiple(query);

            foreach (Entity wf in entities.Entities)
            {
                try
                {
                    SetStateRequest request = new SetStateRequest();
                    request.EntityMoniker = new EntityReference("workflow", wf.Id);
                    request.State = new OptionSetValue(1);
                    request.Status = new Microsoft.Xrm.Sdk.OptionSetValue((int)workflow_statuscode.Activated);
                    _service.Execute(request);
                    Logger.LogInfo("  Activated workflow '{0}'.", wf.Attributes["name"]);
                }
                catch (System.Exception ex)
                {
                    errorString += Environment.NewLine + ex.Message;
                    anyError = true;
                }
            }

        }

        public void AssignWorkflows(Guid userId, string workflowName, ref bool anyError, ref string errorString)
        {
            ColumnSet cols = new ColumnSet(true);

            FilterExpression filter = new FilterExpression();
            filter.FilterOperator = LogicalOperator.And;

            ConditionExpression typeCondition = new ConditionExpression();
            typeCondition.AttributeName = "type";
            typeCondition.Operator = ConditionOperator.Equal;
            typeCondition.Values.Add(1);

            ConditionExpression nameCondition = new ConditionExpression();
            nameCondition.AttributeName = "name";
            nameCondition.Operator = ConditionOperator.Equal;
            nameCondition.Values.Add(workflowName);

            QueryExpression query = new QueryExpression();
            query.EntityName = Workflow.EntityLogicalName;// "workflow";
            query.ColumnSet = cols;
            query.Criteria = filter;

            query.Criteria.AddCondition(typeCondition);
            query.Criteria.AddCondition(nameCondition);

            EntityCollection entities = _service.RetrieveMultiple(query);

            if (entities.Entities.Count > 0)
            {
                try
                {
                    Entity wf = entities.Entities[0];
                    AssignRequest assign = new AssignRequest
                    {
                        Assignee = new EntityReference(SystemUser.EntityLogicalName,
                            userId),
                        Target = new EntityReference(Workflow.EntityLogicalName,
                            wf.Id)
                    };

                    _service.Execute(assign);
                    Logger.LogInfo("  Assigned workflow '{0}' to user '{1}'.", wf.Attributes["name"], userId);
                }
                catch (System.Exception ex)
                {
                    Logger.LogException(ex);
                    anyError = true;
                    errorString += Environment.NewLine + ex.Message;
                }
            }
            else
            {
                anyError = true;
                errorString += Environment.NewLine + string.Format("  workflow {0} is not found", workflowName);
            }

        }
        #endregion Workflow Methods

        #region Customizaion
        private ExportSolutionResponse RunExportWithTimeout(ExportSolutionRequest exportRequest, int timeoutInMilliseconds)
        {
            ExportSolutionResponse response = null;

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(timeoutInMilliseconds);
            try
            {
                System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Run(() => {
                    Logger.LogInfo($"  Execute Export - Start...");
                    response = (ExportSolutionResponse)_service.Execute(exportRequest);
                    Logger.LogInfo($"  Execute Export - End...");
                }, cts.Token);
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                using (cts.Token.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                {
                    if (task != System.Threading.Tasks.Task.WhenAny(task, tcs.Task).GetAwaiter().GetResult())
                    {
                        throw new OperationCanceledException(cts.Token);
                    }
                }
                task.Wait();
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException($"Operation Timeout. The export ran for more than {timeoutInMilliseconds} milliseconds. Max Retry: {MAX_EXPORT_RETRY}.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Unhandled Exception: {ex.Message}");
            }
            finally
            {
                cts.Dispose();
            }

            return response;
        }

        /// <summary>
        /// This Method Exports the solution to the given path
        /// </summary>
        /// <param name="storagePath">path where the solution file is to be stroed</param>
        /// <param name="solutionUniqueName">solution unique name</param>
        /// <param name="isManaged">should the solution be exported as managed</param>
        public void ExportSolution(string storagePath, string solutionUniqueName, bool isManaged, bool publishBeforeExport, ref bool anyError, int exportRetryTimeout = 600)
        {

            try
            {
                bool isCompleted = false;
                int tryCount = 0;
                while (!isCompleted)
                {
                    try
                    {
                        tryCount++;
                        QueryExpression queryCheckForSolution = new QueryExpression
                        {
                            EntityName = Solution.EntityLogicalName,
                            ColumnSet = new ColumnSet(true),
                            Criteria = new FilterExpression()
                        };
                        queryCheckForSolution.Criteria.AddCondition("uniquename", ConditionOperator.Equal, solutionUniqueName);

                        //find solution to check if it exist
                        Logger.LogInfo(string.Format("  Searching for the solution {0}...", solutionUniqueName));
                        string solutionVersion = string.Empty;
                        EntityCollection querySampleSolutionResults = _service.RetrieveMultiple(queryCheckForSolution);
                        Solution solutionResults = null;
                        if (querySampleSolutionResults.Entities.Count > 0)
                        {
                            solutionResults = (Solution)querySampleSolutionResults.Entities[0];
                            if (!string.IsNullOrEmpty(solutionResults.Version))
                            {
                                solutionVersion = solutionResults.Version;
                                Logger.LogInfo(string.Format("  Found the solution {0} of version {1}...", solutionUniqueName, solutionVersion));
                            }
                            else
                                Logger.LogInfo(string.Format("  Found the solution {0}...", solutionUniqueName));
                        }
                        else
                            solutionResults = null;

                        if (solutionResults != null && solutionResults.Contains("ismanaged") && solutionResults.Attributes["ismanaged"] != null)
                        {
                            bool isManagedSolution = (bool)solutionResults.Attributes["ismanaged"];
                            if (!isManagedSolution)
                            {
                                //Publish the solution before the solution export
                                anyError = false;

                                if (publishBeforeExport)
                                    PublishCustomizations(false, ref anyError);

                                //Create the request
                                ExportSolutionRequest exportRequest = new ExportSolutionRequest();
                                exportRequest.Managed = isManaged;
                                exportRequest.SolutionName = solutionUniqueName;

                                ExportSolutionResponse response = null;
                                //Execute the request and get the response
                                Logger.LogInfo($"  Start to export customizations (Attempt #{tryCount})...");
                                response = RunExportWithTimeout(exportRequest, exportRetryTimeout * 1000);

                                //check response
                                if (response == null)
                                    throw new Exception("Export failed. No response.");

                                //check if output directory exists, if not, create the directory
                                if (!Directory.Exists(Path.GetDirectoryName(storagePath)))
                                    Directory.CreateDirectory(Path.GetDirectoryName(storagePath));

                                //Write the output to the file
                                File.WriteAllBytes(storagePath, response.ExportSolutionFile);

                                //write a text file with the version number of the solution
                                if (!string.IsNullOrEmpty(solutionVersion))
                                {
                                    string versionTextPath = storagePath + "." + solutionVersion;
                                    File.WriteAllText(versionTextPath, solutionVersion);
                                }

                                isCompleted = true;
                                Logger.LogInfo($"  Exported to {storagePath}.");
                            }
                            else
                            {
                                isCompleted = true;
                                Logger.LogWarning(string.Format("The solution {0} is Managed, cannot be Exported", solutionUniqueName));
                            }
                        }
                        else
                        {
                            isCompleted = true;
                            Logger.LogWarning(string.Format("  The solution {0} does not exists", solutionUniqueName));
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        //perform a retry if it has not exceed the MAX limit
                        Logger.LogWarning($"Operation Timeout. The export ran for more than {exportRetryTimeout} seconds. Max Retry: {MAX_EXPORT_RETRY}.");
                        if (tryCount < MAX_EXPORT_RETRY)
                        {
                            isCompleted = false;
                            Logger.LogInfo($"Try Count: {tryCount}. Max Retry: {MAX_EXPORT_RETRY}. Retry export again.");

                            //setup connection again
                            SetupConnection(true);
                            Logger.LogInfo($"Reconnect to D365.");
                        }
                        else
                        {
                            isCompleted = true;
                            throw new OperationCanceledException($"Operation Timeout. The export ran for more than {exportRetryTimeout} seconds. Retry Count: {tryCount}. Max Retry: {MAX_EXPORT_RETRY}.");
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                anyError = true;
                Logger.LogError($"Export Exception: {ex.Message}.");
            }
        }

        class BackGroundWorkerAttributesObject
        {
            public IOrganizationService service;
            public OrganizationRequest request;
        }

        public void ImportSolution(string path, string solutionUniqueName, bool isManaged, ref bool anyError, int waitTimeout = 600)
        {
            if (this.CurrentOrgContext.MajorVersion >= 8)
            {
                //CRM2016 and above
                ImportSolutionCurrent(path, solutionUniqueName, isManaged, ref anyError, waitTimeout);
            }
            else
            {
                //CRM2015 and lower
                ImportSolutionLegacy(path, solutionUniqueName, isManaged, ref anyError);
            }
        }

        /// <summary>
        /// Import Customization into CRM using ExecuteAsyncRequest
        /// </summary>
        /// <param name="path"></param>
        /// <param name="solutionUniqueName"></param>
        /// <param name="isManaged"></param>
        /// <param name="anyError"></param>
        private void ImportSolutionCurrent(string path, string solutionUniqueName, bool isManaged, ref bool anyError, int waitTimeout = 600)
        {
            try
            {
                //by default timeouts in 10 minutes if not value passed in
                int asyncWaitTimeoutInSeconds = waitTimeout;
                //check every 20 seconds
                int asyncCheckIntervalInSeconds = 20;
                byte[] fileBytes = File.ReadAllBytes(path);
                ImportSolutionRequest req = new ImportSolutionRequest()
                {
                    CustomizationFile = fileBytes,
                    OverwriteUnmanagedCustomizations = true,
                    PublishWorkflows = true,
                };

                if (CheckImportPriv())
                {
                    throw new Exception("User does not have privilege to Import Customizations, prvImportCustomization");
                }
                else
                {
                    Logger.LogInfo("  Start to import customizations...");

                    ExecuteAsyncRequest asyncReq = new ExecuteAsyncRequest()
                    {
                        Request = req,
                    };
                    ExecuteAsyncResponse resp = (ExecuteAsyncResponse)_service.Execute(asyncReq);
                    if (resp != null)
                    {
                        bool importCompleted = false;
                        Guid asyncJobId = asyncJobId = resp.AsyncJobId;
                        DateTime timeout = DateTime.Now.AddSeconds(asyncWaitTimeoutInSeconds);
                        int waitingCount = 0;
                        int checkCount = 0;
                        while (timeout >= DateTime.Now && importCompleted == false)
                        {
                            checkCount++;
                            AsyncOperation asyncOperation = _service.Retrieve(AsyncOperation.EntityLogicalName, asyncJobId, new ColumnSet(new string[] { "asyncoperationid", "statecode", "statuscode", "startedon", "message" })).ToEntity<AsyncOperation>();
                            switch (asyncOperation.StatusCode.Value)
                            {
                                case (int)asyncoperation_statuscode.Succeeded:
                                    //successful, do nothing
                                    importCompleted = true;
                                    break;

                                case (int)asyncoperation_statuscode.Waiting:
                                    Logger.LogInfo("  Import Customization in Queued (suspended system jobs)...");
                                    waitingCount++;

                                    //for every 2 wait count, we will try to resume it
                                    Math.DivRem(checkCount, 2, out int waitRemainder);
                                    if (waitRemainder == 0)
                                    {
                                        //update the job to resume, set the state to Ready and Status to WaitingForResources
                                        asyncOperation.StateCode = AsyncOperationState.Ready;
                                        asyncOperation.StatusCode = new OptionSetValue((int)asyncoperation_statuscode.WaitingForResources);
                                        _service.Update(asyncOperation);

                                        Logger.LogInfo("  Resume the Import Customization that is in Queued (suspended system jobs)...");
                                    }
                                    else
                                    {
                                        Logger.LogVerbose("  Waiting to resume Import Customization that is in Queued (suspended system jobs)...");
                                    }

                                    Thread.Sleep(asyncCheckIntervalInSeconds * 1000);
                                    break;

                                //pausing //cancelling //failed //cancelled
                                case (int)asyncoperation_statuscode.Pausing:
                                case (int)asyncoperation_statuscode.Canceling:
                                case (int)asyncoperation_statuscode.Canceled:
                                case (int)asyncoperation_statuscode.Failed:
                                    throw new Exception(string.Format("Solution Import Failed: Status Code={0};{1}{2}", asyncOperation.StatusCode.Value, Environment.NewLine, asyncOperation.Message));

                                default:
                                    //sleep for an interval before next check
                                    int remainder = 0;
                                    //for every 5 check count, we will write info log once
                                    Math.DivRem(checkCount, 5, out remainder);
                                    if (remainder == 0)
                                        Logger.LogInfo("  Importing Customization...");
                                    else
                                        Logger.LogVerbose("  Importing Customization...");
                                    Thread.Sleep(asyncCheckIntervalInSeconds * 1000);
                                    break;
                            }
                        }

                        //check if it timeout
                        if (timeout <= DateTime.Now)
                            throw new Exception(string.Format("Import Timeout: Import took longer than {0} seconds", asyncWaitTimeoutInSeconds));

                        //check if it is successful
                        if (importCompleted)
                            Logger.LogInfo("  Import Customizations completed successfully...");
                        else
                            Logger.LogWarning("  Import Customizations completed with unknown status...");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo("Import Customization Error...", ex.Message);
                Logger.LogException(ex);
                anyError = true;
            }
        }

        static bool errorInSolutionImportBackProcess = false;
        /// <summary>
        /// This method imports the solution
        /// </summary>
        /// <param name="path">path</param>
        /// <param name="solutionUniqueName">unique name</param>
        /// <param name="isManaged">ismanaged</param>
        private void ImportSolutionLegacy(string path, string solutionUniqueName, bool isManaged, ref bool anyError)
        {
            try
            {
                ImportSolutionRequest impSolReq = new ImportSolutionRequest();
                errorInSolutionImportBackProcess = false;

                byte[] fileBytes = File.ReadAllBytes(path);
                impSolReq.CustomizationFile = fileBytes;
                impSolReq.OverwriteUnmanagedCustomizations = true;
                impSolReq.PublishWorkflows = true;
                impSolReq.ImportJobId = Guid.NewGuid();
                if (CheckImportPriv())
                {
                    throw new Exception("User does not have privilege to Import Customizations, prvImportCustomization");
                }
                else
                {
                    Logger.LogInfo("For large solutions it might take some time to check the import status of the solution, as system will be busy.");

                    //TODO: needs to be upgraded to use ExecuteAsync
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += worker_DoWork;
                    BackGroundWorkerAttributesObject attributes = new BackGroundWorkerAttributesObject();
                    attributes.service = _service;
                    attributes.request = impSolReq;

                    Logger.LogInfo("  Start to import customizations...");
                    worker.RunWorkerAsync(attributes);
                    TrackImport(impSolReq.ImportJobId, path, ref anyError);
                    if (errorInSolutionImportBackProcess)
                        anyError = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogInfo("Unexpected error while reading solution file or calling import", ex);
                Logger.LogException(ex);
                anyError = true;
            }
        }

        /// <summary>
        /// Checks for the prvImportCustomization of the user
        /// </summary>
        /// <returns></returns>
        bool CheckImportPriv()
        {
            bool userLacksPrivilege = false;

            Entity prvImportCustomizationEntitiy = RetrieveEntityByAttribute("privilege", new string[] { "privilegeid" }, new string[] { "name" }, new object[] { "prvImportCustomization" });
            RetrieveUserPrivilegesRequest retrieveUsrPrivilegesRequest = new RetrieveUserPrivilegesRequest();
            retrieveUsrPrivilegesRequest.UserId = CurrentOrgContext.CurrentUserId;

            RetrieveUserPrivilegesResponse retrieveUsrPrivilegesResponse = (RetrieveUserPrivilegesResponse)_service.Execute(retrieveUsrPrivilegesRequest);

            RolePrivilege userImportCustomizationPriv = retrieveUsrPrivilegesResponse.RolePrivileges.
                                                           ToList<RolePrivilege>().Find(delegate(RolePrivilege importPriv)
                                                           {
                                                               return importPriv.PrivilegeId == prvImportCustomizationEntitiy.Id
                                                                  && importPriv.BusinessUnitId == CurrentOrgContext.CurrentUserBusinessUnitId;
                                                           });
            if (userImportCustomizationPriv == null)
                userLacksPrivilege = true;
            return userLacksPrivilege;
        }

        private Entity RetrieveEntityByAttribute(string EntityName, string[] RetrieveColumns, string[] FilterByColumns, object[] FilterByColumnsValue)
        {
            QueryByAttribute Query = null;
            RetrieveMultipleRequest Request = null;
            RetrieveMultipleResponse Response = null;
            EntityCollection RtrnEntyCollection = null;
            Entity RtrnEntity = null;


            try
            {
                if (EntityName != string.Empty || RetrieveColumns != null || FilterByColumnsValue != null || FilterByColumnsValue != null)
                {
                    Query = new QueryByAttribute();
                    Request = new RetrieveMultipleRequest();
                    Query.EntityName = EntityName;
                    Query.ColumnSet = new ColumnSet(RetrieveColumns);
                    Query.Attributes.AddRange(FilterByColumns);
                    Query.Values.AddRange(FilterByColumnsValue);
                    Request.Query = Query;
                    Response = (RetrieveMultipleResponse)_service.Execute(Request);
                    RtrnEntyCollection = Response.EntityCollection;
                    if (RtrnEntyCollection != null && RtrnEntyCollection.Entities.Count > 0)
                        RtrnEntity = (Entity)RtrnEntyCollection.Entities[0];
                }
            }
            catch (System.Web.Services.Protocols.SoapException _sopEx)
            {
                throw new Exception(_sopEx.Detail.InnerText);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return RtrnEntity;
        }


        static void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackGroundWorkerAttributesObject arguments = (BackGroundWorkerAttributesObject)e.Argument;

                ImportSolutionResponse response = (ImportSolutionResponse)arguments.service.Execute(arguments.request);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                if (ex.Message != null && ex.Message.Contains("The solution file is invalid"))
                {
                    Logger.LogError(ex.Message);
                    errorInSolutionImportBackProcess = true;
                }
            }
            catch (CommunicationException cex)
            {

                //     Logger.LogInfo("A CommunicationException occurred during import. The error \"An error occurred while receiving the HTTP response\" is expected, others need to be investigated.", ex);
                //Logger.LogInfo("  Checking Status of Import");
                Logger.LogError(cex.Message);
                Logger.LogError(cex.StackTrace);
                errorInSolutionImportBackProcess = true;
            }
            catch (Exception aex)
            {
                Logger.LogError(aex.Message);
                Logger.LogError(aex.StackTrace);
                errorInSolutionImportBackProcess = true;
            }
        }


        /// <summary>
        /// Tracks the Solution Import
        /// </summary>
        /// <param name="importJobId">Import Job Id</param>
        private void TrackImport(Guid importJobId, string path, ref bool anyError)
        {
            string progress;
            XmlDocument doc;
            bool processed = ProgressCompleted(importJobId, out progress, out doc);
            progress = (progress == string.Empty) ? "0" : progress;


            while (processed == false && errorInSolutionImportBackProcess != true)
            {
                if (progress.Length > 5)
                    progress = progress.Substring(0, 5);
                Logger.LogInfo("  Import is in Progress, {0} completed", progress);

                //wait for 5 seconds before another check
                System.Threading.Thread.Sleep(5000);
                string oldProgress = progress;
                processed = ProgressCompleted(importJobId, out progress, out doc);

                // leave progress as is when no new progress can be retrieved
                if (progress == string.Empty)
                {
                    progress = oldProgress;
                }

            }
            if (!errorInSolutionImportBackProcess)
            {
                RetrieveFormattedImportJobResultsRequest importLogRequest = new RetrieveFormattedImportJobResultsRequest()
                   {
                       ImportJobId = importJobId
                   };
                RetrieveFormattedImportJobResultsResponse importLogResponse = (RetrieveFormattedImportJobResultsResponse)_service.Execute(importLogRequest);
                DateTime time = DateTime.Now;
                string format = "yyyyMMddHHmm";
                string logFile = Path.Combine(Logger.BackupDirectory, Path.GetFileNameWithoutExtension(path) + "_" + time.ToString(format) + ".xml");

                File.WriteAllText(logFile, importLogResponse.FormattedResults);
                ImportSolutionStatus status = ParseFormatedXml(importLogResponse.FormattedResults);

                String ImportedSolutionName = doc.SelectSingleNode("//solutionManifest/UniqueName").InnerText;
                String SolutionImportResult = doc.SelectSingleNode("//solutionManifest/result/@result").Value;

                Logger.LogInfo("  Report from the ImportJob data");
                Logger.LogInfo("  Solution Unique Name: {0}", ImportedSolutionName);

                switch (status)
                {
                    case ImportSolutionStatus.success:
                        Logger.LogInfo("  Solution Import Result: {0}", "Success");
                        break;
                    case ImportSolutionStatus.warning:
                        Logger.LogInfo("  Solution Import Result: {0}", "Warning");
                        break;
                    case ImportSolutionStatus.error:
                        Logger.LogInfo("  Solution Import Result: {0}", "Failure");
                        anyError = true;
                        break;

                    case ImportSolutionStatus.couldnotparse:
                        {
                            if (processed == true && progress != "100.00")
                            {
                                Logger.LogInfo("  Solution Import Result: {0}", "Failure");
                                anyError = true;
                            }
                            else
                                Logger.LogInfo("  Solution Import Result: {0}", SolutionImportResult);
                            break;
                        }
                    default:
                        break;
                }
                Logger.LogInfo("  Solution Import Log file Available at {0}", logFile);
            }
        }
        private static ImportSolutionStatus ParseFormatedXml(string xml)
        {
            bool isError = false;
            bool isWarning = false;


            // load xml into XDocumemt object
            XDocument doc = XDocument.Parse(xml);

            //Query for Sheet1 from selected Worksheet 
            var worksheet = doc.Descendants().Where(x => (x.Name.LocalName == "Worksheet" && x.FirstAttribute.Value == "Components"));
            //Query for Filled rows 
            var rows = worksheet.Descendants().Where(x => x.Name.LocalName == "Row");

            int rowCount = 0;
            ImportSolutionStatus retVal = ImportSolutionStatus.success;
            foreach (var row in rows)
            {
                //validating the header row, I am assuming very 
                // First row will be the HEADER 
                if (rowCount == 0)
                {
                    //Read first row first cell value 
                    if (string.Compare(row.Descendants().Elements().ElementAt(7).Value, "Status", StringComparison.CurrentCultureIgnoreCase) != 0
                                     && string.Compare(row.Descendants().Elements().ElementAt(9).Value, "ErrorText", StringComparison.CurrentCultureIgnoreCase) != 0)
                    {
                        retVal = ImportSolutionStatus.couldnotparse;
                        break;
                    }
                }
                else
                {
                    //validating records
                    var cell = row.Descendants().Where(y => y.Name.LocalName == "Cell");
                    if (!string.IsNullOrEmpty(cell.ElementAt(7).Value) && cell.ElementAt(7).Value == "Failure")
                        isError = true;
                    else if (!string.IsNullOrEmpty(cell.ElementAt(9).Value))
                        isWarning = true;
                }
                rowCount++;
            }



            if (isError)
                retVal = ImportSolutionStatus.error;
            else if (isWarning)
                retVal = ImportSolutionStatus.warning;

            return retVal;
        }

        private bool ProgressCompleted(Guid importJobId, out string progress, out XmlDocument doc)
        {
            try
            {
                Entity e = _service.Retrieve(ImportJob.EntityLogicalName, importJobId, new ColumnSet(true));

                doc = new System.Xml.XmlDocument();
                doc.LoadXml(e.Attributes["data"] as string);
                progress = doc.SelectSingleNode("/importexportxml/@progress").Value;
                string processed = doc.SelectSingleNode("/importexportxml/@processed").Value;

                if (processed == "true")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                // can happen during last phase of import when server is too busy to respond
                // Logger.LogInfo("  Checking the status of Import  ", ex.Message);
                progress = string.Empty;
                doc = null;
                return false;
            }
        }
        /// <summary>
        /// Pbulishes All customizations and calls PublishAllWorkflows
        /// </summary>
        /// <param name="publishWorkflows"></param>
        /// <param name="anyError"></param>
        public void PublishCustomizations(bool publishWorkflows, ref bool anyError, string[] workflowList = null)
        {
            if (!anyError)
            {
                try
                {
                    Logger.LogInfo("  Publishing customizations...");
                    PublishAllXmlRequest publishRequest = new PublishAllXmlRequest();

                    _service.Execute(publishRequest);
                    Logger.LogInfo("  Published customizations...");
                }
                catch (SoapException soapEx)
                {
                    anyError = true;
                    Logger.LogError("  Error publishing customizations. Soap Error Detail = {0}", soapEx.Detail.InnerXml);
                }
                catch (Exception ex)
                {
                    anyError = true;
                    Logger.LogError("  Error publishing customizations. Error Message = {0}", ex.Message);
                }

                if (!anyError && publishWorkflows)
                {
                    try
                    {
                        Logger.LogInfo("  Publishing Workflows...");
                        PublishAllWorkflows(workflowList);
                    }
                    catch (Exception ex)
                    {
                        anyError = true;
                        Logger.LogError("  Error Publishing Workflows. Error Message = {0}", ex.Message);
                    }
                }
            }
        }
        #endregion

        #region Data Import Methods
        /// <summary>
        /// Imports csv files into crm
        /// </summary>
        /// <param name="fileContents"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        internal bool ImportData(string fileContents, DataImportElement element1, StringDictionary replaceTokens)
        {
            bool success = false;
            string domainUserName = IOUtils.ReplaceStringTokens(element1.DomainUserName, replaceTokens);
            string FileName = IOUtils.ReplaceStringTokens(element1.FileName, replaceTokens);
            string EntityName = IOUtils.ReplaceStringTokens(element1.EntityName, replaceTokens);
            string DataMap = IOUtils.ReplaceStringTokens(element1.DataMap, replaceTokens);
            string owningTeam = IOUtils.ReplaceStringTokens(element1.OwningTeam, replaceTokens);

            Logger.LogInfo("  Importing data from '{0}' for entity '{1}'...", Path.GetFullPath(FileName), EntityName);

            Entity imp = new Entity("import");
            imp.Attributes.Add("modecode", new OptionSetValue((int)ImportModeCode.Create));
            // ImportModeCode 0 = "Create"
            imp.Attributes.Add("name", String.Format("{0} {{{1}}}", Path.GetFileName(FileName), EntityName));
            imp.Attributes.Add("sendnotification", false);
            Guid importId = _service.Create(imp);

            Entity file = new Entity("importfile");
            using (StringReader reader = new StringReader(fileContents))
            {
                file.Attributes.Add("headerrow", reader.ReadLine());
                file.Attributes.Add("content",
                                    String.Format("{0}{1}{2}", file.Attributes["headerrow"], Environment.NewLine,
                                                  reader.ReadToEnd()));
            }
            file.Attributes.Add("name", String.Format("{0} {{{1}}}", Path.GetFileName(FileName), EntityName));
            file.Attributes.Add("isfirstrowheader", true);

            if (!String.IsNullOrEmpty(domainUserName))
            {


                Logger.LogInfo("  Assigning imported records to '{0}'", domainUserName);
                Guid userId = GetUserForDomain(domainUserName);

                if (userId == Guid.Empty)
                {
                    Logger.LogError("  Failed to find user '{0}' in the system - unable to assign records",
                                    domainUserName);
                    return false;
                }
                file.Attributes.Add("recordsownerid", new EntityReference("systemuser", userId));
            }
            else if (!String.IsNullOrEmpty(owningTeam))
            {

                Logger.LogInfo("  Assigning imported records to Team '{0}'", owningTeam);
                Guid teamId = GetTeam(owningTeam);

                if (teamId == Guid.Empty)
                {
                    Logger.LogError("  Failed to find Team '{0}' in the system - unable to assign records",
                                    owningTeam);
                    return false;
                }
                file.Attributes.Add("recordsownerid", new EntityReference("team", teamId));

            }
            else
            {
                file.Attributes.Add("recordsownerid", new EntityReference("systemuser", CurrentOrgContext.CurrentUserId));
            }
            file.Attributes.Add("importid", new EntityReference("import", importId));
            file.Attributes.Add("source", Path.GetFileName(FileName));
            file.Attributes.Add("sourceentityname", Path.GetFileNameWithoutExtension(FileName));
            file.Attributes.Add("fielddelimitercode", new OptionSetValue((int)element1.FieldDelimiter));
            file.Attributes.Add("targetentityname", EntityName);
            file.Attributes.Add("datadelimitercode", new OptionSetValue((int)element1.DataDelimiter));
            file.Attributes.Add("size", file.Attributes["content"].ToString().Length.ToString());
            file.Attributes.Add("enableduplicatedetection", element1.DetectDuplicates);
            file.Attributes.Add("processcode", new OptionSetValue(1)); // Process code 1 = "Process"
            if (string.IsNullOrEmpty(DataMap))
                file.Attributes.Add("usesystemmap", true);
            else
            {
                Guid? mapId = GetImportMap(DataMap);
                if (mapId.HasValue && mapId.Value != Guid.Empty)
                {
                    file.Attributes.Add("importmapid", new EntityReference("importmap", mapId.Value));
                    file.Attributes.Add("usesystemmap", false);
                }
                else
                    throw new Exception("DataMap not found with name : " + DataMap);
            }
            Guid fileId = _service.Create(file);

            ParseImportRequest parseRequest = new ParseImportRequest();
            parseRequest.ImportId = importId;
            _service.Execute(parseRequest);

            TransformImportRequest transRequest = new TransformImportRequest();
            transRequest.ImportId = importId;
            _service.Execute(transRequest);

            ImportRecordsImportRequest importRequest = new ImportRecordsImportRequest();
            importRequest.ImportId = importId;
            ImportRecordsImportResponse response = (ImportRecordsImportResponse)_service.Execute(importRequest);

            if (element1.WaitForCompletion)
            {
                Logger.LogInfo("  Waiting for the import to complete (timeout is {0} seconds)...",
                               element1.WaitTimeout);
                ColumnSet cols = new ColumnSet();
                cols.AddColumns("successcount", "failurecount", "totalcount", "statuscode");
                for (int i = 0; i < element1.WaitTimeout; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    Entity op = _service.Retrieve("importfile", fileId, cols);

                    if (op != null &&
                        (((OptionSetValue)op.Attributes["statuscode"]).Value == (int)import_statuscode.Completed))
                    //Potentially we need to use OptionSetValue here instead of int
                    {
                        if (op.Attributes["successcount"] == op.Attributes["totalcount"])
                        {
                            Logger.LogInfo("  Import Complete.  {0} record(s) imported sucessfully.",
                                           op.Attributes["totalcount"]);
                        }
                        else
                        {
                            Logger.LogInfo("  Import Complete.  Succeeded: {0}.  Failed: {1}.  Total: {2}",
                                           op.Attributes["successcount"], op.Attributes["failurecount"],
                                           op.Attributes["totalcount"]);
                        }
                        success = true;
                        break;
                    }
                    if (((OptionSetValue)op.Attributes["statuscode"]).Value == (int)import_statuscode.Failed)
                    {
                        Logger.LogError(
                            "  Import Failed.  Check Settings -> Data Management -> Imports in CRM for details");
                        break;
                    }
                }
            }
            else
            {
                success = true;
            }

            return success;
        }
        /// <summary>
        /// Gets the Team Guid based on Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Guid GetTeam(string name)
        {
            Guid result = Guid.Empty;

            if (!String.IsNullOrEmpty(name))
            {
                ColumnSet cols = new ColumnSet();
                cols.AddColumn("teamid");

                QueryByAttribute query = new QueryByAttribute();
                query.EntityName = "team";
                query.Attributes.Add("name");
                query.Values.Add(name);

                try
                {
                    EntityCollection entities = _service.RetrieveMultiple(query);

                    if (entities.Entities.Count > 0)
                    {
                        result = (Guid)entities.Entities[0].Attributes["teamid"];
                    }
                }
                catch (Exception)
                {
                    // Do Nothing - return empty GUID
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the User guid based on name
        /// </summary>
        /// <param name="domainName"></param>
        /// <returns></returns>
        public Guid GetUserForDomain(string domainName)
        {
            Guid result = Guid.Empty;

            if (!String.IsNullOrEmpty(domainName))
            {
                ColumnSet cols = new ColumnSet();
                cols.AddColumn("systemuserid");

                QueryByAttribute query = new QueryByAttribute();
                query.EntityName = "systemuser";
                query.Attributes.Add("domainname");
                query.Values.Add(domainName);

                try
                {
                    EntityCollection entities = _service.RetrieveMultiple(query);

                    if (entities.Entities.Count > 0)
                    {
                        result = (Guid)entities.Entities[0].Attributes["systemuserid"];
                    }
                }
                catch (Exception)
                {
                    // Do Nothing - return empty GUID
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the User guid based on name
        /// </summary>
        /// <param name="domainName"></param>
        /// <returns></returns>
        public Guid? GetImportMap(string importName)
        {
            Guid? result = null;

            if (!String.IsNullOrEmpty(importName))
            {
                ColumnSet cols = new ColumnSet();
                cols.AddColumn("importmapid");

                QueryByAttribute query = new QueryByAttribute();
                query.EntityName = "importmap";
                query.Attributes.Add("name");
                query.Values.Add(importName);

                try
                {
                    EntityCollection entities = _service.RetrieveMultiple(query);

                    if (entities.Entities.Count > 0)
                    {
                        result = (Guid)entities.Entities[0].Attributes["importmapid"];
                    }
                }
                catch (Exception)
                {
                    // Do Nothing - return empty GUID
                }
            }
            return result;
        }

        #endregion Data Import Methods

        #region Data Import Methods (ManyToMany)
        internal string ManyToManyRelationshipName { get; set; }
        internal string FirstEntity { get; set; }
        internal string SecondEntity { get; set; }
        internal string FirstEntityKeyField { get; set; }
        internal string SecondEntityKeyField { get; set; }
        private List<KeyValuePair<Guid, Guid>> _import;
        private int _count;

        internal bool ImportDataManyToMany(string fileContents, DataImportElement element1, StringDictionary replaceTokens)
        {
            FirstEntity = SecondEntity = FirstEntityKeyField = SecondEntityKeyField = string.Empty;
            _count = 0;

            string csvFieldDelimiter;
            switch (element1.FieldDelimiter)
            {
                case FieldDelimiter.Colon:
                    csvFieldDelimiter = ":";
                    break;
                case FieldDelimiter.Comma:
                    csvFieldDelimiter = ",";
                    break;
                case FieldDelimiter.Semicolon:
                    csvFieldDelimiter = ";";
                    break;
                case FieldDelimiter.Tab:
                    csvFieldDelimiter = "    ";
                    break;
                default:
                    csvFieldDelimiter = ",";
                    break;
            }

            string csvDataDelimiter;
            switch (element1.DataDelimiter)
            {
                case DataDelimiter.DoubleQuote:
                    csvDataDelimiter = "\"";
                    break;
                case DataDelimiter.SingleQuote:
                    csvDataDelimiter = "'";
                    break;
                case DataDelimiter.None:
                    csvDataDelimiter = "";
                    break;
                default:
                    csvDataDelimiter = "\"";
                    break;
            }

            string fileName = IOUtils.ReplaceStringTokens(element1.FileName, replaceTokens);
            ManyToManyRelationshipName = IOUtils.ReplaceStringTokens(element1.EntityName, replaceTokens);

            Logger.LogInfo("  Importing data from '{0}' for entity '{1}'...", Path.GetFullPath(fileName), ManyToManyRelationshipName);

            var manyToManyRelationshipMetadata = Metadata.GetManyToManyRelationshipMetadata(this, ManyToManyRelationshipName);
            if (manyToManyRelationshipMetadata != null)
            {
                FirstEntity = manyToManyRelationshipMetadata.Entity1LogicalName;
                SecondEntity = manyToManyRelationshipMetadata.Entity2LogicalName;
            }
            else
            {
                Logger.LogInfo("Error: Metadata not found for entity '" + ManyToManyRelationshipName + "'.");
                return false;
            }

            Logger.LogInfo("Reading CSV and getting GUIDs.");

            ImportCsv(fileName, csvFieldDelimiter, csvDataDelimiter);

            return true;
        }

        //Create the specific relation
        private void AddEntityRelation(Guid firstEntityId, Guid secondEntityId)
        {
            ++_count;

            if (firstEntityId == Guid.Empty || secondEntityId == Guid.Empty)
            {
                if (firstEntityId == Guid.Empty)
                {
                    Logger.LogInfo("Error: Can not resolve value of First column at row " + (_count).ToString(CultureInfo.InvariantCulture) + " in file.");
                }
                if (secondEntityId == Guid.Empty)
                {
                    Logger.LogInfo("Error: Can not resolve value of Second column at row " + (_count).ToString(CultureInfo.InvariantCulture) + " in file.");
                }
                return;
            }
            
            var request = new AssociateRequest
            {
                Target = new EntityReference(FirstEntity, firstEntityId),
                RelatedEntities = new EntityReferenceCollection { new EntityReference(SecondEntity, secondEntityId) },
                Relationship = new Relationship(ManyToManyRelationshipName)
            };

            //execute the request
            _service.Execute(request);

            Logger.LogInfo((_count).ToString(CultureInfo.InvariantCulture) + "/" + _import.Count.ToString(CultureInfo.InvariantCulture) + " imported.");
        }

        //Get the Guid for an entity object
        private Guid GetGuidForObject(string entityName, string keyField, string value)
        {
            using (var osc = new OrganizationServiceContext(_service))
            {
                var entity = from b in osc.CreateQuery(entityName)
                             where b.GetAttributeValue<string>(keyField) == value
                             select b;
                if (Enumerable.Count(entity) > 0)
                {
                    return entity.First().Id;
                }
                return Guid.Empty;
            }
        }

        //Read the csv file and start the import
        internal void ImportCsv(string fileName, string csvFieldDelimiter, string csvDataDelimiter)
        {
            _import = new List<KeyValuePair<Guid, Guid>>();

            //Open CSV and import Key-Value-Pairs
            var reader = new StreamReader(fileName, Encoding.GetEncoding("ISO-8859-1"));
            string line;
            int row = 0;
            while ((line = reader.ReadLine()) != null)
            {
                row++;

                var itemArray = line.Trim().Split(new string[] { csvFieldDelimiter }, StringSplitOptions.None).ToList<string>();

                if (itemArray.Count == 2)
                {
                    if (row == 1)
                    {
                        //Read Header column of CSV file
                        FirstEntityKeyField = itemArray[0].Replace(csvDataDelimiter, "").Trim();
                        SecondEntityKeyField = itemArray[1].Replace(csvDataDelimiter, "").Trim();
                    }
                    else
                    {
                        //Read data rows
                        _import.Add(new KeyValuePair<Guid, Guid>(
                            GetGuidForObject(FirstEntity, FirstEntityKeyField, itemArray[0].Replace(csvDataDelimiter, "").Trim())
                            , GetGuidForObject(SecondEntity, SecondEntityKeyField, itemArray[1].Replace(csvDataDelimiter, "").Trim())));    
                    }
                }
            }
            reader.Close();
            _import.ForEach(x => AddEntityRelation(x.Key, x.Value));

            Logger.LogInfo(_import.Count.ToString(CultureInfo.InvariantCulture) + "relation(s) imported.");
        }

        #endregion

        #region Data Export Methods

        public void ExportCrmData(string EntityName, string filename)
        {

            RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = EntityName
            };
            RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)Service.Execute(retrieveEntityRequest);
            Dictionary<string, string> entityAttributes = new Dictionary<string, string>();

            string[] notIncludeList = new string[] { "modifiedonbehalfby", "createdonbehalfby", "utcconversiontimezonecode", "timezoneruleversionnumber", "overriddencreatedon" ,
            "modifiedon","yomifullname","owningbusinessunit","isprivate","exchangerate","createdon","modifiedby","yomilastname","participatesinworkflow","owningteam","owninguser","yomimiddlename",
            "yomifirstname","createdby","importsequencenumber","versionnumber","yomicompanyname"};

            foreach (AttributeMetadata att in retrieveEntityResponse.EntityMetadata.Attributes)
            {

                if (att.DisplayName.UserLocalizedLabel != null && att.AttributeType != AttributeTypeCode.Virtual && !notIncludeList.Contains(att.LogicalName))
                    entityAttributes.Add(att.LogicalName, att.DisplayName.UserLocalizedLabel.Label);
            }

            int fetchCount = 5000;
            int pageNumber = 1;

            QueryExpression pagequery = new QueryExpression();
            pagequery.EntityName = EntityName;
            pagequery.ColumnSet = new ColumnSet(true);
            pagequery.PageInfo = new PagingInfo();
            pagequery.PageInfo.Count = fetchCount;
            pagequery.PageInfo.PageNumber = pageNumber;
            pagequery.PageInfo.PagingCookie = null;

            Logger.LogInfo("Data Export to file {0}", filename);

            while (true)
            {
                // Retrieve the page.
                EntityCollection results = Service.RetrieveMultiple(pagequery);
                if (results.Entities != null)
                {
                    if (pagequery.PageInfo.PageNumber == 1)
                        ExportData(filename, results, true, entityAttributes);
                    else
                        ExportData(filename, results, false, entityAttributes);
                }

                // Check for more records, if it returns true.
                if (results.MoreRecords)
                {
                    pagequery.PageInfo.PageNumber++;
                    pagequery.PageInfo.PagingCookie = results.PagingCookie;
                }
                else
                {
                    // If no more records are in the result nodes, exit the loop.
                    break;
                }
            }
            Logger.LogInfo("Data Export done");

        }

        private void ExportData(string file, EntityCollection data, bool createHeader, Dictionary<string, string> headerColumns)
        {
            FileStream fs;
            StreamWriter sw;
            string fieldDelimiter = ",";
            string dataDelimiter = "\"";

            if (data != null)
            {


                fs = File.Open(file, FileMode.Append);
                sw = new StreamWriter(fs);
                String st = String.Empty;
                int i = 0;
                int lenght = headerColumns.Count;

                //Only write columns the first time when createHeader is true
                if (createHeader)
                {
                    foreach (string key in headerColumns.Keys)
                    {
                        i++;
                        if (headerColumns[key].Contains(fieldDelimiter))
                            st = st + dataDelimiter + headerColumns[key] + dataDelimiter;
                        else
                            st = st + headerColumns[key];

                        if (i != lenght)
                            st += fieldDelimiter.ToString();
                    }

                }

                if (st != String.Empty)
                    sw.WriteLine((string)st);
                //Write the rows
                string fieldValue = string.Empty;

                foreach (Entity r in data.Entities)
                {
                    st = string.Empty;
                    i = 0;
                    foreach (string key in headerColumns.Keys)
                    {
                        i++;

                        fieldValue = string.Empty;
                        if (r.Attributes.Contains(key))
                        {
                            if (r.Attributes[key].GetType().FullName == "Microsoft.Xrm.Sdk.EntityReference")
                            {
                                if (!string.IsNullOrEmpty(((EntityReference)r.Attributes[key]).Name))
                                    fieldValue = ((EntityReference)r.Attributes[key]).Name;
                            }
                            else if (r.Attributes[key].GetType().FullName == "Microsoft.Xrm.Sdk.OptionSetValue")
                            {
                                if (((OptionSetValue)r.Attributes[key]) != null)
                                {
                                    int fieldOption = ((OptionSetValue)r.Attributes[key]).Value;
                                    fieldValue = OptionSetTextFromValue(r.LogicalName, key, fieldOption);
                                }
                            }
                            else if (r.Attributes[key].GetType().FullName == "System.Boolean")
                            {
                                if (r.Attributes[key] != null)
                                    if ((bool)r.Attributes[key])
                                        fieldValue = OptionSetTextFromValue(r.LogicalName, key, 1);
                                    else
                                        fieldValue = OptionSetTextFromValue(r.LogicalName, key, 0);

                            }
                            else
                            {
                                fieldValue = r.Attributes[key].ToString();
                            }


                            if (fieldValue.Contains(dataDelimiter))
                                fieldValue = fieldValue.Replace(dataDelimiter, "\"\"");

                            if (fieldValue.Contains(fieldDelimiter))
                                st += dataDelimiter + fieldValue + dataDelimiter;
                            else
                                st += fieldValue;
                        }
                        else
                            st = st + fieldValue;

                        if (i != lenght)
                            st = st + fieldDelimiter.ToString();
                    }

                    sw.WriteLine((string)st);
                }
                sw.Flush();
                fs.Flush();
                sw.Close();
                fs.Close();
            }
        }

        private string OptionSetTextFromValue(string EntityName, string attribute, int value)
        {
            string retVal = string.Empty;
            AttributeMetadata metaData;
            if (!cacheAttribute.ContainsKey(EntityName + ":" + attribute))
            {
                RetrieveAttributeRequest req = new RetrieveAttributeRequest();
                req.EntityLogicalName = EntityName;
                req.LogicalName = attribute;

                RetrieveAttributeResponse res = (RetrieveAttributeResponse)Service.Execute(req);
                metaData = res.AttributeMetadata;
                cacheAttribute.Add(EntityName + ":" + attribute, res.AttributeMetadata);
            }
            else
                metaData = cacheAttribute[EntityName + ":" + attribute];

            if (metaData.GetType().Name == "StateAttributeMetadata")
            {
                StateAttributeMetadata md = (StateAttributeMetadata)metaData;
                foreach (var stateAttrOption in md.OptionSet.Options)
                {
                    if (stateAttrOption.Value.Value == value)
                    {
                        retVal = stateAttrOption.Label.UserLocalizedLabel.Label;
                        break;
                    }
                }
            }
            else if (metaData.GetType().Name == "StatusAttributeMetadata")
            {
                StatusAttributeMetadata md = (StatusAttributeMetadata)metaData;
                foreach (var statusAttrOption in md.OptionSet.Options)
                {
                    if (statusAttrOption.Value.Value == value)
                    {
                        retVal = statusAttrOption.Label.UserLocalizedLabel.Label;
                        break;
                    }
                }
            }
            else if (metaData.GetType().Name == "BooleanAttributeMetadata")
            {
                BooleanAttributeMetadata md = (BooleanAttributeMetadata)metaData;

                if (value == 0)
                    retVal = md.OptionSet.FalseOption.Label.UserLocalizedLabel.Label;
                else
                    retVal = md.OptionSet.TrueOption.Label.UserLocalizedLabel.Label;
            }
            else
            {
                PicklistAttributeMetadata md = (PicklistAttributeMetadata)metaData;

                foreach (OptionMetadata oMD in md.OptionSet.Options.ToArray())
                {
                    if (oMD.Value.Value == value)
                    {
                        retVal = oMD.Label.UserLocalizedLabel.Label;
                        break;
                    }
                }
            }
            return retVal;
        }
        #endregion

        #region Data Map Methods
        /// <summary>
        /// Export the mapping that we created
        /// </summary>
        /// <param name="mapName">map name</param>
        /// <param name="fileName">file name</param>
        /// <param name="errorMessage">error message</param>
        internal void RetrieveMappingXml(string mapName, string fileName, ref string errorMessage)
        {
            if (string.IsNullOrEmpty(mapName))
            {
                errorMessage = "  Map Name is Empty";
                return;
            }
            try
            {
                Guid? _importMapId = GetImportMap(mapName);
                if (_importMapId == null || (!_importMapId.HasValue && _importMapId.Value != Guid.Empty))
                {
                    Logger.LogWarning("  Data Map " + mapName + " Not found");
                    return;
                }
                // Retrieve the xml for the mapping 
                var exportRequest = new ExportMappingsImportMapRequest
                {

                    ImportMapId = _importMapId.Value,
                    ExportIds = true,
                };

                var exportResponse = (ExportMappingsImportMapResponse)Service.Execute(exportRequest);

                string _mappingXml = exportResponse.MappingsXml;
                File.WriteAllText(fileName, _mappingXml);
                Logger.LogInfo(String.Concat("  Import mapping exported for ", mapName));
            }
            catch (Exception ex)
            {
                errorMessage = "  Error while fetching the data map " + mapName;
                Logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Import a mapping from Xml
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="errorMessage">error message</param>
        internal void ImportMappingsByXml(string fileName, ref string errorMessage)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(fileName) && File.Exists(fileName))
                {
                    Logger.LogError("  File Not Found at " + fileName);
                    return;
                }
                string _mappingXml = File.ReadAllText(fileName);
                if (String.IsNullOrEmpty(_mappingXml))
                {
                    return;
                }
                string mapName = MappingName(_mappingXml);
                // Create the import mapping from the XML
                var request = new ImportMappingsImportMapRequest
                {
                    MappingsXml = _mappingXml,
                    ReplaceIds = true,
                };
                Logger.LogInfo("  Creating mapping \"{0}\" based on XML: {1}", mapName, fileName);
                var response = (ImportMappingsImportMapResponse)Service.Execute(request);

                Guid? _newImportMapId = response.ImportMapId;
                Logger.LogInfo("  New import \"{0}\" mapping created: ", mapName);
            }
            catch (Exception ex)
            {
                errorMessage = "  Error while importing the data map " + fileName;
                Logger.LogError(ex.Message);
            }
        }
        /// <summary>
        /// Delete Data Map by Name
        /// </summary>
        /// <param name="mapName">map name</param>
        /// <param name="errorMessage">error message</param>
        internal void DeleteDataMap(string mapName, ref string errorMessage)
        {

            if (string.IsNullOrEmpty(mapName))
            {
                errorMessage = "  Map Name is Empty";
                return;
            }
            try
            {
                Guid? _importMapId = GetImportMap(mapName);
                if (_importMapId == null || (!_importMapId.HasValue && _importMapId.Value != Guid.Empty))
                {
                    Logger.LogWarning("  Data Map " + mapName + " Not found");
                    return;
                }
                // Retrieve the xml for the mapping 
                Service.Delete(ImportMap.EntityLogicalName, _importMapId.Value);
                Logger.LogInfo("  Data Map \"{0}\" deleted", mapName);
            }
            catch (Exception ex)
            {
                errorMessage = "  Error while deleting the data map " + mapName;
                Logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Parse the XML to change the name attribute
        /// </summary>
        private string MappingName(string _mappingXml)
        {
            if (string.IsNullOrWhiteSpace(_mappingXml))
                return null;

            // Load into XElement
            var xElement = XElement.Parse(_mappingXml);

            // Get the Name attribute
            var nameAttribute = xElement.Attribute("Name");

            // Swap the name out
            if (nameAttribute != null)
            {
                return nameAttribute.Value;
            }
            return null;
        }
        #endregion
    }
}