# Script to safely branch Malf AI changes and update master
Write-Host "=== Step 1: Creating feat/malf-ai branch and saving current changes ===" -ForegroundColor Cyan
git checkout -b feat/malf-ai
git add -A
git commit -m "WIP: Port Malf AI updates from funky-station PR #1493"

Write-Host "`n=== Step 2: Switching to master and pulling origin/master ===" -ForegroundColor Cyan
git checkout master
git fetch origin
git pull --ff-only origin master

Write-Host "`n=== Step 3: Updating submodules ===" -ForegroundColor Cyan
git submodule update --init --recursive

Write-Host "`n=== Finished updating Goob master! ===" -ForegroundColor Green
Write-Host "Your Malf AI changes are safely committed on branch 'feat/malf-ai'." -ForegroundColor Green
