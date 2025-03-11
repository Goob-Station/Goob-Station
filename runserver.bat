@echo off
dotnet run --project Content.Server --configuration Release --config-file "bin/Content.Server/data/server_config.toml"
pause
