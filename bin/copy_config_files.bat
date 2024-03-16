@echo off
set SOURCE_FILE=C:\Users\3nigma\Unity\NekoCatGame\RagdollTrainer\config\Kyle-b0a.yaml
set DESTINATION_DIR=C:\Users\3nigma\source\repos\Kyle-b0a\config\

echo Source: %SOURCE_FILE%
echo Destination: %DESTINATION_DIR%

xcopy "%SOURCE_FILE%" "%DESTINATION_DIR%" /Y /Q
echo Configuration file copied successfully.

:exit
exit /b 0
