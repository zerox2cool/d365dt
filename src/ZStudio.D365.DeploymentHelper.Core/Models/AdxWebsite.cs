using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ZStudio.D365.DeploymentHelper.Core.Models
{
    [DataContract]
    public class AdxWebsite
    {
        [DataMember]
        public string WebsiteId { get; set; }

        [DataMember]
        public string PrimaryDomainName { get; set; }

        public AdxWebsite()
        {
        }

        public AdxWebsite(string websiteId, string primaryDomainName)
        {
            WebsiteId = websiteId;
            PrimaryDomainName = primaryDomainName;
        }
    }
}