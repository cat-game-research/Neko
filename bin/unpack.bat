@echo off

set BIN=%~dp0
set ROOT=%BIN%..\
set PROJ=NekoCatGame
set PACK=NekoCatGame.zip

call %ROOT%miniconda\Scripts\conda init powershell
call %ROOT%miniconda\Scripts\activate %PROJ%

mkdir %PROJ%
tar -xzf dist\%PACK% -C NekoCatGame

call conda-unpack

call conda deactivate
