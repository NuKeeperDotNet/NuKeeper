<p align="center"><img src="./assets/NuKeeperTopBar.jpg"></p>

[![Build Status](https://dev.azure.com/nukeeper/NuKeeper/_apis/build/status/PR%20build?branchName=master&label=build)](https://dev.azure.com/nukeeper/NuKeeper/_build/latest?definitionId=3?branchName=master)
[![Gitter](https://img.shields.io/gitter/room/NuKeeperDotNet/Lobby.js.svg?maxAge=2592000)](https://gitter.im/NuKeeperDotNet/Lobby)
[![NuGet](https://img.shields.io/nuget/v/NuKeeper.svg?maxAge=3600)](https://www.nuget.org/packages/NuKeeper/)
[![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/nukeeper/NuKeeper/4.svg)](https://dev.azure.com/nukeeper/NuKeeper/_build?definitionId=4)   
<p align="center">
  <a href="https://github.com/NuKeeperDotNet/NuKeeper/wiki">NuKeeper Wiki Home</a> •
  <a href="https://github.com/NuKeeperDotNet/NuKeeper/wiki/Getting-Started#getting-nukeeper">Installation</a> •
  <a href="https://github.com/NuKeeperDotNet/NuKeeper/wiki/Getting-Started#using-nukeeper">Getting started</a> •
  <a href="https://github.com/NuKeeperDotNet/NuKeeper/wiki/Recipes">Recipes</a> •
  <a href="https://github.com/NuKeeperDotNet/NuKeeper/wiki/Debugging-NuKeeper">Debugging</a> •
</p>

### NuKeeper

Automagically update NuGet packages in all .NET projects.

### Installation

Installation is very easy. Just run this command and the tool will be installed. 

Install: `dotnet tool install nukeeper --global`

### Platform support

NuKeeper works for .NET Framework and for .NET Core projects. It also has the ability to target private NuGet feeds.

| .NET Framework     |     .NET Core      |    Private Nuget Feed    |
|:------------------:|:------------------:|:------------------------:| 
| :heavy_check_mark: | :heavy_check_mark: |     :heavy_check_mark:   |

Integration with the following platforms is supported.

|     Github         |     AzureDevOps    |      BitBucket     |       GitLab        |
|:------------------:|:------------------:|:------------------:| :------------------:| 
| :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark:  |

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
