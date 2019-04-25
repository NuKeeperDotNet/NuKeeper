
# NuKeeper
Nukeeper is a tool to `automagically` update NuGet packages in all .NET projects. It has command for local usage and for making PR's to a repository on all kinds of git hosting platforms. It supports .NET framework and .NET Core.
 
### Quick install

Installation is very easy. Just run this command and the tool will be installed. 

Install: 
```sh
dotnet tool install nukeeper --global
```

### Platform support

NuKeeper works for .NET Framework and for .NET Core projects. It also has the ability to target private NuGet feeds.

| .NET Framework     |     .NET Core      |    Private Nuget Feed    |
|:------------------:|:------------------:|:------------------------:| 
| :heavy_check_mark: | :heavy_check_mark: |     :heavy_check_mark:   |

Integration with the following platforms is supported.

|     Github         |     AzureDevOps    |      BitBucket     |       GitLab        |       Gitea         |
|:------------------:|:------------------:|:------------------:| :------------------:| :------------------:|
| :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark:  | :heavy_check_mark:  |

### Commands

NuKeeper has different commands and options which can be utilized. Below you'll find a summary of all the commands and what they do.

```bash
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
