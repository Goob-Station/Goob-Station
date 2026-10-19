@echo off
echo ===================================================
echo Step 1: Saving Malf AI changes to feat/malf-ai branch...
echo ===================================================
git checkout -b feat/malf-ai
git add -A
git commit -m "WIP: Port Malf AI updates from funky-station PR #1493"

echo.
echo ===================================================
echo Step 2: Updating master to origin/master...
echo ===================================================
git checkout master
git fetch origin
git pull --ff-only origin master

echo.
echo ===================================================
echo Step 3: Updating submodules...
echo ===================================================
git submodule update --init --recursive

echo.
echo ===================================================
echo DONE!
echo Your Malf AI changes are saved on branch: feat/malf-ai
echo Master is now up to date with origin/master.
echo ===================================================
pause
