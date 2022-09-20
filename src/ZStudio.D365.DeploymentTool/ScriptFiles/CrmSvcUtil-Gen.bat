ECHO OFF
REM SAMPLE BAT to generate early-bound entities using the ZD365DT.CrmSvcUtilExt, you can use your own extension

call ../CrmSvcUtilFiles-Copy.bat

ECHO ALL CODE IS GENERATED WITHOUT [assembly: Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute()], this needs to be in the AssemblyInfo.cs for each project that uses the generated code

REM generate for framework core classes, optionsets will also be generated here

REM generate for entity
crmsvcutil.exe /namespace:"Demo.Crm.Common.XrmEntities.Entities" /out:"../../XrmEntities/XrmEntities.cs" /serviceContextName:"OrgSvcContext"

REM generate for optionset
crmsvcutil.exe /namespace:"Demo.Crm.Common.XrmEntities.OptionSets" /out:"../../XrmEntities/XrmOptionSets.cs" /codewriterfilter:"ZD365DT.CrmSvcUtilExt.OptionSetCodeWriterFilter,ZD365DT.CrmSvcUtilExt" /codecustomization:"ZD365DT.CrmSvcUtilExt.OptionSetCodeCustomization,ZD365DT.CrmSvcUtilExt"

call ../CrmSvcUtilFiles-Del.bat

pause