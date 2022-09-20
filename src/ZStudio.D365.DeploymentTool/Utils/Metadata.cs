using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk;
using ZD365DT.DeploymentTool.Utils.CrmMetadata;

namespace ZD365DT.DeploymentTool.Utils
{
    public static class Metadata
    {
        #region Static Variables
        private static Version _crmVersion = null;
        private static Dictionary<string, EntityMetadata[]> _entityMetadata = new Dictionary<string, EntityMetadata[]>();
        private static Dictionary<string, OptionSetMetadataBase[]> _optionsetMetadataBase = new Dictionary<string, OptionSetMetadataBase[]>();
        private static Dictionary<string, Dictionary<string, RelationshipMetadataBase>> _relationshipMetadataBase = new Dictionary<string, Dictionary<string, RelationshipMetadataBase>>();
        #endregion Static Variables

        #region SetCurrentVersion
        public static void SetCurrentCrmVersion(Version v)
        {
            _crmVersion = v;
        }
        #endregion SetCurrentVersion

        #region LoadCache
        /// <summary>
        /// Load all Entity in the system into a cache, only the Entity Settings
        /// </summary>
        /// <param name="util"></param>
        private static void LoadEntityMetadata(WebServiceUtils util)
        {
            string key = util.CurrentOrgContext.OrganizationId.ToString();
            RetrieveAllEntitiesRequest getAll = new RetrieveAllEntitiesRequest();
            getAll.EntityFilters = EntityFilters.Entity;
            RetrieveAllEntitiesResponse resp = (RetrieveAllEntitiesResponse)util.Service.Execute(getAll);
            if (_entityMetadata.ContainsKey(key))
                _entityMetadata[key] = resp.EntityMetadata;
            else
                _entityMetadata.Add(key, resp.EntityMetadata);
        }

        /// <summary>
        /// Load all Option Sets in the system into a cache
        /// </summary>
        /// <param name="util"></param>
        private static void LoadOptionSetMetadata(WebServiceUtils util)
        {
            string key = util.CurrentOrgContext.OrganizationId.ToString();
            RetrieveAllOptionSetsRequest getAll = new RetrieveAllOptionSetsRequest();
            RetrieveAllOptionSetsResponse resp = (RetrieveAllOptionSetsResponse)util.Service.Execute(getAll);
            if (_optionsetMetadataBase.ContainsKey(key))
                _optionsetMetadataBase[key] = resp.OptionSetMetadata;
            else
                _optionsetMetadataBase.Add(key, resp.OptionSetMetadata);
            //Array.ConvertAll(resp.OptionSetMetadata, item => (OptionSetMetadata)item)
        }

        /// <summary>
        /// Get a Relationship Metadata by querying Relationship Name and cached it
        /// </summary>
        /// <param name="util"></param>
        /// <param name="relationshipName"></param>
        /// <returns></returns>
        private static RelationshipMetadataBase GetRelationshipMetadataBase(WebServiceUtils util, string relationshipName)
        {
            string key = util.CurrentOrgContext.OrganizationId.ToString();
            if (!_relationshipMetadataBase.ContainsKey(key))
            {
                _relationshipMetadataBase.Add(key, new Dictionary<string, RelationshipMetadataBase>());
            }

            if (_relationshipMetadataBase[key].ContainsKey(relationshipName))
                return _relationshipMetadataBase[key][relationshipName];
            else
            {
                //get from CRM
                RetrieveRelationshipRequest req = new RetrieveRelationshipRequest { Name = relationshipName };
                try
                {
                    RetrieveRelationshipResponse resp = (RetrieveRelationshipResponse)util.Service.Execute(req);
                    if (resp != null)
                    {
                        //add to cache
                        _relationshipMetadataBase[key].Add(relationshipName, resp.RelationshipMetadata);
                        return _relationshipMetadataBase[key][relationshipName];
                    }
                    else
                        return null;
                }
                catch
                {
                    //not found
                    return null;
                }
            }
        }
        #endregion LoadCache

        #region GetFromCache
        private static EntityMetadata GetEntityMetadata(WebServiceUtils util, string entityLogicalName, bool retry = false, bool reload = false, bool includeAttributes = false)
        {
            EntityMetadata output = null;

            int index = -1;
            bool newlyLoaded = false;
            string key = util.CurrentOrgContext.OrganizationId.ToString();

            if (!_entityMetadata.ContainsKey(key) || reload)
            {
                LoadEntityMetadata(util);
                newlyLoaded = true;
            }
            output = GetEntityMetadata(_entityMetadata[key], entityLogicalName, out index);

            //if not found and not newly loaded and retry, try reloading cache, just in case it is newly created after the cache has been loaded
            if (output == null && !newlyLoaded && retry)
            {
                LoadEntityMetadata(util);
                output = GetEntityMetadata(_entityMetadata[key], entityLogicalName, out index);
            }

            //attribute metadata is required as well
            if (includeAttributes && output != null && (output.Attributes == null || (output.Attributes != null && output.Attributes.Length <= 0)))
            {
                //need to get the attribute
                UpdateEntityMetadata(util, entityLogicalName, true);
                output = GetEntityMetadata(_entityMetadata[key], entityLogicalName, out index);
            }

            return output;
        }

