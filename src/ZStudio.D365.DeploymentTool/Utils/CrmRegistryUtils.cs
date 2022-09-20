using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ZD365DT.DeploymentTool.Utils
{
    public class CrmRegistryUtils
    {
        /// <summary>
        /// The locatoin in the Registry where CRM stores information
        /// </summary>
        private const string CRM_REG_DIRECTORY = @"software\Microsoft\mscrm\";
        

        /// <summary>
        /// Returns the recorded web location from the registry
        /// </summary>
        public static string GetCrmRootWebLocation()
        {
            string keyName = "ServerUrl";
            return ReadStringFromHKLM(CRM_REG_DIRECTORY, keyName);
        }


        /// <summary>
        /// Returns the location of the Microsoft CRM WebSite in IIS
        /// </summary>
        public static string GetCrmIisSite()
        {
            string keyName = "website";
            return ReadStringFromHKLM(CRM_REG_DIRECTORY, keyName);
        }


        /// <summary>
        /// Returns the location of the Microsoft CRM Web Directory
        /// </summary>
        public static string GetCrmWebDirectory()
        {
            string keyName = "WebSitePath";
            return ReadStringFromHKLM(CRM_REG_DIRECTORY, keyName);
        }


        /// <summary>
        /// Returns the location of the Microsoft CRM Server Install Directory
        /// </summary>
        public static string GetCrmServerInstallDirectory()
        {
            string keyName = "CRM_Server_InstallDir";
            return ReadStringFromHKLM(CRM_REG_DIRECTORY, keyName);
        }


        /// <summary>
        /// Returns the location of the Microsoft CRM Server Bin Directory
        /// </summary>
        public static string GetCrmServerBinDirectory()
        {
            return Path.Combine(GetCrmServerInstallDirectory(), @"Server\bin");
        }


        /// <summary>
        /// Defines the .NET 2.0 ISAPI Filter
        /// </summary>
        public static string GetNetIsapiFilter()
        {
            string path = RuntimeEnvironment.GetRuntimeDirectory();
            string name = "aspnet_isapi.dll";
            return Path.Combine(path, name);
        }


        /// <summary>
        /// Defines the ASP ISAPI Filter
        /// </summary>
        public static string GetInetsrvDir()
        {
            string path = @"SYSTEM\ControlSet001\Services\W3SVC\Parameters";
            string keyName = "InstallPath";
            return ReadStringFromHKLM(path, keyName);
        }


        /// <summary>
        /// Returns a Key from the HKLM area of the registry based on the provided
        /// path and key name
        /// </summary>
        /// <param name="path">The path under HKLM to open</param>
        /// <param name="keyName">The name of the key whose value is to returned</param>
        /// <returns>The values of the key passed in</returns>
        private static string ReadStringFromHKLM(string path, string keyName)
        {
            string result = String.Format(@"KEY NOT FOUND: HKLM\{0}\{1}", path, keyName);
            try
            {
                result = ReadHklmRegistryValue(path, keyName);
            }
            catch
            {
                Logger.LogWarning(@"Key HKLM\{0}\{1} could not be read from the registry.", path, keyName);
            }
            return result;
        }

        #region Win32 Registry Access

        internal static class WinApi
        {
            internal const int ERROR_SUCCESS = 0;
            internal static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);

            [Flags]
            internal enum RegistryAccess : uint
            {
                KEY_QUERY_VALUE = 0x0001,
                KEY_WOW64_64KEY = 0x0100,
                KEY_WOW64_32KEY = 0x0200
            }

            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyEx")]
            internal static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, uint options, uint sam, out IntPtr phkResult);

            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
            internal static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, int lpReserved, out uint lpType, StringBuilder lpData, ref uint lpcbData);

            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern int RegCloseKey(IntPtr hKey);
        }


        private static string ReadHklmRegistryValue(string path, string keyName)
        {
            StringBuilder szBuffer = new StringBuilder(255);
            uint lBufferSize = (uint)szBuffer.Capacity;

            int lResult;
            IntPtr phkResult;
            uint lpType;
            uint options = (uint)(WinApi.RegistryAccess.KEY_QUERY_VALUE | WinApi.RegistryAccess.KEY_WOW64_64KEY);
            lResult = WinApi.RegOpenKeyEx(WinApi.HKEY_LOCAL_MACHINE, path, 0, options, out phkResult);

            if (WinApi.ERROR_SUCCESS != lResult)
            {
                options = (uint)(WinApi.RegistryAccess.KEY_QUERY_VALUE | WinApi.RegistryAccess.KEY_WOW64_32KEY);
                lResult = WinApi.RegOpenKeyEx(WinApi.HKEY_LOCAL_MACHINE, path, 0, options, out phkResult);
                if (WinApi.ERROR_SUCCESS != lResult)
                {
                    throw new Exception(new Win32Exception(Marshal.GetLastWin32Error()).Message);
                }
            }

            lResult = WinApi.RegQueryValueEx(phkResult, keyName, 0, out lpType, szBuffer, ref lBufferSize);
            WinApi.RegCloseKey(phkResult);

            return (WinApi.ERROR_SUCCESS == lResult) ? szBuffer.ToString() : null;
        }

        #endregion Win32 Registry Access
    }
}
