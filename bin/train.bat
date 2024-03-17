@echo off
set ORIGINAL_DIR=%CD%
set BIN=%~dp0
set ROOT=%BIN%..\
set RAGDOLL_TRAINER=RagdollTrainer

set MODE=%1
set NUM_ENVS=%2
set MODE_ARG=

if "%NUM_ENVS%"=="" (
    set NUM_ENVS=1
)

if "%MODE%"=="" goto display_help
if "%MODE%"=="help" goto display_help
if "%MODE%"=="--help" goto display_help

if "%MODE%"=="create" (
    set MODE_ARG=
) else if "%MODE%"=="resume" (
    set MODE_ARG=--resume
) else if "%MODE%"=="force" (
    set MODE_ARG=--force
) else if "%MODE%"=="delete" (
    call :delete_results
    goto eof
) else (
    echo Invalid mode argument provided. Exiting.
    exit /b 1
)

echo Activating conda environment...
call :activate_conda

echo Starting training...
call :run_training

echo Deactivating conda environment...
call :deactivate_conda

echo Returning to original directory...
call :return_to_original_dir
goto eof

:display_help
echo Usage: train.bat [--help] [mode] [num-envs]
echo.
echo Options:
echo --help   - Display this help message.
echo.
echo Modes:
echo create   - Start a new training run.
echo resume   - Resume a previous training run.
echo force    - Force start a new training run and overwrite any existing data.
echo delete   - Delete the training results directory.
echo.
echo num-envs - Optional. Specify the number of environments. Default is 1.
echo.
goto eof

:activate_conda
cd /d %ROOT%
call conda activate NekoCatGame
if %ERRORLEVEL% neq 0 (
    echo Failed to activate conda environment.
    exit /b %ERRORLEVEL%
)
goto :eof

:run_training
cd /d %ROOT%\%RAGDOLL_TRAINER%
set RESULTS_DIR=results\KyleBeta3.b0a-020m
if not exist "%RESULTS_DIR%" (
    echo Warning: No previous training results found. Starting a new training run...
    set MODE_ARG=
)
set ML_AGENTS_CMD=mlagents-learn config\Kyle-b0a.yaml --run-id=KyleBeta3.b0a-020m --time-scale 1 --quality-level 5 --env=Builds\server_windows_x64\RagdollTrainer.exe --num-envs=%NUM_ENVS% --no-graphics %MODE_ARG%
echo %ML_AGENTS_CMD%
call %ML_AGENTS_CMD%
if %ERRORLEVEL% neq 0 (
    echo Failed to start training.
    exit /b %ERRORLEVEL%
)
goto :eof

:deactivate_conda
rem call conda deactivate
goto :eof

:return_to_original_dir
cd /d %ORIGINAL_DIR%
goto :eof

:delete_results
set RESULTS_DIR=%ROOT%\%RAGDOLL_TRAINER%\results\KyleBeta3.b0a-020m
echo Deleting %RESULTS_DIR%
if exist "%RESULTS_DIR%" (
    rmdir /s /q "%RESULTS_DIR%"
    echo Training results deleted successfully.
) else (
    echo Training results directory does not exist.
)
goto :eof

:eof
exit /b 0
