@echo off
echo Switching to feat/malf-ai...
git checkout feat/malf-ai

echo.
echo Rebasing feat/malf-ai onto master...
git rebase master

echo.
echo ========================================================
if %ERRORLEVEL% equ 0 (
    echo Rebase completed successfully with NO conflicts!
) else (
    echo Conflicts were detected during rebase.
    echo Leave this window open - the AI assistant will help resolve them!
)
echo ========================================================
pause
