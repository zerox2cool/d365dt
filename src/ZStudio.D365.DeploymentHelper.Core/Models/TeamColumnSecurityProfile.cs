using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ZStudio.D365.DeploymentHelper.Core.Models
{
    [DataContract]
    public class TeamColumnSecurityProfile
    {
        [DataMember]
        public string ProfileName { get; set; }

        [DataMember]
        public string Teams { get; set; }

        public TeamColumnSecurityProfile()
        {
        }

        public TeamColumnSecurityProfile(string profileName, string teams)
        {
            ProfileName = websiteId;
            Teams = teams;
        }
    }
}