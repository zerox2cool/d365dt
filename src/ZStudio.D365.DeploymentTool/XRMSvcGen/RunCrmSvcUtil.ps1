## Copy files into folder for crmsvcutil to execute
Write-Host "Copy files into folder for crmsvcutil to execute"
copy ..\..\..\src\sdkbin\CrmSvcUtil.exe
## copy ..\..\..\src\sdkbin\CrmSvcUtil.exe.config
copy ..\..\..\src\sdkbin\Microsoft.*.dll
copy ..\..\..\src\ZStudio.D365.DeploymentTool\Binaries\*CrmSvcUtilExt.dll

## You will be able to run crmsvcutil on normal laptop with a App Registration User setup on D365 non-prod instances
## Use the client secret on the connection string of the crmsvcutil.exe.config
Write-Host "ALL CODE IS GENERATED WITHOUT [assembly: Microsoft.Xrm.Sdk.Client.ProxyTypesAssemblyAttribute()], this needs to be in the AssemblyInfo.cs for each project that uses the generated code"

## Generate Entities
.\crmsvcutil.exe /out:"../XRM/Xrm.cs" /serviceContextName:"ServiceContext"

## Generate OptionSets
.\crmsvcutil.exe /out:"../XRM/OptionSets.cs" /codewriterfilter:"ZD365DT.CrmSvcUtilExt.OptionSetCodeWriterFilter,ZD365DT.CrmSvcUtilExt" /codecustomization:"ZD365DT.CrmSvcUtilExt.OptionSetCodeCustomization,ZD365DT.CrmSvcUtilExt"

## Remove all the files
Write-Host "Remove all the files"
del CrmSvcUtil.exe
## del CrmSvcUtil.exe.config
del Microsoft.*.dll
del *CrmSvcUtilExt.dll

Write-Host "Script Completed..."
pause