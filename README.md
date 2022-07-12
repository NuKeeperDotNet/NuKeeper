<p align="center"><img src="./assets/NuKeeperTopBar.jpg"></p>

[![Build Status](https://dev.azure.com/nukeeper/NuKeeper/_apis/build/status/NuKeeper%20PR%20Build?branchName=master)](https://dev.azure.com/nukeeper/NuKeeper/_build/latest?definitionId=4&branchName=master)
[![Gitter](https://img.shields.io/gitter/room/NuKeeperDotNet/Lobby.js.svg?maxAge=2592000)](https://gitter.im/NuKeeperDotNet/Lobby)
[![NuGet](https://img.shields.io/nuget/v/NuKeeper.svg?maxAge=3600)](https://www.nuget.org/packages/NuKeeper/)
[![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/nukeeper/NuKeeper/4.svg)](https://dev.azure.com/nukeeper/NuKeeper/_build?definitionId=4)   

### NuKeeper

#### [Why is NuKeeper Archived](https://github.com/NuKeeperDotNet/NuKeeper/issues/1155)

Automagically update NuGet packages in all .NET projects.

### Installation

Installation is very easy. Just run this command and the tool will be installed. 

Install: `dotnet tool install nukeeper --global`

> Note: NuKeeper has experimental support for running package updates on Linux/macOS. This functionality relies on Mono installation on local system. Please refer to https://www.mono-project.com/ for more information about how to install mono.

### Platform support

NuKeeper works for .NET Framework and for .NET Core projects. It also has the ability to target private NuGet feeds.

| .NET Framework     |     .NET Core      |    Private Nuget Feed    |
|:------------------:|:------------------:|:------------------------:| 
| :heavy_check_mark: | :heavy_check_mark: |     :heavy_check_mark:   |

Integration with the following platforms is supported.

|     Github         |     AzureDevOps    |      BitBucket     |       GitLab       |       Gitea        |
|:------------------:|:------------------:|:------------------:|:------------------:|:------------------:|
| :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: |

### Commands

NuKeeper has different commands and options which can be utilized. Below you'll find a summary of all the commands and what they do.

```
Options:
  --version     Show version information
  -?|-h|--help  Show help information

Commands:
  global        Performs version checks and generates pull requests for all repositories the provided token can access.
  inspect       Checks projects existing locally for possible updates.
  org           Performs version checks and generates pull requests for all repositories in a github organisation.
  repo          Performs version checks and generates pull requests for a single repository.
  update        Applies relevant updates to a local project.
```

[For detailed information about the commands, please check out the wiki](https://github.com/NuKeeperDotNet/NuKeeper/wiki) 

### How To Uninstall

You can uninstall the tool using the following command.

```console
dotnet tool uninstall nukeeper --global
```

### How To Build and Run From Source

You can install the nukeeper dotnet tool of current build using the `InstallNuKeeperDotNetTool` (.bat for Windows, .sh for macOS and Linux) found in the root of the repository.

>Note: this overrides your existing global installation of the NuKeeper dotnet tool.

You can build and package the tool using the following commands. The instructions assume that you are in the root of the repository.

```console
dotnet pack .\NuKeeper\NuKeeper.csproj -o ".\artifacts"
dotnet tool install nukeeper --global --add-source ".\artifacts"
nukeeper --version
```

> Note: On macOS and Linux, `.\NuKeeper\NuKeeper.csproj` and `.\artifacts` will need be switched to `./NuKeeper/NuKeeper.csproj` and `./artifacts` to accommodate for the different slash directions.

### Licensing

NuKeeper is licensed under the [Apache License](http://opensource.org/licenses/apache.html)

* Git automation by [LibGit2Sharp](https://github.com/libgit2/libgit2sharp/) licensed under MIT  
* Github client by [Octokit](https://github.com/octokit/octokit.net) licensed under MIT  
* NuGet protocol [NuGet.Protocol](https://github.com/NuGet/NuGet.Client) licensed under Apache License Version 2.0
* NuGet CommandLine [NuGet commandLine](https://github.com/NuGet/NuGet.Client) licensed under Apache License Version 2.0
* Command line parsing by [McMaster.Extensions.CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils) licensed under Apache License Version 2.0

### Acknowledgements

Logos by [area55](https://github.com/area55git), licensed under a [Creative Commons Attribution 4.0 International License](https://creativecommons.org/licenses/by/4.0/).


<p align="center">
  <img src="https://github.com/NuKeeperDotNet/NuKeeper/blob/master/assets/Footer.svg" />
</p>
