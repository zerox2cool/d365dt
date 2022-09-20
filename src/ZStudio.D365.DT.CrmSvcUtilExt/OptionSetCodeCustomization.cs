/*------------------------------------------------------------
 * Copyright © Zero.Studio 2022
 * created by:  winson
 * created on:  28 aug 2011
--------------------------------------------------------------*/
using System;
using System.CodeDom;
using System.Diagnostics;
using Microsoft.Crm.Services.Utility;

namespace ZD365DT.CrmSvcUtilExt
{
    public sealed class OptionSetCodeCustomization : ICustomizeCodeDomService
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

            // Iterate over all of the namespaces that were generated.
            for (var i = 0; i < codeUnit.Namespaces.Count; ++i)
            {
                var types = codeUnit.Namespaces[i].Types;
                Trace.TraceInformation("Number of types in Namespace {0}: {1}",
                    codeUnit.Namespaces[i].Name, types.Count);
                // Iterate over all of the types that were created in the namespace.
                for (var j = 0; j < types.Count;)
                {
                    // Remove the type if it is not an enum (all OptionSets are enums).
                    if (!types[j].IsEnum)
                    {
                        types.RemoveAt(j);
                    }
                    else
                    {
                        j += 1;
                    }
                }
            }
            Trace.TraceInformation("Exiting ICustomizeCodeDomService.CustomizeCodeDom");
        }
    }
}
