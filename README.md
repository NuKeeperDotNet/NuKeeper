# NuKeeper


![Build Status](https://travis-ci.org/NuKeeperDotNet/NuKeeper.svg?branch=master)

[![Gitter](https://img.shields.io/gitter/room/NuKeeperDotNet/Lobby.js.svg?maxAge=2592000)](https://gitter.im/NuKeeperDotNet/Lobby)

Automagically update NuGet packages in .NET projects.

## Why
 
Because .Net devs are bad at applying NuGet package updates.

## What

**NuKeeper** will compare the NuGet packages used in your solution to the latest versions available on [Nuget.org](https://www.nuget.org), and make PRs containing updates.

## How

Written in .Net core, using http APIs and command-line tools.

## Usage

### Repository mode

Will PR a single repository. Point it at the https url of a github repository, like this:

syntax:
```
C:\Code\NuKeeper\NuKeeper>dotnet run mode=repository github_token=<GitToken> github_repository_uri=<RepoUrl>
```

### Organisation mode

Will discover repositories in that organisation and update them.

```
C:\Code\NuKeeper\NuKeeper>dotnet run mode=organisation github_token=<GitToken> github_api_endpoint=https://api.github.com/ github_organisation_name=<OrgName>
```

### Command-line arguments

| Name                             | Alias | Default value          | Required |
|----------------------------------|-------|------------------------|-----------------|
| mode                             | m     |                        | Yes             |
| github_token                     | t     |                        | Yes             |
| github_repository_uri            | repo  |                        | Depends on mode |
| github_organisation_name         | org   |                        | Depends on mode |
| github_api_endpoint              | api   | https://api.github.com | No              |
| max_pull_requests_per_repository | maxpr | 3                      | No              |


 * *mode* One of `repository` or `organisation`. In `organisation` mode, all the repositories in that organisation will be processed.
 * *github_token* You will need to [create a github personal access token](https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/) to authorise access to your github server in order to raise PRs. Be sure to check the "repo" scope when creating the token.
 * *github_repository_uri* The repository to scan. Required in `repository` mode, not used `organisation` mode.
 * *github_organisation_name* the organisation to scan. Required in `organisation` mode, not used `repository` mode.
 *  *github_api_endpoint* This defaults to `https://api.github.com`. If you are using an internal github server and not the public one, you must set it to the api url for your github server. The value will be e.g. `https://github.mycompany.com/api/v3`. This applies to all modes.
 * *max_pull_requests_per_repository* The maximum number of pull requests to raise on any repository. The default value is 3.

## When to use NuKeeper

If the project is a library that itself produces a NuGet package, it is usually best not to update it aggressively without cause. 
e.g. if `MyFancyLib` depends upon `Newtonsoft.Json` version `9.0.1` then an application that depends upon `MyFancyLib` can use `Newtonsoft.Json` version `9.0.1` _or a later version_.   Updating the reference in `MyFancyLib` to `Newtonsoft.Json` version `10.0.3` takes away some flexibility in the application using `MyFancyLib`. 
[It might even cause problems](https://github.com/Azure/azure-sdk-for-net/issues/3003). 

In an end-product deployable application, aggressive updating of packages is a better tactic.

This is an application of [Postel's Law](https://en.wikipedia.org/wiki/Robustness_principle): Packages should be liberal in the range of package versions that they can accept, and applications should be strict about using up to date packages when they run.

It is similar to [this rule of preferring to use a parameter of a base type or interface](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1011-consider-passing-base-types-as-parameters) as it allows wider use.

## Limitations

You will need command line versions of `git` and `dotnet` installed.

It currently only runs on windows due to using `cmd` to invoke command-line processes for `git` and `dotnet`.

For projects using `packages.config`, `NuGet.exe` no longer runs `install.ps1` and `uninstall.ps1` scripts from command line. 
Those are still executed from Visual Studio, resulting in different behaviour for packages relying on this functionality. 
An example of this is [StyleCop.Analyzers](https://www.nuget.org/packages/StyleCop.Analyzers/) which will not update the `<Analyzers>` node in the project file.


### Footnote

Inspired by [Greenkeeper](https://greenkeeper.io/).

