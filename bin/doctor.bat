@echo off
set BIN_PATH=%~dp0
set ROOT_PATH=%BIN_PATH%..\
set PROJECT_NAME=NekoCatGame
set MINICONDA_PATH=%ROOT_PATH%miniconda\
set CONDA_BIN_PATH=%MINICONDA_PATH%condabin\
set CONDA_CMD=%CONDA_BIN_PATH%conda
set SETUP_FILE=%BIN_PATH%setup.bat
set RUN_FILE=%BIN_PATH%run.py
set /a ERROR_COUNT=0
for %%S in (project miniconda conda run) do call :check_%%S_folder
goto :exit
:check_project_folder
if not exist %ROOT_PATH% (
    echo Project folder not found: %ROOT_PATH%
    set /a ERROR_COUNT+=1
)
goto :eof
:check_miniconda_folder
if not exist %MINICONDA_PATH% (
    echo Miniconda folder not found: %MINICONDA_PATH%
    set /a ERROR_COUNT+=1
)
goto :eof
:check_conda_folder
call %CONDA_CMD% info --envs | findstr /C:"%PROJECT_NAME%" >nul
if errorlevel 1 (
    echo Conda environment not activated: %PROJECT_NAME%
    set /a ERROR_COUNT+=1
)
goto :eof
:check_run_folder
if not exist %RUN_FILE% (
    echo Run.py file not found: %RUN_FILE%
    set /a ERROR_COUNT+=1
)
goto :eof
:exit
if not %ERROR_COUNT% gtr 0 (
    echo Everything is set up correctly =^^_^^=
	echo.
    goto :EOF
)
if %ERROR_COUNT% gtr 0 (
    echo There were %ERROR_COUNT% errors in the installation and setup of the previous batch file.
    echo.
    echo Please press any key to begin the healing process.
    echo.
    pause
    call %SETUP_FILE%
    echo.
)
exit /b 1
