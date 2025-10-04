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
        private string _teams = null;

        public Guid? ProfileId { get; set; }

        [DataMember]
        public string ProfileName { get; set; }

        [DataMember]
        public string Teams
        {
            get
            {
                return _teams;
            }
            set
            {
                _teams = value;
                ParseTeams();
            }
        }

        public List<BusinessUnitTeam> BusinessUnitTeams { get; set; }

        public TeamColumnSecurityProfile()
        {
        }

        public TeamColumnSecurityProfile(string profileName, string teams)
        {
            ProfileName = profileName;
            Teams = teams;

            ParseTeams();
        }

        private void ParseTeams()
        {
            if (!string.IsNullOrEmpty(_teams))
            {
                BusinessUnitTeams = new List<BusinessUnitTeam>();
                var buteamnames = _teams.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var but in buteamnames)
                {
                    BusinessUnitTeams.Add(new BusinessUnitTeam(but));
                }
            }
        }
    }

    public class BusinessUnitTeam
    {
        public Guid? BusinessUnitId { get; set; }

        public Guid? TeamId { get; set; }

        public string BusinessUnitName { get; set; }

        public string TeamName { get; set; }

        public BusinessUnitTeam()
        {
        }

        public BusinessUnitTeam(string buteamname)
        {
            var parts = buteamname.Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                BusinessUnitName = parts[0].Trim();
                TeamName = parts[1].Trim();
            }
            else
            {
                throw new ArgumentException($"The Team name '{buteamname}' is not valid. It should be in the format 'Business Unit Name:Team Name'.");
            }
        }
    }
}