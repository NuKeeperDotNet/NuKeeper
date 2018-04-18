# NuKeeper


[![Build Status](https://travis-ci.org/NuKeeperDotNet/NuKeeper.svg?branch=master)](https://travis-ci.org/NuKeeperDotNet/NuKeeper/)
[![Gitter](https://img.shields.io/gitter/room/NuKeeperDotNet/Lobby.js.svg?maxAge=2592000)](https://gitter.im/NuKeeperDotNet/Lobby)

Automagically generate pull requests to update NuGet packages in .NET projects.

## Why

Because .Net developers are bad at applying NuGet package updates. To increase visibility of package updates, and decrease cycle time.

Why do we deploy code changes frequently but seldom update NuGet packages? In [Continuous delivery](https://en.wikipedia.org/wiki/Continuous_delivery), we know that there is a vicious cycle of "deploys are infrequent and contain lots of changes, therefore deploys are hard and dangerous, therefore deploys are infrequent and contain lots of changes" and a virtuous cycle of "deploys are frequent and contain incremental changes, therefore deploys are easy and low risk, therefore deploys are frequent and contain incremental changes" and so we work hard to move into the second cycle, and afterwards, life is easier.

But NuGet package updates are a form of change that should be deployed, and we likewise want to change the cycle from "NuGet package updates are infrequent and contain lots of package changes, therefore NuGet package updates are hard and dangerous..." to "NuGet package updates are frequent and contain small changes, therefore NuGet package updates are easy and routine...".

## What

Automate the routine task of discovering and applying NuGet package updates.

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
| fork_mode                        | No                | Yes (`fork`)         | PreferFork                          |
| report_mode                      | No                | Yes (`report`)       | Off                                 |
| min_package_age                  | No                | Yes (`age`)          | 7d                                  |

 * *github_api_endpoint* This is the api endpoint for the github instance you're targetting. If you are using an internal github server and not the public one, you must set it to the api url for your github server. The value will be e.g. `https://github.mycompany.com/api/v3`. This applies to all modes.
 * *max_pull_requests_per_repository* The maximum number of pull requests to raise on any repository.
 * *nuget_sources* Semicolon-separated list of NuGet repositories to use when searching for updates and when installing them.
 * *log_level*. Controls how much output is displayed. Values are, from least output to most output: `Silent`, `Terse`, `Info`, `Verbose`.
 * *allowed_version_change* The greatest level of update that is allowed to packages, based on the version number difference. Values are:  `Major`, `Minor`, `Patch`.
 See [Semver](http://semver.org/) for what these mean.
 The default value `Major` will allow updates to the overall latest version even if it means accepting a new major version.

    For example, if you are currently using package `Foo` at version `1.2.3` and these new versions are available: `1.2.4` - a patch version change, `1.3.0` - a minor version change and `2.0.0` - a new major version.
    * If the allowed version change is `Major` (the default) you will get an update to the overall latest version, i.e. `2.0.0`.
    * If you set the allowed version change to `Minor`, you will get an update to `1.3.0` as now changes to the major version number are not allowed. Version `1.2.4` is also allowed, but the largest allowed update is applied.
    * If the allowed version change is `Patch` you will only get an update to version `1.2.4`.

 * *fork_mode* Values are `PreferFork`, `PreferSingleRepository` and `SingleRepositoryOnly`. Prefer to make branches on a fork of the target repository, or on that repository itself. See the section "Branches, forks and pull requests" below.
 * *report_mode* Values are `Off`, `On`, `ReportOnly`. This setting controls if a CSV report file of possible updates is generated. The default value `Off` means that no report is generated. `On` will generate it and then proceed with the run, `ReportOnly` is used to generate the report and then exit without making any PRs.

 *  *min_package_age* In order to not consume packages immediately after they are released, exclude updates that do not meet a minimum age. 
The default is 7 days.
This age is the duration between the published date of the selected package update and now. 
 A value can be expressed in command options as an integer and a unit suffix, 
where the unit is one of `h` for hour, `d` for days, `w` for weeks. A zero with no unit is also allowed.
Examples: `0` = zero, `12h` = 12 hours, `3d` = 3 days, `2w` = two weeks. 

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
| fork                             | No                         |
| report                           | No                         |
| age                              | No                         |

 * *mode* One of `repository` or `organisation`, or synonyms `repo` and `org`. In `organisation` mode, all the repositories in that organisation will be processed.
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
 * *fork* Overrides  `fork_mode` in `config.json`
 * *report* Overrides `report_mode` in `config.json`
 * *age* Overrides `min_package_age` in `config.json`


## When to use NuKeeper

If the project is a library that itself produces a NuGet package, it is usually best not to update it aggressively without cause. Consider carefully whether you want to force your users to also update entire dependency chains.

e.g. if `MyFancyLib` depends upon `Newtonsoft.Json` version `9.0.1` then an application that depends upon `MyFancyLib` can use `Newtonsoft.Json` version `9.0.1` _or a later version_. Updating the reference in `MyFancyLib` to `Newtonsoft.Json` version `10.0.3` takes away some flexibility in the application using `MyFancyLib`.
[It might even cause problems](https://github.com/Azure/azure-sdk-for-net/issues/3003).

Libraries should, however, update their packages when there is a breaking change in the features that they use or another compelling reason. e.g. If `MyFancyLib` uses `Newtonsoft.Json` version `8.0.1`, but since it only calls `JsonConvert.DeserializeObject<>` many versions of `Newtonsoft.Json` can be used.
But now I am converting `MyFancyLib` to NetStandard for use in .NET Core. The lowest version of `Newtonsoft.Json` that supports this is `9.0.1`, so we use that.
Although there are later versions of `Newtonsoft.Json`, this gives `MyFancyLib` what it needs and allows clients the most choice within the constraint of supporting NetStandard. Another compelling reason to update a dependency would be if there is a bug fix that impacts the working of `MyFancyLib`, so users of `MyFancyLib` really should apply it.

In an end-product deployable application, frequent updating of packages is a better tactic.
Supported by comprehensive automated testing, regular updates will keep your application up to date with security fixes and prevent it from relying on potentially outdated libraries.

This is an application of [Postel's Law](https://en.wikipedia.org/wiki/Robustness_principle): Packages should be liberal in the range of package versions that they can accept, and applications should be strict about using up to date packages when they run.

It is similar to [this rule of preferring to use a parameter of a base type or interface](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1011-consider-passing-base-types-as-parameters) as it allows wider use.

## Branches, forks and pull requests

Nukeeper needs a repository that it can pull a copy of the code from, and a repository that it can push a new branch to. These might or might not be the same repository.

In the most general case, there are two repositories. The standard term for these is `upstream` and `origin`,
but [bear in mind that `origin` is forked off `upstream`. `origin` is the working copy, and the canonical original is `upstream`](https://stackoverflow.com/questions/9257533/what-is-the-difference-between-origin-and-upstream-on-github). In the NuKeeper code these are sometimes referred to as the "pull fork" and "push fork" respectively, since we pull from the first and push to the second.

### There are two possible workflows:

**Single-repository workflow**. The pull fork and push fork are the same repository. NuKeeper will pull from the repository, branch locally, make a change,  and push a change back to a branch on the same repository, then PR back to the `master` branch.

In this workflow, NuKeeper needs permission to push to the target repository.

**Fork workflow**. The pull fork and push fork are not the same repository. NuKeeper will pull from the upstream, branch locally, make a change, and push it back to a branch on the origin fork, then PR back to the `master` branch on the upstream.

This workflow can be used if:
-  the user (identified by the github token) already has a repository with the right name, that is a fork of the target repository and we have permission to push there.
- Or the user does not have a repository with the right name, but it can be created as a fork of the target.

This is automatic, NuKeeper will find the fork, or attempt to create it if it does not exist.

The `ForkMode` option controls which workflows will be tried, and in what order. Values are `PreferFork`, `PreferSingleRepository` and `SingleRepositoryOnly`. The default is `PreferFork`.

In `PreferFork` mode, both workflows will be tried, with the Fork workflow tried first.
In `PreferSingleRepository` mode, both workflows will be tried, with the single-repository workflow tried first.
In the `SingleRepositoryOnly`, only the single-repository workflow will be tried.

If NuKeeper does not find a repository to push to, it will fail to process the upstream repository.

Public open-source projects on `github.com` that allow PRs from any outside user are very unlikely to allow that outsider to push to the project's repository, and so this case usually uses the fork workflow. Contributing to an open-source project starts with forking the repo to your own github account.

Some organisations use the single-repository workflow, with all team members allowed to push to the shared repository. This is simpler in most ways.

## Limitations and warnings

NuKeeper works with github and git, no other source control systems are supported. You can however use the public `github.com`, or an internal hosted github instance by specifying its location with the `-api` option.

You will need the command line version of `dotnet` installed.

It currently only runs on windows due to using `cmd` to invoke command-line processes for `dotnet`.

For projects using `packages.config`, `NuGet.exe` no longer runs `install.ps1` and `uninstall.ps1` scripts from command line.
Those are still executed from Visual Studio, resulting in different behaviour for packages relying on this functionality.
An example of this is [StyleCop.Analyzers](https://www.nuget.org/packages/StyleCop.Analyzers/) which will not update the `<Analyzers>` node in the project file.


### Footnote

Inspired by [Greenkeeper](https://greenkeeper.io/).

