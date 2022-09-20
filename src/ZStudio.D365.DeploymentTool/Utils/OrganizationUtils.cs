using System;
using System.ServiceModel;
using System.Threading;
using Microsoft.Xrm.Sdk.Deployment;

namespace ZD365DT.DeploymentTool.Utils
{
    public class OrganizationUtils
    {

        /// <summary>
        /// Create Organization
        /// </summary>
        /// <param name="serviceName">The Organisation to Create</param>
        internal static void CreateOrganization(IDeploymentService _deployService
            , string uniqueName
            , string friendlyName
            , string sqlServerName
            , string srsUrl
            , string baseCurrencyCode
            , string baseCurrencyName
            , string baseCurrencySymbol
            , int baseCurrencyPrecision, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                Guid? operationGuid = CreateOrganizationRequest(_deployService, new Microsoft.Xrm.Sdk.Deployment.Organization
                        {
                            UniqueName = uniqueName,
                            FriendlyName = friendlyName,
                            SqlServerName = sqlServerName,
                            SrsUrl = srsUrl,
                            BaseCurrencyCode = baseCurrencyCode,
                            BaseCurrencyName = baseCurrencyName,
                            BaseCurrencySymbol = baseCurrencySymbol,
                            BaseCurrencyPrecision = baseCurrencyPrecision,
                            SqlCollation = "Latin1_General_CI_AI",
                            State = Microsoft.Xrm.Sdk.Deployment.OrganizationState.Enabled
                        });
            }
            catch (FaultException<DeploymentServiceFault> ex)
            {
                Logger.LogException(ex);
                errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                errorMessage = ex.Message;
            }


        }
        internal static void DisableEnableOrganization(IDeploymentService deploymentService, string orgName,OrganizationState state, out string errorMessage)
        {
            try
            {
                errorMessage = string.Empty;
                DeploymentServiceClient client = (DeploymentServiceClient)deploymentService;
                EntityInstanceId id = new EntityInstanceId
                {
                    Name = orgName
                };

                Logger.LogInfo("Retrieve Organization based on Name");
                // disable the organization
                Microsoft.Xrm.Sdk.Deployment.Organization organization = (Microsoft.Xrm.Sdk.Deployment.Organization)client.Retrieve(DeploymentEntityType.Organization, id);

                Logger.LogInfo(state.ToString() +" the Organization");
                organization.State = state;
                client.Update(organization);

                Logger.LogInfo(state.ToString() + " the Organization Success");

            }
            catch (FaultException<DeploymentServiceFault> ex)
            {
                Logger.LogException(ex);
                errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                errorMessage = ex.Message;
            }
        }
        internal static void DeleteOrganization(IDeploymentService deploymentService, string orgName, out string errorMessage)
        {
            try
            {
                errorMessage = string.Empty;
                DeploymentServiceClient client = (DeploymentServiceClient)deploymentService;
                EntityInstanceId id = new EntityInstanceId
                   {
                       Name = orgName
                   };

                Logger.LogInfo("Retrieve Organization based on Name");
                // disable the organization
                Microsoft.Xrm.Sdk.Deployment.Organization organization = (Microsoft.Xrm.Sdk.Deployment.Organization)client.Retrieve(DeploymentEntityType.Organization, id);
                
                Logger.LogInfo("Disable the Organization");
                organization.State = OrganizationState.Disabled;
                client.Update(organization);

                Logger.LogInfo("Disable the Organization Success");

                // delete the organization
                DeleteRequest request = new DeleteRequest
                {
                    EntityType = DeploymentEntityType.Organization,
                    InstanceTag = id
                };
                Logger.LogInfo("Delete the Organization");
                DeleteResponse response = (DeleteResponse)client.Execute(request);
                Logger.LogInfo("Delete the Organization Success");

            }
            catch (FaultException<DeploymentServiceFault> ex)
            {
                Logger.LogException(ex);
                errorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                errorMessage = ex.Message;
            }

        }


        private static Guid? CreateOrganizationRequest(IDeploymentService deploymentService, Microsoft.Xrm.Sdk.Deployment.Organization org)
        {

            BeginCreateOrganizationRequest req = new BeginCreateOrganizationRequest();
            req.Organization = org;
            BeginCreateOrganizationResponse response;
            Guid operation = Guid.Empty;

            try
            {
                Logger.LogInfo("Creating the organization process started" );
                response = deploymentService.Execute(req) as BeginCreateOrganizationResponse;

                operation = response != null ? response.OperationId : Guid.Empty;

                Logger.LogInfo("OperationGuid : " + response.OperationId.ToString());

            }
            catch (TimeoutException ex)
            {
                Logger.LogException(ex);
            }


            if (operation != Guid.Empty)
            {
                EntityInstanceId operationid = new EntityInstanceId()
                  {
                      Id = operation
                  };

                DeferredOperationStatus createOrgStatus;

                createOrgStatus = deploymentService.Retrieve(DeploymentEntityType.DeferredOperationStatus, operationid) as DeferredOperationStatus;

                //The process of creating new ORG is quite slow, thus the status is not checked regularly

                while (createOrgStatus.State != DeferredOperationState.Completed && createOrgStatus.State != DeferredOperationState.Failed)
                {
                    Thread.Sleep(new TimeSpan(0, 2, 0));
                    try
                    {

                        createOrgStatus = deploymentService.Retrieve(DeploymentEntityType.DeferredOperationStatus, operationid) as DeferredOperationStatus;
                        Logger.LogInfo("Org Create Status :" + createOrgStatus.State.ToString());
                    }
                    catch (TimeoutException ex)
                    {
                        Logger.LogWarning("Time out exception encountered while checking progress of the organization creation. Retry will happen in 2 minutes :" + ex.Message);
                    }                    
                    catch (Exception ex)
                    {
                        Logger.LogWarning("Exception encountered while checking progress of the organization creation. Retry will happen in 2 minutes :" + ex.Message);
                    }
                }

            }

            return Guid.Empty;
        }

    }
}
