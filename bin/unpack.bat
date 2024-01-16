@echo off

set BIN=%~dp0
set ROOT=%BIN%..\
set DIST=%ROOT%dist\
set PROJ=NekoCatGame
set PACK=NekoCatGame.zip

call %ROOT%miniconda\Scripts\conda init powershell
call %ROOT%miniconda\Scripts\activate %PROJ%

echo make dir ( %PROJ% )
mkdir %PROJ%

echo unpack %PROJ% ( %DIST%%PACK% )...
tar -xzf %DIST%%PACK% -C %PROJ%

echo conda unpack...
call conda-unpack

call conda deactivate