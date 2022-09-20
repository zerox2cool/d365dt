using System;

namespace ZD365DT.DeploymentTool.Utils 
{
    public class CrmAsyncServiceUtil : WindowsServiceUtils
    {
        #region Private Constants

        private const string CRM_ASYNC_SERVICE = "MSCRMAsyncService";

        #endregion Private Constants

        /// <summary>
        /// Returns true if the Microsoft Microsoft CRM Asynchronous Processinghronous Processing Service is installed
        /// </summary>
        /// <returns>true if the Microsoft CRM Asynchronous Processing Service is installed, otherwise false</returns>
        public static bool CrmAsyncServiceInstalled()
        {
            return ServiceInstalled(CRM_ASYNC_SERVICE);
        }

        /// <summary>
        /// Starts the Microsoft CRM Asynchronous Processing Service on the server
        /// </summary>
        public static void StartCrmAsyncService()
        {
            if (!IsServiceRunning(CRM_ASYNC_SERVICE))
            {
                Logger.LogInfo("Attempting to start Microsoft CRM Asynchronous Processing Service...");
                StartService(CRM_ASYNC_SERVICE);
                Logger.LogInfo("Microsoft CRM Asynchronous Processing Service Started.");
            }
        }


        /// <summary>
        /// Stops the Microsoft CRM Asynchronous Processing Service on the server
        /// </summary>
        public static void StopCrmAsyncService()
        {
            if (IsServiceRunning(CRM_ASYNC_SERVICE))
            {
                Logger.LogInfo("Attempting to stop Microsoft CRM Asynchronous Processing Service...");
                StopService(CRM_ASYNC_SERVICE);
                Logger.LogInfo("Microsoft CRM Asynchronous Processing Service Stopped.");
            }
        }
    }
}
