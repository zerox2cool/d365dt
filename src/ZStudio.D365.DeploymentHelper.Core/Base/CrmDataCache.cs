using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStudio.D365.DeploymentHelper.Core.Base
{
    /// <summary>
    /// Generic CRM Data cache object
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class CrmDataCache<TEntity> : CrmDataCacheBase<TEntity> where TEntity : Entity
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityLogicalName">Entity logical name.</param>
        /// <param name="cacheExpirationInSeconds">Default to 3600 seconds (1 hour) when this is NULL.</param>
        /// <param name="columns">Columns to fetch, will retrieve all columns when NULL.</param>
        public CrmDataCache(string entityLogicalName, int? cacheExpirationInSeconds = default(int?), params string[] columns) : base(entityLogicalName, cacheExpirationInSeconds, columns)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityLogicalName">Entity logical name.</param>
        /// <param name="cacheExpirationInSeconds">Default to 3600 seconds (1 hour) when this is NULL.</param>
        /// <param name="columns">Columns to fetch, will retrieve all columns when NULL.</param>
        public CrmDataCache(string entityLogicalName, bool isActiveOnly, int? cacheExpirationInSeconds = default(int?), params string[] columns) : base(entityLogicalName, isActiveOnly, cacheExpirationInSeconds, columns)
        {
        }
    }
}