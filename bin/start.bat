rem todo
@echo off
cls

set ROOT=%~dp0
set BIN=%ROOT%bin\

echo '''start.bat'''
call %BIN%setup-offline.bat


exit /b -1