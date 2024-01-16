@echo off

set BIN=%~dp0
set ROOT=%BIN%..\
set RUN=run.py

call %ROOT%miniconda\Scripts\conda init powershell
call %ROOT%miniconda\Scripts\activate NekoCatGame

python %BIN%%RUN%

call conda deactivate