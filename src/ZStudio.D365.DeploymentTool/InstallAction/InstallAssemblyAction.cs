/*------------------------------------------------------------
 * Copyright © Zero.Studio 2022
 * created by:  winson
 * created on:  30 aug 2012
 * created by:  winson
 * modified on : 05 may 2020
--------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;
using System.IO;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace ZD365DT.DeploymentTool
{
    class InstallAssemblyAction : InstallAction
    {
        private readonly AssemblyCollection assemblyCollection;
        private readonly WebServiceUtils utils;

        public override string ActionName { get { return "Assemblies"; } }
        public override InstallType ActionType { get { return InstallType.OtherAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallAssemblyAction(AssemblyCollection assemblyCollections, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            assemblyCollection = assemblyCollections;
            utils = webServiceUtils;
        }
        protected override void RunBackupAction()
        {
        }
        protected override void RunInstallAction()
        {
            Logger.LogInfo(" ");
            Logger.LogInfo("Installing {0}...", ActionName);

            foreach (AssemblyElement element in assemblyCollection)
            {
                
                string assembliespath = Path.GetFullPath(IOUtils.ReplaceStringTokens(element.Source,Context.Tokens));
                string solution = IOUtils.ReplaceStringTokens(element.Solution, Context.Tokens);
                List<string> assemblyFiles = RetrieveAssemblyFiles(assembliespath);
                bool anyErrors = false;
                foreach (string file in assemblyFiles)
                {
                    UpdateAssembly(file, solution,ref anyErrors);
                }
                if (anyErrors)
                    throw new Exception("There was an error executing install workflow assemblies");
            }
        }

        public override string GetExecutionErrorMessage()
        {
           return "Failed to Install Assemblies.";
        }

        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.AssemblyInstallFailed;
        }

        protected override void RunUninstallAction()
        {
        }

        internal List<string> RetrieveAssemblyFiles(string path)
        {
            Logger.LogInfo("Retrieve Assembly Files from path {0}", path);

            DirectoryInfo diFiles = new DirectoryInfo(path);
            FileInfo[] fiFiles = diFiles.GetFiles("*.dll");
            List<string> returnList = new List<string>();
            foreach (FileInfo file in fiFiles)
            {
                returnList.Add(file.FullName);
            }
            return returnList;
        }

        public void UpdateAssembly(string file, string solution,ref bool anyErrors)
        {
            Logger.LogInfo("Check if the Assembly exists");

            QueryExpression query = new QueryExpression();
            query.EntityName = "pluginassembly";
            query.ColumnSet = new ColumnSet(new string[] { "pluginassemblyid" });
            query.Criteria = new FilterExpression();
            query.Criteria.AddCondition("name", ConditionOperator.Equal, new object[] { Path.GetFileNameWithoutExtension(file) });
            EntityCollection entitys = utils.Service.RetrieveMultiple(query);
            if (entitys.Entities.Count == 0)
            {
                Logger.LogInfo("WARNING: No assembly to update for {0}. Assembly will be added, but steps must be added manually.", Path.GetFileNameWithoutExtension(file));

                Entity entity = new Entity("pluginassembly");
                FileInfo info = new FileInfo(file);
                byte[] buffer = new byte[info.Length];
                int num = ((int)Math.Ceiling((double)(((double)info.Length) / 3.0))) * 4;
                char[] outArray = new char[num];
                using (FileStream stream = File.OpenRead(file))
                {
                    stream.Read(buffer, 0, (int)info.Length);
                    Convert.ToBase64CharArray(buffer, 0, buffer.Length, outArray, 0);
                }
                entity["content"] = new string(outArray);
                Console.WriteLine("Inserting {0}", Path.GetFileName(file));

                //Using CreateRequest because we want to add an optional parameter
                CreateRequest cr = new CreateRequest()
                {
                    Target = entity
                };

                //Set the SolutionUniqueName optional parameter so the Web Resources will be
                // created in the context of a specific solution.
                if (!string.IsNullOrEmpty(solution))
                {
                    cr.Parameters.Add("SolutionUniqueName", solution);
                }
                bool added = false;
                try
                {
                    utils.Service.Execute(cr);
                    added = true;
                }
                catch (Exception ex)
                {
                    Logger.LogInfo(" Assembly not Added {0}", Path.GetFileNameWithoutExtension(file));
                    Logger.LogException(ex);
                    anyErrors = true;
                }
                if (added)
                    Logger.LogInfo("Assembly added {0}", Path.GetFileNameWithoutExtension(file));
            }
            else
            {

                Entity entity = entitys.Entities[0];
                FileInfo info = new FileInfo(file);
                byte[] buffer = new byte[info.Length];
                int num = ((int)Math.Ceiling((double)(((double)info.Length) / 3.0))) * 4;
                char[] outArray = new char[num];
                using (FileStream stream = File.OpenRead(file))
                {
                    stream.Read(buffer, 0, (int)info.Length);
                    Convert.ToBase64CharArray(buffer, 0, buffer.Length, outArray, 0);
                }
                entity["content"] = new string(outArray);
                Logger.LogInfo("Updating Assembly {0}", Path.GetFileNameWithoutExtension(file));

                //Using UpdateRequest because we want to add an optional parameter
                UpdateRequest ur = new UpdateRequest()
                {
                    Target = entity
                };

                //Set the SolutionUniqueName optional parameter so the Web Resources will be
                // created in the context of a specific solution.
                if (!string.IsNullOrEmpty(solution))
                {
                    ur.Parameters.Add("SolutionUniqueName", solution);
                }
                bool updated = false;
                try
                {
                    utils.Service.Execute(ur);
                    updated = true;
                }
                catch (Exception ex)
                {
                    Logger.LogInfo(" Assembly not Updated {0}", Path.GetFileNameWithoutExtension(file));
                    Logger.LogException(ex);
                    anyErrors = true;
                }
                if (updated)
                    Logger.LogInfo("Updated Assembly {0}", Path.GetFileNameWithoutExtension(file));
            }
        }

        public override int Index
        {
            get { return assemblyCollection.ElementInformation.LineNumber; }
        }
    }
}