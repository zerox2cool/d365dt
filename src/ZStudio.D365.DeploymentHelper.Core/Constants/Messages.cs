using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStudio.D365.DeploymentHelper.Core.Constants
{
    public class Messages
    {
        public class WarningMessages
        {
            public const string COMPONENT_WITH_FLOW = "NOTE: When activating a component in D365 with system cloud flow as part of the component for the FIRST time, the helper will fail 'Cannot create a new flow without specific user context.' when running as service principals. The component need to be manually activated for the first time.";
        }
    }
}