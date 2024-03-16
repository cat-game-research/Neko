@echo off
set SOURCE_PATH=C:\Users\3nigma\Unity\NekoCatGame\RagdollTrainer\results\KyleBeta2.b0a-020m\Kyle\
set DESTINATION_PATH=C:\Users\3nigma\source\repos\Kyle-b0a\models\

cd /d "%SOURCE_PATH%"
for %%f in (*.onnx) do (
    echo Copying and renaming: "%%f"
    xcopy "%%f" "%DESTINATION_PATH%" /Y > NUL
    set "filename=%%~nf"
    setlocal enabledelayedexpansion
    if not "!filename:~0,9!"=="Kyle-b0a-" (
        set "newname=Kyle-b0a-!filename:~5!"
        move /Y "%DESTINATION_PATH%%%f" "%DESTINATION_PATH%!newname!.onnx" > NUL
    )
    endlocal
)

echo Operation completed.
