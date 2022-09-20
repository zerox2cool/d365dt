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
    public class InstallDefaultPublisherAction : InstallAction
    {
        private readonly Guid DefaultPublisherId = new Guid("d21aab71-79e7-11dd-8874-00188b01e34f");

        private readonly DefaultPublisherCollection pubCollection;
        private readonly WebServiceUtils utils;
        private int? _ParentLineNumber = null;

        public override string ActionName { get { return "DefaultPublisher"; } }
        public override InstallType ActionType { get { return InstallType.PublisherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallDefaultPublisherAction(DefaultPublisherCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils, int parentLineNumber) : base(context)
        {
            pubCollection = collections;
            utils = webServiceUtils;

            //load the publisher collection from the XML file provided, should upgrade all other config to load from external config XML in future
            if (!string.IsNullOrEmpty(pubCollection.PublisherConfig))
            {
                string xmlFilePath = Path.GetFullPath(IOUtils.ReplaceStringTokens(pubCollection.PublisherConfig, Context.Tokens));
                Logger.LogInfo("Loading {0} from the file {1}...", ActionName, xmlFilePath);

                //install the publishers from the Publisher Config XML, generate a new Publisher Collection object
                DefaultPublisherCollection pub = new DefaultPublisherCollection(xmlFilePath);
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
                return string.Format("Failed to Update the {0}.", ActionName);
            }
            return string.Format("Failed to Update the {0}.", ActionName);
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

            if (pubCollection.Count > 1)
                throw new Exception($"There can only be one default publisher in the system. The configuration contain {pubCollection.Count} default publishers.");

            //loop thru the publisher collection and install each of them
            foreach (DefaultPublisherElement element in pubCollection)
            {
                try
                {
                    InstallDefaultPublisher(element);
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

        private void InstallDefaultPublisher(DefaultPublisherElement element)
        {
            string displayName = IOUtils.ReplaceStringTokens(element.DisplayName, Context.Tokens);
            string prefix = IOUtils.ReplaceStringTokens(element.Prefix, Context.Tokens);
            string pubDescription = element.Description;

            Logger.LogInfo(" ");
            Logger.LogInfo(" Installing {0}...", ActionName);

            //retrieve the default publisher
            bool updateExisting = false;
            Publisher enPub = SolutionManager.GetPublisherRecord(utils.Service, DefaultPublisherId);
            if (enPub != null)
            {
                //update existing
                updateExisting = true;
                Logger.LogInfo("  Found the Default Publisher '{0}' in the system, proceed to update the publisher.", enPub.UniqueName);
            }
            else
            {
                throw new Exception("The Default Publisher cannot be found.");
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

            if (publisherId != null)
                Logger.LogInfo("  PublisherID '{0}'.", publisherId.Value.ToString());
            else
                throw new Exception("Fail to update Default Publisher.");
        }

        protected override void RunUninstallAction()
        {
            //no uninstall action
            Logger.LogWarning("Uninstall not supported for {0}.", ActionName);
        }
    }
}