Project Info
This is the D365 CE Deployment Tool console application that is the core of the code used to deploy customization configured on XMLs

Build Events

Pre-Build
SET ILMERGE_VERSION=3.0.40
copy "%USERPROFILE%\.nuget\packages\ilmerge\%ILMERGE_VERSION%\tools\net452\*.*" "$(ProjectDir)Binaries"



Post-Build
echo Copy DLL and PDB to test project for debugging and testing
REM copy "$(TargetPath)" "$(SolutionDir)Test\D365DT.Test.Demo.Deployment\Build\Executables\"
REM copy "$(TargetDir)$(TargetName).pdb" "$(SolutionDir)Test\D365DT.Test.Demo.Deployment\Build\"

REM NugetPkg project
IF "$(ConfigurationName)"=="Release" echo Calling Post-Build script to copy output into NugetPkg project for Release config only to NugetPkg project
IF "$(ConfigurationName)"=="Release" copy "$(TargetPath)" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\ZD365DT\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)$(OutDir)*.dll" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\ZD365DT\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)..\sdkbin\*.dll" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\ZD365DT\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)Binaries\ILMerge.exe" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\ZD365DT\ILMerge\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)Binaries\System.Compiler.dll" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\ZD365DT\ILMerge\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)ScriptFiles\MergeDll*.bat" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\ZD365DT\ILMerge\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)..\sdkbin\CrmSvcUtil.*" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\ZD365DT\CrmSvcUtil\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)Binaries\ZD365DT.CrmSvcUtilExt.dll" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\ZD365DT\CrmSvcUtil\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)ScriptFiles\CrmSvcUtil*.bat" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\ZD365DT\CrmSvcUtil\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)..\sdkbin\SolutionPackager.exe" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\ZD365DT\SolutionPackager\"

IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)ConfigSample\*.*" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\ZD365DT\ConfigSample\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)ConfigSchema\*.*" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\ZD365DT\ConfigSchema\"

IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)ReleaseNote.txt" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)SetNupkgVersion.ps1" "$(SolutionDir)..\pkg\ZStudio.D365.DTPkg\"