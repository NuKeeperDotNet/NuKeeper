---
title: "Installation"
weight: 10
---


You will first need [.NET Core 2.1 or later installed](https://www.microsoft.com/net).

NuKeeper [can be found on NuGet as a global tool](https://www.nuget.org/packages/NuKeeper/). The [source is on GitHub](https://github.com/NuKeeperDotNet/NuKeeper).

Install NuKeeper with:

`dotnet tool install nukeeper --global`

Update NuKeeper with:

`dotnet tool update nukeeper --global`

# Using NuKeeper

Running `nukeeper` with no arguments shows the help.

```bat
nukeeper --help
```

Help on a NuKeeper command

```bat
nukeeper repo --help
```

## Update command


### Custom NuGet feeds

If you have a custom or private NuGet feed, it is recommended that you use [a `NuGet.config` file](https://docs.microsoft.com/en-us/nuget/reference/nuget-config-file#packagesources) in your repository. It will be used by NuKeeper, and by other tools. You can specify a list of NuGet sources on the command line. e.g.

```bat
nukeeper update --source https://api.nuget.org/v3/index.json --source http://packages.mycompany.com/nugetfeed
```

If you override this, you can chose to include the global public `api.nuget.org` feed or not. You may not need to, if your internal feed proxies the nuget.org feed.

The order of precedence is:
 1) sources on command line
 2) `NuGet.config` file
 3) Default public NuGet.org feed



## Global command

|     Github         |     AzureDevOps    |      BitBucket     |       GitLab        |
|:------------------:|:------------------:|:------------------:| :------------------:|
| :heavy_check_mark: | :x: |        :x:         |         :x:         |

Use the global command to update a particular package across your entire github server.

```bat
nukeeper global mygithubtoken --include PackageToUpdate --api https://github.mycompany.com/api/v3
```

## Output and debug logging

To make logging show more details.

```bat
nukeeper inspect C:\code\MyApp --verbosity detailed
```

Send debug logs to a file instead of console.

```bat
nukeeper inspect C:\code\MyApp --logfile somefile.log
```

To turn off logging entirely.

```bat
nukeeper inspect C:\code\MyApp --logdestination off
```

Code metrics as the only output

```bat
nukeeper inspect C:\code\MyApp --logdestination off --outputformat metrics
```

Report updates in Comma-separated value format, to file as output.

```bat
nukeeper inspect nukeeper inspect C:\code\MyApp --outputformat csv --outputfile packages.csv
```

Run Nukeeper on a remote repository, and just generate project metrics, no Pull Requests.

```bat
nukeeper repo https://github.com/myorg/myrepo mygithubtoken --maxpackageupdates 0 --logdestination off --outputformat metrics
```

Run NuKeeper on a remote repository without any logging or output at all, just Pull Requests generated

```bat
 nukeeper repo https://github.com/myorg/myrepo mygithubtoken --logdestination off --outputdestination off
```

---

For all possible options and modes, see [Configuration](Configuration)
