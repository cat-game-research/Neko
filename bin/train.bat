@echo off

set BIN=%~dp0
set ROOT=%BIN%..\

rem call conda activate NekoCatGame
call mlagents-learn config/Walker.yaml --run-id=WalkerAlpha2.a0a-140m --time-scale 1 --quality-level 5 --env=Builds/windows_x64/RagdollTrainer.exe --num-envs=10 --no-graphics-monitor --resume
rem call conda deactivate
