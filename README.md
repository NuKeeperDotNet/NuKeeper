# NuKeeper


![Build Status](https://travis-ci.org/NuKeeperDotNet/NuKeeper.svg?branch=master)

[![Gitter](https://img.shields.io/gitter/room/NuKeeperDotNet/Lobby.js.svg?maxAge=2592000)](https://gitter.im/NuKeeperDotNet/Lobby)

Automagically update nuget packages in .NET projects.

## Why
 
Because .Net devs are bad at applying nuget package updates.

## What

**NuKeeper** will compare the nuget packages used in your solution to the latest versions available on [Nuget.org](https://www.nuget.org/), and make PRs containg updates.


## How

Written in .Net core.

You will need command line versions of `git` and `dotnet`.

You will need [a github api token](https://github.com/blog/1509-personal-api-tokens) with access to public repositories.

## Repository mode

Will PR a single repository. Point it at the https url of a github repo, like this:

syntax:
```
C:\Code\NuKeeper\NuKeeper>dotnet run mode=repository github_token=<GitToken> github_repository_uri=<RepoUrl>
```

## Organisation mode

Will discover repositories in that organisation and update them.

```
C:\Code\NuKeeper\NuKeeper>dotnet run mode=organisation github_token=<GitToken> github_api_endpoint=https://api.github.com/ github_organisation_name=<OrgName>
```

## Limitations

For projects using `packages.config`, `NuGet.exe` no longer runs `install.ps1` and `uninstall.ps1` scripts from command line. Those are still executed from Visual Studio, resulting in different behaviour for packages relying on this functionality. An example of this is [StyleCop.Analyzers](https://www.nuget.org/packages/StyleCop.Analyzers/) which will not update the `<Analyzers>` node in the project file.

Inspired by [Greenkeeper](https://greenkeeper.io/).

