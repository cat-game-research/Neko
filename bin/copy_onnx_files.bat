@echo off
set SOURCE_PATH=C:\Users\3nigma\Unity\NekoCatGame\RagdollTrainer\results\KyleBeta2.b0a-020m\Kyle\
set DESTINATION_PATH=C:\Users\3nigma\source\repos\Kyle-b0a\models\

echo Copying ONNX files from "%SOURCE_PATH%" to "%DESTINATION_PATH%"...
xcopy "%SOURCE_PATH%*.onnx" "%DESTINATION_PATH%" /Y

echo Renaming copied ONNX files...
cd /d "%DESTINATION_PATH%"
for %%f in (Kyle-*.onnx) do (
    set "filename=%%~nf"
    setlocal enabledelayedexpansion
    if not "!filename:~0,9!"=="Kyle-b0a-" (
        set "newname=Kyle-b0a-!filename:~5!"
        echo Renaming: "%%f" to "!newname!.onnx"
        ren "%%f" "!newname!.onnx"
    )
    endlocal
)

echo Operation completed.

:exit
exit /b 0
