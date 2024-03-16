@echo off

:: Save the current directory
set ORIGINAL_DIR=%CD%

:: Set the directory where the train.bat file is located as the working directory
set BIN=%~dp0
set ROOT=%BIN%..\

:: Navigate to the NekoCatGame directory
cd /d %ROOT%

:: Activate the NekoCatGame conda environment
call conda activate NekoCatGame
if %ERRORLEVEL% neq 0 (
    echo Failed to activate conda environment.
    exit /b %ERRORLEVEL%
)

:: Run the mlagents-learn command with the specified parameters
call mlagents-learn RagdollTrainer\config\Kyle-b0a.yaml --run-id=KyleBeta2.b0a-020m --time-scale 1 --quality-level 5 --env=RagdollTrainer\Builds\server_windows_x64\RagdollTrainer.exe --num-envs=4 --no-graphics
if %ERRORLEVEL% neq 0 (
    echo Failed to start training.
    exit /b %ERRORLEVEL%
)

:: Deactivate the conda environment (optional)
call conda deactivate

:: Return to the original directory
cd /d %ORIGINAL_DIR%

exit /b 0