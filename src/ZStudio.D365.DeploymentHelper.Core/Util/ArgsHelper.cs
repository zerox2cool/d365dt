using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStudio.D365.DeploymentHelper.Core.Util
{
    public static class ArgsHelper
    {
        public static string MaskCrmConnectionString(string crmConnectionString)
        {
            if (crmConnectionString.ToLower().Contains("password="))
            {
                return crmConnectionString.Substring(0, crmConnectionString.ToLower().IndexOf("password=")).Trim();
            }

            if (crmConnectionString.ToLower().Contains("clientsecret="))
            {
                return crmConnectionString.Substring(0, crmConnectionString.ToLower().IndexOf("clientsecret=")).Trim();
            }

            if (crmConnectionString.ToLower().Contains("thumbprint="))
            {
                return crmConnectionString.Substring(0, crmConnectionString.ToLower().IndexOf("thumbprint=")).Trim();
            }

            return crmConnectionString;
        }
    }
}