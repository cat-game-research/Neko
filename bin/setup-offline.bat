@echo off
cls
setlocal
	call :set_variables
	call :install_miniconda
	call :unpack_nekocatgame
	call :check_install
endlocal
goto :eof

:set_variables
	set BIN=%~dp0
	set ROOT=%BIN%..\
	set DIST=%ROOT%dist\
	set PROJ=NekoCatGame
	set ENV_FILE=environment.yml
	set ENV=%ROOT%%ENV_FILE%
	set MINICONDA_URL=https://repo.anaconda.com/miniconda/Miniconda3-latest-Windows-x86_64.exe
	set MINICONDA=miniconda
	set MINICONDA_EXE=%MINICONDA%.exe
	set MINICONDA_PATH=%ROOT%%MINICONDA%\
	set CONDA=%ROOT%%MINICONDA%\Scripts\conda
	set UNPACK=%BIN%unpack.bat
	set DOCTOR=%BIN%doctor.bat
goto :eof

:install_miniconda
if exist %BIN%%MINICONDA_EXE% (
    echo Installing miniconda...
    start /wait "" %BIN%%MINICONDA_EXE% /InstallationType=JustMe /RegisterPython=0 /S /Q /D=%MINICONDA_PATH%
    echo.
) else (
    echo Error: missing %MINICONDA_EXE%
    echo.
	exit /b 1
)
goto :eof

:unpack_nekocatgame
	call %UNPACK%
goto :eof

:check_install
	echo Checking the installation...
	call %DOCTOR%
goto :eof
