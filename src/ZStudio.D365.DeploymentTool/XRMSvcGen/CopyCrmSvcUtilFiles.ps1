## Copy files into folder for crmsvcutil to execute
Write-Host "Copy files into folder for crmsvcutil to execute"
copy ..\..\..\src\sdkbin\CrmSvcUtil.exe
## copy ..\..\..\src\sdkbin\CrmSvcUtil.exe.config
copy ..\..\..\src\sdkbin\Microsoft.*.dll
copy ..\..\..\src\ZStudio.D365.DeploymentTool\Binaries\*CrmSvcUtilExt.dll
