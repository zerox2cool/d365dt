using System;

namespace ZD365DT.DeploymentTool.Utils
{
    public class ConfigXml
    {
        public const string ATTR_NAME = "name";
        public const string ATTR_VALUE = "value";

        public class Namespace
        {
            public const string NS_PREFIX = "a";

            //register plugin XML namespace
            public const string PLUGIN_XMLNS = "http://schemas.zerostudioinc.com/crmdeployment/2016/pluginregister";
            //theme XML namespace
            public const string THEME_XMLNS = "http://schemas.zerostudioinc.com/crmdeployment/2016/themes";
            //publisher collection XML namespace
            public const string PUBLISHER_XMLNS = "http://schemas.zerostudioinc.com/crmdeployment/2016/publishers";
            //solution collection XML namespace
            public const string SOLUTION_XMLNS = "http://schemas.zerostudioinc.com/crmdeployment/2016/solutions";
            //global optionset definition XML namespace
            public const string GLOBALOP_XMLNS = "http://schemas.zerostudioinc.com/crmdeployment/2016/globaloptionsets";
            //entity definition XML namespace
            public const string ENTITIES_XMLNS = "http://schemas.zerostudioinc.com/crmdeployment/2016/entities";
            //n to n relation definition XML namespace
            public const string NTONRELATIONS_XMLNS = "http://schemas.zerostudioinc.com/crmdeployment/2016/manytomanyrelations";
            //customization collection XML namespace
            public const string CUSTOMIZATION_XMLNS = "http://schemas.zerostudioinc.com/crmdeployment/2016/customizations";
            //metadata collection XML namespace
            public const string METADATA_XMLNS = "http://schemas.zerostudioinc.com/crmdeployment/2016/metadatas";
            //solution collection XML namespace
            public const string EXECCONTEXT_XMLNS = "http://schemas.zerostudioinc.com/crmdeployment/2016/executioncontexts";
        }

        public class Theme
        {
            public const string NODE_NAME = "theme";

            public class Element
            {
                //XML element names
                public const string NAME = "name";
                public const string LOGO = "logo";
                public const string LOGOTOOLTIP = "logoTooltip";
                public const string NAVBARCOLOR = "navigationBarColor";
                public const string NAVBARSHELFCOLOR = "navigationBarShelfColor";
                public const string HEADERCOLOR = "headerColor";
                public const string GLOBALLINKCOLOR = "globalLinkColor";
                public const string SELECTEDLINKEFFECT = "selectedLinkEffect";
                public const string HOVERLINKEFFECT = "hoverLinkEffect";
                public const string PROCESSCONTROLCOLOR = "processControlColor";
                public const string DEFAULTENTITYCOLOR = "defaultEntityColor";
                public const string DEFAULTCUSTOMENTITYCOLOR = "defaultCustomEntityColor";
                public const string CONTROLSHADE = "controlShade";
                public const string CONTROLBORDER = "controlBorder";

                //new on D365-2018 (v9)
                public const string MAINCOLOR = "mainColor";
                public const string ACCENTCOLOR = "accentColor";
                public const string PAGEHEADERFILLCOLOR = "pageHeaderFillColor";
                public const string PANELHEADERFILLCOLOR = "panelHeaderFillColor";
            }
        }

        public class DefaultPublisher
        {
            public const string NODE_NAME = "publisher";

            public class Element
            {
                //XML element names
                public const string DISPLAYNAME = "displayname";
                public const string PREFIX = "prefix";
                public const string OPTONVALUEPREFIX = "optionvalueprefix";
                public const string DESCRIPTION = "description";
                public const string EMAIL = "email";
                public const string PHONE = "phone";
                public const string WEBSITE = "website";
                public const string ADDRESS_LINE1 = "addressline1";
                public const string ADDRESS_LINE2 = "addressline2";
                public const string ADDRESS_CITY = "addresscity";
                public const string ADDRESS_STATE = "addressstate";
                public const string ADDRESS_POSTALCODE = "addresspostalcode";
                public const string ADDRESS_COUNTRY = "addresscountry";
            }
        }

        public class Publisher
        {
            public const string NODE_NAME = "publisher";

            public class Element
            {
                //XML element names
                public const string NAME = "name";
                public const string DISPLAYNAME = "displayname";
                public const string PREFIX = "prefix";
                public const string OPTONVALUEPREFIX = "optionvalueprefix";
                public const string DESCRIPTION = "description";
                public const string EMAIL = "email";
                public const string PHONE = "phone";
                public const string WEBSITE = "website";
                public const string ADDRESS_LINE1 = "addressline1";
                public const string ADDRESS_LINE2 = "addressline2";
                public const string ADDRESS_CITY = "addresscity";
                public const string ADDRESS_STATE = "addressstate";
                public const string ADDRESS_POSTALCODE = "addresspostalcode";
                public const string ADDRESS_COUNTRY = "addresscountry";
            }
        }

