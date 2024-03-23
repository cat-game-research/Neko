@echo off
cls
setlocal enabledelayedexpansion

rem Check for help argument
if /I "%~1"=="help" (
    echo Usage: huggingface.bat [action] [name] [type] [version] [sequence] [destination]
    echo Example: huggingface.bat copy Kyle Beta4 b0a 020M C:\Users\3nigma\source\repos\
    echo.
    echo [action] - The action to perform, currently only 'copy' is supported.
    echo [name] - The name of the project or model.
    echo [type] - The type of the project or model.
    echo [version] - The version of the project or model.
    echo [sequence] - The sequence identifier for the project or model.
    echo [destination] - The destination path where the files will be copied.
    exit /b 0
)

rem  Ex: .\bin\huggingface.bat copy Kyle Beta3 b0a 020m C:\Users\3nigma\source\repos\
echo [HuggingFace CMD] Starting batch process with parameters: Action=%~1, Name=%~2, Type=%~3, Version=%~4, Sequence=%~5, DESTINATION=%~6

set ACTION=%~1
set NAME=%~2
set TYPE=%~3
set VERSION=%~4
set SEQUENCE=%~5
set DESTINATION=%~6

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

if "%TYPE%"=="" (
    echo No type specified. Exiting.
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

if "%DESTINATION%"=="" (
    echo No destination specified. Exiting.
    exit /b 1
)

echo Parameters validated, proceeding with file operations...
call :copy_files

goto :eof

:copy_files
set ORIGINAL_DIR=%CD%
set BIN=%~dp0
set ROOT=%BIN%..\
set RESULTS_DIR=results\%NAME%%TYPE%.%VERSION%-%SEQUENCE%\%NAME%
set SOURCE_PATH_ONNX=%ROOT%RagdollTrainer\%RESULTS_DIR%
rem set DESTINATION=C:\Users\3nigma\source\repos\
set DESTINATION_PATH_ONNX=%DESTINATION%%NAME%-%VERSION%\models\
set SOURCE_FILE_CONFIG=%ROOT%RagdollTrainer\config\%NAME%-%VERSION%.yaml
set SOURCE_FILE_SAC_CONFIG=%ROOT%RagdollTrainer\config\%NAME%-%VERSION%.sac.yaml
set DESTINATION_DIR_CONFIG=%DESTINATION%%NAME%-%VERSION%\config\
set SOURCE_PATH_TF=%DESTINATION%%NAME%-%VERSION%\models\
set DESTINATION_PATH_TF=%ROOT%RagdollTrainer\Assets\TFModels\%NAME%-%VERSION%\

echo Copying ONNX files from: "%SOURCE_PATH_ONNX%" to: "%DESTINATION_PATH_ONNX%"
cd /d "%SOURCE_PATH_ONNX%"
if not exist "%SOURCE_PATH_ONNX%" (
    echo ONNX source path does not exist.
    goto copy_config
)
for %%f in (*.onnx) do (
    echo Found ONNX file: "%%f"
    xcopy "%%f" "%DESTINATION_PATH_ONNX%" /Y > NUL
    set "filename=%%~nf"
    if not "!filename:~0,9!"=="%TYPE%-%VERSION%-" (
        set "newname=%NAME%%TYPE%-%VERSION%-!filename:~5!"
        echo Renaming to: "!newname!.onnx"
        move /Y "%DESTINATION_PATH_ONNX%%%f" "%DESTINATION_PATH_ONNX%!newname!.onnx" > NUL
    )
)

:copy_config
echo Checking for config file at: "%SOURCE_FILE_CONFIG%"
if not exist "%SOURCE_FILE_CONFIG%" (
    echo Config file does not exist.
    goto copy_tf
)
echo Copying config file to: "%DESTINATION_DIR_CONFIG%"
xcopy "%SOURCE_FILE_CONFIG%" "%DESTINATION_DIR_CONFIG%" /Y /Q
echo Checking for config file at: "%SOURCE_FILE_SAC_CONFIG%"
if not exist "%SOURCE_FILE_SAC_CONFIG%" (
    echo Config file does not exist.
    goto copy_tf
)
echo Copying config file to: "%DESTINATION_DIR_CONFIG%"
xcopy "%SOURCE_FILE_SAC_CONFIG%" "%DESTINATION_DIR_CONFIG%" /Y /Q

:copy_tf
echo Checking for TensorFlow ONNX files at: "%SOURCE_PATH_TF%"
if not exist "%SOURCE_PATH_TF%*.onnx" (
    echo No TensorFlow ONNX files found in source.
    goto end_copy
)
echo Copying TensorFlow model files to: "%DESTINATION_PATH_TF%"
xcopy "%SOURCE_PATH_TF%*.onnx" "%DESTINATION_PATH_TF%" /Y

:end_copy
echo Returning to original directory: "%ORIGINAL_DIR%"
cd /d "%ORIGINAL_DIR%"

echo Operation completed successfully.
goto :eof

:eof
exit /b 0
