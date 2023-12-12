using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZStudio.D365.DeploymentHelper.Core.Util
{
    /// <summary>
    /// Contains helper methods that uses QueryExpression to retrieve data from CRM
    /// </summary>
    public class FetchHelper
    {
        #region Variables
        private IOrganizationService svc = null;
        #endregion Variables

        #region Constructor
        public FetchHelper(IOrganizationService crmSvc)
        {
            svc = crmSvc;
        }
        #endregion Constructor

        #region Public
        #region GetGuidByBlockSize
        /// <summary>
        /// Get a list of Guid blocks from the array of entities by block size.
        /// </summary>
        /// <param name="coll">Array of Entities</param>
        /// <param name="blockSize">The block size.</param>
        /// <returns>Return a dictionary with list of GUID.</returns>
        public Dictionary<int, List<Guid>> GetGuidByBlockSize(Entity[] coll, int blockSize)
        {
            Dictionary<int, List<Guid>> result = null;
            if (coll?.Length > 0)
            {
                int blockSequence = 0;
                int totalCount = coll.Length;
                int currentCount = 0;

                result = new Dictionary<int, List<Guid>>();
                while (currentCount < totalCount)
                {
                    blockSequence++;

                    int count = 0;
                    List<Guid> ids = new List<Guid>();
                    for (int i = currentCount; i < totalCount; i++)
                    {
                        ids.Add(coll[i].Id);

                        count++;
                        currentCount++;
                        if (count >= blockSize)
                            break;
                    }

                    result.Add(blockSequence, ids);
                }
            }

            return result;
        }

        /// <summary>
        /// Get a list of Guid blocks from the array of guids by block size.
        /// </summary>
        /// <param name="guids">Array of GUIDs</param>
        /// <param name="blockSize">The block size.</param>
        /// <returns>Return a dictionary with list of GUID.</returns>
        public Dictionary<int, List<Guid>> GetGuidByBlockSize(Guid[] guids, int blockSize)
        {
            Dictionary<int, List<Guid>> result = null;
            if (guids?.Length > 0)
            {
                int blockSequence = 0;
                int totalCount = guids.Length;
                int currentCount = 0;

                result = new Dictionary<int, List<Guid>>();
                while (currentCount < totalCount)
                {
                    blockSequence++;

                    int count = 0;
                    List<Guid> ids = new List<Guid>();
                    for (int i = currentCount; i < totalCount; i++)
                    {
                        ids.Add(guids[i]);

                        count++;
                        currentCount++;
                        if (count >= blockSize)
                            break;
                    }

                    result.Add(blockSequence, ids);
                }
            }

            return result;
        }

        /// <summary>
        /// Get a list of Guid blocks from the array of entities by block size.
        /// </summary>
        /// <param name="coll">Array of Entities</param>
        /// <param name="blockSize">The block size.</param>
        /// <returns>Return a dictionary with list of GUIDs of the entities.</returns>
        public Dictionary<int, List<Guid>> GetGuidByBlockSize<T>(T[] coll, int blockSize) where T : Entity
        {
            return GetGuidByBlockSize(coll, blockSize);
        }
        #endregion GetGuidByBlockSize

        #region GetEntityByBlockSize
        /// <summary>
        /// Split the array of entities by block size.
        /// </summary>
        /// <param name="coll">Array of Entities</param>
        /// <param name="blockSize">The block size.</param>
        /// <returns>Return a dictionary with list of Entities.</returns>
        public Dictionary<int, List<T>> GetEntityByBlockSize<T>(T[] coll, int blockSize) where T : Entity
        {
            return GetEntityByBlockSize(coll, blockSize);
        }

        /// <summary>
        /// Split the array of entities by block size.
        /// </summary>
        /// <param name="coll">Array of Entities</param>
        /// <param name="blockSize">The block size.</param>
        /// <returns>Return a dictionary with list of Entities.</returns>
        public Dictionary<int, List<Entity>> GetEntityByBlockSize(Entity[] coll, int blockSize)
        {
            Dictionary<int, List<Entity>> result = null;
            if (coll?.Length > 0)
            {
                int blockSequence = 0;
                int totalCount = coll.Length;
                int currentCount = 0;

                result = new Dictionary<int, List<Entity>>();
                while (currentCount < totalCount)
                {
                    blockSequence++;

                    int count = 0;
                    List<Entity> ents = new List<Entity>();
                    for (int i = currentCount; i < totalCount; i++)
                    {
                        ents.Add(coll[i]);

                        count++;
                        currentCount++;
                        if (count >= blockSize)
                            break;
                    }

                    result.Add(blockSequence, ents);
                }
            }

            return result;
        }
        #endregion GetEntityByBlockSize

        #region CreateEntity
        public Guid? CreateEntity(Entity NewRecord)
        {
            if (NewRecord != null)
                return svc.Create(NewRecord);
            else
                return null;
        }
        #endregion CreateEntity

        #region genericlookup
        #region Retrieve
        /// <summary>
        /// Get the records from CRM as Xrm object based on the search condition parameters pass in
        /// </summary>
        /// <param name="query">The query expression.</param>
        /// <returns>Returns the entities as an array of Xrm object</returns>
        public Entity[] RetrieveEntityByQuery(QueryExpression query)
        {
            EntityCollection result = svc.RetrieveMultiple(query);
            if (result != null && result.Entities.Count > 0)
                return result.Entities.ToArray();
            else
                return null;
        }

        /// <summary>
        /// Get the records from CRM as Xrm object based on the search condition parameters pass in
        /// </summary>
        /// <param name="query">The query expression.</param>
        /// <returns>Returns the entities as an array of Xrm object</returns>
        public Entity[] RetrieveAllEntityByQuery(QueryExpression query)
        {
            List<Entity> obj = new List<Entity>();
            int pagenumber = 1;
            EntityCollection result = svc.RetrieveMultiple(query);

            while (result != null && result.Entities.Count > 0)
            {
                Entity[] ent = result.Entities.ToArray();
                for (int i = 0; i < ent.Length; i++)
                    obj.Add(ent[i]);
                ent = null;

                if (result.MoreRecords)
                {
                    //increase page number
                    pagenumber++;
                    query.PageInfo.PageNumber = pagenumber;
                    query.PageInfo.PagingCookie = result.PagingCookie;
                    result = svc.RetrieveMultiple(query);
                }
                else
                    result = null;
            }

            if (obj != null && obj.Count > 0)
                return obj.ToArray();
            else
                return null;
        }

        /// <summary>
        /// Get the records from CRM as Xrm object based on the search condition parameters pass in
        /// </summary>
        /// <typeparam name="T">Xrm Object</typeparam>
        /// <param name="query">The query expression.</param>
        /// <returns>Returns the entities as an array of Xrm object</returns>
        public T[] RetrieveAllEntityByQuery<T>(QueryExpression query) where T : class
        {
            List<T> obj = new List<T>();
            int pagenumber = 1;
            EntityCollection result = svc.RetrieveMultiple(query);

            while (result != null && result.Entities.Count > 0)
            {
                Entity[] ent = result.Entities.ToArray();
                for (int i = 0; i < ent.Length; i++)
                    obj.Add(ent[i] as T);
                ent = null;

                if (result.MoreRecords)
                {
                    //increase page number
                    pagenumber++;
                    query.PageInfo.PageNumber = pagenumber;
                    query.PageInfo.PagingCookie = result.PagingCookie;
                    result = svc.RetrieveMultiple(query);
                }
                else
                    result = null;
            }

            if (obj != null && obj.Count > 0)
                return obj.ToArray();
            else
                return null;
        }

        /// <summary>
        /// Get the records from CRM as Xrm object based on the search condition parameters pass in
        /// </summary>
        /// <typeparam name="T">Xrm Object</typeparam>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="ConditionSchemaName">Schema Name of the attribute to search</param>
        /// <param name="ConditionValue">Search condition value</param>
        /// <param name="ColumnSets">Specify the columns to retrieve</param>
        /// <param name="OrderBySchemaName">Order By attribute schema name</param>
        /// <param name="OrderByType">Order By Ascending or Descending</param>
        /// <returns>Returns the entities as an array of Xrm object</returns>
        public T[] RetrieveAllEntityByCondition<T>(string EntityLogicalName, string[] ConditionSchemaName, object[] ConditionValue, string[] ColumnSets = null, string[] OrderBySchemaName = null, OrderType[] OrderByType = null) where T : class
        {
            List<T> obj = new List<T>();
            int pagenumber = 1;
            QueryExpression query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, ConditionSchemaName, ConditionValue, ColumnSets, pagenumber, OrderBySchemaName, OrderByType);
            EntityCollection result = svc.RetrieveMultiple(query);

            while (result != null && result.Entities.Count > 0)
            {
                Entity[] ent = result.Entities.ToArray();
                for (int i = 0; i < ent.Length; i++)
                    obj.Add(ent[i] as T);
                ent = null;

                if (result.MoreRecords)
                {
                    //increase page number
                    pagenumber++;
                    query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, ConditionSchemaName, ConditionValue, ColumnSets, pagenumber, OrderBySchemaName, OrderByType);
                    result = svc.RetrieveMultiple(query);
                }
                else
                    result = null;
            }

            if (obj != null && obj.Count > 0)
                return obj.ToArray();
            else
                return null;
        }

        /// <summary>
        /// Get all CrmId for an entity based on the search condition parameters pass in
        /// </summary>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="EntityCrmIdSchemaName">The PrimaryKey of the Entity where the CrmId is</param>
        /// <param name="ConditionSchemaName">Schema Name of the attribute to search</param>
        /// <param name="ConditionValue">Search condition value</param>
        /// <param name="OrderBySchemaName">Order By attribute schema name</param>
        /// <param name="OrderByType">Order By Ascending or Descending</param>
        /// <returns>Returns array of CrmId Guid</returns>
        public List<Guid> RetrieveAllCrmIdByCondition(string EntityLogicalName, string EntityCrmIdSchemaName, string[] ConditionSchemaName, object[] ConditionValue, string[] OrderBySchemaName = null, OrderType[] OrderByType = null)
        {
            List<Guid> ids = new List<Guid>();
            int pagenumber = 1;
            QueryExpression query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, ConditionSchemaName, ConditionValue, new string[] { EntityCrmIdSchemaName }, pagenumber, OrderBySchemaName, OrderByType);
            EntityCollection result = svc.RetrieveMultiple(query);

            while (result != null && result.Entities.Count > 0 && result.Entities[0].Attributes.ContainsKey(EntityCrmIdSchemaName))
            {
                for (int i = 0; i < result.Entities.Count; i++)
                    ids.Add((Guid)result.Entities[i].Attributes[EntityCrmIdSchemaName]);

                if (result.MoreRecords)
                {
                    //increase page number
                    pagenumber++;
                    query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, ConditionSchemaName, ConditionValue, new string[] { EntityCrmIdSchemaName }, pagenumber, OrderBySchemaName, OrderByType);
                    result = svc.RetrieveMultiple(query);
                }
                else
                    result = null;
            }

            if (ids != null && ids.Count > 0)
                return ids;
            else
                return null;
        }

        /// <summary>
        /// Get all CrmId for an entity based on the StateCode, pass in BusinessUnitId to filter by OwningBusinessUnit
        /// </summary>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="EntityCrmIdSchemaName">The PrimaryKey of the Entity where the CrmId is</param>
        /// <param name="StateCode">The statecode of the records to search</param>
        /// <param name="BusinessUnitId">The BusinessUnitId of records to filter</param>
        /// <returns>Returns array of CrmId Guid</returns>
        public List<Guid> RetrieveAllCrmIdByStateCode(string EntityLogicalName, string EntityCrmIdSchemaName, int StateCode, Guid? BusinessUnitId = null)
        {
            List<Guid> ids = new List<Guid>();
            int pagenumber = 1;
            QueryExpression query = null;
            if (BusinessUnitId != null)
                query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode, XRMDataImport.OutOfBoxCrmAttributes.SchemaName.owningbusinessunit }, new object[] { StateCode, BusinessUnitId.Value }, new string[] { EntityCrmIdSchemaName }, pagenumber);
            else
                query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode }, new object[] { StateCode }, new string[] { EntityCrmIdSchemaName }, pagenumber);
            EntityCollection result = svc.RetrieveMultiple(query);

            while (result != null && result.Entities.Count > 0 && result.Entities[0].Attributes.ContainsKey(EntityCrmIdSchemaName))
            {
                for (int i = 0; i < result.Entities.Count; i++)
                    ids.Add((Guid)result.Entities[i].Attributes[EntityCrmIdSchemaName]);

                if (result.MoreRecords)
                {
                    //increase page number
                    pagenumber++;
                    if (BusinessUnitId != null)
                        query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode, XRMDataImport.OutOfBoxCrmAttributes.SchemaName.owningbusinessunit }, new object[] { StateCode, BusinessUnitId.Value }, new string[] { EntityCrmIdSchemaName }, pagenumber);
                    else
                        query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode }, new object[] { StateCode }, new string[] { EntityCrmIdSchemaName }, pagenumber);
                    result = svc.RetrieveMultiple(query);
                }
                else
                    result = null;
            }

            if (ids != null && ids.Count > 0)
                return ids;
            else
                return null;
        }

        /// <summary>
        /// Get all CrmId and Owner for an entity based on the StateCode, pass in BusinessUnitId to filter by OwningBusinessUnit
        /// </summary>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="EntityCrmIdSchemaName">The PrimaryKey of the Entity where the CrmId is</param>
        /// <param name="StateCode">The statecode of the records to search</param>
        /// <param name="BusinessUnitId">The BusinessUnitId of records to filter</param>
        /// <returns>Returns a list of CrmId Guid and the owner of the record</returns>
        public List<KeyValuePair<Guid, EntityReference>> RetrieveAllCrmIdAndOwnerByStateCode(string EntityLogicalName, string EntityCrmIdSchemaName, int StateCode, Guid? BusinessUnitId = null)
        {
            List<KeyValuePair<Guid, EntityReference>> ids = new List<KeyValuePair<Guid, EntityReference>>();
            int pagenumber = 1;
            string[] columnsets = new string[] { EntityCrmIdSchemaName, XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid };
            QueryExpression query = null;
            if (BusinessUnitId != null)
                query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode, XRMDataImport.OutOfBoxCrmAttributes.SchemaName.owningbusinessunit }, new object[] { StateCode, BusinessUnitId.Value }, columnsets, pagenumber);
            else
                query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode }, new object[] { StateCode }, columnsets, pagenumber);
            EntityCollection result = svc.RetrieveMultiple(query);

            while (result != null && result.Entities.Count > 0 && result.Entities[0].Attributes.ContainsKey(EntityCrmIdSchemaName) && result.Entities[0].Attributes.ContainsKey(XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid))
            {
                for (int i = 0; i < result.Entities.Count; i++)
                    ids.Add(new KeyValuePair<Guid, EntityReference>((Guid)result.Entities[i].Attributes[EntityCrmIdSchemaName], (EntityReference)result.Entities[i].Attributes[XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid]));

                if (result.MoreRecords)
                {
                    //increase page number
                    pagenumber++;
                    if (BusinessUnitId != null)
                        query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode, XRMDataImport.OutOfBoxCrmAttributes.SchemaName.owningbusinessunit }, new object[] { StateCode, BusinessUnitId.Value }, columnsets, pagenumber);
                    else
                        query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode }, new object[] { StateCode }, columnsets, pagenumber);
                    result = svc.RetrieveMultiple(query);
                }
                else
                    result = null;
            }

            if (ids != null && ids.Count > 0)
                return ids;
            else
                return null;
        }

        /// <summary>
        /// Get all CrmId and Owner for an entity based on the StateCode and exclude source system, pass in BusinessUnitId to filter by OwningBusinessUnit
        /// </summary>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="EntityCrmIdSchemaName">The PrimaryKey of the Entity where the CrmId is</param>
        /// <param name="StateCode">The statecode of the records to search</param>
        /// <param name="BusinessUnitId">The BusinessUnitId of records to filter</param>
        /// <returns>Returns a list of CrmId Guid and the owner of the record</returns>
        public List<KeyValuePair<Guid, EntityReference>> RetrieveAllCrmIdAndOwnerByStateCodeExcludeSourceSystem(string EntityLogicalName, string EntityCrmIdSchemaName, int StateCode, string SourceSystemIdSchemaName, Guid ExcludeIntegrationSourceSystemId, Guid? BusinessUnitId = null)
        {
            List<KeyValuePair<Guid, EntityReference>> ids = new List<KeyValuePair<Guid, EntityReference>>();
            int pagenumber = 1;
            string[] columnsets = new string[] { EntityCrmIdSchemaName, XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid };
            QueryExpression query = null;
            if (BusinessUnitId != null)
                query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode, XRMDataImport.OutOfBoxCrmAttributes.SchemaName.owningbusinessunit }, new object[] { StateCode, BusinessUnitId.Value }, columnsets, pagenumber);
            else
                query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode }, new object[] { StateCode }, columnsets, pagenumber);
            //add additional condition to exclude source system
            query.Criteria.AddCondition(new ConditionExpression(SourceSystemIdSchemaName, ConditionOperator.NotEqual, ExcludeIntegrationSourceSystemId));
            EntityCollection result = svc.RetrieveMultiple(query);

            while (result != null && result.Entities.Count > 0 && result.Entities[0].Attributes.ContainsKey(EntityCrmIdSchemaName) && result.Entities[0].Attributes.ContainsKey(XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid))
            {
                for (int i = 0; i < result.Entities.Count; i++)
                    ids.Add(new KeyValuePair<Guid, EntityReference>((Guid)result.Entities[i].Attributes[EntityCrmIdSchemaName], (EntityReference)result.Entities[i].Attributes[XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid]));

                if (result.MoreRecords)
                {
                    //increase page number
                    pagenumber++;
                    if (BusinessUnitId != null)
                        query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode, XRMDataImport.OutOfBoxCrmAttributes.SchemaName.owningbusinessunit }, new object[] { StateCode, BusinessUnitId.Value }, columnsets, pagenumber);
                    else
                        query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode }, new object[] { StateCode }, columnsets, pagenumber);
                    //add additional condition to exclude source system
                    query.Criteria.AddCondition(new ConditionExpression(SourceSystemIdSchemaName, ConditionOperator.NotEqual, ExcludeIntegrationSourceSystemId));
                    result = svc.RetrieveMultiple(query);
                }
                else
                    result = null;
            }

            if (ids != null && ids.Count > 0)
                return ids;
            else
                return null;
        }
        #endregion Retrieve

        #region Get
        /// <summary>
        /// Get the record from CRM as Xrm object based on the search condition parameters pass in
        /// </summary>
        /// <typeparam name="T">Xrm Object</typeparam>
        /// <param name="query">The CRM retrieve Query</param>
        /// <param name="ThrowExceptionIfMoreThanOneFound">Indicate whether exception is thrown when more than one record is found for the search condition</param>
        /// <returns>Returns the entity as Xrm object</returns>
        public T GetFirstEntityRetrieve<T>(QueryExpression query, bool ThrowExceptionIfMoreThanOneFound = false) where T : class
        {
            T obj = null;
            EntityCollection result = svc.RetrieveMultiple(query);
            if (result != null && result.Entities.Count > 0)
            {
                //more than one record found, throw exception when required
                if (ThrowExceptionIfMoreThanOneFound && result.Entities.Count > 1)
                    throw new Exception(string.Format(XRMDataImport.Message.GetFirstDuplicateError, result.Entities.Count.ToString(), query.EntityName, GetListOfCrmIds(result, query.EntityName)));

                obj = result[0] as T;
            }
            return obj;
        }

        /// <summary>
        /// Get the record from CRM as Xrm object based on the search condition parameters pass in
        /// </summary>
        /// <typeparam name="T">Xrm Object</typeparam>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="ConditionSchemaName">Schema Name of the attribute to search</param>
        /// <param name="ConditionValue">Search condition value</param>
        /// <param name="ColumnSets">Specify the columns to retrieve</param>
        /// <param name="OrderBySchemaName">Order By attribute schema name</param>
        /// <param name="OrderByType">Order By Ascending or Descending</param>
        /// <param name="ThrowExceptionIfMoreThanOneFound">Indicate whether exception is thrown when more than one record is found for the search condition</param>
        /// <returns>Returns the entity as Xrm object</returns>
        public T GetFirstEntityRetrieve<T>(string EntityLogicalName, string[] ConditionSchemaName, object[] ConditionValue, string[] ColumnSets = null, string[] OrderBySchemaName = null, OrderType[] OrderByType = null, bool ThrowExceptionIfMoreThanOneFound = false) where T : class
        {
            T obj = null;
            QueryExpression query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, ConditionSchemaName, ConditionValue, ColumnSets, null, OrderBySchemaName, OrderByType);
            EntityCollection result = svc.RetrieveMultiple(query);
            if (result != null && result.Entities.Count > 0)
            {
                //more than one record found, throw exception when required
                if (ThrowExceptionIfMoreThanOneFound && result.Entities.Count > 1)
                    throw new Exception(string.Format(XRMDataImport.Message.GetFirstDuplicateError, result.Entities.Count.ToString(), EntityLogicalName, GetListOfCrmIds(result, EntityLogicalName)));

                obj = result[0] as T;
            }
            return obj;
        }

        /// <summary>
        /// Get the record from CRM as Xrm object based on the search condition parameters pass in
        /// </summary>
        /// <typeparam name="T">Xrm Object</typeparam>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="ConditionSchemaName">Schema Name of the attribute to search</param>
        /// <param name="ConditionValue">Search condition value</param>
        /// <param name="ColumnSets">Specify the columns to retrieve</param>
        /// <param name="OrderBySchemaName">Order By attribute schema name</param>
        /// <param name="OrderByType">Order By Ascending or Descending</param>
        /// <param name="ThrowExceptionIfMoreThanOneFound">Indicate whether exception is thrown when more than one record is found for the search condition</param>
        /// <returns>Returns the entity as Xrm object</returns>
        public T GetFirstEntityRetrieve<T>(string EntityLogicalName, string[] ConditionSchemaName, object[] ConditionValue, out bool InactiveRecord, out EntityReference Owner, string[] ColumnSets = null, string[] OrderBySchemaName = null, OrderType[] OrderByType = null, bool ThrowExceptionIfMoreThanOneFound = false) where T : class
        {
            T obj = null;
            InactiveRecord = false;
            Owner = null;

            //make sure the statecode and ownerid is being retrieve as well
            if (ColumnSets != null)
            {
                List<string> tempcolumns = ColumnSets.ToList();
                if (!ColumnSets.Contains(XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode, new CasualStringComparerObject()))
                    tempcolumns.Add(XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode);
                if (!ColumnSets.Contains(XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid, new CasualStringComparerObject()))
                    tempcolumns.Add(XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid);
            }
            QueryExpression query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, ConditionSchemaName, ConditionValue, ColumnSets, null, OrderBySchemaName, OrderByType);
            EntityCollection result = svc.RetrieveMultiple(query);
            if (result != null && result.Entities.Count > 0)
            {
                //more than one record found, throw exception when required
                if (ThrowExceptionIfMoreThanOneFound && result.Entities.Count > 1)
                    throw new Exception(string.Format(XRMDataImport.Message.GetFirstDuplicateError, result.Entities.Count.ToString(), EntityLogicalName, GetListOfCrmIds(result, EntityLogicalName)));

                obj = result[0] as T;

                //get statecode
                if (result.Entities[0].Attributes.ContainsKey(XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode) && result.Entities[0][XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode] != null)
                {
                    if (((OptionSetValue)result.Entities[0][XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode]).Value == XRMDataImport.OutOfBoxCrmAttributes.StateValue.Inactive)
                        InactiveRecord = true;
                    else
                        InactiveRecord = false;
                }
                else
                    InactiveRecord = false;

                //get owner
                if (result.Entities[0].Attributes.ContainsKey(XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid) && result.Entities[0][XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid] != null)
                    Owner = (EntityReference)result.Entities[0][XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid];
            }
            return obj;
        }

        /// <summary>
        /// Get the CrmId from CRM for the required entity based on the search condition parameters pass in
        /// </summary>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="EntityCrmIdSchemaName">The PrimaryKey of the Entity where the CrmId is</param>
        /// <param name="ConditionSchemaName">Schema Name of the attribute to search</param>
        /// <param name="ConditionValue">Search condition value</param>
        /// <param name="ThrowExceptionIfMoreThanOneFound">Indicate whether exception is thrown when more than one record is found for the search condition</param>
        /// <returns>Returns the CrmId Guid for the entity</returns>
        public Guid? GetFirstCrmIdForEntity(string EntityLogicalName, string EntityCrmIdSchemaName, string[] ConditionSchemaName, object[] ConditionValue, string[] OrderBySchemaName = null, OrderType[] OrderByType = null, bool ThrowExceptionIfMoreThanOneFound = false)
        {
            Guid? id = null;
            QueryExpression query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, ConditionSchemaName, ConditionValue, new string[] { EntityCrmIdSchemaName }, null, OrderBySchemaName, OrderByType);
            EntityCollection result = svc.RetrieveMultiple(query);
            if (result != null && result.Entities.Count > 0 && result.Entities[0].Attributes.ContainsKey(EntityCrmIdSchemaName))
            {
                //more than one record found, throw exception when required
                if (ThrowExceptionIfMoreThanOneFound && result.Entities.Count > 1)
                    throw new Exception(string.Format(XRMDataImport.Message.GetFirstDuplicateError, result.Entities.Count.ToString(), EntityLogicalName, GetListOfCrmIds(result, EntityLogicalName)));

                id = Guid.Parse(result.Entities[0][EntityCrmIdSchemaName].ToString());
            }
            return id;
        }

        /// <summary>
        /// Get the CrmId from CRM for the required entity based on the search condition parameters pass in
        /// </summary>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="EntityCrmIdSchemaName">The PrimaryKey of the Entity where the CrmId is</param>
        /// <param name="ConditionSchemaName">Schema Name of the attribute to search</param>
        /// <param name="ConditionValue">Search condition value</param>
        /// <param name="InactiveRecord">Returns whether the existing record in CRM is Inactive when found</param>
        /// <param name="Owner">Returns the owner of the existing record in CRM when found</param>
        /// <param name="ThrowExceptionIfMoreThanOneFound">Indicate whether exception is thrown when more than one record is found for the search condition</param>
        /// <returns>Returns the CrmId Guid for the entity and pass out the state of the entity</returns>
        public Guid? GetFirstCrmIdForEntity(string EntityLogicalName, string EntityCrmIdSchemaName, string[] ConditionSchemaName, object[] ConditionValue, out bool InactiveRecord, out EntityReference Owner, bool ThrowExceptionIfMoreThanOneFound = false)
        {
            Guid? id = null;
            InactiveRecord = false;
            Owner = null;
            QueryExpression query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, ConditionSchemaName, ConditionValue, new string[] { EntityCrmIdSchemaName, XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode, XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid });
            EntityCollection result = svc.RetrieveMultiple(query);
            if (result != null && result.Entities.Count > 0 && result.Entities[0].Attributes.ContainsKey(EntityCrmIdSchemaName))
            {
                //more than one record found, throw exception when required
                if (ThrowExceptionIfMoreThanOneFound && result.Entities.Count > 1)
                    throw new Exception(string.Format(XRMDataImport.Message.GetFirstDuplicateError, result.Entities.Count.ToString(), EntityLogicalName, GetListOfCrmIds(result, EntityLogicalName)));

                id = Guid.Parse(result.Entities[0][EntityCrmIdSchemaName].ToString());

                //get statecode
                if (result.Entities[0].Attributes.ContainsKey(XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode) && result.Entities[0][XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode] != null)
                {
                    if (((OptionSetValue)result.Entities[0][XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode]).Value == XRMDataImport.OutOfBoxCrmAttributes.StateValue.Inactive)
                        InactiveRecord = true;
                    else
                        InactiveRecord = false;
                }
                else
                    InactiveRecord = false;

                //get owner
                if (result.Entities[0].Attributes.ContainsKey(XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid) && result.Entities[0][XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid] != null)
                    Owner = (EntityReference)result.Entities[0][XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid];
            }
            return id;
        }

        /// <summary>
        /// Get the record from CRM as Entity object using the CrmId
        /// </summary>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="EntityCrmIdSchemaName">The PrimaryKey of the Entity where the CrmId is</param>
        /// <param name="CrmId">The PrimaryKey Guid</param>
        /// <returns>Returns the entity object</returns>
        public Entity GetEntityByCrmId(string EntityLogicalName, string EntityCrmIdSchemaName, Guid CrmId)
        {
            Entity obj = null;
            QueryExpression query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { EntityCrmIdSchemaName }, new object[] { CrmId });
            EntityCollection result = svc.RetrieveMultiple(query);
            if (result != null && result.Entities.Count > 0 && result.Entities[0].Attributes.ContainsKey(EntityCrmIdSchemaName))
                obj = result[0];
            return obj;
        }

        /// <summary>
        /// Get the record from CRM as Entity object using the CrmId
        /// </summary>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="EntityCrmIdSchemaName">The PrimaryKey of the Entity where the CrmId is</param>
        /// <param name="CrmId">The PrimaryKey Guid</param>
        /// <param name="InactiveRecord">Returns whether the existing record in CRM is Inactive when found</param>
        /// <param name="Owner">Returns the owner of the existing record in CRM when found</param>
        /// <returns>Returns the entity object</returns>
        public Entity GetEntityByCrmId(string EntityLogicalName, string EntityCrmIdSchemaName, Guid CrmId, out bool InactiveRecord, out EntityReference Owner)
        {
            Entity obj = null;
            InactiveRecord = false;
            Owner = null;
            QueryExpression query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { EntityCrmIdSchemaName }, new object[] { CrmId });
            EntityCollection result = svc.RetrieveMultiple(query);
            if (result != null && result.Entities.Count > 0 && result.Entities[0].Attributes.ContainsKey(EntityCrmIdSchemaName))
            {
                obj = result[0];

                //get statecode
                if (result.Entities[0].Attributes.ContainsKey(XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode) && result.Entities[0][XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode] != null)
                {
                    if (((OptionSetValue)result.Entities[0][XRMDataImport.OutOfBoxCrmAttributes.SchemaName.statecode]).Value == XRMDataImport.OutOfBoxCrmAttributes.StateValue.Inactive)
                        InactiveRecord = true;
                    else
                        InactiveRecord = false;
                }
                else
                    InactiveRecord = false;

                //get owner
                if (result.Entities[0].Attributes.ContainsKey(XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid) && result.Entities[0][XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid] != null)
                    Owner = (EntityReference)result.Entities[0][XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid];
            }
            return obj;
        }

        /// <summary>
        /// Get the record from CRM as Xrm object using the CrmId
        /// </summary>
        /// <typeparam name="T">Xrm Object</typeparam>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="EntityCrmIdSchemaName">The PrimaryKey of the Entity where the CrmId is</param>
        /// <param name="CrmId">The PrimaryKey Guid</param>
        /// <returns>Returns the Xrm object</returns>
        public T GetEntityByCrmId<T>(string EntityLogicalName, string EntityCrmIdSchemaName, Guid CrmId) where T : class
        {
            T obj = null;
            QueryExpression query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { EntityCrmIdSchemaName }, new object[] { CrmId });
            EntityCollection result = svc.RetrieveMultiple(query);
            if (result != null && result.Entities.Count > 0 && result.Entities[0].Attributes.ContainsKey(EntityCrmIdSchemaName))
                obj = result[0] as T;
            return obj;
        }

        /// <summary>
        /// Get the ownerid of a record from CRM using the CrmId
        /// </summary>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="EntityCrmIdSchemaName">The PrimaryKey of the Entity where the CrmId is</param>
        /// <param name="CrmId">The PrimaryKey Guid</param>
        /// <returns>Returns the owner as entityreference object</returns>
        public EntityReference GetRecordOwnerByCrmId(string EntityLogicalName, string EntityCrmIdSchemaName, Guid CrmId)
        {
            EntityReference obj = null;
            QueryExpression query = CreateQueryExpressionForRetrieveMultiple(EntityLogicalName, new string[] { EntityCrmIdSchemaName }, new object[] { CrmId }, new string[] { XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid });
            EntityCollection result = svc.RetrieveMultiple(query);
            if (result != null && result.Entities.Count > 0 && result.Entities[0].Attributes.ContainsKey(XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid))
                obj = (EntityReference)result[0][XRMDataImport.OutOfBoxCrmAttributes.SchemaName.ownerid];
            return obj;
        }
        #endregion Get

        #region Aggregate
        /// <summary>
        /// Get the total record count from CRM based on the search condition parameters pass in
        /// </summary>
        /// <param name="entityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="query">The query expression.</param>
        /// <returns>Returns the total count.</returns>
        public long GetTotalRecordCountByQuery(string entityLogicalName, QueryExpression query)
        {
            long totalCount = 0;
            int pagenumber = 1;
            EntityCollection result = svc.RetrieveMultiple(query);

            while (result != null && result.Entities.Count > 0)
            {
                totalCount += result.Entities.Count;
                if (result.MoreRecords)
                {
                    //increase page number
                    pagenumber++;
                    query.PageInfo.PageNumber = pagenumber;
                    query.PageInfo.PagingCookie = result.PagingCookie;
                    result = svc.RetrieveMultiple(query);
                }
                else
                    result = null;
            }
            return totalCount;
        }
        #endregion Aggregate
        #endregion genericlookup
        #endregion Public

        #region Helper
        #region CreateQueryExpressionForRetrieveMultiple
        /// <summary>
        /// Create Query Expression for RetrieveMultiple with condition with LogicalOperator.And
        /// </summary>
        /// <param name="EntityLogicalName">EntityLogicalName of the entity to lookup</param>
        /// <param name="ConditionSchemaName">Schema Name of the attribute to search</param>
        /// <param name="ConditionValue">Search condition value</param>
        /// <param name="ColumnSets">Specify the columns to retrieve</param>
        /// <param name="PageNumber">Page Number of the query</param>
        /// <param name="OrderBySchemaName">Order By attribute schema name</param>
        /// <param name="OrderByType">Order By Ascending or Descending</param>
        /// <returns>Return a query expression object to be used in RetrieveMultiple</returns>
        private QueryExpression CreateQueryExpressionForRetrieveMultiple(string EntityLogicalName, string[] ConditionSchemaName, object[] ConditionValue, string[] ColumnSets = null, int? PageNumber = null, string[] OrderBySchemaName = null, OrderType[] OrderByType = null)
        {
            QueryExpression query = new QueryExpression(EntityLogicalName);
            query.NoLock = true;

            //column set
            if (ColumnSets == null)
                query.ColumnSet = new ColumnSet(true);
            else
                query.ColumnSet = new ColumnSet(ColumnSets);

            //filter expression
            if (ConditionSchemaName != null)
            {
                query.Criteria = new FilterExpression(LogicalOperator.And);
                for (int i = 0; i < ConditionSchemaName.Length; i++)
                {
                    if (string.IsNullOrEmpty(ConditionSchemaName[i]))
                        throw new ArgumentException("Shared.CreateQueryExpressionForRetrieveAll: Attribute (ConditionSchemaName) for Search Condition in CRM QueryExpression cannot be null.");
                    else
                        query.Criteria.AddCondition(new ConditionExpression(ConditionSchemaName[i], ConditionOperator.Equal, ConditionValue[i]));
                }
            }

            //order by
            if (OrderBySchemaName != null)
            {
                for (int i = 0; i < OrderBySchemaName.Length; i++)
                {
                    if (string.IsNullOrEmpty(OrderBySchemaName[i]))
                        throw new ArgumentException("Shared.CreateQueryExpressionForRetrieveAll: Attribute (OrderBySchemaName) for Order By Condition in CRM QueryExpression cannot be null.");
                    else
                    {
                        if (OrderByType != null)
                            query.AddOrder(OrderBySchemaName[i], OrderByType[i]);
                        else
                            query.AddOrder(OrderBySchemaName[i], OrderType.Ascending);
                    }
                }
            }

            //paging
            if (PageNumber != null)
            {
                query.PageInfo = new PagingInfo();
                query.PageInfo.ReturnTotalRecordCount = true;
                query.PageInfo.PageNumber = PageNumber.Value;
            }

            return query;
        }
        #endregion CreateQueryExpressionForRetrieveMultiple

        #region GetListOfCrmIds
        /// <summary>
        /// Returns the Entity's Primary Key (ID) field name:
        /// <para>NOTE: Uses a new method to get the ID, if it does not work, will required to manually add target Ids to case statement</para>
        /// </summary>
        /// <param name="EntityLogicalName">The entity name to match</param>
        /// <returns>PrimaryKey Schema Name</returns>
        private string GetCrmIdSchemaName(string EntityLogicalName)
        {
            if (string.IsNullOrEmpty(EntityLogicalName))
                throw new ArgumentException("EntityLogicalName");

            //new approach by appending the id behind the entity logical name
            string schemaname = null;
            schemaname = EntityLogicalName.ToLower() + XRMDataImport.OutOfBoxCrmAttributes.SchemaName.primarykeyid;

            return schemaname;
        }

        /// <summary>
        /// Returns the list of CrmIds in a list separated by comma
        /// </summary>
        /// <param name="Result">Entity Collection of all the entities</param>
        /// <param name="EntityLogicalName">Entity logical name</param>
        /// <returns>Returns the list of CrmIds in a list separated by comma</returns>
        private string GetListOfCrmIds(EntityCollection Result, string EntityLogicalName)
        {
            string ids = string.Empty;
            string entityCrmIdSchemaName = GetCrmIdSchemaName(EntityLogicalName);
            bool first = true;
            foreach (Entity e in Result.Entities)
            {
                if (e.Attributes.ContainsKey(entityCrmIdSchemaName))
                {
                    if (first)
                        ids += Guid.Parse(e[entityCrmIdSchemaName].ToString()).ToString();
                    else
                        ids += ", " + Guid.Parse(e[entityCrmIdSchemaName].ToString()).ToString();
                    first = false;
                }
            }

            if (ids.Length > 0)
                return ids;
            else
                return "null";
        }
        #endregion GetListOfCrmIds
        #endregion Helper

        #region XRMDataImport
        internal class XRMDataImport
        {
            #region Message
            //generic error messages
            public class Message
            {
                public const string GetFirstDuplicateError = "There are {0} record(s) of '{1}' found in the system that match the search condition. The records has the CrmId: {2}.";
            }
            #endregion Message

            #region OutOfBoxCrmAttributes
            //out-of-box CRM common attributes
            public class OutOfBoxCrmAttributes
            {
                public class SchemaName
                {
                    public const string primarykeyid = "id";
                    public const string statecode = "statecode";
                    public const string ownerid = "ownerid";
                    public const string owningbusinessunit = "owningbusinessunit";
                }

                public class StateValue
                {
                    public const int Active = 0;
                    public const int Inactive = 1;
                }

                public class StatusCodeValue
                {
                    public const int Active = 1;
                    public const int Inactive = 2;
                }
            }
            #endregion OutOfBoxCrmAttributes
        }
        #endregion XRMDataImport
    }
}