        public class ExecutionContext
        {
            public const string NODE_NAME = "add";
        }

        public class Solution
        {
            public const string NODE_NAME = "solution";

            public class Element
            {
                //XML element names
                public const string NAME = "name";
                public const string DISPLAYNAME = "displayname";
                public const string PUBLISHERNAME = "publisheruniquename";
                public const string DESCRIPTION = "description";
                public const string VERSION = "version";
            }
        }

        public class Metadata
        {
            public const string NODE_NAME = "metadata";

            public class Element
            {
                //XML element names
                public const string METADATAFILE = "metadatafile";
                public const string CUSTOMIZATIONPREFIX = "customizationprefix";
                public const string SOLUTIONNAME = "solutionname";
                public const string PUBLISH = "publish";
                public const string LANGUAGECODE = "languagecode";
            }
        }

        public class Customization
        {
            public const string NODE_NAME = "customization";

            public class Element
            {
                //XML element names
                public const string FILE = "file";
                public const string SOLUTIONNAME = "solutionName";
                public const string ACTION = "action";
                public const string DEACTIVATEWORKFLOW = "deactivateworkflow";
                public const string PUBLISHBEFOREEXPORT = "publishBeforeExport";
                public const string BACKUPBEFOREIMPORT = "backupBeforeImport";
                public const string WAITTIMEOUT = "waitTimeout";
                public const string EXPORTRETRYTIMEOUT = "exportRetryTimeout";
                public const string ISMANAGED = "ismanaged";
            }
        }

        public class GlobalOp
        {
            public const string NODE_NAME = "globaloptionset";

            public class Attribute
            {
                //XML attribute names
                public const string NAME = "name";
                public const string DISPLAYNAME = "displayname";
                public const string ISSYSTEM = "issystem";
            }

            public class Element
            {
                //XML element names
                public const string DESCRIPTION = "description";
                public const string OPTIONS = "options";
            }
        }

        /// <summary>
        /// Options XML Node of the GlobalOptionset XML
        /// </summary>
        public class Options
        {
            public const string NODE_NAME = "option";

            public class Attribute
            {
                //XML attribute names
                public const string VALUE = "value";
                public const string LABEL = "label";
                public const string DESCRIPTION = "description";
                public const string COLOR = "color";
                public const string ORDER = "order";
            }
        }

        public class Entity
        {
            public const string NODE_NAME = "entity";

            public class Attribute
            {
                //XML attribute names
                public const string NAME = "name";
                public const string DISPLAYNAME = "displayname";
                public const string DISPLAYPLURALNAME = "displaypluralname";
                public const string ISSYSTEM = "issystem";

                public const string OWNERSHIP = "ownership";

                public const string ISACTIVITYENTITY = "isactivityentity";
                public const string DISPLAYINACTIVITYMENU = "displayinactivitymenu";

                public const string COLOR = "color";

                public const string ISBUSINESSPROCESSENABLED = "businessprocess";

                public const string ISNOTESENABLED = "notes";
                public const string ISCONNECTIONSENABLED = "connections";
                public const string ISACTIVITIESENABLED = "activities";

                public const string ISACTIVITYPARTYENABLED = "activityparty";
                public const string ISMAILMERGEENABLED = "mailmerge";
                public const string ISDOCUMENTMANAGEMENTENABLED = "documentmanagement";
                public const string ISACCESSTEAMSENABLED = "accessteams";
                public const string ISQUEUESENABLED = "queues";
                public const string AUTOROUTETOOWNERQUEUE = "autoroutetoownerqueue";
                public const string ISKNOWLEDGEMANAGEMENTENABLED = "knowledgemanagement";

                public const string ISQUICKCREATEENABLED = "allowquickcreate";
                public const string ISDUPLICATEDETECTIONENABLED = "duplicatedetection";
                public const string ISAUDITENABLED = "auditing";
                public const string ISCHANGETRACKINGENABLED = "changetracking";

                public const string ISVISIBLEINPHONEEXPRESS = "phoneexpress";
                public const string ISVISIBLEINMOBILECLIENT = "mobile";
                public const string READONLYINMOBILECLIENT = "readonlymobile";
                public const string ISAVAILABLEOFFLINE = "availableoffline";

                public const string ISENTITYHELPURLENABLED = "usecustomhelp";
                public const string ENTITYHELPURL = "entityhelpurl";

                public const string ISDONOTINCLUDESUBCOMPONENTS = "donotincludesubcomponents";

                public const string SMALLICON = "smallicon";
                public const string MEDIUMICON = "mediumicon";
                public const string VECTORICON = "vectoricon";
            }

            public class Element
            {
                //XML element names
                public const string DESCRIPTION = "description";
                public const string PRIMARYFIELD = "primaryfield";
                public const string ATTRIBUTES = "attributes";
            }
        }

