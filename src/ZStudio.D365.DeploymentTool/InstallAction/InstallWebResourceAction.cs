using System;
using System.Collections.Generic;
using System.Linq;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;
using System.Xml.Linq;
using System.IO;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace ZD365DT.DeploymentTool
{
    class InstallWebResourceAction : InstallAction
    {
        const int INVALID_CONTENT_TYPE = 0;
        const int CONTENT_TYPE_HTML = 1;
        const int CONTENT_TYPE_CSS = 2;
        const int CONTENT_TYPE_JS = 3;
        const int CONTENT_TYPE_XML = 4;
        const int CONTENT_TYPE_PNG = 5;
        const int CONTENT_TYPE_JPG = 6;
        const int CONTENT_TYPE_GIF = 7;
        const int CONTENT_TYPE_XAP = 8;
        const int CONTENT_TYPE_XSL = 9;
        const int CONTENT_TYPE_ICO = 10;
        const int CONTENT_TYPE_SVG = 11;
        const int CONTENT_TYPE_RESX = 12;
        private readonly WebResourceCollection webresourceCollection;
        private readonly WebServiceUtils utils;

        public override string ActionName { get { return "Web Resource"; } }
        public override InstallType ActionType { get { return InstallType.WebResourceAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallWebResourceAction(WebResourceCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils)
            : base(context)
        {
            webresourceCollection = collections;
            utils = webServiceUtils;
        }

        public override int Index
        {
            get { return webresourceCollection.ElementInformation.LineNumber; }
        }

        protected override void RunBackupAction()
        {
           
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo(" ");
            Logger.LogInfo("Installing {0}...", ActionName);

            foreach (WebResourceElement element in webresourceCollection)
            {
                ImportWebresources(element);
            }
        }

        protected override void RunUninstallAction()
        {
           // throw new NotImplementedException();
        }

        public void ImportWebresources(WebResourceElement element)
        {

            string filepath = Path.GetFullPath(IOUtils.ReplaceStringTokens(element.Source, Context.Tokens));
            string filename =  IOUtils.ReplaceStringTokens(element.FileName,Context.Tokens);
            string solution =  IOUtils.ReplaceStringTokens(element.Solution,Context.Tokens);
            string customizationPrefix = IOUtils.ReplaceStringTokens(element.CustomizationPrefix,Context.Tokens);
            string _customizationPrefix = string.IsNullOrEmpty(customizationPrefix) ? string.Empty : customizationPrefix + "_";

            Logger.LogInfo("Installing WebResources from the file {0}", filepath);
            List<WebResourceInternal> files = RetrieveWebResourceFiles(filepath, filename, _customizationPrefix, element);

            foreach (WebResourceInternal webResource in files)
            {
                string Name = _customizationPrefix + webResource.name;

                //Set the Web Resource properties
                Entity wr = new Entity("webresource");
                wr.Attributes.Add("content", webResource.content);
                wr.Attributes.Add("displayname", webResource.displayName);
                wr.Attributes.Add("description", webResource.description);
                wr.Attributes.Add("name", Name);
                wr.Attributes.Add("webresourcetype", new OptionSetValue(webResource.type));


                // Add a silverlight version if the web resource is a silverlight resource
                if (webResource.type == (int)WebResourceInternal.WebResourceType.XAP)
                {
                    wr.Attributes.Add("silverlightversion", "4.0");
                }

                // Check if the webresource does already exist. If so, update otherwise create.
                Guid? webresourceid = WebResourceInternal.GetWebResourceId(utils, Name);
                if (webresourceid != null)
                {


                    wr.Id = (Guid)webresourceid;

                    //Using UpdateRequest because we want to add an optional parameter
                    UpdateRequest ur = new UpdateRequest()
                    {
                        Target = wr
                    };
                    //Set the SolutionUniqueName optional parameter so the Web Resources will be
                    // created in the context of a specific solution.
                    if (!string.IsNullOrEmpty(solution))
                        ur.Parameters.Add("SolutionUniqueName", solution);

                    Logger.LogInfo("Updating {0}", Name);
                    bool updated = false;
                    try
                    {
                        utils.Service.Execute(ur);
                        updated = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogInfo(" WebResource not Updated {0}", Name);
                        Logger.LogException(ex);
                    }
                    if (updated)
                        Logger.LogInfo("Updated WebResource {0}", Name);
                }
                else
                {
                    // Using CreateRequest because we want to add an optional parameter
                    CreateRequest cr = new CreateRequest
                    {
                        Target = wr
                    };

                    //Set the SolutionUniqueName optional parameter so the Web Resources will be
                    // created in the context of a specific solution.
                    if (!string.IsNullOrEmpty(solution))
                        cr.Parameters.Add("SolutionUniqueName", solution);

                    Logger.LogInfo("Inserting {0}", Name);


                    bool added = false;
                    try
                    {
                        utils.Service.Execute(cr);
                        added = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogInfo(" WebResource not Inserted {0}", Name);
                        Logger.LogException(ex);
                    }
                    if (added)
                        Logger.LogInfo("Inserted WebResource {0}", Name);
                }
            }
        }

        internal List<WebResourceInternal> RetrieveWebResourceFiles(string filePath, string fileName, string customizationPrefix, WebResourceElement element)
        {
            List<WebResourceInternal> returnList = new List<WebResourceInternal>();
            if (element.sType == SourceType.configxml)
            {
                //Read the descriptive data from the XML file
                XDocument xmlDoc = XDocument.Load(filePath + "\\" + fileName);

                //Create a collection of anonymous type references to each of the Web Resources
                var webResources = from webResource in xmlDoc.Descendants("webResource")
                                   select new
                                   {
                                       path = webResource.Element("path").Value,
                                       displayName = webResource.Element("displayName").Value,
                                       description = webResource.Element("description").Value,
                                       name = webResource.Element("name").Value,
                                       type = webResource.Element("type").Value
                                   };


                foreach (var webResource in webResources)
                {
                    WebResourceInternal wr = new WebResourceInternal();
                    if (element.UpdateDescription)
                        wr.description =IOUtils.ReplaceStringTokens( webResource.description,Context.Tokens);
                    wr.displayName = IOUtils.ReplaceStringTokens(webResource.displayName,Context.Tokens);
                    wr.name =IOUtils.ReplaceStringTokens( webResource.name,Context.Tokens);
                    wr.path = IOUtils.ReplaceStringTokens(webResource.path, Context.Tokens);
                    wr.type = Convert.ToInt32(IOUtils.ReplaceStringTokens(webResource.type, Context.Tokens));

                    wr.content = getEncodedFileContents(filePath + @"\" + IOUtils.ReplaceStringTokens(webResource.path,Context.Tokens));
                    returnList.Add(wr);
                }
            }
            else
            {
                DirectoryInfo dirInfo = new DirectoryInfo(filePath);
                IEnumerable<FileInfo> files = null;
                if (element.IncludeSubfolders)
                    files = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories);
                else
                    files = dirInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly);

                foreach (FileInfo file in files)
                {
                    string sourcepath = Path.GetFullPath(IOUtils.ReplaceStringTokens(element.Source, Context.Tokens));
                    int contentTypeNumber = GetContentType(file.FullName);

                    if (contentTypeNumber != INVALID_CONTENT_TYPE)
                    {
                        string name = file.Name;
                        if (element.UsePathInName)
                        {
                            //include folder path in the web resource name and display name
                            name = file.FullName.Replace(sourcepath, string.Empty);
                            name = name.Replace("\\", "/");

                            //make sure the name is not longer than 100 characters, currently only UI on CRM is blocking, back-end insert can accept more than 100
                            //if (name.Length > 100)
                            //    name = name.Substring(name.Length - 100, 100);
                        }

                        WebResourceInternal wr = new WebResourceInternal();
                        if (element.UpdateDescription)
                            wr.description = file.FullName;
                        if (element.UseExtensionInName)
                        {
                            //file extension is included in the web resource name as well
                            wr.displayName = name;
                            wr.name = name;
                        }
                        else
                        {
                            //strip the file extension from the filename so web resource name has no extension
                            wr.displayName = name.Replace(file.Extension, string.Empty);
                            wr.name = name.Replace(file.Extension, string.Empty);
                        }

                        //append the file extension as the prefix of the web resource name
                        if (element.UseExtensionAsPrefix)
                        {
                            wr.displayName = file.Extension.Remove(0, 1) + "_" + wr.displayName;
                            wr.name = file.Extension.Remove(0, 1) + "_" + wr.name;
                        }

                        //append the customization prefix on the web resource display name
                        if (element.UseCustomizationPrefixInDisplayName)
                        {
                            wr.displayName = customizationPrefix + wr.displayName;
                        }

                        wr.path = file.FullName;
                        wr.type = contentTypeNumber;
                        wr.content = getEncodedFileContents(file.FullName);
                        returnList.Add(wr);
                    }
                }
            }
            return returnList;
        }

        //Encodes a Web Resource File
        static public string getEncodedFileContents(String pathToFile)
        {
            FileStream fs = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
            byte[] binaryData = new byte[fs.Length];
            long bytesRead = fs.Read(binaryData, 0, (int)fs.Length);
            fs.Close();
            return System.Convert.ToBase64String(binaryData, 0, binaryData.Length);
        }
        /// <summary>
        /// Gets the web resourcce content type number based on file extension
        /// Returns 0 if invalid content type
        /// </summary>
        public static int GetContentType(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".htm": return CONTENT_TYPE_HTML;
                case ".html": return CONTENT_TYPE_HTML;
                case ".css": return CONTENT_TYPE_CSS;
                case ".js": return CONTENT_TYPE_JS;
                case ".xml": return CONTENT_TYPE_XML;
                case ".png": return CONTENT_TYPE_PNG;
                case ".jpg": return CONTENT_TYPE_JPG;
                case ".jpeg": return CONTENT_TYPE_JPG;
                case ".gif": return CONTENT_TYPE_GIF;
                case ".xap": return CONTENT_TYPE_XAP;
                case ".xsl": return CONTENT_TYPE_XSL;
                case ".ico": return CONTENT_TYPE_ICO;
                case ".svg": return CONTENT_TYPE_SVG;
                case ".resx": return CONTENT_TYPE_RESX;
                default: return INVALID_CONTENT_TYPE;
            }
        }
    }
}