        private static EntityMetadata GetEntityMetadata(WebServiceUtils util, Guid metadataId, bool retry = false, bool reload = false, bool includeAttributes = false)
        {
            EntityMetadata output = null;

            int index = -1;
            bool newlyLoaded = false;
            string key = util.CurrentOrgContext.OrganizationId.ToString();
            if (!_entityMetadata.ContainsKey(key) || reload)
            {
                LoadEntityMetadata(util);
                newlyLoaded = true;
            }
            output = GetEntityMetadata(_entityMetadata[key], metadataId, out index);

            //if not found and not newly loaded and retry, try reloading cache, just in case it is newly created after the cache has been loaded
            if (output == null && !newlyLoaded && retry)
            {
                LoadEntityMetadata(util);
                output = GetEntityMetadata(_entityMetadata[key], metadataId, out index);
            }

            //attribute metadata is required as well
            if (includeAttributes && output != null && (output.Attributes == null || (output.Attributes != null && output.Attributes.Length <= 0)))
            {
                //need to get the attribute
                UpdateEntityMetadata(util, output.LogicalName, true);
                output = GetEntityMetadata(_entityMetadata[key], metadataId, out index);
            }

            return output;
        }

        /// <summary>
        /// Get the matching item in the array and also return the index of the array item
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="entityLogicalName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static EntityMetadata GetEntityMetadata(EntityMetadata[] arr, string entityLogicalName, out int index)
        {
            index = -1;
            foreach (EntityMetadata d in arr)
            {
                index++;
                if (string.Compare(d.LogicalName, entityLogicalName, true) == 0)
                {
                    return d;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the matching item in the array and also return the index of the array item
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="metadataId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static EntityMetadata GetEntityMetadata(EntityMetadata[] arr, Guid metadataId, out int index)
        {
            index = -1;
            foreach (EntityMetadata d in arr)
            {
                index++;
                if (d.MetadataId == metadataId)
                {
                    return d;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the matching item in the array and also return the index of the array item
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="attributeName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static AttributeMetadata GetAttributeMetadata(AttributeMetadata[] arr, string attributeName, out int index)
        {
            index = -1;
            foreach (AttributeMetadata d in arr)
            {
                index++;
                if (string.Compare(d.LogicalName, attributeName, true) == 0)
                {
                    return d;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the matching item in the array and also return the index of the array item
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="metadataId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static AttributeMetadata GetAttributeMetadata(AttributeMetadata[] arr, Guid metadataId, out int index)
        {
            index = -1;
            foreach (AttributeMetadata d in arr)
            {
                index++;
                if (d.MetadataId == metadataId)
                {
                    return d;
                }
            }
            return null;
        }

        private static OptionSetMetadata GetOptionSetMetadata(WebServiceUtils util, string optionSchemaName, bool retry = false, bool reload = false)
        {
            OptionSetMetadata output = null;

            int index = -1;
            bool newlyLoaded = false;
            string key = util.CurrentOrgContext.OrganizationId.ToString();
            if (!_optionsetMetadataBase.ContainsKey(key) || reload)
            {
                LoadOptionSetMetadata(util);
                newlyLoaded = true;
            }
            output = GetOptionSetMetadata(_optionsetMetadataBase[key], optionSchemaName, out index);

            //if not found and not newly loaded and retry, try reloading cache, just in case it is newly created after the cache has been loaded
            if (output == null && !newlyLoaded && retry)
            {
                LoadOptionSetMetadata(util);
                output = GetOptionSetMetadata(_optionsetMetadataBase[key], optionSchemaName, out index);
            }

            return output;
        }

        private static OptionSetMetadata GetOptionSetMetadata(WebServiceUtils util, Guid metadataId, bool retry = false, bool reload = false)
        {
            OptionSetMetadata output = null;

            int index = -1;
            bool newlyLoaded = false;
            string key = util.CurrentOrgContext.OrganizationId.ToString();
            if (!_optionsetMetadataBase.ContainsKey(key) || reload)
            {
                LoadOptionSetMetadata(util);
                newlyLoaded = true;
            }
            output = GetOptionSetMetadata(_optionsetMetadataBase[key], metadataId, out index);

            //if not found and not newly loaded and retry, try reloading cache, just in case it is newly created after the cache has been loaded
            if (output == null && !newlyLoaded && retry)
            {
                LoadOptionSetMetadata(util);
                output = GetOptionSetMetadata(_optionsetMetadataBase[key], metadataId, out index);
            }

            return output;
        }

        /// <summary>
        /// Get the matching item in the array and also return the index of the array item
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="optionSchemaName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static OptionSetMetadata GetOptionSetMetadata(OptionSetMetadataBase[] arr, string optionSchemaName, out int index)
        {
            index = -1;
            foreach (OptionSetMetadataBase o in arr)
            {
                index++;
                if (string.Compare(o.Name, optionSchemaName, true) == 0 && o.GetType() == typeof(OptionSetMetadata))
                {
                    return (OptionSetMetadata)o;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the matching item in the array and also return the index of the array item
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="metadataId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static OptionSetMetadata GetOptionSetMetadata(OptionSetMetadataBase[] arr, Guid metadataId, out int index)
        {
            index = -1;
            foreach (OptionSetMetadataBase o in arr)
            {
                index++;
                if (o.MetadataId == metadataId && o.GetType() == typeof(OptionSetMetadata))
                {
                    return (OptionSetMetadata)o;
                }
            }
            return null;
        }
        #endregion GetFromCache

        #region UpdateToCache
        /// <summary>
        /// Add/Update an Option Set into the cache by using the MetadataId
        /// </summary>
        /// <param name="util"></param>
        /// <param name="metadataId"></param>
        public static void UpdateOptionSetMetadata(WebServiceUtils util, Guid metadataId)
        {
            string key = util.CurrentOrgContext.OrganizationId.ToString();
            if (_optionsetMetadataBase.ContainsKey(key))
            {
                //get from CRM and add/update to cache
                OptionSetMetadataBase op = GetGlobalOptionSetBaseById(util, metadataId);
                if (op != null)
                {
                    int index = -1;
                    if (GetOptionSetMetadata(_optionsetMetadataBase[key], metadataId, out index) == null)
                    {
                        int newSize = _optionsetMetadataBase[key].Length + 1;
                        OptionSetMetadataBase[] m = _optionsetMetadataBase[key];
                        Array.Resize<OptionSetMetadataBase>(ref m, newSize);
                        m[newSize - 1] = op;
                        _optionsetMetadataBase[key] = m;
                    }
                    else
                    {
                        _optionsetMetadataBase[key][index] = op;
                    }
                }
            }
        }

        /// <summary>
        /// Add/Update an Entity Metadata into the cache by using the Logical Name and include the Attributes Metadata as well if needed
        /// </summary>
        /// <param name="util"></param>
        /// <param name="entityLogicalName"></param>
        /// <param name="includeAttributes"></param>
        public static void UpdateEntityMetadata(WebServiceUtils util, string entityLogicalName, bool includeAttributes = false)
        {
            string key = util.CurrentOrgContext.OrganizationId.ToString();
            if (_entityMetadata.ContainsKey(key))
            {
                //get from CRM and add/update to cache
                EntityMetadata en = GetSingleEntityMetadataByName(util, entityLogicalName, includeAttributes);
                if (en != null)
                {
                    int index = -1;
                    if (GetEntityMetadata(_entityMetadata[key], entityLogicalName, out index) == null)
                    {
                        int newSize = _entityMetadata[key].Length + 1;
                        EntityMetadata[] m = _entityMetadata[key];
                        Array.Resize<EntityMetadata>(ref m, newSize);
                        m[newSize - 1] = en;
                        _entityMetadata[key] = m;
                    }
                    else
                    {
                        _entityMetadata[key][index] = en;
                    }
                }
            }
        }
        #endregion UpdateToCache

        #region Entity
        /// <summary>
        /// Get OTC of an entity (cached)
        /// </summary>
        public static int GetEntityOTC(WebServiceUtils util, string entityLogicalName)
        {
            EntityMetadata en = GetEntityMetadata(util, entityLogicalName);
            if (en == null)
                return 0;
            else
                return en.ObjectTypeCode.Value;
        }

        /// <summary>
        /// Get Metadata ID of an entity (cached)
        /// </summary>
        public static Guid? GetMetadataId(WebServiceUtils util, string entityLogicalName)
        {
            EntityMetadata en = GetEntityMetadata(util, entityLogicalName);
            if (en == null)
                return null;
            else
                return en.MetadataId;
        }

        /// <summary>
        /// Get logical name of an entity by metadataid (cached)
        /// </summary>
        public static string GetEntityLogicalName(WebServiceUtils util, Guid metadataId)
        {
            EntityMetadata en = GetEntityMetadata(util, metadataId);
            if (en == null)
                return null;
            else
                return en.LogicalName;
        }

        /// <summary>
        /// Get a Entity Metadata by Name (cached)
        /// </summary>
        /// <param name="util"></param>
        /// <param name="entityLogicalName"></param>
        /// <returns></returns>
        public static EntityMetadata GetEntityMetadataByName(WebServiceUtils util, string entityLogicalName, bool includeAttributes = false)
        {
            return GetEntityMetadata(util, entityLogicalName, false, false, includeAttributes);
        }

        /// <summary>
        /// Get a Entity Metadata by GUID (cached)
        /// </summary>
        /// <param name="util"></param>
        /// <param name="metadataId"></param>
        /// <returns></returns>
        public static EntityMetadata GetEntityMetadataById(WebServiceUtils util, Guid metadataId, bool includeAttributes = false)
        {
            return GetEntityMetadata(util, metadataId, false, false, includeAttributes);
        }

        /// <summary>
        /// Get a Entity Metadata by querying Logical Name (NOT CACHED)
        /// NOTE: DO NOT CACHE THIS, CALLED BY THE UPDATE, NEED REAL-TIME DATA
        /// </summary>
        /// <param name="util"></param>
        /// <param name="metadataId"></param>
        /// <returns></returns>
        public static EntityMetadata GetSingleEntityMetadataByName(WebServiceUtils util, string entityLogicalName, bool includeAttributes = false)
        {
            EntityMetadata ent = null;

            try
            {
                EntityFilters filters = EntityFilters.Default;
                if (includeAttributes)
                    filters = EntityFilters.Entity | EntityFilters.Attributes;

                RetrieveEntityRequest req = new RetrieveEntityRequest() { EntityFilters = filters, LogicalName = entityLogicalName };
                //the execution will throw exception when it does not exists
                RetrieveEntityResponse resp = (RetrieveEntityResponse)util.Service.Execute(req);
                if (resp != null && resp.EntityMetadata != null)
                    ent = resp.EntityMetadata;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not find"))
                    ent = null;
                else
                    throw ex;
            }

            return ent;
        }
        #endregion Entity

        #region Attribute
        /// <summary>
        /// Get a Attribute Metadata by Name (cached)
        /// </summary>
        /// <param name="util"></param>
        /// <param name="entityLogicalName"></param>
        /// <param name="includeAttributes"></param>
        /// <returns></returns>
        public static AttributeMetadata GetAttributeMetadataByName(WebServiceUtils util, string entityLogicalName, string attributeName)
        {
            AttributeMetadata atr = null;

            int index = -1;
            EntityMetadata en = GetEntityMetadata(util, entityLogicalName, false, false, true);
            if (en != null)
            {
                atr = GetAttributeMetadata(en.Attributes, attributeName, out index);
            }

            return atr;
        }

        /// <summary>
        /// Get a Attribute Metadata by ID (cached)
        /// </summary>
        /// <param name="util"></param>
        /// <param name="entityLogicalName"></param>
        /// <param name="metadataId"></param>
        /// <returns></returns>
        public static AttributeMetadata GetAttributeMetadataById(WebServiceUtils util, string entityLogicalName, Guid metadataId)
        {
            AttributeMetadata atr = null;

            int index = -1;
            EntityMetadata en = GetEntityMetadata(util, entityLogicalName, false, false, true);
            if (en != null)
            {
                atr = GetAttributeMetadata(en.Attributes, metadataId, out index);
            }

            return atr;
        }

        /// <summary>
        /// Get a Attribute Metadata by querying Logical Name (NOT CACHED)
        /// NOTE: DO NOT CACHE THIS, CALLED BY THE UPDATE, NEED REAL-TIME DATA
        /// </summary>
        /// <param name="util"></param>
        /// <param name="entityLogicalName"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static AttributeMetadata GetSingleAttributeMetadataByName(WebServiceUtils util, string entityLogicalName, string attributeName)
        {
            AttributeMetadata atr = null;

            try
            {
                RetrieveAttributeRequest req = new RetrieveAttributeRequest()
                {
                    EntityLogicalName = entityLogicalName,
                    LogicalName = attributeName,
                };

                //the execution will throw exception when it does not exists
                RetrieveAttributeResponse resp = (RetrieveAttributeResponse)util.Service.Execute(req);
                if (resp != null && resp.AttributeMetadata != null)
                    atr = resp.AttributeMetadata;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not find"))
                    atr = null;
                else
                    throw ex;
            }

            return atr;
        }
        #endregion Attribute

        #region Relationship
        /// <summary>
        /// Get ManyToManyRelationship metadata by relationship name (cached)
        /// </summary>
        /// <param name="util"></param>
        /// <param name="relationshipName"></param>
        /// <returns></returns>
        public static ManyToManyRelationshipMetadata GetManyToManyRelationshipMetadata(WebServiceUtils util, string relationshipName)
        {
            RelationshipMetadataBase meta = GetRelationshipMetadataBase(util, relationshipName);
            if (meta != null)
                return (ManyToManyRelationshipMetadata)meta;
            else
                return null;
        }

        /// <summary>
        /// Get OneToManyRelationship metadata by relationship name (cached)
        /// </summary>
        /// <param name="util"></param>
        /// <param name="relationshipName"></param>
        /// <returns></returns>
        public static OneToManyRelationshipMetadata GetOneToManyRelationshipMetadata(WebServiceUtils util, string relationshipName)
        {
            RelationshipMetadataBase meta = GetRelationshipMetadataBase(util, relationshipName);
            if (meta != null)
                return (OneToManyRelationshipMetadata)meta;
            else
                return null;
        }

        /// <summary>
        /// Get the relationship metadata GUID by relationship name
        /// </summary>
        /// <param name="util"></param>
        /// <param name="relationshipName"></param>
        /// <returns></returns>
        public static Guid? GetRelationshipMetadataIdByName(WebServiceUtils util, string relationshipName)
        {
            RelationshipMetadataBase meta = GetRelationshipMetadataBase(util, relationshipName);
            if (meta != null && meta.MetadataId != null)
                return meta.MetadataId.Value;
            else
                return null;
        }

        /// <summary>
        /// Determine whether the entity can participate in a many-to-many relationship
        /// </summary>
        /// <param name="util"></param>
        /// <param name="entityLogicalName"></param>
        /// <returns></returns>
        public static bool CanBeInManyToManyRelation(WebServiceUtils util, string entityLogicalName)
        {
            EntityMetadata en = GetEntityMetadataByName(util, entityLogicalName);
            if (en != null)
                return en.CanBeInManyToMany.Value;
            else
                return false;
        }
        #endregion Relationship

        #region OptionSet
        /// <summary>
        /// Get a Global OptionSet by GUID
        /// NOTE: DO NOT CACHE THIS, CALLED BY THE UPDATE, NEED REAL-TIME DATA
        /// </summary>
        /// <param name="util"></param>
        /// <param name="metadataId"></param>
        /// <returns></returns>
        public static OptionSetMetadata GetGlobalOptionSetById(WebServiceUtils util, Guid metadataId)
        {
            OptionSetMetadata opt = null;

            try
            {
                RetrieveOptionSetRequest req = new RetrieveOptionSetRequest() { MetadataId = metadataId };
                //the execution will throw exception when it does not exists
                RetrieveOptionSetResponse resp = (RetrieveOptionSetResponse)util.Service.Execute(req);
                if (resp != null && resp.OptionSetMetadata != null)
                    opt = (OptionSetMetadata)resp.OptionSetMetadata;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not find"))
                    opt = null;
                else
                    throw ex;
            }

            return opt;
        }

        public static OptionSetMetadataBase GetGlobalOptionSetBaseById(WebServiceUtils util, Guid metadataId)
        {
            OptionSetMetadataBase opt = null;

            try
            {
                RetrieveOptionSetRequest req = new RetrieveOptionSetRequest() { MetadataId = metadataId };
                //the execution will throw exception when it does not exists
                RetrieveOptionSetResponse resp = (RetrieveOptionSetResponse)util.Service.Execute(req);
                if (resp != null && resp.OptionSetMetadata != null)
                    opt = resp.OptionSetMetadata;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not find"))
                    opt = null;
                else
                    throw ex;
            }

            return opt;
        }

        /// <summary>
        /// Get a Global OptionSet GUID by using the name (cached)
        /// </summary>
        /// <param name="util"></param>
        /// <param name="schemaName">Name of the OptionSet with Prefix</param>
        /// <returns></returns>
        public static Guid? GetGlobalOptionSetIdByName(WebServiceUtils util, string schemaName, bool retry = false, bool reload = false)
        {
            OptionSetMetadata op = GetOptionSetMetadata(util, schemaName, retry, reload);
            if (op == null)
                return null;
            else
                return op.MetadataId;
        }

        /// <summary>
        /// Get a Global OptionSet metadata by using the name (cached)
        /// </summary>
        /// <param name="util"></param>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        public static OptionSetMetadata GetGlobalOptionSetByName(WebServiceUtils util, string schemaName, bool retry = false, bool reload = false)
        {
            return GetOptionSetMetadata(util, schemaName, retry, reload);
        }
        #endregion OptionSet

        #region Create/Update OptionSet
        /// <summary>
        /// Create a Global OptionSet, add it into a CRM solution (if provided) and return the GUID
        /// </summary>
        /// <param name="svc"></param>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Guid CreateGlobalOptionSet(IOrganizationService svc, CrmGlobalOp globalOp, string uniqueSolutionName = null)
        {
            OptionSetMetadata op = new OptionSetMetadata
            {
                Name = globalOp.Name,
                DisplayName = new Label(globalOp.DisplayName, globalOp.LanguageCode),
                Description = new Label(globalOp.Description, globalOp.LanguageCode),
                IsGlobal = true,
                OptionSetType = OptionSetType.Picklist
            };
            foreach (CrmOption c in globalOp.Options)
                op.Options.Add(c.ToOptionMetadata());

            CreateOptionSetRequest req = new CreateOptionSetRequest()
            {
                //create a global option set (OptionSetMetadata)
                OptionSet = op
            };

            //add to solution on creation
            if (!string.IsNullOrEmpty(uniqueSolutionName))
                req.SolutionUniqueName = uniqueSolutionName;

            //execute the request
            CreateOptionSetResponse resp = (CreateOptionSetResponse)svc.Execute(req);
            return resp.OptionSetId;
        }

        /// <summary>
        /// Update a Global Optionset, insert a new option if it does not exists yet, update the option label if it has been changed, delete the option if it does not exists anymore, reorder the options in the Global Optionset
        /// </summary>
        /// <returns></returns>
        public static bool UpdateGlobalOptionSet(WebServiceUtils util, Guid optionId, CrmGlobalOp globalOp, string uniqueSolutionName = null)
        {
            bool result = false;
            OptionSetMetadata opt = GetGlobalOptionSetById(util, optionId);
            if (opt != null)
            {
                //merge the optionset metadata from CRM into this copy to check if the display name or description has change
                globalOp.MergeWith(opt);
                if (globalOp.Action == InstallationAction.Update)
                {
                    //update the global optionset display name and description
                    UpdateOptionSetRequest globalReq = new UpdateOptionSetRequest
                    {
                        OptionSet = new OptionSetMetadata
                        {
                            DisplayName = new Label(globalOp.DisplayName, globalOp.LanguageCode),
                            Name = globalOp.Name,
                            IsGlobal = true,
                            Description = new Label(globalOp.Description, globalOp.LanguageCode)
                        },
                    };

                    //add to solution on creation
                    if (!string.IsNullOrEmpty(uniqueSolutionName))
                        globalReq.SolutionUniqueName = uniqueSolutionName;

                    util.Service.Execute(globalReq);
                }

                //merge the options in CRM into this defined copy to decide what action to take on each option value
                globalOp.Options.MergeWith(opt.Options.ToArray());

                //insert any new options not in CRM yet
                if (globalOp.Options.OptionToCreate() != null && globalOp.Options.OptionToCreate().Length > 0)
                {
                    foreach (CrmOption opToAction in globalOp.Options.OptionToCreate())
                    {
                        InsertOptionValueRequest req = new InsertOptionValueRequest
                        {
                            OptionSetName = globalOp.Name,
                            Value = opToAction.Value,
                            Label = new Label(opToAction.Label, opToAction.LanguageCode),
                            Description = new Label(opToAction.Description, opToAction.LanguageCode)
                        };

                        util.Service.Execute(req);
                    }
                }

                //update any existing options in CRM
                if (globalOp.Options.OptionToUpdate() != null && globalOp.Options.OptionToUpdate().Length > 0)
                {
                    foreach (CrmOption opToAction in globalOp.Options.OptionToUpdate())
                    {
                        UpdateOptionValueRequest req = new UpdateOptionValueRequest
                        {
                            OptionSetName = globalOp.Name,
                            Value = opToAction.Value,
                            Label = new Label(opToAction.Label, opToAction.LanguageCode),
                            Description = new Label(opToAction.Description, opToAction.LanguageCode)
                        };

                        util.Service.Execute(req);
                    }
                }

                //delete any existing options in CRM that is no longer in the list
                if (globalOp.Options.OptionToDelete() != null && globalOp.Options.OptionToDelete().Length > 0)
                {
                    foreach (CrmOption opToAction in globalOp.Options.OptionToDelete())
                    {
                        DeleteOptionValueRequest req = new DeleteOptionValueRequest
                        {
                            OptionSetName = globalOp.Name,
                            Value = opToAction.Value
                        };

                        util.Service.Execute(req);
                    }
                }

                //update the ordering of all the remaining options in case the order has change
                ReorderGlobalOptionSet(util.Service, globalOp.Name, globalOp.Options.ToOptionMetadataArray(true));

                result = true;
            }
            else
            {
                throw new Exception(string.Format("Global Optionset with the ID '{0}' not found.", optionId.ToString()));
            }

            return result;
        }

        /// <summary>
        /// Submit a reorder request for a Global Optionset
        /// </summary>
        /// <param name="svc"></param>
        /// <param name="name"></param>
        /// <param name="orderedOptionList"></param>
        public static void ReorderGlobalOptionSet(IOrganizationService svc, string name, OptionMetadata[] orderedOptionList)
        {
            //create the request.
            OrderOptionRequest req = new OrderOptionRequest
            {
                OptionSetName = name,
                Values = orderedOptionList.Select(x => x.Value.Value).ToArray()
            };

            //execute the request
            svc.Execute(req);
        }

        /// <summary>
        /// Update a Local Optionset, insert a new option if it does not exists yet, update the option label if it has been changed, delete the option if it does not exists anymore, reorder the options in the Global Optionset
        /// </summary>
        /// <returns></returns>
        public static bool UpdateLocalOptionSet(WebServiceUtils util, string entityLogicalName, string attributeLogicalName, CrmOptionCollection opt)
        {
            bool result = false;
            
            //insert any new options not in CRM yet
            if (opt.OptionToCreate() != null && opt.OptionToCreate().Length > 0)
            {
                foreach (CrmOption opToAction in opt.OptionToCreate())
                {
                    InsertOptionValueRequest req = new InsertOptionValueRequest
                    {
                        EntityLogicalName = entityLogicalName,
                        AttributeLogicalName = attributeLogicalName,
                        Value = opToAction.Value,
                        Label = new Label(opToAction.Label, opToAction.LanguageCode),
                        Description = new Label(opToAction.Description, opToAction.LanguageCode)
                    };

                    util.Service.Execute(req);
                }
            }

            //update any existing options in CRM
            if (opt.OptionToUpdate() != null && opt.OptionToUpdate().Length > 0)
            {
                foreach (CrmOption opToAction in opt.OptionToUpdate())
                {
                    UpdateOptionValueRequest req = new UpdateOptionValueRequest
                    {
                        EntityLogicalName = entityLogicalName,
                        AttributeLogicalName = attributeLogicalName,
                        Value = opToAction.Value,
                        Label = new Label(opToAction.Label, opToAction.LanguageCode),
                        Description = new Label(opToAction.Description, opToAction.LanguageCode)
                    };

                    util.Service.Execute(req);
                }
            }

            //delete any existing options in CRM that is no longer in the list
            if (opt.OptionToDelete() != null && opt.OptionToDelete().Length > 0)
            {
                foreach (CrmOption opToAction in opt.OptionToDelete())
                {
                    DeleteOptionValueRequest req = new DeleteOptionValueRequest
                    {
                        EntityLogicalName = entityLogicalName,
                        AttributeLogicalName = attributeLogicalName,
                        Value = opToAction.Value
                    };

                    util.Service.Execute(req);
                }
            }

            //update the ordering of all the remaining options in case the order has change
            ReorderLocalOptionSet(util.Service, entityLogicalName, attributeLogicalName, opt.ToOptionMetadataArray(true));
            result = true;

            return result;
        }

        /// <summary>
        /// Submit a reorder request for a Local Optionset
        /// </summary>
        /// <param name="svc"></param>
        /// <param name="name"></param>
        /// <param name="orderedOptionList"></param>
        public static void ReorderLocalOptionSet(IOrganizationService svc, string entityLogicalName, string attributeLogicalName, OptionMetadata[] orderedOptionList)
        {
            //create the request.
            OrderOptionRequest req = new OrderOptionRequest
            {
                EntityLogicalName = entityLogicalName,
                AttributeLogicalName = attributeLogicalName,
                Values = orderedOptionList.Select(x => x.Value.Value).ToArray()
            };

            //execute the request
            svc.Execute(req);
        }
        #endregion Create/Update OptionSet

        #region Create/Update Entity
        /// <summary>
        /// Create a Custom Entity and return the GUID
        /// </summary>
        /// <param name="svc"></param>
        /// <param name="ent"></param>
        /// <returns></returns>
        public static Guid CreateCrmEntity(IOrganizationService svc, CrmEntity ent)
        {
            if (ent.EntityRequestToCreate != null)
            {
                CreateEntityRequest req = ent.EntityRequestToCreate;

                //remove HasFeedback if deploying to CRM 2016 below v8.1, HasFeedback only exists on CRM 2016 SP1 onwards
                if (_crmVersion.Major >= 8 && _crmVersion.Minor < 1 && req.Parameters.ContainsKey("HasFeedback"))
                    req.Parameters.Remove("HasFeedback");

                CreateEntityResponse resp = (CreateEntityResponse)svc.Execute(req);
                return resp.EntityId;
            }
            return Guid.Empty;
        }

        /// <summary>
        /// Update an existing System/Custom Entity
        /// </summary>
        /// <param name="svc"></param>
        /// <param name="ent"></param>
        public static void UpdateCrmEntity(IOrganizationService svc, UpdateEntityRequest req)
        {
            if (req != null)
            {
                //remove HasFeedback if deploying to CRM 2016 below v8.1, HasFeedback only exists on CRM 2016 SP1 onwards
                if (_crmVersion.Major >= 8 && _crmVersion.Minor < 1 && req.Parameters.ContainsKey("HasFeedback"))
                    req.Parameters.Remove("HasFeedback");

                UpdateEntityResponse resp = (UpdateEntityResponse)svc.Execute(req);
            }
        }

        /// <summary>
        /// Update an existing System/Custom Entity
        /// </summary>
        /// <param name="svc"></param>
        /// <param name="ent"></param>
        public static void UpdateCrmEntity(IOrganizationService svc, CrmEntity ent)
        {
            if (ent.EntityRequestToUpdate != null)
            {
                UpdateEntityRequest req = ent.EntityRequestToUpdate;

                //remove HasFeedback if deploying to CRM 2016 below v8.1, HasFeedback only exists on CRM 2016 SP1 onwards
                if (_crmVersion.Major >= 8 && _crmVersion.Minor < 1 && req.Parameters.ContainsKey("HasFeedback"))
                    req.Parameters.Remove("HasFeedback");

                UpdateEntityResponse resp = (UpdateEntityResponse)svc.Execute(req);
            }
        }
        #endregion Create/Update Entity

        #region Create/Update Attribute
        /// <summary>
        /// Create a Custom Attribute in an Entity, add it into a CRM solution (if provided) and return the GUID
        /// </summary>
        /// <returns></returns>
        public static Guid CreateCrmAttribute(WebServiceUtils util, CrmEntity ent, CrmAttribute attr, string uniqueSolutionName = null)
        {
            bool create1toNRelationship = false;
            AttributeMetadata attrMeta = attr.CreateCrmAttributeMetadata(util, out create1toNRelationship);

            if (!create1toNRelationship)
            {
                //create an attribute without relationship
                CreateAttributeRequest req = new CreateAttributeRequest
                {
                    EntityName = ent.Name,
                    Attribute = attrMeta
                };

                //add to solution on creation
                if (!string.IsNullOrEmpty(uniqueSolutionName))
                    req.SolutionUniqueName = uniqueSolutionName;

                CreateAttributeResponse resp = (CreateAttributeResponse)util.Service.Execute(req);
                return resp.AttributeId;
            }
            else
            {
                //create an attribute with relationship
                if (attr.CrmDataType == CrmAttributeDataType.Lookup)
                {
                    //lookup
                    CreateOneToManyRequest req = new CreateOneToManyRequest()
                    {
                        Lookup = (LookupAttributeMetadata)attrMeta,
                        OneToManyRelationship = attr.CreateOneToManyRelationshipLookup(ent),
                    };

                    //add to solution on creation
                    if (!string.IsNullOrEmpty(uniqueSolutionName))
                        req.SolutionUniqueName = uniqueSolutionName;

                    CreateOneToManyResponse resp = (CreateOneToManyResponse)util.Service.Execute(req);
                    return resp.AttributeId;
                }
                else if (attr.CrmDataType == CrmAttributeDataType.Customer)
                {
                    //customer
                    OneToManyRelationshipMetadata[] customerRelationship = attr.CreateOneToManyRelationshipCustomer(ent);
                    CreateCustomerRelationshipsRequest req = new CreateCustomerRelationshipsRequest
                    {
                        Lookup = (LookupAttributeMetadata)attrMeta,
                        OneToManyRelationships = customerRelationship,
                    };
                    CreateCustomerRelationshipsResponse resp = (CreateCustomerRelationshipsResponse)util.Service.Execute(req);
                    return resp.AttributeId;
                }
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Update an existing System/Custom Attribute, add it into a CRM solution (if provided)
        /// </summary>
        /// <param name="util"></param>
        /// <param name="ent"></param>
        /// <param name="attr"></param>
        /// <param name="uniqueSolutionName"></param>
        public static void UpdateCrmAttribute(WebServiceUtils util, AttributeMetadata crmAtr, CrmEntity ent, CrmAttribute attr, bool is1toNRelationship, CrmOptionCollection localOptionToUpdate, out bool updateMetadataCache, string uniqueSolutionName = null)
        {
            updateMetadataCache = false;
            if (attr.AttributeMetadataToUpdate != null)
            {
                if (!is1toNRelationship)
                {
                    //update an attribute without relationship
                    UpdateAttributeRequest req = new UpdateAttributeRequest
                    {
                        EntityName = ent.Name,
                        Attribute = attr.AttributeMetadataToUpdate,
                    };

                    //add to solution on update
                    if (!string.IsNullOrEmpty(uniqueSolutionName))
                        req.SolutionUniqueName = uniqueSolutionName;

                    UpdateAttributeResponse resp = (UpdateAttributeResponse)util.Service.Execute(req);

                    //update the local option set list if there are any changes
                    if (localOptionToUpdate != null && !attr.IsGlobalOptionSet && ((attr.CrmDataType == CrmAttributeDataType.OptionSet && crmAtr.AttributeType.Value == AttributeTypeCode.Picklist) || (attr.CrmDataType == CrmAttributeDataType.MultiOptionSet && crmAtr.AttributeType.Value == AttributeTypeCode.Virtual)))
                    {
                        UpdateLocalOptionSet(util, ent.Name, attr.Name, localOptionToUpdate);

                        //updating local option set will require cache to be updated
                        updateMetadataCache = true;
                    }
                }
                else
                {
                    //update an attribute with relationship
                    if (attr.CrmDataType == CrmAttributeDataType.Lookup)
                    {
                        //update the attribute first, then followed by the relationship
                        UpdateAttributeRequest req = new UpdateAttributeRequest
                        {
                            EntityName = ent.Name,
                            Attribute = attr.AttributeMetadataToUpdate,
                        };

                        //add to solution on update
                        if (!string.IsNullOrEmpty(uniqueSolutionName))
                            req.SolutionUniqueName = uniqueSolutionName;

                        UpdateAttributeResponse resp = (UpdateAttributeResponse)util.Service.Execute(req);

                        //update the relationship next if it is for a custom lookup
                        if (!attr.IsSystem)
                        {
                            UpdateRelationshipRequest reqRel = new UpdateRelationshipRequest()
                            {
                                Relationship = attr.CreateOneToManyRelationshipLookup(ent),
                            };

                            //add to solution on update
                            if (!string.IsNullOrEmpty(uniqueSolutionName))
                                reqRel.SolutionUniqueName = uniqueSolutionName;

                            UpdateRelationshipResponse respRel = (UpdateRelationshipResponse)util.Service.Execute(reqRel);
                        }

                        //updating lookup attribute set will require cache to be updated
                        updateMetadataCache = true;
                    }
                    else if (attr.CrmDataType == CrmAttributeDataType.Customer)
                    {
                        //update the attribute first, then followed by the relationship
                        UpdateAttributeRequest req = new UpdateAttributeRequest
                        {
                            EntityName = ent.Name,
                            Attribute = attr.AttributeMetadataToUpdate,
                        };

                        //add to solution on update
                        if (!string.IsNullOrEmpty(uniqueSolutionName))
                            req.SolutionUniqueName = uniqueSolutionName;

                        UpdateAttributeResponse resp = (UpdateAttributeResponse)util.Service.Execute(req);

                        //updating customer attribute set will require cache to be updated
                        updateMetadataCache = true;
                    }
                }
            }
        }
        #endregion Create/Update Attribute

        #region Create/Update Relationship
        /// <summary>
        /// Create a many-to-many relationship between two eligible entity
        /// </summary>
        /// <param name="util"></param>
        /// <param name="rel"></param>
        /// <param name="uniqueSolutionName"></param>
        /// <returns></returns>
        public static Guid CreateCrmManyToManyRelation(WebServiceUtils util, CrmManyToManyRelation rel, string uniqueSolutionName = null)
        {
            //check if the entities are eligible for N:N relationship
            if (CanBeInManyToManyRelation(util, rel.Entity1.Name) && CanBeInManyToManyRelation(util, rel.Entity2.Name))
            {
                CreateManyToManyRequest req = new CreateManyToManyRequest
                {
                    IntersectEntitySchemaName = rel.Name, //relationship name
                    ManyToManyRelationship = CreateManyToManyRelationshipMeta(rel)
                };

                //add to solution on creation
                if (!string.IsNullOrEmpty(uniqueSolutionName))
                    req.SolutionUniqueName = uniqueSolutionName;

                CreateManyToManyResponse resp = (CreateManyToManyResponse)util.Service.Execute(req);
                return resp.ManyToManyRelationshipId;
            }
            else
            {
                throw new FormatException(string.Format("Either the entity '{0}' or '{1}' cannot be created as a N:N relationship.", rel.Entity1.Name, rel.Entity2.Name));
            }
        }

        #region CreateManyToManyRelationship
        /// <summary>
        /// Create many-to-many relationship metadata
        /// </summary>
        /// <param name="rel"></param>
        /// <returns></returns>
        private static ManyToManyRelationshipMetadata CreateManyToManyRelationshipMeta(CrmManyToManyRelation rel)
        {
            ManyToManyRelationshipMetadata relationMeta = new ManyToManyRelationshipMetadata
            {
                SchemaName = rel.Name, //relationship name
                IsValidForAdvancedFind = rel.IsValidForAdvancedFind, //Searchable
                Entity1LogicalName = rel.Entity1.Name, //reference entity logical name
                Entity1AssociatedMenuConfiguration = new AssociatedMenuConfiguration
                {
                    Behavior = (AssociatedMenuBehavior)((int)rel.Entity1.CrmMenuBehavior), //associated menu behavior of the reference entity
                    Group = (AssociatedMenuGroup)((int)rel.Entity1.CrmMenuGroup), //associated menu group of the reference entity
                    Label = new Label(rel.Entity1.CustomLabel, rel.Entity1.LanguageCode), //custom label
                    Order = rel.Entity1.MenuOrder //display order
                },
                Entity2LogicalName = rel.Entity2.Name, //reference entity logical name
                Entity2AssociatedMenuConfiguration = new AssociatedMenuConfiguration
                {
                    Behavior = (AssociatedMenuBehavior)((int)rel.Entity2.CrmMenuBehavior), //associated menu behavior of the reference entity
                    Group = (AssociatedMenuGroup)((int)rel.Entity2.CrmMenuGroup), //associated menu group of the reference entity
                    Label = new Label(rel.Entity2.CustomLabel, rel.Entity2.LanguageCode), //custom label
                    Order = rel.Entity2.MenuOrder //display order
                }
            };
            return relationMeta;
        }
        #endregion CreateManyToManyRelationship
        #endregion Create/Update Relationship
    }
}