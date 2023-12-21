using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZStudio.D365.Shared.Framework.Core.Query;

namespace ZStudio.D365.DeploymentHelper.Core.Util
{
    public static class SolutionHelper
    {
        #region ComponentType
        public enum ComponentType
        {
            CloudFlow = 29,
            SLA = 152,
            AutoRecordRules = 154,
        }
        #endregion ComponentType

        public static Guid? GetSolutionIdByName(IOrganizationService svc, string solutionName)
        {
            FetchHelper fetch = new FetchHelper(svc);
            XrmQueryExpression query = new XrmQueryExpression("solution")
                .Condition("uniquename", ConditionOperator.Equal, solutionName);

            Entity[] coll = fetch.RetrieveEntityByQuery(query.ToQueryExpression());
            if (coll?.Length > 0)
                return coll[0].Id;
            return null;
        }

        public static Entity[] GetSolutionComponents(IOrganizationService svc, Guid solutionId)
        {
            FetchHelper fetch = new FetchHelper(svc);
            XrmQueryExpression query = new XrmQueryExpression("solutioncomponent")
                .Columns("solutioncomponentid", "solutionid", "componenttype", "objectid")
                .Condition("solutionid", ConditionOperator.Equal, solutionId);

            Entity[] coll = fetch.RetrieveEntityByQuery(query.ToQueryExpression());
            if (coll?.Length > 0)
                return coll;
            return null;
        }

        public static Entity[] GetSolutionComponentsByComponentType(IOrganizationService svc, Guid solutionId, int componentType)
        {
            FetchHelper fetch = new FetchHelper(svc);
            XrmQueryExpression query = new XrmQueryExpression("solutioncomponent")
                .Columns("solutioncomponentid", "solutionid", "componenttype", "objectid")
                .Condition("componenttype", ConditionOperator.Equal, componentType)
                .Condition("solutionid", ConditionOperator.Equal, solutionId);

            Entity[] coll = fetch.RetrieveEntityByQuery(query.ToQueryExpression());
            if (coll?.Length > 0)
                return coll;
            return null;
        }
    }
}