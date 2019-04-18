---
title: "Custom Feeds"
---

If you have a custom or private NuGet feed, it is recommended that you use [a `NuGet.config` file](https://docs.microsoft.com/en-us/nuget/reference/nuget-config-file#packagesources) in your repository. It will be used by NuKeeper, and by other tools. You can specify a list of NuGet sources on the command line. e.g.

```sh
nukeeper update --source https://api.nuget.org/v3/index.json --source http://packages.mycompany.com/nugetfeed
```

If you override this, you can chose to include the global public `api.nuget.org` feed or not. You may not need to, if your internal feed proxies the nuget.org feed.

The order of precedence is:
 1) sources on command line
 2) `NuGet.config` file
 3) Default public NuGet.org feed