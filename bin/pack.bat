@echo off
cls
set BIN=%~dp0
set ROOT=%BIN%..\
set DIST=%ROOT%dist\

set ENV_NAME=NekoCatGame
set OUTPUT_NAME=NekoCatGame.zip

rd /s /q %DIST%
mkdir %DIST%

call conda activate NekoCatGame
call conda pack --format zip  --compress-level 9 --n-threads -1 --output %DIST%%OUTPUT_NAME%  --force

call conda deactivate

