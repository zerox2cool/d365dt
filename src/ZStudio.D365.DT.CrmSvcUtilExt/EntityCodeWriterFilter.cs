/*------------------------------------------------------------
 * Copyright © Zero.Studio 2022
 * created by:  winson
 * created on:  28 aug 2011
--------------------------------------------------------------*/
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk.Metadata;

namespace ZD365DT.CrmSvcUtilExt
{
    public sealed class EntityCodeWriterFilter : ICodeWriterFilterService
    {
        //list of entity names to generate classes
        private HashSet<string> _validEntities = new HashSet<string>();

        //reference to the default service
        private ICodeWriterFilterService _defaultService = null;

        public EntityCodeWriterFilter(ICodeWriterFilterService defaultService)
        {
            this._defaultService = defaultService;

            //load all entity to generate into hashset
            LoadFilterData();
        }

        /// <summary>
        /// loads the entity to generate data from the the XML file
        /// </summary>
        private void LoadFilterData()
        {
            try
            {
                //search for XML file with the name ending with XrmEntityList.xml
                string[] xmlfiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*XrmEntityList.xml", SearchOption.TopDirectoryOnly);
                if (xmlfiles != null && xmlfiles.Length > 0)
                {
                    XElement xml = XElement.Load(xmlfiles[0]);
                    XElement entitiesElement = xml.Element("entities");
                    foreach (XElement entityElement in entitiesElement.Elements("entity"))
                        _validEntities.Add(entityElement.Value.ToLowerInvariant());
                }
                else
                    _validEntities = new HashSet<string>();
            }
            catch
            {
                _validEntities = new HashSet<string>();
            }
        }

        /// <summary>
        /// Use filter entity list to determine if the entity class should be generated.
        /// </summary>
        public bool GenerateEntity(EntityMetadata entityMetadata, IServiceProvider services)
        {
            //if there is a filter list then check it
            if (_validEntities.Count > 0)
                return (_validEntities.Contains(entityMetadata.LogicalName.ToLowerInvariant()));
            else
                return _defaultService.GenerateEntity(entityMetadata, services);
        }

        //all other methods just use default implementation:
        public bool GenerateAttribute(AttributeMetadata attributeMetadata, IServiceProvider services)
        {
            return _defaultService.GenerateAttribute(attributeMetadata, services);
        }

        public bool GenerateOption(OptionMetadata optionMetadata, IServiceProvider services)
        {
            return _defaultService.GenerateOption(optionMetadata, services);
        }

        public bool GenerateOptionSet(OptionSetMetadataBase optionSetMetadata, IServiceProvider services)
        {
            return _defaultService.GenerateOptionSet(optionSetMetadata, services);
        }

        public bool GenerateRelationship(RelationshipMetadataBase relationshipMetadata, EntityMetadata otherEntityMetadata, IServiceProvider services)
        {
            return _defaultService.GenerateRelationship(relationshipMetadata, otherEntityMetadata, services);
        }

        public bool GenerateServiceContext(IServiceProvider services)
        {
            return _defaultService.GenerateServiceContext(services);
        }
    }
}