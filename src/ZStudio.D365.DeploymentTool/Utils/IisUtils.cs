using System;
using System.DirectoryServices;
using System.IO;

namespace ZD365DT.DeploymentTool.Utils
{
    public class IisUtils : WindowsServiceUtils
    {
        #region Private Constants

        private const string IIS_SERVICE = "W3SVC";

        #endregion Private Constants

        /// <summary>
        /// Removes a Virtual Directory from IIS
        /// </summary>
        /// <param name="name">The name of the virtual directory to remove</param>
        public static void RemoveVirtualDirectory(string name)
        {
            DeleteTree(GetCrmIisRoot() + "/" + name);
        }


        /// <summary>
        /// Creates a .Net 2.0 Virtual Directory in IIS
        /// </summary>
        /// <param name="vdirname">The name of the virtual directory</param>
        /// <param name="appPoolName">The name of the AppPool for the virtual directory</param>
        /// <param name="dirpath">The physical path referenced by the virtual directory</param>
        /// <param name="allowAnonymousAccess">
        /// <c>true</c> if anonymous access should be available, 
        /// <c>false</c> if only integrated authentication is valid
        /// </param>
        public static void CreateVirtualDirectory(string vDirName, string physicalPath, bool allowAnonymousAccess)
        {
            string metabasePath = GetCrmIisRoot();
            Logger.LogInfo("  Creating virtual directory {0}/{1}, mapping the Root application to {2} and setting Anonymous Access = {3}", metabasePath, vDirName, physicalPath, allowAnonymousAccess);

            using (DirectoryEntry site = new DirectoryEntry(metabasePath))
            {
                string className = site.SchemaClassName.ToString();
                if ((className.EndsWith("Server")) || (className.EndsWith("VirtualDir")))
                {
                    DirectoryEntries vdirs = site.Children;
                    DirectoryEntry newVDir = vdirs.Add(vDirName, (className.Replace("Service", "VirtualDir")));
                    newVDir.Properties["Path"][0] = physicalPath;
                    newVDir.Properties["AccessScript"][0] = true;
                    newVDir.Properties["AppIsolated"][0] = "1";
                    newVDir.Properties["AccessFlags"][0] = 0x00000205; // AccessExecute | AccessRead | AccessScript
                    newVDir.Properties["DirBrowseFlags"][0] = 0x4000003E; //DirBrowseShowDate | DirBrowseShowTime | DirBrowseShowSize | DirBrowseShowExtension | DirBrowseShowLongDate | EnableDefaultDoc
                    newVDir.Properties["AppRoot"][0] = metabasePath.Replace("/LM", "IIS://"); //"/LM" + metabasePath.Substring(metabasePath.IndexOf("/", ("IIS://".Length)));
                    newVDir.Properties["AuthAnonymous"][0] = allowAnonymousAccess;
                    newVDir.Properties["AuthNTLM"][0] = true;
                    newVDir.CommitChanges();

                    Logger.LogInfo("  Created virtual directory {0}/{1}, mapping the Root application to {2}", metabasePath, vDirName, physicalPath);
                }
                else
                {
                    throw new ApplicationException(String.Format("Failed to create virtual directory {0}/{1}.  Virtual directory can only be created in a site or virtual directory node", metabasePath, vDirName));
                }
            }
        }


        public static void AssignVirtualDirectoryToAppPool(string vDirName, string appPoolName)
        {
            string metabasePath = GetCrmIisRoot() + "/" + vDirName;
            Logger.LogInfo("  Assigning application {0} to the application pool named {1}...", metabasePath, appPoolName);

            using (DirectoryEntry vDir = new DirectoryEntry(metabasePath))
            {
                string className = vDir.SchemaClassName.ToString();
                if (className.EndsWith("VirtualDir"))
                {
                    object[] param = { 0, appPoolName, true };
                    vDir.Invoke("AppCreate3", param);
                    vDir.Properties["AppIsolated"][0] = "2";
                    vDir.Properties["AppFriendlyName"][0] = vDirName;
                    vDir.CommitChanges();
                    Logger.LogInfo("  Application {0} assigned to the application pool named {1}", metabasePath, appPoolName);
                }
                else
                {
                    throw new ApplicationException(String.Format("Failed to assign virtual directory {0} to application pool {1}.  Only virtual directories can be assigned to application pools", metabasePath, appPoolName));
                }
            }
        }


