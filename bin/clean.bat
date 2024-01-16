@echo off
setlocal

set BIN=%~dp0
set ROOT=%BIN%..\

set PROJ=NekoCatGame
set CLEAN=Cleaning...
set MINI=miniconda
set LOGS=Logs
set ML=ml-agents
set DIST=dist

call :main
endlocal
exit /b 1

:main
echo %CLEAN%
call :remove %ROOT%%DIST%
call :remove %ROOT%%MINI%
call :remove %ROOT%%ML%
call :remove %ROOT%%PROJ%\%LOGS%
goto :eof

:remove
for %%f in (%1*.exe) do (
  del /q %%f
)
for /d %%d in (%1*) do (
  rd /s /q %%d
)
goto :eof
