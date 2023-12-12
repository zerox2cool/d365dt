using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStudio.D365.DeploymentHelper.Core.Base
{
    #region HelperTypeAttribute
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HelperTypeAttribute : Attribute
    {
        /// <summary>
        /// The Helper Tool Type
        /// </summary>
        public string HelperType { get; set; }

        public HelperTypeAttribute(string helperType)
        {
            if (string.IsNullOrEmpty(helperType))
                throw new ArgumentNullException(nameof(helperType));
            this.HelperType = helperType.ToUpper();
        }
    }
    #endregion HelperTypeAttribute
}