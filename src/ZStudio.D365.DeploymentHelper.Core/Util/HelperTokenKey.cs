using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStudio.D365.DeploymentHelper.Core.Util
{
    public static class HelperTokenKey
    {
        public const string ORGFRIENDLYNAME_TOKEN = "@orgfriendlyname@";
        public const string ORGID_TOKEN = "@orgid@";
        public const string ORGTENANTID_TOKEN = "@orgtenantid@";
        public const string ORGSERVICEURL_TOKEN = "@orgserviceurl@";
        public const string ORGDATAURL_TOKEN = "@orgdataurl@";
        public const string ORGWEBAPPURL_TOKEN = "@orgweburl@";

        public const string ROOTBUNAME_TOKEN = "@rootbuname@";
        public const string ROOTBUID_TOKEN = "@rootbuid@";

        public const string LOGONUSERID_TOKEN = "@logonuserid@";
        public const string LOGONUSEREMAIL_TOKEN = "@logonuseremail@";
        public const string LOGONUSERFULLNAME_TOKEN = "@logonuserfullname@";
    }
}