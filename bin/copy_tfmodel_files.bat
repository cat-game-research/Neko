@echo off
set SOURCE_PATH=C:\Users\3nigma\source\repos\Kyle-b0a\models\
set DESTINATION_PATH=C:\Users\3nigma\Unity\NekoCatGame\RagdollTrainer\Assets\TFModels\Kyle-b0a\

echo Copying ONNX files from "%SOURCE_PATH%" to "%DESTINATION_PATH%"...
xcopy "%SOURCE_PATH%*.onnx" "%DESTINATION_PATH%" /Y

echo Operation completed.

:exit
exit /b 0
