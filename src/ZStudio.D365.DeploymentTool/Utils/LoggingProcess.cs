using System;
using System.Diagnostics;
using System.Threading;

namespace ZD365DT.DeploymentTool.Utils
{
    public class LoggingProcess
    {
        private Process process;
        private Thread outputThread;
        private Thread errorThread;
        private bool stopLogging = false;
        private string fileName;

        public int Execute(string fileName, string arguments, string workingDirectory)
        {
            return Execute(fileName, arguments, workingDirectory, true);
        }

        public int Execute(string fileName, string arguments, string workingDirectory, bool logOutput)
        {
            this.fileName = fileName;

            int returnCode = -1;
            try
            {
                Logger.LogInfo("Executing: {0} {1}.  Working directory:{2}", fileName, arguments, workingDirectory);

                stopLogging = false;

                process = new Process();
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                if (logOutput)
                {
                    outputThread = new Thread(OutputThreadRun);
                    errorThread = new Thread(ErrorThreadRun);
                }

                process.Start();

                try
                {
                    Logger.LogInfo("  Setting the priority of the process to '{0}'", ProcessPriorityClass.AboveNormal);
                    process.PriorityClass = ProcessPriorityClass.AboveNormal;
                }
                catch
                {
                    Logger.LogWarning("Failed to set the priority of the process to '{0}'", ProcessPriorityClass.AboveNormal);
                }

                if (logOutput)
                {
                    outputThread.Start();
                    errorThread.Start();
                }

                process.WaitForExit();
                returnCode = process.ExitCode;
                stopLogging = true;

                if (logOutput)
                {
                    outputThread.Join();
                    errorThread.Join();
                }
            }
            finally
            {
                outputThread = null;
                errorThread = null;
            }
            return returnCode;
        }


        private void OutputThreadRun()
        {
            try 
            {
                string logString = String.Empty;
                while (!stopLogging && !process.HasExited)
                {
                    logString += process.StandardOutput.ReadToEnd();
                    Thread.Sleep(100);
                }
                if (logString != String.Empty)
                {
                    Logger.LogInfo("Output from: {0}{1}{2}", fileName, Environment.NewLine, logString.Trim());
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Error capturing standard output of process...");
                Logger.LogException(ex);
            }
        }

        private void ErrorThreadRun()
        {
            try
            {
                string logString = String.Empty;
                while (!stopLogging && !process.HasExited)
                {
                    logString += process.StandardError.ReadToEnd();
                    Thread.Sleep(100);
                }
                if (logString != String.Empty)
                {
                    Logger.LogWarning("Error output from: {0}{1}{2}", fileName, Environment.NewLine, logString.Trim());
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Error capturing error output of process...");
                Logger.LogException(ex);
            }
        }
    }
}
