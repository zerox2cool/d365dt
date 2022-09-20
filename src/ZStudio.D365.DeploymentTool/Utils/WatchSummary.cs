using System;
using System.Collections;
using System.Collections.Generic;

namespace ZD365DT.DeploymentTool.Utils
{
    public class WatchSummary
    {
        Dictionary<string, long> _summary = null;

        public WatchSummary()
        {
            _summary = new Dictionary<string, long>();
            foreach (InstallType t in Enum.GetValues(typeof(InstallType)))
            {
                _summary.Add(t.ToString(), 0);
            }
        }

        public void AddElapsed(InstallType installType, long miliseconds)
        {
            string key = installType.ToString();
            if (_summary.ContainsKey(key))
            {
                _summary[key] += miliseconds;
            }
        }

        public void DisplaySummary()
        {
            Logger.LogInfo("Deployment Elapsed Time Summary");
            long total = 0;
            foreach (var a in _summary)
            {
                if (a.Value > 0)
                {
                    total += a.Value;
                    Logger.LogInfo("'{0}': {1} seconds", a.Key, (decimal)a.Value / 1000);
                }
            }
            Logger.LogInfo("Grand Total: {0} seconds", (decimal)total / 1000);
        }
    }
}