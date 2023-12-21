REM START DeleteTablePermission
echo off

echo Source CRM Connection String (can be with password/client secret or login prompted - NOTE: password is visible):
set /p connectionString=

echo Helper to Run (must specify the helper program to run):
set helper=DeleteTablePermission

echo Config File, serialised Dictionary string,object JSON (optional, pass in NULL when no value):
set config=SampleConfig\DeleteTablePermission.json

echo Token Key, delimited by double-semicolon (;;) (optional, pass in NULL when no value):
set key=websiteid

echo Token Data, delimited by double-semicolon (;;) (optional, pass in NULL when no value):
set data=b5dc8e6b-6625-ee11-9965-000d3a6ad5b3

set devmode=0
if exist "bin/Debug" set devmode=1

set debug=false
if %devmode%==1 cd bin/Debug/
if %devmode%==1 set debug=true

ZStudio.D365.DeploymentHelper.exe /connectionString:"%connectionString%" /helper:"%helper%" /config:"%config%" /key:"%key%" /data:"%data%" /debug:%debug% /sleep:0
set status=%errorlevel%


if %status% gtr 0 goto :failed
goto :end


:failed
echo CRM DEPLOYMENT HELPER FAILED
REM THROW ERROR by CAUSING ERROR
Error.exe
goto :end


:end
echo SCRIPT END...
if %devmode%==1 cd..
if %devmode%==1 cd..
REM if %devmode%==1 pause