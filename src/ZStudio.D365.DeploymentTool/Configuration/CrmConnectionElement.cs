using System;
using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{
    public enum CrmAuthenticationType
    {
        /// <summary>
        /// OnPremise only
        /// </summary>
        AD = 0,

        /// <summary>
        /// For ADFS enabled
        /// </summary>
        IFD = 1,

        /// <summary>
        /// For D365 CE connecting to CDS
        /// </summary>
        OAuth = 2,

        /// <summary>
        /// For D365 CE connecting to CDS
        /// </summary>
        Certificate = 3,

        /// <summary>
        /// For D365 CE connecting to CDS
        /// </summary>
        ClientSecret = 4,

        /// <summary>
        /// Deprecated
        /// </summary>
        Office365 = 5,
    }

    /// <summary>
    /// The class that holds onto each element returned by the configuration manager.
    /// </summary>
    public class CrmConnectionElement : ConfigurationElement
    {
       
        [ConfigurationProperty("crmconnectionstring", IsKey = true, IsRequired = true)]
        public string CrmConnectionString
        {
            get { return (string)base["crmconnectionstring"]; }
            set { base["crmconnectionstring"] = value; }
        }

        [ConfigurationProperty("organization", IsRequired = true)]
        public string OrganizationName
        {
            get { return (string)base["organization"]; }
            set { base["organization"] = value; }
        }

        [ConfigurationProperty("authentication", IsRequired = false, DefaultValue = "OAuth")]
        public string AuthenticationTypeText
        {
            get { return (string)base["authentication"]; }
            set { base["authentication"] = value; }
        }

        [ConfigurationProperty("enforceTls12", IsRequired = false, DefaultValue = "false")]
        public string EnforceTls12
        {
            get { return (string)base["enforceTls12"]; }
            set { base["enforceTls12"] = value; }
        }

        public CrmAuthenticationType AuthenticationType { get; private set; }

        public void SetAuthenticationType(string authType)
        {
            try
            {
                AuthenticationType = (CrmAuthenticationType)Enum.Parse(typeof(CrmAuthenticationType), authType);
            }
            catch (Exception ex)
            {
                throw new Exception($"The authentication value is invalid ({ex.Message}). Only the following value is allowed: AD, IFD, OAuth, Certificate, ClientSecret");
            }
        }
    }
}