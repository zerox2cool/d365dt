using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Configuration;
using System.Text;
using System.Collections.Specialized;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallPublisherAction : InstallAction
    {
        private readonly PublisherCollection pubCollection;
        private readonly WebServiceUtils utils;
        private int? _ParentLineNumber = null;

        public override string ActionName { get { return "Publisher"; } }
        public override InstallType ActionType { get { return InstallType.PublisherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallPublisherAction(PublisherCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils, int parentLineNumber) : base(context)
        {
            pubCollection = collections;
            utils = webServiceUtils;

            //load the publisher collection from the XML file provided, should upgrade all other config to load from external config XML in future
            if (!string.IsNullOrEmpty(pubCollection.PublisherConfig))
            {
                string xmlFilePath = Path.GetFullPath(IOUtils.ReplaceStringTokens(pubCollection.PublisherConfig, Context.Tokens));
                Logger.LogInfo("Loading {0} from the file {1}...", ActionName, xmlFilePath);

                //install the publishers from the Publisher Config XML, generate a new Publisher Collection object
                PublisherCollection pub = new PublisherCollection(xmlFilePath);
                pubCollection = pub;
                _ParentLineNumber = parentLineNumber;
            }
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "Publishers"); }
        }

        public override int Index
        {
            get
            {
                if (_ParentLineNumber == null)
                    return pubCollection.ElementInformation.LineNumber;
                else
                    return _ParentLineNumber.Value;
            }
        }

        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return string.Format("Failed to Create/Update the {0}.", ActionName);
            }
            return string.Format("Failed to Create/Update the {0}.", ActionName);
        }

        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.PublisherFailed;
        }

        protected override void RunBackupAction()
        {
            //nothing to backup
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo(" ");
            Logger.LogInfo("Installing {0} Collection...", ActionName);

            bool anyErrors = false;
            string errorMessage = string.Empty;

            //loop thru the publisher collection and install each of them
            foreach (PublisherElement element in pubCollection)
            {
                try
                {
                    InstallPublisher(element);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    anyErrors = true;
                }
            }
            
            if (anyErrors)
            {
                throw new Exception(string.Format("There was an error executing {0} install. Message: {1}", ActionName, errorMessage));
            }
        }

        private void InstallPublisher(PublisherElement element)
        {
            string uniqueName = IOUtils.ReplaceStringTokens(element.Name, Context.Tokens);
            string displayName = IOUtils.ReplaceStringTokens(element.DisplayName, Context.Tokens);
            string prefix = IOUtils.ReplaceStringTokens(element.Prefix, Context.Tokens);
            string pubDescription = element.Description;

            Logger.LogInfo(" ");
            Logger.LogInfo(" Installing {0}...", ActionName);

            //check if publisher exists
            bool updateExisting = false;
            Publisher enPub = SolutionManager.GetPublisherRecord(utils.Service, uniqueName);
            if (enPub != null)
            {
                //update existing
                updateExisting = true;
                Logger.LogInfo("  Found the Publisher '{0}' in the system, proceed to update the publisher.", uniqueName);
            }
            else
            {
                //create new record
                enPub = new Publisher
                {
                    UniqueName = uniqueName
                };
                Logger.LogInfo("  Publisher '{0}' does not exists, proceed to create the publisher.", uniqueName);
            }

            //set publisher values
            enPub.FriendlyName = displayName;
            enPub.CustomizationPrefix = prefix;
            enPub.Description = pubDescription;
            enPub.CustomizationOptionValuePrefix = element.OptionValuePrefix;
            enPub.EMailAddress = element.Email;
            enPub.Address1_Telephone1 = element.Phone;
            enPub.SupportingWebsiteUrl = element.Website;
            enPub.Address1_Line1 = element.AddressLine1;
            enPub.Address1_Line2 = element.AddressLine2;
            enPub.Address1_City = element.AddressCity;
            enPub.Address1_StateOrProvince = element.AddressState;
            enPub.Address1_PostalCode = element.AddressPostalCode;
            enPub.Address1_Country = element.AddressCountry;

            Guid? publisherId = null;
            if (updateExisting)
            {
                publisherId = enPub.Id;
                utils.Service.Update(enPub);
            }
            else
            {
                publisherId = utils.Service.Create(enPub);
            }
            if (publisherId != null)
                Logger.LogInfo("  PublisherID '{0}'.", publisherId.Value.ToString());
            else
                throw new Exception("Fail to retrieve Publisher GUID.");
        }

        protected override void RunUninstallAction()
        {
            //no uninstall action
            Logger.LogWarning("Uninstall not supported for {0}.", ActionName);
        }
    }
}