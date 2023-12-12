REM START TestConnection
echo off

echo Source CRM Connection String (can be with password/client secret or login prompted - NOTE: password is visible):
set /p connectionString=

echo Helper to Run (must specify the helper program to run):
set helper=TestConnection

REM echo Config File, serialised Dictionary string,object JSON (optional, pass in NULL when no value):
REM set config=

REM echo Token Key, delimited by double-semicolon (;;) (optional, pass in NULL when no value):
REM set key=

REM echo Token Data, delimited by double-semicolon (;;) (optional, pass in NULL when no value):
REM set data=

set devmode=0
if exist "bin/Debug" set devmode=1

set debug=false
if %devmode%==1 cd bin/Debug/
if %devmode%==1 set debug=true

ZStudio.D365.DeploymentHelper.exe /connectionString:"%connectionString%" /helper:"%helper%" /debug:%debug%
set status=%errorlevel%


if %status% gtr 0 goto :failed
goto :end


:failed
echo CRM DEPLOYMENT HELPER FAILED
goto :end


:end
echo SCRIPT END...
if %devmode%==1 cd..
if %devmode%==1 cd..
REM if %devmode%==1 pause