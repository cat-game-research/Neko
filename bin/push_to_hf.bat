@echo off
cls
setlocal
call :set_variables
call :check_hf_login
call :navigate_to_repo
call :add_all_onnx_to_git
call :commit_changes
call :push_to_hf
endlocal
goto :eof

:set_variables
set BIN=%~dp0
set ROOT=%BIN%..
set REPO_DIR=\RagdollTrainer\Assets\TFModels
set HF_CLI=huggingface-cli
set GIT=git
set HF_TOKEN_FILE=%USERPROFILE%\.huggingface\token
goto :eof

:check_hf_login
if exist %HF_TOKEN_FILE% (
    echo Hugging Face login detected.
) else (
    echo You need to log in to Hugging Face to continue.
    call %HF_CLI% login
)
echo.
goto :eof

:navigate_to_repo
echo Navigating to the repository...
cd /d %ROOT%
echo.
goto :eof

:add_all_onnx_to_git
echo Adding all ONNX models to git...
call %GIT% add %ROOT%%REPO_DIR%\*.onnx
echo.
goto :eof

:commit_changes
echo Committing changes...
call %GIT% commit -m "Add all ONNX models"
echo.
goto :eof

:push_to_hf
echo Pushing to Hugging Face...
call %GIT% push
echo.
goto :eof
