using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ZStudio.D365.DeploymentHelper.Core.Models
{
    [DataContract]
    public class AdxSiteSetting
    {
        [DataMember]
        public string WebsiteId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }

        public AdxSiteSetting()
        {
        }

        public AdxSiteSetting(string websiteId, string name, string val)
        {
            WebsiteId = websiteId;
            Name = name;
            Value = val;
        }
    }
}