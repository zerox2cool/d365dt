using System;
using System.IO;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ZD365DT.DeploymentTool
{
    public class InstallAssignSecurityRoleAction : InstallAction
    {
        private AssignSecurityRoleCollection assignCollection;
        private WebServiceUtils utils;
        private Dictionary<string, AttributeMetadata> cacheAttribute = new Dictionary<string, AttributeMetadata>();

        public override string ActionName { get { return "Assign Security Role"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallAssignSecurityRoleAction(AssignSecurityRoleCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils)
            : base(context)
        {
            assignCollection = collections;
            utils = webServiceUtils;
        }

        public override int Index
        {
            get { return assignCollection.ElementInformation.LineNumber; }
        }

        protected override void RunBackupAction()
        {
            //Do nothing for Backup
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo("Executing {0}...", ActionName);
            if (assignCollection != null)
            {
                bool anyError = false;
                string errorString = string.Empty;
                foreach (AssignSecurityRoleElement element in assignCollection)
                {
                    
                    Guid userId = Guid.Empty;
                    Guid teamId = Guid.Empty;

                    

                    string user = IOUtils.ReplaceStringTokens(element.DomainUserName, Context.Tokens);
                    string team = IOUtils.ReplaceStringTokens(element.TeamName, Context.Tokens);
                    string roleName = IOUtils.ReplaceStringTokens(element.SecurityRole, Context.Tokens);

                    if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(team))
                    {
                        anyError = true;
                        Logger.LogError("Define Team or User Name ");
                        errorString += Environment.NewLine + " Define Team or User Name ";
                    }
                    else if (string.IsNullOrEmpty(user) && string.IsNullOrEmpty(team))
                    {
                        anyError = true;
                        Logger.LogError("Define Team or User Name ");
                        errorString += Environment.NewLine + " Define Team or User Name ";
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(user))
                            userId = utils.GetUserForDomain(user);
                        else if (!string.IsNullOrEmpty(team))
                            teamId = utils.GetTeam(team);


                        //Get role                   
                        Role role = RetrieveRoleByName(roleName, utils.CurrentOrgContext.CurrentUserBusinessUnitId);

                        if (role != null)
                        {
                            if (userId != Guid.Empty)
                            {
                                // Associate the user with the role when needed.
                                if (!UserInRole(userId, role.Id))
                                {

                                    AssociateRequest associate = new AssociateRequest()
                                    {
                                        Target = new EntityReference(SystemUser.EntityLogicalName, userId),
                                        RelatedEntities = new EntityReferenceCollection()
                                        {
                                            new EntityReference(Role.EntityLogicalName, role.Id)
                                        },
                                        Relationship = new Relationship("systemuserroles_association")
                                    };
                                    utils.Service.Execute(associate);
                                    Logger.LogInfo("Associated User {0} to Role {1}", user, roleName);
                                }
                                else
                                    Logger.LogInfo("User {0} already is associated to Role {1}", user, roleName);
                            }
                            else if (teamId != Guid.Empty)
                            {
                                AssociateRequest associate = new AssociateRequest()
                                {
                                    Target = new EntityReference(Team.EntityLogicalName, teamId),
                                    RelatedEntities = new EntityReferenceCollection()
                                        {
                                            new EntityReference(Role.EntityLogicalName, role.Id)
                                        },
                                    Relationship = new Relationship("teamroles_association")
                                };
                                utils.Service.Execute(associate);                                
                                Logger.LogInfo("Associated Team {0} to Role {1}", team, roleName);
                            }
                        }
                        else
                        {
                            anyError = true;
                            Logger.LogError("Role {0} count not be found", roleName);
                            errorString += Environment.NewLine + string.Format("Role {0} count not be found", roleName);
                        }
                    }
                }

                if (anyError)
                    throw new Exception(errorString);
            }
        }

        private bool UserInRole(Guid userId, Guid roleId)
        {
            // Establish a SystemUser link for a query.
            LinkEntity systemUserLink = new LinkEntity()
            {
                LinkFromEntityName = SystemUserRoles.EntityLogicalName,
                LinkFromAttributeName = "systemuserid",
                LinkToEntityName = SystemUser.EntityLogicalName,
                LinkToAttributeName = "systemuserid",
                LinkCriteria =
                {
                    Conditions = 
                    {
                        new ConditionExpression(
                            "systemuserid", ConditionOperator.Equal, userId)
                    }
                }
            };

            // Build the query.
            QueryExpression query = new QueryExpression()
            {
                EntityName = Role.EntityLogicalName,
                ColumnSet = new ColumnSet("roleid"),
                LinkEntities = 
                {
                    new LinkEntity()
                    {
                        LinkFromEntityName = Role.EntityLogicalName,
                        LinkFromAttributeName = "roleid",
                        LinkToEntityName = SystemUserRoles.EntityLogicalName,
                        LinkToAttributeName = "roleid",
                        LinkEntities = {systemUserLink}
                    }
                },
                Criteria =
                {
                    Conditions = 
                    {
                        new ConditionExpression("roleid", ConditionOperator.Equal, roleId)
                    }
                }
            };

            // Retrieve matching roles.
            EntityCollection ec = utils.Service.RetrieveMultiple(query);

            if (ec.Entities.Count > 0)
                return true;

            return false;
        }



        private Role RetrieveRoleByName(String roleStr, Guid businessUnitId)
        {

           
            QueryExpression roleQuery = new QueryExpression
            {
                EntityName = Role.EntityLogicalName,
                ColumnSet = new ColumnSet("roleid"),
                Criteria =
                {
                    Conditions =
                {
                    new ConditionExpression("name", ConditionOperator.Equal, roleStr),
                    new ConditionExpression("businessunitid",ConditionOperator.Equal,businessUnitId)
                }
                }
            };

            EntityCollection roles = utils.Service.RetrieveMultiple(roleQuery);
            if (roles.Entities.Count > 0)
                return utils.Service.RetrieveMultiple(roleQuery).Entities[0].ToEntity<Role>();
            else
                return null;
        }


        protected override void RunUninstallAction()
        {
            // throw new NotImplementedException();
        }

        public override string GetExecutionErrorMessage()
        {
            return "Failed to Assign Security Role.";
        }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.AssignSecurityRole;
        }
    }
}
