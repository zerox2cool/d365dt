using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZStudio.D365.Shared.Framework.Util;

namespace ZStudio.D365.DeploymentHelper.Core.Base
{
    /// <summary>
    /// Interface for the Helper Tool Logic that performs the required functionalities for Deployment Helper.
    /// </summary>
    public interface IHelperToolLogic
    {
        /// <summary>
        /// The helper name.
        /// </summary>
        string HelperName { get; set; }

        /// <summary>
        /// Indicate the program is running in debug mode.
        /// </summary>
        bool DebugMode { get; set; }

        /// <summary>
        /// CRM connection string
        /// </summary>
        string CrmConnectionString { get; }

        /// <summary>
        /// Configuration in serialized JSON.
        /// </summary>
        string ConfigJson { get; }

        /// <summary>
        /// CRM Connector
        /// </summary>
        CrmConnector CrmConn { get; }

        /// <summary>
        /// The <see cref="IOrganizationService"/> to D365.
        /// </summary>
        IOrganizationService OrgService { get; }

        /// <summary>
        /// The logger storage.
        /// </summary>
        StringBuilder Logger { get; set; }

        /// <summary>
        /// Variable Token replacement value for the data.
        /// </summary>
        Dictionary<string, string> Tokens { get; }

        /// <summary>
        /// Logs from the execution logger <see cref="Logger"/>.
        /// </summary>
        string Logs { get; }

        /// <summary>
        /// The result.
        /// </summary>
        bool? IsSuccess { get; }

        /// <summary>
        /// This main method to execute the helper operation to be implemented by the base class.
        /// </summary>
        /// <returns>Returns the execution result</returns>
        bool Run(out string exceptionMessage);

        /// <summary>
        /// Get the Logs from the Logger storage.
        /// </summary>
        /// <param name="isClear"></param>
        string GetLog(bool isClear = false);

        /// <summary>
        /// Log a text to the Logger storage.
        /// </summary>
        /// <param name="text"></param>
        void Log(string text);

        /// <summary>
        /// Log a formatted text to the Logger storage.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="p"></param>
        void Log(string format, params object[] p);

        /// <summary>
        /// Log a debug text to the Logger storage.
        /// </summary>
        /// <param name="text"></param>
        void Debug(string text);
    }
}