using System;
using System.Configuration;
using System.Collections.Specialized;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool.Context
{
    public enum DeploymentType
    {
        Install,
        Uninstall
    }

    public interface IDeploymentContext
    {
        void RefreshTokensFromCrm();

        DeploymentAction DeploymentAction { get; }

        StringDictionary Tokens { get; }

        DeploymentConfigurationSection DeploymentConfig { get; }
    }
}
