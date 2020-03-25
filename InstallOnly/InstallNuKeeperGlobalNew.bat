@echo off

title Installing NuKeeper dotnet tool with downgrade functionality

echo Uninstall existing NuKeeper dotnet tool:
dotnet tool uninstall nukeeper --global

echo Install NuKeeper dotnet tool:
dotnet tool install nukeeper --global --add-source ".\artifacts"

echo Installed NuKeeper version:
nukeeper --version

pause