        /// <summary>
        /// Primary Field XML Node of the Entity XML
        /// </summary>
        public class PrimaryField
        {
            public const string NODE_NAME = "primaryfield";

            public class Attribute
            {
                //XML attribute names
                public const string NAME = "name";
                public const string DISPLAYNAME = "displayname";
                public const string DESCRIPTION = "description";
                public const string LENGTH = "length";
                public const string FIELDREQUIREMENT = "fieldrequirement";
                public const string ISVALIDFORADVANCEDFIND = "searchable";
                public const string ISAUDITENABLED = "auditing";
            }
        }

        /// <summary>
        /// Attributes XML Node of the Entity XML
        /// </summary>
        public class Attrs
        {
            public const string NODE_NAME = "attribute";

            public class Attribute
            {
                //XML attribute names
                public const string NAME = "name";
                public const string DISPLAYNAME = "displayname";
                public const string ISSYSTEM = "issystem";
                public const string FIELDREQUIREMENT = "fieldrequirement";

                public const string ISVALIDFORADVANCEDFIND = "searchable";
                public const string ISAUDITENABLED = "auditing";
                public const string ISSECURED = "fieldsecurity";

                public const string DATATYPE = "datatype";
                public const string FIELDTYPE = "fieldtype";
                public const string FORMATTYPE = "formattype";

                //length and min/max values
                public const string LENGTH = "length";
                public const string MINVALUE = "minvalue";
                public const string MAXVALUE = "maxvalue";

                //currency
                public const string PRECISIONSOURCE = "precisionsource";
                public const string PRECISION = "precision";

                //two options
                public const string TWOOPTIONSDEFAULT = "twooptionsdefault";
                public const string TRUELABEL = "truelabel";
                public const string TRUECOLOR = "truecolor";
                public const string FALSELABEL = "falselabel";
                public const string FALSECOLOR = "falsecolor";

                //optionset
                public const string OPTIONSETDEFAULT = "optionsetdefault";
                public const string GLOBALOPTIONSETNAME = "globaloptionsetname";
                public const string ISSYSTEMGLOBALOPTIONSET = "issystemglobaloptionset";

                //datetime
                public const string DATETIMEBEHAVIOR = "datetimebehavior";

                //lookup
                public const string LOOKUPREFERENCEENTITYNAME = "lookupreferenceentityname";
                public const string ISSYSTEMREFERENCEENTITY = "issystemreferenceentity";
                public const string LOOKUPRELATIONSHIPNAME = "lookuprelationshipname";
                public const string LOOKUPMENUORDER = "lookupmenuorder";
                public const string LOOKUPMENUBEHAVIOR = "lookupmenubehavior";
                public const string LOOKUPMENUCUSTOMLABEL = "lookupmenucustomlabel";
                public const string LOOKUPMENUGROUP = "lookupmenugroup";
                public const string LOOKUPRELATIONSHIPBEHAVIORASSIGN = "lookuprelationshipbehaviorassign";
                public const string LOOKUPRELATIONSHIPBEHAVIORSHARE = "lookuprelationshipbehaviorshare";
                public const string LOOKUPRELATIONSHIPBEHAVIORUNSHARE = "lookuprelationshipbehaviorunshare";
                public const string LOOKUPRELATIONSHIPBEHAVIORREPARENT = "lookuprelationshipbehaviorreparent";
                public const string LOOKUPRELATIONSHIPBEHAVIORDELETE = "lookuprelationshipbehaviordelete";
                public const string LOOKUPRELATIONSHIPBEHAVIORROLLUPVIEW = "lookuprelationshipbehaviorrollupview";

                public const string IMEMODE = "imemode";
            }

            public class Element
            {
                //XML element names
                public const string DESCRIPTION = "description";
                public const string OPTIONS = "options";
            }
        }

        public class NtoNRelation
        {
            public const string NODE_NAME = "manytomanyrelation";

            public class Attribute
            {
                //XML attribute names
                public const string NAME = "name";
                public const string ISSYSTEM = "issystem";
                public const string ISVALIDFORADVANCEDFIND = "searchable";
            }

            public class Element
            {
                //XML element names
                public const string ENTITY1 = "entity1";
                public const string ENTITY2 = "entity2";
            }
        }

        public class RelationEntity
        {
            public const string NODE1_NAME = "entity1";
            public const string NODE2_NAME = "entity2";

            public class Attribute
            {
                //XML attribute names
                public const string NAME = "name";
                public const string ISSYSTEM = "issystem";
                public const string MENUORDER = "menuorder";
                public const string MENUBEHAVIOR = "menubehavior";
                public const string MENUGROUP = "menugroup";
                public const string CUSTOMLABEL = "customlabel";
            }
        }
    }
}