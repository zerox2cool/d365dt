using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace ZD365DT.DeploymentTool.Utils
{
    public class WindowsServiceUtils
    {
        /// <summary>
        /// Stops and re-starts a service
        /// </summary>
        /// <param name="serviceName">The name of the service to stop and start</param>
        internal static void BounceService(string serviceName)
        {
            StopService(serviceName);
            StartService(serviceName);
        }

        /// <summary>
        /// Stops and re-starts a service
        /// </summary>
        /// <param name="serviceName">The name of the service to stop and start</param>
        internal static void BounceService(string serviceName,string serverName)
        {
            StopService(serviceName, serverName);
            StartService(serviceName, serverName);
        }

        /// <summary>
        /// Checks whether a service is installed
        /// </summary>
        /// <param name="serviceName">The name of the service</param>
        /// <returns>true is the service is installed, else false</returns>
        internal static bool ServiceInstalled(string serviceName)
        {
            ServiceController service = null;
            try
            {
                service = new ServiceController(serviceName);
                // this will throw an Exception if the service is not present
                ServiceControllerStatus status = service.Status; 
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (service != null)
                {
                    service.Dispose();
                }
            }         
        }

        /// <summary>
        /// Checks whether a service is installed
        /// </summary>
        /// <param name="serviceName">The name of the service</param>
        /// <param name="serverName">The name of the Machine</param>
        /// <returns>true is the service is installed, else false</returns>
        internal static bool ServiceInstalled(string serviceName,string serverName)
        {
            ServiceController service = null;
            try
            {
                service = new ServiceController(serviceName,serverName);
                // this will throw an Exception if the service is not present
                ServiceControllerStatus status = service.Status;
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (service != null)
                {
                    service.Dispose();
                }
            }
        }

        /// <summary>
        /// Returns true if the service is running, otherwise false.
        /// </summary>
        /// <param name="serviceName">The name of the service to start</param>  
        /// <returns>true if the service is running, else false</returns>
        internal static bool IsServiceRunning(string serviceName)
        {
            bool result = false;
            if (ServiceInstalled(serviceName))
            {
                using (ServiceController service = new ServiceController(serviceName))
                {
                    result = service.Status == ServiceControllerStatus.Running;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true if the service is running, otherwise false.
        /// </summary>
        /// <param name="serviceName">The name of the service to start</param>  
        /// <param name="serverName">The name of the Machine</param>
        /// <returns>true if the service is running, else false</returns>
        internal static bool IsServiceRunning(string serviceName,string serverName)
        {
            bool result = false;
            if (ServiceInstalled(serviceName,serverName))
            {
                using (ServiceController service = new ServiceController(serviceName,serverName))
                {
                    result = service.Status == ServiceControllerStatus.Running;
                }
            }
            return result;
        }

        /// <summary>
        /// Starts a service
        /// </summary>
        /// <param name="serviceName">The name of the service to start</param>
        internal static void StartService(string serviceName)
        {
            if (ServiceInstalled(serviceName))
            {
                using (ServiceController service = new ServiceController(serviceName))
                {
                    do
                    {
                        service.Refresh();
                    }
                    while
                    (
                        service.Status == ServiceControllerStatus.ContinuePending ||
                        service.Status == ServiceControllerStatus.PausePending ||
                        service.Status == ServiceControllerStatus.StartPending ||
                        service.Status == ServiceControllerStatus.StopPending
                    );
                    if (service.Status == ServiceControllerStatus.Stopped)
                    {
                        service.Start();
                    }
                    else if (service.Status == ServiceControllerStatus.Paused)
                    {
                        service.Continue();
                    }
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(2.0));
                }
            }
        }

        /// <summary>
        /// Starts a service
        /// </summary>
        /// <param name="serviceName">The name of the service to start</param>
        /// <param name="serverName">The name of the Machine</param>
        internal static void StartService(string serviceName,string serverName)
        {
            if (ServiceInstalled(serviceName,serverName))
            {
                using (ServiceController service = new ServiceController(serviceName,serverName))
                {
                    do
                    {
                        service.Refresh();
                    }
                    while
                    (
                        service.Status == ServiceControllerStatus.ContinuePending ||
                        service.Status == ServiceControllerStatus.PausePending ||
                        service.Status == ServiceControllerStatus.StartPending ||
                        service.Status == ServiceControllerStatus.StopPending
                    );
                    if (service.Status == ServiceControllerStatus.Stopped)
                    {
                        service.Start();
                    }
                    else if (service.Status == ServiceControllerStatus.Paused)
                    {
                        service.Continue();
                    }
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(2.0));
                }
            }
        }

        /// <summary>
        /// Stops a service
        /// </summary>
        /// <param name="serviceName">The name of the service to stop</param>
        
        internal static void StopService(string serviceName)
        {
            using(ServiceController service = new ServiceController(serviceName))
            {
                if (service != null)
                {
                    do
                    {
                        service.Refresh();
                    }
                    while
                    (
                        service.Status == ServiceControllerStatus.ContinuePending ||
                        service.Status == ServiceControllerStatus.PausePending ||
                        service.Status == ServiceControllerStatus.StartPending ||
                        service.Status == ServiceControllerStatus.StopPending
                    );
                    if (ServiceControllerStatus.Running == service.Status ||
                        ServiceControllerStatus.Paused == service.Status)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(2.0));
                    }
                }
            }

            // Wait upto 2 minutes for the process to exit as well
		    Process[] procs = Process.GetProcessesByName(serviceName);
            foreach (Process proc in procs)
            {
                Logger.LogInfo("Waiting for process {0} (id {1}) to end...", proc.ProcessName, proc.Id);
                proc.WaitForExit(TimeSpan.FromMinutes(2.0).Milliseconds);
		    }
            // Wait a further 5 seconds for the service to release any remaining resources
            Thread.Sleep(TimeSpan.FromSeconds(5.0));
        }

        /// <summary>
        /// Stops a service
        /// </summary>
        /// <param name="serviceName">The name of the service to stop</param>
        /// <param name="serverName">The name of the Machine</param>
        internal static void StopService(string serviceName,string serverName)
        {
            using (ServiceController service = new ServiceController(serviceName,serverName))
            {
                if (service != null)
                {
                    do
                    {
                        service.Refresh();
                    }
                    while
                    (
                        service.Status == ServiceControllerStatus.ContinuePending ||
                        service.Status == ServiceControllerStatus.PausePending ||
                        service.Status == ServiceControllerStatus.StartPending ||
                        service.Status == ServiceControllerStatus.StopPending
                    );
                    if (ServiceControllerStatus.Running == service.Status ||
                        ServiceControllerStatus.Paused == service.Status)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(2.0));
                    }
                }
            }

            // Wait upto 2 minutes for the process to exit as well
            Process[] procs = Process.GetProcessesByName(serviceName,serverName);
            foreach (Process proc in procs)
            {
                Logger.LogInfo("Waiting for process {0} (id {1}) to end...", proc.ProcessName, proc.Id);
                proc.WaitForExit(TimeSpan.FromMinutes(2.0).Milliseconds);
            }
            // Wait a further 5 seconds for the service to release any remaining resources
            Thread.Sleep(TimeSpan.FromSeconds(5.0));
        }
    }
}
