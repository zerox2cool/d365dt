using System;
using System.Collections.Generic;
using ZStudio.D365.DeploymentHelper.Core.Base;

namespace ZStudio.D365.DeploymentHelper.Core.CmdLineTools
{
    [HelperType(nameof(TestConnection))]
    public class TestConnection : HelperToolBase
    {
        public TestConnection(string crmConnectionString, string configJson, Dictionary<string, string> tokens, bool debugMode, int debugSleep) : base(crmConnectionString, configJson, tokens, debugMode, debugSleep)
        {
        }

        protected override bool OnRun_Implementation(out string exceptionMessage)
        {
            exceptionMessage = string.Empty;

            Log($"{HelperName} on run implementation.");
            Log(LOG_SEGMENT);
            Log($"CRM Connection String is valid.");

            return true;
        }
    }
}