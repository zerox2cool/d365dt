ECHO OFF
REM SAMPLE BAT to generate early-bound entities using the ZD365DT.CrmSvcUtilExt

call CopyCrmSvcUtilFiles.bat

ECHO ALL CODE IS GENERATED WITHOUT [assembly: Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute()], this needs to be in the AssemblyInfo.cs for each project that uses the generated code

REM generate for framework core classes, optionsets will also be generated here

REM generate for entity
crmsvcutil.exe /out:"../XRM/Xrm.cs" /serviceContextName:"ServiceContext"

REM generate for optionset
crmsvcutil.exe /out:"../XRM/OptionSets.cs" /codewriterfilter:"ZD365DT.CrmSvcUtilExt.OptionSetCodeWriterFilter,ZD365DT.CrmSvcUtilExt" /codecustomization:"ZD365DT.CrmSvcUtilExt.OptionSetCodeCustomization,ZD365DT.CrmSvcUtilExt"

call DelCrmSvcUtilFiles.bat

pause