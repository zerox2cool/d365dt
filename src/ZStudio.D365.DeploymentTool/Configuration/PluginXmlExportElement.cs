using System.Configuration;

namespace ZD365DT.DeploymentTool.Configuration
{

    /// <summary>
    /// The class that holds onto each element returned by the configuration manager.
    /// </summary>
    public class PluginXmlExportElement : ConfigurationElement
    {

        [ConfigurationProperty("solution", IsKey = true, IsRequired = true)]
        public string SolutionName
        {
            get { return (string)base["solution"]; }
            set { base["solution"] = value; }
        }

        [ConfigurationProperty("filename", IsRequired = true)]
        public string FileName
        {
            get { return (string)base["filename"]; }
            set { base["filename"] = value; }
        }       

    }
}
