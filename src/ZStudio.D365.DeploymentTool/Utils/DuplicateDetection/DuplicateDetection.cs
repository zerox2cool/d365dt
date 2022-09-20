using System;
using System.Xml.Serialization;
using Microsoft.Xrm.Sdk;


namespace ZD365DT.DeploymentTool.Utils.DuplicateDetection
{
    [Serializable]
    public enum MatchType
    {
        ExactMatch = 0,
        SameFirstCharacters = 1,
        SameLastCharacters = 2,
        SameDate = 3,
        SameDateAndTime = 4,
        ExactMatchPickListLabel = 5,
        ExactMatchPickListValue = 6
    }


    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "DuplicateDetectionRules")]
    public class DuplicateDetection
    {
        #region Member Variables

        private DuplicateDetectionRule[] duplicateDetectionRules;

        #endregion Member Variables

        #region Properties

        [XmlElement("DuplicateDetectionRule")]
        public DuplicateDetectionRule[] DuplicateDetectionRules
        {
            get { return this.duplicateDetectionRules; }
            set { this.duplicateDetectionRules = value; }
        }

        #endregion Properties
    }


    [Serializable]
    public class DuplicateDetectionRule
    {
        #region Member Variables

        private string baseEntity;
        private bool isCaseSensitive;
        private string matchingEntity;
        private string name;
        private string description;
        private DuplicateDetectionRulesCondition[] conditions;

        #endregion Member Variables

        #region Constructors

        public DuplicateDetectionRule()
        {
            // Default constructor for serialization
        }

        public DuplicateDetectionRule(DuplicateRule rule)
        {
            Name = rule.Name;
            Description = rule.Description;
            BaseEntity = rule.BaseEntityName;
            if (rule.IsCaseSensitive.HasValue)
            {
                IsCaseSensitive = rule.IsCaseSensitive.Value;
            }
            MatchingEntity = rule.MatchingEntityName;
        }

        #endregion Constructors

        #region Public Properties

        [XmlElement]
        public string BaseEntity
        {
            get { return this.baseEntity; }
            set { this.baseEntity = value; }
        }

        [XmlElement]
        public bool IsCaseSensitive
        {
            get { return this.isCaseSensitive; }
            set { this.isCaseSensitive = value; }
        }

        [XmlElement]
        public string MatchingEntity
        {
            get { return this.matchingEntity; }
            set { this.matchingEntity = value; }
        }

        [XmlElement]
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        [XmlElement]
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        [XmlElement("Condition")]
        public DuplicateDetectionRulesCondition[] Conditions
        {
            get { return this.conditions; }
            set { this.conditions = value; }
        }

        #endregion Public Properties

        #region Public Methods

        public DuplicateRule GetDuplicateRule()
        {
            DuplicateRule rule = new DuplicateRule();
            rule.Name = Name;
            rule.Description = Description;
            rule.BaseEntityName = BaseEntity;            
            rule.IsCaseSensitive = new bool?( IsCaseSensitive);
            rule.MatchingEntityName = MatchingEntity;

            return rule;
        }

        #endregion Public Methods
    }


    [Serializable]
    public class DuplicateDetectionRulesCondition
    {
        #region Member Variables

        private string baseAttribute;
        private string matchingAttribute;
        private MatchType matchType;
        private int noOfMatchCharacters;

        #endregion Member Variables

        #region Constructors

        public DuplicateDetectionRulesCondition()
        {
            // Default constructor for serialization
        }

        public DuplicateDetectionRulesCondition(DuplicateRuleCondition condition)
        {
            BaseAttribute = condition.BaseAttributeName;
            MatchingAttribute = condition.MatchingAttributeName;
            MatchType = (MatchType)condition.OperatorCode.Value;
            if (condition.OperatorParam != null)
            {
                NoOfMatchCharacters = condition.OperatorParam.Value;
            }
        }

        #endregion Constructors

        #region Public Properties

        [XmlElement]
        public string BaseAttribute
        {
            get { return this.baseAttribute; }
            set { this.baseAttribute = value; }
        }

        [XmlElement]
        public string MatchingAttribute
        {
            get { return this.matchingAttribute; }
            set { this.matchingAttribute = value; }
        }

        [XmlElement]
        public MatchType MatchType
        {
            get { return this.matchType; }
            set { this.matchType = value; }
        }

        [XmlElement]
        public int NoOfMatchCharacters
        {
            get { return this.noOfMatchCharacters; }
            set { this.noOfMatchCharacters = value; }
        }

        #endregion Public Properties

        #region Public Methods

        public DuplicateRuleCondition GetDuplicateRule()
        {
            DuplicateRuleCondition condition = new DuplicateRuleCondition();
            condition.BaseAttributeName = BaseAttribute;
            condition.MatchingAttributeName = MatchingAttribute;
            condition.OperatorCode = new OptionSetValue();
            condition.OperatorCode.Value = (int)MatchType;
            condition.OperatorParam = NoOfMatchCharacters;
            

            return condition;
        }

        #endregion Public Methods
    }
}