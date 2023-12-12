using System;
using System.Collections.Generic;
using ZStudio.D365.DeploymentHelper.Core.Base;

namespace ZStudio.D365.DeploymentHelper.Core.CmdLineTools
{
    [HelperType(nameof(TestConnection))]
    public class TestConnection : HelperToolBase
    {
        public TestConnection(string crmConnectionString, Dictionary<string, object> config, Dictionary<string, string> tokens, bool debugMode) : base(crmConnectionString, config, tokens, debugMode)
        {
        }

        protected override bool OnRun_Implementation(out string exceptionMessage)
        {
            exceptionMessage = string.Empty;

            Log($"{HelperName} on run implementation.");

            return true;
        }
    }
}