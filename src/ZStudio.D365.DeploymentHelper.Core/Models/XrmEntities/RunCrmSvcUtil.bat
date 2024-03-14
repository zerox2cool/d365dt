ECHO OFF
REM SAMPLE BAT to generate early-bound entities using the ZD365DT.CrmSvcUtilExt

REM call CopyCrmSvcUtilFiles.bat

ECHO ALL CODE IS GENERATED WITHOUT [assembly: Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute()], this needs to be in the AssemblyInfo.cs for each project that uses the generated code

REM generate for framework core classes, optionsets will also be generated here

cd crmsvcutil

REM generate for entity
crmsvcutil.exe /namespace:"ZStudio.D365.DeploymentHelper.Core.Models.Entities" /out:"../XrmEntities.cs" /serviceContextName:"OrgSvcContext"

REM generate for optionset
crmsvcutil.exe /namespace:"ZStudio.D365.DeploymentHelper.Core.Models.OptionSets" /out:"../XrmOptionSets.cs" /codewriterfilter:"ZD365DT.CrmSvcUtilExt.OptionSetCodeWriterFilter,ZD365DT.CrmSvcUtilExt" /codecustomization:"ZD365DT.CrmSvcUtilExt.OptionSetCodeCustomization,ZD365DT.CrmSvcUtilExt"

REM call DelCrmSvcUtilFiles.bat

pause