using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD365DT.DeploymentTool.Utils
{
    public class CommonHelper
    {
        #region String Helpers
        /// <summary>
        /// Check if a string is inside the array, not case-sensitive
        /// </summary>
        /// <param name="text">String to find</param>
        /// <param name="text_array">Array of string</param>
        /// <returns>Return true if found</returns>
        public static bool StringInArray(string text, string[] text_array)
        {
            return text_array.Contains(text, new CasualStringComparerObject());
        }
        #endregion String Helpers
    }

    #region CasualStringComparerObject
    /// <summary>
    /// A case-insensitive string comparer
    /// </summary>
    public class CasualStringComparerObject : IEqualityComparer<string>
    {
        public bool Equals(string s1, string s2)
        {
            if (s1.Trim().ToLower() == s2.Trim().ToLower())
                return true;
            else
                return false;
        }

        public int GetHashCode(string s)
        {
            return s.GetHashCode();
        }
    }
    #endregion CasualStringComparerObject
}