using ZD365DT.DeploymentTool.Configuration;
using ZD365DT.DeploymentTool.Utils;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;

namespace ZD365DT.DeploymentTool.Context
{
    public class DeploymentContext : IDeploymentContext
    {
        #region Constant Lookup Keys
        private const string WORKING_DIRECTORY = "@WORKING_DIRECTORY@";        
        private const string MACHINE_NAME = "@MACHINE_NAME@";
        #endregion Constant Lookup Keys

        private readonly StringDictionary tokens;
        private readonly DeploymentConfigurationSection deploymentConfig;

        public DeploymentConfigurationSection DeploymentConfig { get { return deploymentConfig; } }
        public DeploymentAction DeploymentAction { get { return deploymentConfig.Action; } }
        public StringDictionary Tokens { get { return tokens; } }

        public DeploymentContext(DeploymentConfigurationSection deploymentConfig)
        {
            tokens = new StringDictionary();
            this.deploymentConfig = deploymentConfig;

            #region Add the Standard Context Items
            tokens.Add(WORKING_DIRECTORY, Environment.CurrentDirectory);
            tokens.Add(MACHINE_NAME, Environment.MachineName);
            #endregion Add the Standard Context Items

            #region Add the Dynamic Context Items from the Command Line
            bool isLoadedFromCmdLine = false;
            CommandLineTokens cmdLineTokens = CommandLineTokens.FromCommandLineArguments();
            foreach (CommandLineArgument token in cmdLineTokens)
            {
                string keyName = String.Format("@{0}@", token.Token.ToUpper());
                if (!tokens.ContainsKey(keyName))
                {
                    tokens.Add(keyName, token.Value);
                }
                else
                {
                    tokens[keyName] = token.Value;
                }

                isLoadedFromCmdLine = true;
            }
            if (isLoadedFromCmdLine)
                Logger.LogInfo("Loaded Execution Context from command line...");
            #endregion Add the Dynamic Context Items from the Command Line

            #region Add the Dynamic Context Items from the app.config
            if (deploymentConfig != null)
            {
                foreach (ExecutionContextCollection context in deploymentConfig.ExecutionContexts)
                {
                    ExecutionContextCollection contextCopy = context;
                    if (!string.IsNullOrEmpty(context.ContextFile))
                    {
                        //replace context file if it is a token that is passed in thru the command-line, enable the use of different context file in the same DeploymentConfiguration XML
                        string contextFile = IOUtils.ReplaceStringTokens(context.ContextFile, tokens);

                        //get XML
                        string xmlFilePath = Path.GetFullPath(contextFile);
                        Logger.LogInfo("Loading Execution Context from the file {0}...", xmlFilePath);

                        //install the publishers from the Publisher Config XML, generate a new Publisher Collection object
                        contextCopy = new ExecutionContextCollection(xmlFilePath);
                    }

                    bool isLoadedFromContextConfig = false;
                    foreach (ExecutionContextElement key in contextCopy)
                    {
                        string keyName = String.Format("@{0}@", key.Name.ToUpper());
                        if (!tokens.ContainsKey(keyName))
                        {
                            tokens.Add(keyName, key.Value);
                        }
                        else
                        {
                            tokens[keyName] = key.Value;
                        }
                        isLoadedFromContextConfig = true;
                    }

                    if (isLoadedFromContextConfig && string.IsNullOrEmpty(context.ContextFile))
                        Logger.LogInfo("Loaded Execution Context from the configuration...");
                }
            }
            #endregion Add the Dynamic Context Items from the app.config

            #region Add Security Roles
            //LoadTokensFromSecurityRoles();
            #endregion Add Security Roles

            #region List All Context Value
            ListAllExecutionContextValue();
            #endregion List All Context Value
        }

        /// <summary>
        /// Refreshes the Metadata and Security Role tokens from CRM
        /// </summary>
        public void RefreshTokensFromCrm()
        {
          
        }

        /// <summary>
        /// Loads or refreshes the Guids for the Security Role tokens from CRM
        /// </summary>
        private void LoadTokensFromSecurityRoles()
        {
            /*ColumnSet roleCols = new ColumnSet();
            roleCols.AddColumns("name", "roleid");

            QueryExpression roleQuery = new QueryExpression();
            roleQuery.EntityName = EntityName.role.ToString();
            roleQuery.ColumnSet = roleCols;
            roleQuery.Criteria = new FilterExpression();
            roleQuery.Criteria.FilterOperator = LogicalOperator.And;
            roleQuery.Criteria.AddCondition("parentroleid", ConditionOperator.Null);

            BusinessEntityCollection roles = WebServiceUtils.Instance.Service.RetrieveMultiple(roleQuery);
            foreach (role role in roles.BusinessEntities)
            {
                string key = String.Format("@SECURITY_ROLE_{0}@", role.name.Replace(' ', '_').ToUpper());
                if (!tokens.ContainsKey(key))
                {
                    tokens.Add(key, role.roleid.Value.ToString());
                }
                else
                {
                    tokens[key] = role.roleid.Value.ToString();
                }
            }*/
        }

        private void ListAllExecutionContextValue()
        {
            Logger.LogInfo("Execution Context value loaded for this execution...");
            foreach (DictionaryEntry s in tokens)
            {
                //hide password/clientsecret/thumbprint from the logs
                object value = s.Value;

                if (s.Value != null && !string.IsNullOrEmpty(s.Value.ToString()) && s.Value.ToString().ToLower().Contains("password="))
                    value = s.Value.ToString().Substring(0, s.Value.ToString().ToLower().IndexOf("password="));

                if (s.Value != null && !string.IsNullOrEmpty(s.Value.ToString()) && s.Value.ToString().ToLower().Contains("clientsecret="))
                    value = s.Value.ToString().Substring(0, s.Value.ToString().ToLower().IndexOf("clientsecret="));

                if (s.Value != null && !string.IsNullOrEmpty(s.Value.ToString()) && s.Value.ToString().ToLower().Contains("thumbprint="))
                    value = s.Value.ToString().Substring(0, s.Value.ToString().ToLower().IndexOf("thumbprint="));

                Logger.LogInfo("  {0} = {1}", s.Key, value);
            }
        }
    }
}