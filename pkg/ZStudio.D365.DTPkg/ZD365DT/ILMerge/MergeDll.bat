REM START
@echo on
REM Pre-Build action to copy the required files from the project's output to build folder
set ConfigurationName=%~1%
set SolutionDir=%~2%
set ProjectDir=%~3%
set TargetDir=%~4%
set TargetFilePath=%~5%
set TargetFileName=%~6%
set TargetName=%~7%
set SolutionName=%~8%

set InputFiles=%TargetFileName% D365DT.Test.Demo.XrmEntity.dll
set ComponentsDir=src\ZD365DT.DeploymentTool\Binaries\
set TargetPlatform=v4
set KeyFile=%SolutionDir%Test\D365DT.Test.Demo.Assembly\D365DT.Test.Demo.Assembly.snk

echo Configuration: %ConfigurationName%
echo Solution Directory: %SolutionDir%
echo Project Directory: %ProjectDir%
echo Target Directory: %TargetDir%
echo Target File: %TargetFilePath%
echo Target Filename: %TargetFileName%
echo Target Name: %TargetName%
echo Solution Name: %SolutionName%
echo Input File: %InputFiles%
echo Key File: %KeyFile%
echo ILMerge Directory: %SolutionDir%%ComponentsDir%

echo Copy ILMerge into Output Directory
copy "%SolutionDir%%ComponentsDir%ILMerge.exe" "%TargetDir%ILMerge.exe"
copy "%SolutionDir%%ComponentsDir%System.Compiler.dll" "%TargetDir%System.Compiler.dll"

echo Delete merge.log if exist
cd "%TargetDir%"
IF exist "..\merge.log" del ..\merge.log

echo Call ILMerge.exe
ILMerge.exe %InputFiles% /out:..\%TargetFileName% /targetplatform:%TargetPlatform% /keyfile:%KeyFile% /wildcards:true /log:..\merge.log

echo Copy the Generated file to overwrite the build file
copy "..\%TargetFileName%" %TargetFilePath% /y
copy "..\%TargetName%.pdb" "%TargetDir%%TargetName%.pdb" /y

echo Copy the Generated file to CRM assembly folder (for on-premise debugging) if it exists
IF exist "C:\Program Files\Microsoft Dynamics CRM\Server\bin\assembly\" copy /y %TargetFilePath% "C:\Program Files\Microsoft Dynamics CRM\Server\bin\assembly\"
IF exist "C:\Program Files\Microsoft Dynamics CRM\Server\bin\assembly\" copy /y "%TargetDir%%TargetName%.pdb" "C:\Program Files\Microsoft Dynamics CRM\Server\bin\assembly\"

echo Copy the Generated file to web resource folder (for deployment) if it exists
IF exist "%SolutionDir%Test\D365DT.Test.Demo.WebResources\dlls\" copy /y %TargetFilePath% "%SolutionDir%Test\D365DT.Test.Demo.WebResources\dlls\"

echo Delete the ILMerge from the Output Directory
del "%TargetDir%ILMerge.exe" /f /q
del "%TargetDir%System.Compiler.dll" /f /q

echo Delete the generated files
del "..\%TargetName%.*" /f /q