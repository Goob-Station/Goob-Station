@echo off
echo Fast-forwarding master...
git pull --ff-only

echo.
echo Syncing submodules (RobustToolbox)...
git submodule update --init --recursive

echo.
echo Done! Master is fully up to date.
pause
