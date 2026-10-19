@echo off
echo Staging resolved conflict files...
git add -A

echo.
echo Continuing rebase...
git -c core.editor=true rebase --continue

echo.
echo ========================================================
if %ERRORLEVEL% equ 0 (
    echo Rebase successfully completed!
    echo Your Malf AI branch is now cleanly on top of latest master!
) else (
    echo Something needs attention. Check output above.
)
echo ========================================================
pause
