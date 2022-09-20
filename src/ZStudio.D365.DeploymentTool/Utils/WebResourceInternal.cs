using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ZD365DT.DeploymentTool.Utils
{
    public class WebResourceInternal
    {
        private const string KEY_FORMAT = "{0}-{1}";

        #region Static Variables
        private static Dictionary<string, Guid> _webresourceCache = new Dictionary<string, Guid>();
        #endregion Static Variables

        public string path { get; set; }
        public string content { get; set; }
        public string displayName { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public int type { get; set; }
        public enum WebResourceType
        {
            HTML = 1,
            CSS = 2,
            JS = 3,
            XML = 4,
            PNG = 5,
            JPG = 6,
            GIF = 7,
            XAP = 8,
            XSL = 9,
            ICO = 10
        }

        /// <summary>
        /// Return the GUID of a web resource in CRM by searching on the name (cached)
        /// </summary>
        /// <param name="svc"></param>
        /// <param name="webResourceName"></param>
        /// <returns></returns>
        public static Guid? GetWebResourceId(WebServiceUtils util, string webResourceName)
        {
            webResourceName = webResourceName.ToLower();
            string key = string.Format(KEY_FORMAT, util.CurrentOrgContext.OrganizationId, webResourceName);
            if (_webresourceCache.ContainsKey(key))
            {
                return _webresourceCache[key];
            }
            else
            {
                QueryByAttribute qba = new QueryByAttribute("webresource");
                qba.Attributes.AddRange("name");
                qba.Values.AddRange(webResourceName);
                qba.ColumnSet = new ColumnSet("webresourceid");

                EntityCollection retrievedWrs = util.Service.RetrieveMultiple(qba);
                Guid? webresourceid = null;
                if (retrievedWrs.Entities.Count > 0)
                {
                    Entity retrievedWr = retrievedWrs.Entities[0];
                    webresourceid = retrievedWr.Id;
                    _webresourceCache.Add(key, webresourceid.Value);
                }
                return webresourceid;
            }
        }
    }
}