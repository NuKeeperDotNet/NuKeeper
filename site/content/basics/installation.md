---
title: "Installation"
weight: 10
---


You will first need [.NET Core 2.1 or later installed](https://www.microsoft.com/net).

NuKeeper [can be found on NuGet as a global tool](https://www.nuget.org/packages/NuKeeper/). The [source is on GitHub](https://github.com/NuKeeperDotNet/NuKeeper).

Install NuKeeper with:

```sh
dotnet tool install nukeeper --global
```

Update NuKeeper with:

```sh
dotnet tool update nukeeper --global
``` 

> Note: NuKeeper has experimental support for running package updates on Linux/MacOs. This functionality relies on Mono installation on local system. Please refer to https://www.mono-project.com/ for more information about how to install mono.

# Using NuKeeper

Running `nukeeper` with no arguments shows the help.

```sh
nukeeper --help
```

Help on a NuKeeper command

```sh
nukeeper repo --help
```