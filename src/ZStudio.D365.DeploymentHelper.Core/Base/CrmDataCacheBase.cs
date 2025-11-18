using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZStudio.D365.Shared.Framework.Core.Query;

namespace ZStudio.D365.DeploymentHelper.Core.Base
{
    /// <summary>
    /// CRM Data Cache base class.
    /// </summary>
    /// <typeparam name="TEntity">CRM entity</typeparam>
    public abstract class CrmDataCacheBase<TEntity> : IDisposable where TEntity : Entity
    {
        #region Variables
        //expiry time of cache in seconds
        private int _cacheExpiration = 3600; //default 1 hour

        private string[] _columns = null; //all columns

        /// <summary>
        /// Cache
        /// </summary>
        protected List<TEntity> _cache = new List<TEntity>();
        private DateTime _cacheLastUpdate = DateTime.MinValue;
        #endregion Variables

        #region Property
        /// <summary>
        /// CRM EntityLogicalName
        /// </summary>
        public string EntityLogicalName { get; private set; }

        /// <summary>
        /// Indicate to retrieve active record only.
        /// </summary>
        public bool IsActiveOnly { get; set; }

        /// <summary>
        /// Cache
        /// </summary>
        public List<TEntity> Cache
        {
            get
            {
                return _cache;
            }
            set
            {
                _cache = value;
            }
        }

        /// <summary>
        /// Cache Last Updated time
        /// </summary>
        public DateTime CacheLastUpdate
        {
            get
            {
                return _cacheLastUpdate;
            }
            private set
            {
                _cacheLastUpdate = value;
            }
        }

        /// <summary>
        /// Cache expiry duration in seconds
        /// </summary>
        public int CacheExpiration
        {
            get
            {
                return _cacheExpiration;
            }
            set
            {
                _cacheExpiration = value;
            }
        }

        /// <summary>
        /// Columns to retrieve
        /// </summary>
        public string[] Columns
        {
            get
            {
                return _columns;
            }
            set
            {
                _columns = value;
            }
        }
        #endregion Property