        /// <summary>
        /// Assigns a MIME Type and extension to a Virtual Directory
        /// </summary>
        /// <param name="vDirName">The name of the virtual directory under CRM</param>
        /// <param name="extension">The MIME extension</param>
        /// <param name="mimeType">The MIME Type</param>
        public static void AssignMimeTypeToVirtualDirectory(string vDirName, string extension, string mimeType)
        {
            string metabasePath = GetCrmIisRoot() + "/" + vDirName;
            Logger.LogInfo("  Assigning MIME Type {0} with extension {1} to the virtual directory named {2}...", mimeType, extension, metabasePath);

            using (DirectoryEntry mimeMap = new DirectoryEntry(metabasePath))
            {
                PropertyValueCollection propValues = mimeMap.Properties["MimeMap"];

                IISOle.MimeMap newMimeType = new IISOle.MimeMap();
                newMimeType.Extension = extension;
                newMimeType.MimeType = mimeType;

                propValues.Add(newMimeType);
                mimeMap.CommitChanges();
            }
        }


        /// <summary>
        /// Removes an Application Pool (AppPool) from IIS
        /// </summary>
        /// <param name="name"></param>
        public static void RemoveAppPool(string name)
        {
            DeleteTree("IIS://localhost/W3SVC/AppPools/" + name);
        }


        /// <summary>
        /// Creates an Application Pool (AppPool) in IIS running under the NetworkService credential set
        /// </summary>
        /// <param name="name">The name of the AppPool</param>
        public static void CreateAppPool(string appPoolName)
        {
            string metabasePath = "IIS://localhost/W3SVC/AppPools";
            Logger.LogInfo("  Creating application pool named {0}/{1}...", metabasePath, appPoolName);

            DirectoryEntry apppools = new DirectoryEntry(metabasePath);
            DirectoryEntry newpool = apppools.Children.Add(appPoolName, "IIsApplicationPool");            
            newpool.CommitChanges();
            Logger.LogInfo("  Created application pool named {0}/{1}", metabasePath, appPoolName);
        }


        /// <summary>
        /// Checks whether the App Pool exists in IIS
        /// </summary>
        /// <param name="appPoolName">The name of the app pool to check</param>
        /// <returns><c>true</c> if the app pool exists, otherwise <c>false</c>.</returns>
        public static bool AppPoolExists(string appPoolName)
        {
            bool appPoolExists = false;
            DirectoryEntry appPools = new DirectoryEntry("IIS://localhost/W3SVC/AppPools");
            foreach (DirectoryEntry appPool in appPools.Children)
            {
                if (appPool.SchemaClassName.Equals("IIsApplicationPool", StringComparison.InvariantCultureIgnoreCase) && 
                    appPool.Name.Equals(appPoolName, StringComparison.InvariantCultureIgnoreCase))
                {
                    appPoolExists = true;
                    break;
                }
            }
            return appPoolExists;
        }


