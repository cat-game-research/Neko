@echo off
rem This script will build the RagdollTrainer project into builds dir

set BIN_PATH=%~dp0
set ROOT_PATH=%BIN_PATH%..\
set BUILDS_TEXT=Builds
set BUILDS_PATH=%ROOT_PATH%%BUILDS_TEXT%\

set PROJECT_TEXT=NekoCatGame
set PROJECT_PATH=%ROOT_PATH%%PROJECT_TEXT%\
set LOGS_TEXT=Logs
set LOGS_PATH=%ROOT_PATH%%LOGS_TEXT%\
set LOG_FILE_TEXT=log-build.txt
set LOG_FILE_PATH=%LOGS_PATH%%LOG_FILE_TEXT%
set /p UNITY_VERSION=< %BIN_PATH%unity.version
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\%UNITY_VERSION%\Editor\Unity.exe"
set WINDOWS_64_TEXT=windows_64
set WINDOWS_64_PATH=%BUILDS_PATH%%WINDOWS_64_TEXT%\
set SERVER_WINDOWS_64_TEXT=server_windows_x64
set SERVER_WINDOWS_64_PATH=%BUILDS_PATH%%SERVER_WINDOWS_64_TEXT%\
set WIN64_TEXT=Win64
set BUILD_TARGET=%WIN64_TEXT%

cls

echo Using...    %UNITY_VERSION% (%UNITY_PATH%)

:build_project_windows_64
echo building... "%PROJECT_PATH%"...
%UNITY_PATH% -batchmode -projectPath %PROJECT_PATH% -buildWindows64Player %WINDOWS_64_PATH% -quit
echo saved!      "%WINDOWS_64_PATH%"

:build_project_server_windows_64
echo building... "%PROJECT_PATH%"...
%UNITY_PATH% -batchmode -projectPath %PROJECT_PATH% -buildTarget %BUILD_TARGET% -standaloneBuildSubtarget Server -buildOutput %SERVER_WINDOWS_64_PATH% -quit
echo saved!      "%SERVER_WINDOWS_64_PATH%"

:exit
exit /b 1