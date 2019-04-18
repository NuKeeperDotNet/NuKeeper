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

# Using NuKeeper

Running `nukeeper` with no arguments shows the help.

```sh
nukeeper --help
```

Help on a NuKeeper command

```sh
nukeeper repo --help
```