using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZD365DT.DeploymentTool.Utils.CrmMetadata
{
    public enum InstallationStatus
    {
        New = 5,
        Installed = 10,
        Skipped = 15,
        Error = 20,
        DependencyError = 25
    }

    public enum InstallationAction
    {
        NoAction = 0,
        Create = 1,
        Update = 2,
        Delete = 3
    }

    public abstract class CrmMetadataObject
    {
        public InstallationStatus Status { get; set; }
        public string ExceptionMessage { get; set; }
        public string LogMessage { get; set; }

        public Guid? MetadataId { get; set; }
        public InstallationAction? Action { get; set; }

        public CrmMetadataObject()
        {
            Status = InstallationStatus.New;
            Action = InstallationAction.Create;
        }
    }
}