        #region Destructor
        /// <summary>
        /// Destructor
        /// </summary>
        ~CrmDataCacheBase()
        {
            ClearCache();
            //Debug.WriteLine("~CrmDataCacheBase() fired...");
        }
        #endregion Destructor

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            ClearCache();
            //Debug.WriteLine("CrmDataCacheBase.Dispose() fired...");
        }
        #endregion Dispose

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityLogicalName">Entity logical name.</param>
        /// <param name="cacheExpirationInSeconds">Default to 3600 seconds (1 hour) when this is NULL.</param>
        /// <param name="columns">Columns to fetch, will retrieve all columns when NULL.</param>
        public CrmDataCacheBase(string entityLogicalName, int? cacheExpirationInSeconds = null, params string[] columns)
        {
            if (string.IsNullOrEmpty(entityLogicalName))
                throw new ArgumentNullException(nameof(entityLogicalName));

            EntityLogicalName = entityLogicalName;
            if (cacheExpirationInSeconds != null)
                CacheExpiration = cacheExpirationInSeconds.Value;
            Columns = columns;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityLogicalName">Entity logical name.</param>
        /// <param name="cacheExpirationInSeconds">Default to 3600 seconds (1 hour) when this is NULL.</param>
        /// <param name="columns">Columns to fetch, will retrieve all columns when NULL.</param>
        public CrmDataCacheBase(string entityLogicalName, bool isActiveOnly, int? cacheExpirationInSeconds = null, params string[] columns)
        {
            if (string.IsNullOrEmpty(entityLogicalName))
                throw new ArgumentNullException(nameof(entityLogicalName));

            EntityLogicalName = entityLogicalName;
            if (cacheExpirationInSeconds != null)
                CacheExpiration = cacheExpirationInSeconds.Value;
            Columns = columns;
            IsActiveOnly = isActiveOnly;
        }
        #endregion Constructor

        #region Virtual Methods
        #region Cache
        /// <summary>
        /// Check if the cache has expired.
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsCacheExpired()
        {
            if ((DateTime.Now - CacheLastUpdate).TotalSeconds > CacheExpiration || Cache.Count == 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Clear the in-memory cache.
        /// </summary>
        public virtual void ClearCache()
        {
            Cache.Clear();
            CacheLastUpdate = DateTime.MinValue;
        }

        /// <summary>
        /// Default Get Cache Query that can be override.
        /// </summary>
        /// <returns></returns>
        protected virtual XrmQueryExpression GetCacheQuery()
        {
            XrmQueryExpression query = new XrmQueryExpression(EntityLogicalName, IsActiveOnly);
            if (Columns != null && Columns.Length > 0)
                query.Columns(Columns);
            return query;
        }

        /// <summary>
        /// Build the initial cache.
        /// </summary>
        /// <param name="svc"></param>
        /// <returns></returns>
        protected virtual List<TEntity> BuildCache(IOrganizationService svc)
        {
            if (IsCacheExpired())
            {
                lock (_cache)
                {
                    _cache = new List<TEntity>();
                    int pagenumber = 1;
                    XrmQueryExpression query = GetCacheQuery();
                    EntityCollection result = svc.RetrieveMultiple(query.ToQueryExpression());
                    while (result != null && result.Entities.Count > 0)
                    {
                        Entity[] ent = result.Entities.ToArray();
                        for (int i = 0; i < ent.Length; i++)
                            _cache.Add((TEntity)ent[i]);
                        ent = null;

                        if (result.MoreRecords)
                        {
                            //increase page number
                            pagenumber++;
                            query.PageInfo.PageNumber = pagenumber;
                            query.PageInfo.PagingCookie = result.PagingCookie;
                            result = svc.RetrieveMultiple(query.ToQueryExpression());
                        }
                        else
                            result = null;
                    }
                    _cacheLastUpdate = DateTime.Now;
                }

                return _cache;
            }
            else
                return _cache;
        }

        /// <summary>
        /// Get the cache.
        /// </summary>
        /// <param name="svc"></param>
        /// <returns></returns>
        public virtual List<TEntity> GetCache(IOrganizationService svc)
        {
            try
            {
                //build data list
                return BuildCache(svc);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error getting cache. {ex.Message}");
            }
        }
        #endregion Cache

        #region Get
        /// <summary>
        /// Default Get by CRM GUID
        /// </summary>
        /// <param name="svc"></param>
        /// <param name="id"></param>
        /// <param name="reloadAndRetry"></param>
        /// <returns></returns>
        public virtual TEntity GetById(IOrganizationService svc, Guid id, bool reloadAndRetry = false)
        {
            TEntity output = null;

            //build data list
            List<TEntity> cache = BuildCache(svc);

            //get the record
            var result = (from crmData in cache where crmData.Id == id select crmData);
            if (result.Count() > 0)
                output = result.First();
            else if (reloadAndRetry)
            {
                //reload and retry again in case the key was just added, prevents the need to do iisreset to force cache reset
                CacheLastUpdate = DateTime.MinValue;
                cache = BuildCache(svc);
                result = (from crmData in cache where crmData.Id == id select crmData);
                if (result.Count() > 0)
                    output = result.First();
                else
                    throw new ApplicationException(string.Format("The record with ID '{0}' does not exists.", id.ToString()));
            }

            return output;
        }

        /// <summary>
        /// Get by a CRM attribute.
        /// </summary>
        /// <param name="svc"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        /// <param name="reloadAndRetry"></param>
        /// <returns></returns>
        public virtual TEntity GetByStringAttribute(IOrganizationService svc, string attributeName, string attributeValue, bool reloadAndRetry = false)
        {
            TEntity output = null;

            //build data list
            List<TEntity> cache = BuildCache(svc);

            try
            {
                //get the record
                var result = (from crmData in cache where (crmData.Contains(attributeName) && crmData[attributeName].ToString().Equals(attributeValue, StringComparison.CurrentCultureIgnoreCase)) || (!crmData.Contains(attributeName) && string.IsNullOrEmpty(attributeValue)) select crmData);
                if (result.Count() > 0)
                    output = result.First();
                else if (reloadAndRetry)
                {
                    //reload and retry again in case the key was just added, prevents the need to do iisreset to force cache reset
                    CacheLastUpdate = DateTime.MinValue;
                    cache = BuildCache(svc);
                    result = (from crmData in cache where (crmData.Contains(attributeName) && crmData[attributeName].ToString().Equals(attributeValue, StringComparison.CurrentCultureIgnoreCase)) || (!crmData.Contains(attributeName) && string.IsNullOrEmpty(attributeValue)) select crmData);
                    if (result.Count() > 0)
                        output = result.First();
                    else
                        throw new ApplicationException(string.Format("The record with attribute '{0}' and value '{1}' does not exists.", attributeName, attributeValue));
                }
            }
            catch
            {
                throw new ApplicationException(string.Format("The record with attribute '{0}' and value '{1}' is not a valid Get for this entity.", attributeName, attributeValue));
            }

            return output;
        }

        /// <summary>
        /// Get collection by a CRM attribute.
        /// </summary>
        /// <param name="svc"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        /// <param name="reloadAndRetry"></param>
        /// <returns></returns>
        public virtual TEntity[] GetCollectionByStringAttribute(IOrganizationService svc, string attributeName, string attributeValue, bool reloadAndRetry = false)
        {
            TEntity[] output = null;

            //build data list
            List<TEntity> cache = BuildCache(svc);

            try
            {
                //get the record
                var result = (from crmData in cache where (crmData.Contains(attributeName) && crmData[attributeName].ToString().Equals(attributeValue, StringComparison.CurrentCultureIgnoreCase)) || (!crmData.Contains(attributeName) && string.IsNullOrEmpty(attributeValue)) select crmData);
                if (result.Count() > 0)
                    output = result.ToArray();
                else if (reloadAndRetry)
                {
                    //reload and retry again in case the key was just added, prevents the need to do iisreset to force cache reset
                    CacheLastUpdate = DateTime.MinValue;
                    cache = BuildCache(svc);
                    result = (from crmData in cache where (crmData.Contains(attributeName) && crmData[attributeName].ToString().Equals(attributeValue, StringComparison.CurrentCultureIgnoreCase)) || (!crmData.Contains(attributeName) && string.IsNullOrEmpty(attributeValue)) select crmData);
                    if (result.Count() > 0)
                        output = result.ToArray();
                    else
                        throw new ApplicationException(string.Format("The record with attribute '{0}' and value '{1}' does not exists.", attributeName, attributeValue));
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("The record with attribute '{0}' and value '{1}' is not a valid Get for this entity. Exception: {2}", attributeName, attributeValue, ex.Message), ex);
            }

            return output;
        }
        #endregion Get
        #endregion Virtual Methods
    }
}