using System;
using System.IO;
using System.Threading;
using ZD365DT.DeploymentTool.Utils;
using ZD365DT.DeploymentTool.Context;
using ZD365DT.DeploymentTool.Configuration;

namespace ZD365DT.DeploymentTool
{
    [Flags]
    public enum ExecutionReturnCode : int
    {
        Success = 0,
        CustomizationsFailed = 1,
        ReportsFailed = 2,
        PluginsWorkflowFailed = 4,
        WebsiteFailed = 8,
        WindowsServiceFailed = 16,
        DuplicateDetectionFailed = 32,
        SqlFailed = 64,
        ExternalActionFailed = 128,
        CopyFailed = 256,
      
        DataImportFailed = 512,
        UnknownFailure = 1024,
        InvalidCommandline = 2000,
        InvalidConfiguration = 2048,
        IsvConfigMergeFailed = 4096,
        SiteMapMergeFailed = 65536,
        AssemblyInstallFailed = 8192,
        DeleteFailed = 16384,
        DeleteDataFailed = 32768,
        IncrementSolutionBuildVersion = 65536,
        AssignSecurityRole = 131072,
        AssignWorkflowRole = 262144,
        ActivateWorkflowRole = 524288,
        StopService = 1048576,
        StartService = 2097152,
        PluginXmlExport = 4194304,
        RibbonFailed = 8388608,
        SitemapFailed = 16777216,
        DeleteExportFailed = 33554432,
        OrgCreateDelete = 67108864,
        DataMapFailed = 134217728,
        ThemeFailed = 16999216,
        PublisherFailed = 16999288,
        SolutionFailed = 16999289,
        EntityDefFailed = 16999300,
        GlobalOpDefFailed = 16999400,
        ManyToManyRelationDefFailed = 16999405,
        PatchCustomizationXmlFailed = 16999410
    }

    /// <summary>
    /// The list of avaliable installation type in the Deployment Tool that is currently being stopwatch and summarized. If no specific type is defined, it will all be summarized as Other Action.
    /// </summary>
    public enum InstallType : int
    {
        OtherAction = 0,
        EntityDefAction = 5,
        GlobalOpDefAction = 10,
        ThemeAction = 15,
        PublisherAction = 20,
        SolutionAction = 25,
        CustomizationAction = 30,
        WebResourceAction = 35,
        PluginWorkflowAction = 40,
        ManyToManyRelationDefAction = 45,
        DuplicateDetectionAction = 50,
    }

    public abstract class InstallAction : IComparable<InstallAction>
    {
        #region Private Variables
        private readonly IDeploymentContext context;
        #endregion Private Variables

        #region Static Constructor
        static InstallAction()
        {
            //authenticationType = authType;
        }
        #endregion Static Constructor

        #region Constructor
        public InstallAction(IDeploymentContext context)
        {
            this.context = context;

            try
            {
                if (!Directory.Exists(BackupDirectory))
                {
                    Directory.CreateDirectory(BackupDirectory);
                }
            }
            catch (Exception)
            {
                //do nothing
            }
        }
        #endregion Constructor

        #region Aplication Settings
        protected virtual string BackupDirectory { get { return Logger.BackupDirectory; } }
        #endregion Application Settings

        #region Public Methods
        public ExecutionReturnCode Execute()
        {
            try
            {
                RunBackupAction();
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Error occured running backup: {0}. Install will continue", ex.Message);
            } 
            try
            {
                if (context.DeploymentAction == DeploymentAction.Install)
                {   
                    RunInstallAction();
                }
                else
                {
                    RunUninstallAction();
                }
                return ExecutionReturnCode.Success;
            }
            catch (Exception ex)
            {
                try
                {
                    //if (authenticationType == CrmAuthenticationType.OnPremise)
                    //{
                    //    IisUtils.StartIis();
                    //    CrmAsyncServiceUtil.StartCrmAsyncService();
                    //}
                }
                catch
                {
                    // Do nothing, if this casued the initial error then the error will already be being logged
                }
                Logger.LogException(ex);
                return GetExecutionErrorReturnCode();
            }
        }

        public virtual string GetExecutionErrorMessage()
        {
            return "An unknown error occured";
        }

        /// <summary>
        /// When overridden, specifies the execution index (order) for the InsallAction
        /// </summary>
        public abstract int Index { get; }

        /// <summary>
        /// The name of the Installation Action
        /// </summary>
        public abstract string ActionName { get; }

        /// <summary>
        /// The Installation Action type, used to determine where to sum the elapsed time to.
        /// </summary>
        public abstract InstallType ActionType { get; }

        /// <summary>
        /// Return TRUE when stopwatch is being timed within the Action, the outer loop will not add the elapsed time to the summary total.
        /// </summary>
        public abstract bool EnableStopwatchDetail { get; }
        #endregion Public Methods

        protected IDeploymentContext Context { get { return context; } }

        protected virtual ExecutionReturnCode GetExecutionErrorReturnCode()
        {
            return ExecutionReturnCode.UnknownFailure;
        }

        protected abstract void RunBackupAction();

        protected abstract void RunInstallAction();

        protected abstract void RunUninstallAction();

        #region IComparable Members
        public int CompareTo(InstallAction other)
        {
            return this.Index.CompareTo(other.Index);
        }
        #endregion
    }
}