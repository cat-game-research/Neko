@echo off
rem Check if nvcc is in the PATH
where /q nvcc
if %errorlevel% neq 0 (
    echo CUDA is not installed or not in the PATH
    exit /b 1
)

rem Get the CUDA version from nvcc output
for /f "tokens=6" %%a in ('nvcc --version ^| find /i "release"') do (
    set cuda_version=%%a
)

rem Check if the CUDA driver is compatible with the CUDA toolkit
nvidia-smi -q -d SUPPORTED_CLOCKS > nul 2>&1
if %errorlevel% neq 0 (
    echo CUDA driver is not compatible with CUDA %cuda_version%
    exit /b 2
)

rem Display the CUDA information
echo CUDA is installed and working on this computer
echo CUDA version: %cuda_version%
exit /b 0
