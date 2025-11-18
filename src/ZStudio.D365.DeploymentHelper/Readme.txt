Project Info
This is the D365 CE Deployment Helper console application, new v2 to eventually replace the Legacy Deployment Tool.

Build Events

Pre-Build



Post-Build
echo Copy DLL and PDB to test project for debugging and testing
REM copy "$(TargetPath)" "$(SolutionDir)Test\D365DT.Test.Demo.Deployment\Build\Executables\"
REM copy "$(TargetDir)$(TargetName).pdb" "$(SolutionDir)Test\D365DT.Test.Demo.Deployment\Build\"

REM NugetPkg project
IF "$(ConfigurationName)"=="Release" echo Calling Post-Build script to copy output into NugetPkg project for Release config only to NugetPkg project
IF "$(ConfigurationName)"=="Release" copy "$(TargetPath)" "$(SolutionDir)..\pkg\ZStudio.D365.DTHelperPkg\ZD365DTHelper\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)$(OutDir)*.dll" "$(SolutionDir)..\pkg\ZStudio.D365.DTHelperPkg\ZD365DTHelper\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)$(OutDir)*.config" "$(SolutionDir)..\pkg\ZStudio.D365.DTHelperPkg\ZD365DTHelper\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)$(OutDir)*.bat" "$(SolutionDir)..\pkg\ZStudio.D365.DTHelperPkg\ZD365DTHelper\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)$(OutDir)SampleConfig\*.json" "$(SolutionDir)..\pkg\ZStudio.D365.DTHelperPkg\ZD365DTHelper\SampleConfig\"

IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)ReadmeGuide.md" "$(SolutionDir)..\pkg\ZStudio.D365.DTHelperPkg\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)ReleaseNote.md" "$(SolutionDir)..\pkg\ZStudio.D365.DTHelperPkg\"
IF "$(ConfigurationName)"=="Release" copy "$(ProjectDir)SetNupkgVersion.ps1" "$(SolutionDir)..\pkg\ZStudio.D365.DTHelperPkg\"