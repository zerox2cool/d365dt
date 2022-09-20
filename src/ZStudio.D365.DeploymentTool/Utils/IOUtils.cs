using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;


namespace ZD365DT.DeploymentTool.Utils
{
    /// <summary>
    /// Collection of IO Utilities
    /// </summary>
    public class IOUtils
    {
        /// <summary>
        /// List of valid file extensions on which tokenizations can run
        /// </summary>
        private static string[] tokenizeExtensions = { ".xml", ".txt", ".htm", ".html", ".js", ".css", ".config", ".template", ".rwd", ".bat", ".cmd", ".aspx", ".asmx", ".sql",".csv" };


        /// <summary>
        /// Grants Users Read, Execute, Create Files and Directory cascading permission.
        /// Grants NETWORK SERVICE Full Control cascading permission.
        /// </summary>
        /// <param name="dirname">The directory on which the cascading permissions is to be granted</param>
        public static void GrantCascadingUserAndNetworkServicePermissionsOnDirectory(string dirname)
        {
            if (Directory.Exists(dirname))
            {
                DirectorySecurity sec = Directory.GetAccessControl(dirname, AccessControlSections.All);

                sec.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                    FileSystemRights.ReadAndExecute | FileSystemRights.CreateFiles | FileSystemRights.CreateDirectories,
                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));

                sec.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null),
                    FileSystemRights.FullControl,
                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));

                Directory.SetAccessControl(dirname, sec);
            }
            else
            {
                Logger.LogWarning("Failed to find directory {0} on which to grant permissions", dirname);
            }
        }


        /// <summary>
        /// Makes sure that all files under a directory are writable
        /// </summary>
        /// <param name="dirName">The top level directory</param>
        public static void SetAllWritableUnderDirectory(string dirName)
        {
            if (Directory.Exists(dirName))
            {
                string[] directories = Directory.GetDirectories(dirName);
                foreach (string dir in directories)
                {
                    SetAllWritableUnderDirectory(dir);
                }

                string[] files = Directory.GetFiles(dirName);
                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                }
            }
        }


        /// <summary>
        /// Copies a directory and all its sub-directories and files
        /// </summary>
        /// <param name="source">The source directory</param>
        /// <param name="destination">The destination directory</param>
        public static void CopyDirectory(string source, string destination)
        {
            CopyDirectoryReplaceTokens(source, destination, new StringDictionary());
        }


        /// <summary>
        /// Copies a directory and all its sub-directories and files, replacing tokens in the files
        /// </summary>
        /// <param name="source">The source directory</param>
        /// <param name="destination">The destination directory</param>
        /// <param name="replaceTokens">StringDictionary containing the list of values to replace for the specified keys</param>
        public static void CopyDirectoryReplaceTokens(string source, string destination, StringDictionary replaceTokens)
        {
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }
            string[] files = Directory.GetFileSystemEntries(source);
            foreach (string sourceElement in files)
            {
                // Get the fully qualified destination path
                string destinationElement = Path.Combine(destination, Path.GetFileName(sourceElement));
                if (Directory.Exists(sourceElement))
                {
                    CopyDirectoryReplaceTokens(sourceElement, destinationElement, replaceTokens);
                }
                else
                {
                    CopyFileReplaceTokens(sourceElement, destinationElement, replaceTokens);
                }
            }
        }


        /// <summary>
        /// Copies a directory and all its sub-directories and files
        /// </summary>
        /// <param name="source">The source directory</param>
        /// <param name="destination">The destination directory</param>
        public static void CopyFile(string source, string destination)
        {
            CopyFileReplaceTokens(source, destination, new StringDictionary());
        }


        /// <summary>
        /// Copies a file, replacing the tokens defined if the file is valid for tokenization, 
        /// otherwise the file is copied straight across.
        /// </summary>
        /// <param name="sourceFileName">The source file to read</param>
        /// <param name="targetFilename">The target file to write</param>
        /// <param name="replaceTokens">StringDictionary containing the list of values to replace for the specified keys</param>
        public static void CopyFileReplaceTokens(string sourceFileName, string targetFilename, StringDictionary replaceTokens)
        {
            if (File.Exists(sourceFileName))
            {
                File.SetAttributes(sourceFileName, FileAttributes.Normal);
            }
            else
            {
                throw new FileNotFoundException(String.Format("File '{0}' was not found and could not be copied", sourceFileName));
            }
            if (File.Exists(targetFilename))
            {
                File.SetAttributes(targetFilename, FileAttributes.Normal);
                File.Delete(targetFilename);
            }
            if (IsFileValidForTokenization(sourceFileName) && replaceTokens.Count > 0)
            {
                string fileText = ReadFileReplaceTokens(sourceFileName, replaceTokens);
                if (!Directory.Exists(Path.GetDirectoryName(targetFilename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFilename));
                File.WriteAllText(targetFilename, fileText, Encoding.UTF8);
            }
            else
            {
                if (!Directory.Exists(Path.GetDirectoryName(targetFilename)))
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFilename));
                File.Copy(sourceFileName, targetFilename);
            }
        }


        /// <summary>
        /// Reads a file, replaces the tokens and returns the file contents
        /// </summary>
        /// <param name="sourceFileName">The source file to read</param>
        /// <param name="replaceTokens">StringDictionary containing the list of values to replace for the specified keys</param>
        /// <returns>The file text with the tokens replaced</returns>
        public static string ReadFileReplaceTokens(string sourceFileName, StringDictionary replaceTokens)
        {
            string fileText = File.ReadAllText(sourceFileName, Encoding.UTF8);
            return ReplaceStringTokens(fileText, replaceTokens);
        }


        /// <summary>
        /// Replaces all the tokens in a string and returns the updated version
        /// </summary>
        /// <param name="source">The string read</param>
        /// <param name="replaceTokens">StringDictionary containing the list of values to replace for the specified keys</param>
        /// <returns>The string with the tokens replaced</returns>
        public static string ReplaceStringTokens(string source, StringDictionary replaceTokens)
        {
            if (!String.IsNullOrEmpty(source))
            {
                foreach (DictionaryEntry entry in replaceTokens)
                {
                    if (entry.Key != null && entry.Value != null)
                    {
                        source = source.Replace(entry.Key.ToString().ToUpper(), entry.Value.ToString());
                    }
                }
            }
            return source;
        }

        /// <summary>
        /// From a full file path search string, returns all the files matching the search
        /// </summary>
        /// <param name="seachString">The path to search</param>
        /// <returns></returns>
        public static string[] GetFilesFromDirectoryMatchingSearch(string seachString)
        {
            string dir = Path.GetDirectoryName(seachString);
            string fileSearch = Path.GetFileName(seachString);

            return Directory.GetFiles(dir, fileSearch);
        }



        /// <summary>
        /// Returns </>true</c> if the extension of the passed in fileName is valid for 
        /// tokenization, otherwise <c>false</c>
        /// </summary>
        /// <param name="fileName">The name of the file to check</param>
        /// <returns><c>true</c> if the file can be tokenized, otherwise <c>false</c></returns>
        private static bool IsFileValidForTokenization(string fileName)
        {
            bool result = false;

            string extension = Path.GetExtension(fileName);
            if (!String.IsNullOrEmpty(extension))
            {
                foreach (string tokenizeExtension in tokenizeExtensions)
                {
                    if (extension.ToLower() == tokenizeExtension.ToLower())
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }
    }
}
