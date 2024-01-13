@echo off
cls
setlocal
call :set_variables
call :download_miniconda
call :install_miniconda
call :create_conda_environment
call :check_installation
endlocal
goto :eof

:set_variables
set BIN=%~dp0
set ROOT=%BIN%..\
set PROJ=NekoCatGame
set ENV_FILE=environment.yml
set ENV=%ROOT%%ENV_FILE%
set MINICONDA_URL=https://repo.anaconda.com/miniconda/Miniconda3-latest-Windows-x86_64.exe
set MINICONDA=miniconda
set MINICONDA_EXE=%MINICONDA%.exe
set MINICONDA_PATH=%ROOT%%MINICONDA%\
set CONDA=%ROOT%%MINICONDA%\Scripts\conda
set DOCTOR=%BIN%doctor.bat
goto :eof

:download_miniconda
echo Downloading miniconda...
echo.
call curl %MINICONDA_URL% -o %MINICONDA_EXE%
echo.
goto :eof

:install_miniconda
echo Installing miniconda...
start /wait "" %MINICONDA_EXE% /InstallationType=JustMe /RegisterPython=0 /S /Q /D=%MINICONDA_PATH%
del %MINICONDA_EXE%
echo.
goto :eof

:create_conda_environment
echo Creating the conda environment...
call %CONDA% env create -n %PROJ% -f %ENV%
goto :eof

:check_installation
echo Checking the installation...
call %DOCTOR%
goto :eof
