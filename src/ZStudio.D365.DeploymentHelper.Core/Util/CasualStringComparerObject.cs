using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStudio.D365.DeploymentHelper.Core.Util
{
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