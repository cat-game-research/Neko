@echo off
cls

setlocal
	set BIN=%~dp0
	set ROOT=%BIN%..\
	set DIST=%ROOT%dist\
	set PROJ=NekoCatGame
	set PACK=NekoCatGame.zip
	set START=start.bat
	set OUTPUT=%PROJ%_Setup.zip

call :main
exit /b

:main
	echo Create distribution...
	echo.
	call :remove %DIST%%OUTPUT%
	call :remove %DIST%builds\
	call :copy %ROOT%RagdollTrainer\Builds\* %DIST%builds\
	call :remove %DIST%builds\server_windows_x64\RagdollTrainer_Data\ML-Agents
	call :remove %DIST%builds\windows_x64\RagdollTrainer_Data\ML-Agents
	echo.
	call :copy %BIN% %DIST%bin\
	call :move %DIST%bin\ %DIST% %START%
	call :zip_files %DIST% %OUTPUT%
	echo.
endlocal
exit /b

:remove
echo remove ( %1 )
	IF EXIST %1 rd /s /q %1
exit /b

:copy 
	echo copy ( %1 ... %2 )
	echo.
	if exist %2 (del /q %2\*) else (md %2)
	xcopy /s /e /y %1 %2
	echo.
exit /b

:move
	echo move ( %3 )
	echo.
	move %1%3 %2
	echo.
exit /b

:zip_files
	echo zipping files...
	echo.
	tar --exclude=%2 -a -cvf %1%2 -C %1 .
exit /b
