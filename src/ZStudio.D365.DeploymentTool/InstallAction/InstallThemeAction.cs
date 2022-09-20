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
    public class InstallThemeAction : InstallAction
    {
        private readonly ThemeCollection themeCollection;
        private readonly WebServiceUtils utils;

        public override string ActionName { get { return "Theme"; } }
        public override InstallType ActionType { get { return InstallType.ThemeAction; } }
        public override bool EnableStopwatchDetail { get { return false; } }

        public InstallThemeAction(ThemeCollection collections, IDeploymentContext context, WebServiceUtils webServiceUtils) : base(context)
        {
            themeCollection = collections;
            utils = webServiceUtils;
        }

        protected override string BackupDirectory
        {
            get { return Path.Combine(base.BackupDirectory, "Theme"); }
        }

        public override int Index { get { return themeCollection.ElementInformation.LineNumber; } }

        public override string GetExecutionErrorMessage()
        {
            if (Context.DeploymentAction == DeploymentAction.Install)
            {
                return "Failed to Create/Update the Theme.";
            }
            return "Failed to Create/Update the Theme.";
        }

        protected override ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.ThemeFailed;
        }

        protected override void RunBackupAction()
        {
            //nothing to backup
        }

        protected override void RunInstallAction()
        {
            Logger.LogInfo("Installing CRM {0} Collection...", ActionName);

            bool anyErrors = false;
            string errorMessage = string.Empty;
            foreach (ThemeElement element in themeCollection)
            {
                try
                {
                    InstallTheme(element);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    anyErrors = true;
                }
            }

            if (anyErrors)
            {
                throw new Exception(string.Format("There was an error executing CRM Theme. Message: {0}", errorMessage));
            }
        }

        private void InstallTheme(ThemeElement element)
        {
            string themeSettingFilePath = Path.GetFullPath(IOUtils.ReplaceStringTokens(element.ThemeSettingFile, Context.Tokens));
            bool publishTheme = element.PublishTheme;
            
            Logger.LogInfo(" ");
            Logger.LogInfo(" Installing CRM Theme from the file {0}...", themeSettingFilePath);
            if (File.Exists(themeSettingFilePath))
            {
                XmlNamespaceManager nsmgr = null;
                XmlDocument doc = ConfigXmlHelper.LoadXmlDoc(themeSettingFilePath, out nsmgr, ConfigXml.Namespace.THEME_XMLNS);
                if (doc == null)
                    throw new Exception("  Unable to load the CRM Theme file into a XML document.");
                else
                {
                    //get theme name
                    XmlNodeList nodelist = ConfigXmlHelper.GetXmlNodeList(doc, ConfigXml.Theme.NODE_NAME, nsmgr);
                    if (nodelist != null && nodelist.Count > 0)
                    {
                        XmlNode node = nodelist[0];
                        string themeName = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.NAME, nsmgr);
                        if (!string.IsNullOrEmpty(themeName))
                        {
                            bool updateExisting = false;
                            Entity enTheme = GetThemeRecord(utils.Service, themeName);
                            if (enTheme != null)
                            {
                                //update existsing theme
                                updateExisting = true;
                                Logger.LogInfo("  Found the theme '{0}' in the system, proceed to update the theme.", themeName);
                            }
                            else
                            {
                                //create new theme record
                                enTheme = new Entity("theme");
                                enTheme["name"] = themeName;
                                Logger.LogInfo("  Theme '{0}' does not exists, proceed to create the theme.", themeName);
                            }

                            #region Set Theme Values
                            //set the values into the entity
                            if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.LOGO, nsmgr)))
                            {
                                //get the GUID for the web resource logo
                                Guid? webresourceid = WebResourceInternal.GetWebResourceId(utils, ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.LOGO, nsmgr));
                                if (webresourceid == null)
                                {
                                    Logger.LogError("  The web resource with the name '{0}' for the theme logo is not found.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.LOGO, nsmgr));
                                    throw new Exception("Logo for the CRM Theme not found.");
                                }
                                else
                                {
                                    Logger.LogInfo("  Set theme Logo to '{0}' ({1}).", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.LOGO, nsmgr), webresourceid.Value.ToString());
                                    enTheme["logoid"] = new EntityReference("webresource", webresourceid.Value);
                                }
                            }

                            #region For 2016 (v8)
                            if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.LOGOTOOLTIP, nsmgr)))
                            {
                                Logger.LogInfo("  Set theme Logo Tooltip to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.LOGOTOOLTIP, nsmgr));
                                enTheme["logotooltip"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.LOGOTOOLTIP, nsmgr);
                            }

                            if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.NAVBARCOLOR, nsmgr)))
                            {
                                Logger.LogInfo("  Set theme Navigation Bar Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.NAVBARCOLOR, nsmgr));
                                enTheme["navbarbackgroundcolor"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.NAVBARCOLOR, nsmgr);
                            }

                            if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.NAVBARSHELFCOLOR, nsmgr)))
                            {
                                Logger.LogInfo("  Set theme Navigation Shelf Bar Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.NAVBARSHELFCOLOR, nsmgr));
                                enTheme["navbarshelfcolor"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.NAVBARSHELFCOLOR, nsmgr);
                            }

                            if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.HEADERCOLOR, nsmgr)))
                            {
                                Logger.LogInfo("  Set theme Header Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.HEADERCOLOR, nsmgr));
                                enTheme["headercolor"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.HEADERCOLOR, nsmgr);
                            }

                            if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.GLOBALLINKCOLOR, nsmgr)))
                            {
                                Logger.LogInfo("  Set theme Global Link Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.GLOBALLINKCOLOR, nsmgr));
                                enTheme["globallinkcolor"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.GLOBALLINKCOLOR, nsmgr);
                            }

                            if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.SELECTEDLINKEFFECT, nsmgr)))
                            {
                                Logger.LogInfo("  Set theme Selected Link Effect Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.SELECTEDLINKEFFECT, nsmgr));
                                enTheme["selectedlinkeffect"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.SELECTEDLINKEFFECT, nsmgr);
                            }

                            if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.HOVERLINKEFFECT, nsmgr)))
                            {
                                Logger.LogInfo("  Set theme Hover Link Effect Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.HOVERLINKEFFECT, nsmgr));
                                enTheme["hoverlinkeffect"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.HOVERLINKEFFECT, nsmgr);
                            }

                            if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.PROCESSCONTROLCOLOR, nsmgr)))
                            {
                                Logger.LogInfo("  Set theme Process Control Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.PROCESSCONTROLCOLOR, nsmgr));
                                enTheme["processcontrolcolor"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.PROCESSCONTROLCOLOR, nsmgr);
                            }

                            if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.DEFAULTENTITYCOLOR, nsmgr)))
                            {
                                Logger.LogInfo("  Set theme Default Entity Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.DEFAULTENTITYCOLOR, nsmgr));
                                enTheme["defaultentitycolor"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.DEFAULTENTITYCOLOR, nsmgr);
                            }

                            if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.DEFAULTCUSTOMENTITYCOLOR, nsmgr)))
                            {
                                Logger.LogInfo("  Set theme Default Custom Entity Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.DEFAULTCUSTOMENTITYCOLOR, nsmgr));
                                enTheme["defaultcustomentitycolor"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.DEFAULTCUSTOMENTITYCOLOR, nsmgr);
                            }

                            if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.CONTROLSHADE, nsmgr)))
                            {
                                Logger.LogInfo("  Set theme Control Shade Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.CONTROLSHADE, nsmgr));
                                enTheme["controlshade"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.CONTROLSHADE, nsmgr);
                            }

                            if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.CONTROLBORDER, nsmgr)))
                            {
                                Logger.LogInfo("  Set theme Control Border Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.CONTROLBORDER, nsmgr));
                                enTheme["controlborder"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.CONTROLBORDER, nsmgr);
                            }
                            #endregion For 2016 (v8)

                            #region New for D365-2018 (v9)
                            if (utils.CurrentOrgContext.MajorVersion >= 9)
                            {
                                //only available on CRM online post Dec-2017, v9
                                if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.MAINCOLOR, nsmgr)))
                                {
                                    Logger.LogInfo("  Set theme Main Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.MAINCOLOR, nsmgr));
                                    enTheme["maincolor"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.MAINCOLOR, nsmgr);
                                }

                                if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.ACCENTCOLOR, nsmgr)))
                                {
                                    Logger.LogInfo("  Set theme Accent Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.ACCENTCOLOR, nsmgr));
                                    enTheme["accentcolor"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.ACCENTCOLOR, nsmgr);
                                }

                                if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.PAGEHEADERFILLCOLOR, nsmgr)))
                                {
                                    Logger.LogInfo("  Set theme Page Header Fill Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.PAGEHEADERFILLCOLOR, nsmgr));
                                    enTheme["pageheaderbackgroundcolor"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.PAGEHEADERFILLCOLOR, nsmgr);
                                }

                                if (!string.IsNullOrEmpty(ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.PANELHEADERFILLCOLOR, nsmgr)))
                                {
                                    Logger.LogInfo("  Set theme Panel Header Fill Color to '{0}'.", ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.PANELHEADERFILLCOLOR, nsmgr));
                                    enTheme["panelheaderbackgroundcolor"] = ConfigXmlHelper.GetXmlNodeAttributeValue(node, ConfigXml.Theme.Element.PANELHEADERFILLCOLOR, nsmgr);
                                }
                            }
                            #endregion New for D365-2018 (v9)
                            #endregion Set Theme Values

                            Guid? themeId = null;
                            if (updateExisting)
                            {
                                themeId = enTheme.Id;
                                utils.Service.Update(enTheme);
                            }
                            else
                            {
                                themeId = utils.Service.Create(enTheme);
                            }
                            if (themeId != null)
                                Logger.LogInfo("  ThemeID '{0}'.", themeId.Value.ToString());
                            else
                                throw new Exception("Fail to retrieve CRM theme GUID.");

                            if (element.PublishTheme)
                            {
                                //publish the theme
                                PublishThemeRequest req = new PublishThemeRequest();
                                req.Target = new EntityReference("theme", themeId.Value);

                                Logger.LogInfo("  Publishing theme...");
                                PublishThemeResponse resp = (PublishThemeResponse)utils.Service.Execute(req);
                                if (resp != null)
                                {
                                    Logger.LogInfo("  ThemeID '{0}' published...", themeId.Value.ToString());
                                }
                            }
                        }
                        else
                            throw new Exception("The CRM theme name is required, please make sure the name node has a value.");
                    }
                    else
                        throw new Exception("Unable to load the Theme node from the XML file.");
                }
            }
            else
            {
                Logger.LogError("  CRM Theme XML '{0}' was not found.", themeSettingFilePath);
                throw new Exception("CRM Theme file not found.");
            }
        }

        protected override void RunUninstallAction()
        {
            //no uninstall action
            Logger.LogWarning("Uninstall not supported for {0}.", ActionName);
        }

        private Entity GetThemeRecord(IOrganizationService svc, string name)
        {
            Entity result = null;

            QueryExpression q = new QueryExpression("theme");
            q.ColumnSet = new ColumnSet(true);
            q.Criteria = new FilterExpression(LogicalOperator.And);
            q.Criteria.AddCondition(new ConditionExpression("name", ConditionOperator.Equal, name));
            EntityCollection output = svc.RetrieveMultiple(q);
            if (output != null && output.Entities.Count > 0)
            {
                if (output.Entities.Count == 1)
                    result = output[0];
                else
                    throw new Exception(string.Format("There are more than one CRM theme with the name {0} in the system.", name));
            }

            return result;
        }
    }
}