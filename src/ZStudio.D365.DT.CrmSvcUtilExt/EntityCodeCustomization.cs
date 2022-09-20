/*------------------------------------------------------------
 * Copyright © Zero.Studio 2022
 * created by:  winson
 * created on:  10 mar 2015
--------------------------------------------------------------*/
using System;
using System.CodeDom;
using System.Diagnostics;
using Microsoft.Crm.Services.Utility;

namespace ZD365DT.CrmSvcUtilExt
{
    public sealed class EntityCodeCustomization : ICustomizeCodeDomService
    {
        /// <summary>
        /// Remove the unnecessary classes that we generated for entities. 
        /// </summary>
        public void CustomizeCodeDom(CodeCompileUnit codeUnit, IServiceProvider services)
        {
            Trace.TraceInformation("Entering ICustomizeCodeDomService.CustomizeCodeDom");
            Trace.TraceInformation(
                "Number of Namespaces generated: {0}", codeUnit.Namespaces.Count);

            //#if REMOVE_PROXY_TYPE_ASSEMBLY_ATTRIBUTE

            foreach (CodeAttributeDeclaration attribute in codeUnit.AssemblyCustomAttributes)
            {
                Trace.TraceInformation("Attribute BaseType is {0}",
                    attribute.AttributeType.BaseType);
                if (attribute.AttributeType.BaseType ==
                    "Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute")
                {
                    codeUnit.AssemblyCustomAttributes.Remove(attribute);
                    break;
                }
            }

            //#endif

            Trace.TraceInformation("Exiting ICustomizeCodeDomService.CustomizeCodeDom");
        }
    }
}