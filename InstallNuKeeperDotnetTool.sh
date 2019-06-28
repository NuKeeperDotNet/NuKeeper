#!/usr/bin/env bash
echo "Build solution:"
dotnet build 

echo "Pack NuKeeper dotnet tool:"
dotnet pack .\NuKeeper\NuKeeper.csproj -o ".\artifacts"

echo "Uninstall existing NuKeeper dotnet tool:"
dotnet tool uninstall nukeeper --global

echo "Install NuKeeper dotnet tool:"
dotnet tool install nukeeper --global --add-source ".\artifacts"

echo "Installed NuKeeper version:"
nukeeper --version

# Keep window open.
$SHELL