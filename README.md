# NuKeeper


[![Build Status](https://travis-ci.org/NuKeeperDotNet/NuKeeper.svg?branch=master)](https://travis-ci.org/NuKeeperDotNet/NuKeeper/)
[![Gitter](https://img.shields.io/gitter/room/NuKeeperDotNet/Lobby.js.svg?maxAge=2592000)](https://gitter.im/NuKeeperDotNet/Lobby)

Automagically generate pull requests to update NuGet packages in .NET projects.

## Why
 
Because .Net devs are bad at applying NuGet package updates.

## What

**NuKeeper** will compare the NuGet packages used in your solution to the latest versions available on [Nuget.org](https://www.nuget.org), and make PRs containing updates.

## How

Written in .Net core, using http APIs and command-line tools.

## Usage

### Repository mode

In Repository Mode **NuKeeper** will perform version checks and generate PRs for a single repository. From within the **NuKeeper** folder, point it at the https url of a github repository, like this:

```
$ dotnet run mode=repository t=<GitToken> github_repository_uri=<RepoUrl>
```

### Organisation mode

In Organisation Mode **NuKeeper** will perform the checks and generate PRs for all repositories in the specified organisation that the provided `GitToken` has access to.

```
$ dotnet run mode=organisation t=<GitToken> github_organisation_name=<OrgName>
```
### Environment Variables

| Name                             | Required?                       | Overridable via CLI? |
|----------------------------------|---------------------------------|----------------------|
| NuKeeper_github_token            | Unless provided on command line | Yes (`t`)            |

 * *NuKeeper_github_token* You will need to [create a github personal access token](https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/) to authorise access to your github server in order to raise PRs. Be sure to check the "repo" scope when creating the token. 

 If you have just created the environment variables, remember to restart your terminal and IDE before proceeding.

### Config Json

| Name                             | Required          | Overridable via CLI? | Default                             |
|----------------------------------|-------------------|----------------------|-------------------------------------|
| github_api_endpoint              | No                | Yes (`api`)          | https://api.github.com              |
| max_pull_requests_per_repository | No                | Yes (`maxpr`)        | 3                                   |
| nuget_sources                    | No                | Yes (`sources`)      | https://api.nuget.org/v3/index.json |
| log_level                        | No                | Yes (`log`)          | Info                                |
| allowed_version_change           | No                | Yes (`change`)       | Major                               |
 
 * *github_api_endpoint* This is the api endpoint for the github instance you're targetting. If you are using an internal github server and not the public one, you must set it to the api url for your github server. The value will be e.g. `https://github.mycompany.com/api/v3`. This applies to all modes.
 * *max_pull_requests_per_repository* The maximum number of pull requests to raise on any repository. 
 * *nuget_sources* Semicolon-separated list of NuGet repositories to use when searching for updates and when installing them.
 * *log_level*. Controls how much output is displayed. Values are, from least output to most output: `Silent`, `Terse`, `Info`, `Verbose`.
 * *allowed_version_change* What level of update is allowed to packages, based on the version number difference. Values are:  `Major`, `Minor`, `Patch`. 
 See [Semver](http://semver.org/) for what these mean. 
 The default value `Major` will allow updates to the overall latest version even if it means accepting a new major version.

    For example, if you are currently using package `Foo` at version `1.2.3` and these new versions are available: `1.2.4` - a patch version change, `1.3.0` - a minor version change and `2.0.0` - a new major version. 
    * If the allowed version change is `Major` (the default) you will get an update to the overall latest version - `2.0.0`. 
    * If you set the allowed version change to `Minor`, you will get an update to `1.3.0` as now changes to the major version number are not allowed, 
	* If the allowed version change is `Patch` you will only get an update to version `1.2.4`.

### Command-line arguments

| Name                             | Required                   |
|----------------------------------|----------------------------|
| mode (m)                         | Yes                        |
| t                                | No                         |
| github_repository_uri (repo)     | Yes in Repository mode     |
| github_organisation_name (org)   | Yes in Organisation mode   |
| api                              | No                         |
| maxpr                            | No                         |
| log                              | No                         |
| include (i)                      | No                         |
| exclude (e)                      | No                         |
| sources                          | No                         |
| change                           | No                         |

 * *mode* One of `repository` or `organisation`. In `organisation` mode, all the repositories in that organisation will be processed.
 * *t* Overrides `NuKeeper_github_token` in environment variables.
 * *github_repository_uri* The repository to scan. Required in `repository` mode, not used `organisation` mode. Aliased to `repo`.
 * *github_organisation_name* the organisation to scan. Required in `organisation` mode, not used in `repository` mode. Aliased to `org`.
 * *api* Overrides `github_api_endpoint` in `config.json`. Must be a fully qualified URL.
 * *maxpr* Overrides `max_pull_requests_per_repository` in `config.json`.
 * *log* Overrides `log_level` in `config.json`.
 * *include* Only consider packages matching this regex pattern.
 * *exclude* Do not consider packages matching this regex pattern.
 * *sources* Overrides `nuget_sources` in `config.json`.
 * *change* Overrides  `allowed_version_change` in `config.json`



## When to use NuKeeper

If the project is a library that itself produces a NuGet package, it is usually best not to update it aggressively without cause.  Consider carefully whether you want to force your users to also update entire dependency chains.

e.g. if `MyFancyLib` depends upon `Newtonsoft.Json` version `9.0.1` then an application that depends upon `MyFancyLib` can use `Newtonsoft.Json` version `9.0.1` _or a later version_.   Updating the reference in `MyFancyLib` to `Newtonsoft.Json` version `10.0.3` takes away some flexibility in the application using `MyFancyLib`. 
[It might even cause problems](https://github.com/Azure/azure-sdk-for-net/issues/3003). 

Libraries should, however, update their packages when there is a breaking change in the features that they use or another compelling reason. e.g. `MyFancyLib` uses `Newtonsoft.Json` version `8.0.1`, but since it only calls `JsonConvert.DeserializeObject<>` many versions of `Newtonsoft.Json` can be used. 
But now I am converting `MyFancyLib` to NetStandard for .Net Core. The lowest version of `Newtonsoft.Json` that supports this is `9.0.1`, so we use that. 
Although there are later versions of `Newtonsoft.Json`, this gives `MyFancyLib` what it needs and allows clients the most choice within the constraint of supporting NetStandard.

In an end-product deployable application, frequent updating of packages is a better tactic.
Supported by comprehensive automated testing, regular updates will keep your application up to date with security fixes and prevent it from relying on potentially outdated libraries.

This is an application of [Postel's Law](https://en.wikipedia.org/wiki/Robustness_principle): Packages should be liberal in the range of package versions that they can accept, and applications should be strict about using up to date packages when they run.

It is similar to [this rule of preferring to use a parameter of a base type or interface](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1011-consider-passing-base-types-as-parameters) as it allows wider use.

## Limitations and warnings

You will need the command line version of `dotnet` installed.

It currently only runs on windows due to using `cmd` to invoke command-line processes for `dotnet`.

For projects using `packages.config`, `NuGet.exe` no longer runs `install.ps1` and `uninstall.ps1` scripts from command line. 
Those are still executed from Visual Studio, resulting in different behaviour for packages relying on this functionality. 
An example of this is [StyleCop.Analyzers](https://www.nuget.org/packages/StyleCop.Analyzers/) which will not update the `<Analyzers>` node in the project file.


### Footnote

Inspired by [Greenkeeper](https://greenkeeper.io/).

