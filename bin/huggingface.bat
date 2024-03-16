@echo off
setlocal enabledelayedexpansion

set ACTION=%~1
set NAME=%~2
set VERSION=%~3
set SEQUENCE=%~4

if "%ACTION%"=="" (
    echo No action specified. Exiting.
    exit /b 1
)

if /I not "%ACTION%"=="copy" (
    echo Invalid action. Only 'copy' is supported. Exiting.
    exit /b 1
)

if "%NAME%"=="" (
    echo No name specified. Exiting.
    exit /b 1
)

if "%VERSION%"=="" (
    echo No version specified. Exiting.
    exit /b 1
)

if "%SEQUENCE%"=="" (
    echo No sequence specified. Exiting.
    exit /b 1
)

call :copy_files

goto :eof

:copy_files
set ORIGINAL_DIR=%CD%
set BIN=%~dp0
set ROOT=%BIN%..\
set RESULTS_DIR=results\%NAME%%VERSION%.%SEQUENCE%\Kyle
set SOURCE_PATH_ONNX=C:\Users\3nigma\Unity\NekoCatGame\RagdollTrainer\%RESULTS_DIR%
set DESTINATION_PATH_ONNX=C:\Users\3nigma\source\repos\%NAME%\models\
set SOURCE_FILE_CONFIG=C:\Users\3nigma\Unity\NekoCatGame\RagdollTrainer\config\%NAME%-%VERSION%.yaml
set DESTINATION_DIR_CONFIG=C:\Users\3nigma\source\repos\%NAME%\config\
set SOURCE_PATH_TF=C:\Users\3nigma\source\repos\%NAME%\models\
set DESTINATION_PATH_TF=C:\Users\3nigma\Unity\NekoCatGame\RagdollTrainer\Assets\TFModels\%NAME%\

echo Copying ONNX files...
cd /d "%SOURCE_PATH_ONNX%"
if not exist "%SOURCE_PATH_ONNX%" (
    echo ONNX source path does not exist.
    goto copy_config
)
for %%f in (*.onnx) do (
    echo Found ONNX file: "%%f"
    xcopy "%%f" "%DESTINATION_PATH_ONNX%" /Y > NUL
    set "filename=%%~nf"
    if not "!filename:~0,9!"=="%NAME%-%VERSION%-" (
        set "newname=%NAME%-%VERSION%-!filename:~5!"
        echo Renaming to: "!newname!.onnx"
        move /Y "%DESTINATION_PATH_ONNX%%%f" "%DESTINATION_PATH_ONNX%!newname!.onnx" > NUL
    )
)

:copy_config
echo Copying config file...
if not exist "%SOURCE_FILE_CONFIG%" (
    echo Config file does not exist.
    goto copy_tf
)
xcopy "%SOURCE_FILE_CONFIG%" "%DESTINATION_DIR_CONFIG%" /Y /Q

:copy_tf
echo Copying TensorFlow model files...
if not exist "%SOURCE_PATH_TF%*.onnx" (
    echo No TensorFlow ONNX files found in source.
    goto end_copy
)
xcopy "%SOURCE_PATH_TF%*.onnx" "%DESTINATION_PATH_TF%" /Y

:end_copy
echo Returning to original directory...
cd /d "%ORIGINAL_DIR%"

echo Operation completed.
goto :eof

:eof
exit /b 0
