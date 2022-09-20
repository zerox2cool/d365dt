using System;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    public class InstallSqlAction : InstallAction
    {
        private readonly SqlCollection sqlScripts;
        private readonly WebServiceUtils utils;

        public override string ActionName { get { return "SQL Script"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallSqlAction(SqlCollection collection, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {   
                sqlScripts = collection;
                utils = webServiceUtils;
        }


        public override string GetExecutionErrorMessage()
        {
            return "Failed to execute SQL Scripts.";
        }


        public override int Index { get { return sqlScripts.ElementInformation.LineNumber; } }


        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.SqlFailed;
        }


        protected override void RunBackupAction()
        {
            // Do nothing for backup - Executed SQL Scripts cannot be backed up
        }


        protected override void RunInstallAction()
        {
            if (sqlScripts != null)
            {
                Logger.LogInfo(" ");
                Logger.LogInfo("Executing {0} for install...", ActionName);

                bool anyErrors = ExecuteSqlScripts(ExecuteActionOn.Intall);
                if (anyErrors)
                {
                    throw new Exception("There were errors executing SQL Scripts");
                }
            }
        }


        protected override void RunUninstallAction()
        {
            if (sqlScripts != null)
            {
                Logger.LogInfo("Executing SQL Scripts for uninstall...");

                bool anyErrors = ExecuteSqlScripts(ExecuteActionOn.Uninstall);
                if (anyErrors)
                {
                    throw new Exception("There were errors executing SQL Scripts");
                }
            }
        }

        #region Execute Script Methods

        private bool ExecuteSqlScripts(ExecuteActionOn executActionOn)
        {
            bool anyErrors = false;

            //if (utils.authenticationType != CrmAuthenticationType.OnPremise)
            //{
            //    throw new Exception(String.Format("{0} deployments do not support execution of SQL Scripts.", utils.authenticationType));
            //}
            if (sqlScripts != null)
            {
                foreach (SqlElement sqlScript in sqlScripts)
                {
                    string sourceFile = IOUtils.ReplaceStringTokens(sqlScript.SourceFile, Context.Tokens);
                    string sourceDirectory = IOUtils.ReplaceStringTokens(sqlScript.SourceDirectory, Context.Tokens);
                    string name = IOUtils.ReplaceStringTokens(sqlScript.Name, Context.Tokens);

                    try
                    {                        
                        
                        string connectionString = IOUtils.ReplaceStringTokens(sqlScript.ConnectionString, Context.Tokens);

                        if (sqlScript.ExectuteActionOn == executActionOn)
                        {
                            if (!String.IsNullOrEmpty(sourceFile) && !String.IsNullOrEmpty(sourceDirectory))
                            {
                                anyErrors = true;
                                Logger.LogError("  Both source file and directory has been defined for SQL action '{0}'. Define any one of them.", name);
                            }
                            if (!String.IsNullOrEmpty(sourceFile))
                            {
                                if (File.Exists(Path.GetFullPath(sourceFile)))
                                {
                                    string script = IOUtils.ReadFileReplaceTokens(Path.GetFullPath(sourceFile), Context.Tokens);

                                    if (sqlScript.ExecuteInTransaction)
                                    {
                                        Logger.LogInfo("  Executing script '{0}' in a transaction", name);
                                        
                                        ExecuteInTransaction(script, connectionString);
                                    }
                                    else
                                    {
                                        Logger.LogInfo("  Executing script '{0}' without a transaction", name);
                                        
                                        ExecuteWithoutTransaction(script, connectionString);
                                    }
                                }
                                else
                                {
                                    Logger.LogError("  Error executing script '{0}', source file '{1}' was not found", name, sourceFile);
                                }
                            }
                            else if (!String.IsNullOrEmpty(sourceDirectory))
                            {
                                if (Directory.Exists(Path.GetFullPath(sourceDirectory)))
                                {
                                    

                                    // Sort the files
                                    string[] scriptFileNames = Directory.GetFiles(sourceDirectory);
                                    Array.Sort(scriptFileNames);

                                    foreach (string scriptFileName in scriptFileNames)
                                    {
                                        string script = IOUtils.ReadFileReplaceTokens(Path.GetFullPath(scriptFileName), Context.Tokens);

                                        if (sqlScript.ExecuteInTransaction)
                                        {
                                            Logger.LogInfo("  Executing script file '{0}' in a transaction", Path.GetFileName(scriptFileName));
                                            ExecuteInTransaction(script, connectionString);
                                        }
                                        else
                                        {
                                            Logger.LogInfo("  Executing script file '{0}' without a transaction", Path.GetFileName(scriptFileName));
                                            ExecuteWithoutTransaction(script, connectionString);
                                        }
                                    }
                                }
                                else
                                {
                                    anyErrors = true;
                                    Logger.LogError("  Error executing script '{0}', source directory '{1}' was not found", name, sourceDirectory);
                                }
                            }
                            else
                            {
                                anyErrors = true;
                                Logger.LogError("  No source file or directory has been defined for SQL action '{0}'", name);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        anyErrors = true;
                        Logger.LogError("Error executing script '{0}' from '{1}'.  Message: {2}", name, sourceFile, ex.Message);
                    }
                }
            }
            else
            {
                Logger.LogWarning("  No SQL scripts defined in the configuration file");
            }
            return anyErrors;
        }

        private void ExecuteInTransaction(string script, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction = connection.BeginTransaction("ScriptTransaction");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    ExecuteScript(script, command);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch
                    {
                        // Transaction rollback failed - not a lot more we can do here
                    }
                    throw ex;
                }
            }
        }

        private void ExecuteWithoutTransaction(string script, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();

                ExecuteScript(script, command);
            }
        }

        private void ExecuteScript(string sqlScript, SqlCommand command)
        {
            string[] scriptParts = SqlScriptParser.SplitSqlQueryOnGo(sqlScript);

            foreach (string scriptPart in scriptParts)
            {
                if (!String.IsNullOrEmpty(scriptPart.Trim()))
                {
                    command.CommandText = scriptPart;
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion Execute Script Methods
    }
}
