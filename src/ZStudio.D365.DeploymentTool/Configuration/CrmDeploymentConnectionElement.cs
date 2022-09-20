using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ZD365DT.DeploymentTool.Configuration
{

    /// <summary>
    /// The class that holds onto each element returned by the configuration manager.
    /// </summary>
    public class CrmDeploymentServiceElement : ConfigurationElement
    {
       
        [ConfigurationProperty("serviceurl", IsKey = true, IsRequired = true)]
        public string ServiceUrl
        {
            get { return (string)base["serviceurl"]; }
            set { base["serviceurl"] = value; }
        }


        [ConfigurationProperty("username", IsRequired = true)]
        public string Username
        {
            get { return (string)base["username"]; }
            set { base["username"] = value; }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get { return (string)base["password"]; }
            set { base["password"] = value; }
        }

        [ConfigurationProperty("domain", IsRequired = true)]
        public string Domain
        {
            get { return (string)base["domain"]; }
            set { base["domain"] = value; }
        }
        
               
    }
}