        /// <summary>
        /// Returns true if the application pool is in use or is the "DefaultAppPool"
        /// </summary>
        /// <param name="appPoolName">The name of the application pool to check</param>
        /// <returns><c>true</c> if the App Pool is in use, otherwise <c>false</c>.</returns>
        public static bool AppPoolUsed(string appPoolName)
        {
            bool appPoolUsed = false;

            // Prevent deletion of the Default Application Pool
            if (appPoolName.Equals("DefaultAppPool", StringComparison.InvariantCultureIgnoreCase))
            {
                appPoolUsed = true;
            }
            else
            {
                DirectoryEntry root = new DirectoryEntry(GetCrmIisRoot());
                foreach (DirectoryEntry dir in root.Children)
                {
                    if (dir.Properties.Contains("AppPoolId"))
                    {
                        if (appPoolName.Equals(dir.Properties["AppPoolId"].Value.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            appPoolUsed = true;
                            break;
                        }
                    }
                }
            }
            return appPoolUsed;
        }


        /// <summary>
        /// Starts the World Wide Web Publishing Service on the server
        /// </summary>
        public static void StartIis()
        {
            if (!IsServiceRunning(IIS_SERVICE))
            {
                Logger.LogInfo("Attempting to start IIS.");
                StartService(IIS_SERVICE);
                Logger.LogInfo("IIS Started.");
            }
        }


        /// <summary>
        /// Stops the World Wide Web Publishing Service on the server
        /// </summary>
        public static void StopIis()
        {
            if (IsServiceRunning(IIS_SERVICE))
            {
                Logger.LogInfo("Attempting to stop IIS.");
                StopService(IIS_SERVICE);
                Logger.LogInfo("IIS Stopped.");
            }
        }


        /// <summary>
        /// Adds .NET 2.0 Exctentions to the website
        /// </summary>
        /// <param name="vDirName">
        /// The name of the Virtual Directory to which the associations should be added
        /// </param>
        public static void AddDotNet2AssociationTo(string vDirName)
        {
            string aspNETPath = CrmRegistryUtils.GetNetIsapiFilter();
            string inetsrvPath = CrmRegistryUtils.GetInetsrvDir();
            string aspPath = Path.Combine(inetsrvPath, "asp.dll");
            string httpodbcPath = Path.Combine(inetsrvPath, "httpodbc.dll");
            string ssincPath = Path.Combine(inetsrvPath, "ssinc.dll");

            string metabasePath = GetCrmIisRoot();
            Logger.LogInfo("  Applying .Net 2.0 Associations to {0}/{1}...", metabasePath, vDirName);

            DirectoryEntry vDir = new DirectoryEntry(metabasePath + "/" + vDirName);
            vDir.Properties["ScriptMaps"].Clear();

            vDir.Properties["ScriptMaps"].Add(@".asp," + aspPath + ",5,GET,HEAD,POST,TRACE");
            vDir.Properties["ScriptMaps"].Add(@".cer," + aspPath + ",5,GET,HEAD,POST,TRACE");
            vDir.Properties["ScriptMaps"].Add(@".cdx," + aspPath + ",5,GET,HEAD,POST,TRACE");
            vDir.Properties["ScriptMaps"].Add(@".asa," + aspPath + ",5,GET,HEAD,POST,TRACE");
            vDir.Properties["ScriptMaps"].Add(@".idc," + httpodbcPath + ",5,GET,POST");
            vDir.Properties["ScriptMaps"].Add(@".shtm," + ssincPath + ",5,GET,POST");
            vDir.Properties["ScriptMaps"].Add(@".shtml," + ssincPath + ",5,GET,POST");
            vDir.Properties["ScriptMaps"].Add(@".stm," + ssincPath + ",5,GET,POST");
            vDir.Properties["ScriptMaps"].Add(@".asax," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".ascx," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".ashx," + aspNETPath + ",1,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".asmx," + aspNETPath + ",1,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".aspx," + aspNETPath + ",1,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".axd," + aspNETPath + ",1,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".vsdisco," + aspNETPath + ",1,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".rem," + aspNETPath + ",1,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".soap," + aspNETPath + ",1,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".config," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".cs," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".csproj," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".vb," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".vbproj," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".webinfo," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".licx," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".resx," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".resources," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".jsl," + aspNETPath + ",1,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".java," + aspNETPath + ",1,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".vjsproj," + aspNETPath + ",1,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".master," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".skin," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".compiled," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".browser," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".mdb," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".jsl," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".vjsproj," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".sitemap," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".msgx," + aspNETPath + ",1,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".ad," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".dd," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".ldd," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".sd," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".cd," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".adprototype," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".lddprototype," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".sdm," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".sdmDocument," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".ldb," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".svc," + aspNETPath + ",1,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".mdf," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".ldf," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".java," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".exclude," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");
            vDir.Properties["ScriptMaps"].Add(@".refresh," + aspNETPath + ",5,GET,HEAD,POST,DEBUG");

            vDir.Invoke("SetInfo");
            vDir.CommitChanges();
        }


        #region Helper Methods

        /// <summary>
        /// Deletes an entire Tree from IIS
        /// </summary>
        /// <example>IIS://localhost/W3SVC/1/Root/MyVDir</example>
        /// <example>IIS://localhost/W3SVC/AppPools/MyAppPool</example>
        /// <param name="metabasePath">The path to the directory to be deleted in IIS</param>
        private static void DeleteTree(string metabasePath)
        {
            Logger.LogInfo("  Deleting {0}...", metabasePath);

            try
            {
                DirectoryEntry tree = new DirectoryEntry(metabasePath);
                tree.DeleteTree();
                tree.CommitChanges();
                Logger.LogInfo("  Deleted {0}", metabasePath);
            }
            catch (DirectoryNotFoundException)
            {
                // Do nothing, this always appears to be thrown at the end of the delete
                Logger.LogInfo("  Deleted {0}", metabasePath);
            }
            catch (Exception)
            {
                Logger.LogInfo("  {0} not found, so no deletion required", metabasePath);
            }
        }

        private static string GetCrmIisRoot()
        {
            string rootPath = CrmRegistryUtils.GetCrmIisSite() + "/Root";
            return rootPath.Replace("/LM", "IIS://localhost");
        }

        #endregion Helper Methods

    }
}
