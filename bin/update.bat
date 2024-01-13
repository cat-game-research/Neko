@echo off
setlocal

set BIN_PATH=%~dp0
set ROOT_PATH=%BIN_PATH%..\

set PROJECT_TEXT=NekoCatGame
set PROJECT_PATH=%ROOT_PATH%%PROJECT_TEXT%\
set MINICONDA_TEXT=miniconda
set MINICONDA_PATH=%ROOT_PATH%%MINICONDA_TEXT%\
set ENVIRONMENT=%ROOT_PATH%environment.yml
set CONDA=%MINICONDA_PATH%\Scripts\conda

call :main

endlocal
exit /b 1

:main
echo Updating...
call %CONDA% env update -n %PROJECT_TEXT% -f %ENVIRONMENT% --prune --quiet
goto :eof
