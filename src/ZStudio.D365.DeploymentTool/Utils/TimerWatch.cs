using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ZD365DT.DeploymentTool.Utils
{
    public class TimerWatch
    {
        private Stopwatch _watch = null;
        private string _subject = string.Empty;
        private InstallType _type = InstallType.OtherAction;
        private bool _addtosummary = false;
        private WatchSummary _summary = null;
        private bool Watch = false;

        public TimerWatch(string subject, InstallType installType, bool addToSummary, WatchSummary summary = null)
        {
            _watch = new Stopwatch();
            _subject = subject;
            _type = installType;
            _addtosummary = addToSummary;
            _summary = summary;

            try
            {
                Watch = DeployCrm.Stopwatch;
            }
            catch (Exception)
            {
                Watch = false;
            }
        }

        public void Start()
        {
            _watch.Start();
        }

        public long Restart(string subject)
        {
            //stop and log first
            long elapsed = Stop();

            //change subject and restart the clock
            _subject = subject;
            _watch.Restart();

            return elapsed;
        }

        public long Stop()
        {
            _watch.Stop();

            if (Watch)
                Logger.LogInfo("Elapsed Time for '{0}' is {1} seconds...", _subject, (decimal)_watch.ElapsedMilliseconds / 1000);

            if (_addtosummary && _summary != null)
            {
                //add time to summary
                _summary.AddElapsed(_type, _watch.ElapsedMilliseconds);
            }

            return _watch.ElapsedMilliseconds;
        }
    }
}