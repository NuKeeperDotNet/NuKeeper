# NuKeeper

Automagically update nuget packages in .NET projects.

Written in .Net core.

you will need commandline versions of `git` and `dotnet`.

You will need [a github api token](https://github.com/blog/1509-personal-api-tokens) with access to public repos.

## Repository mode

Will PR a asingle repository. Point it at the https url of a github repo, like this:

syntax:
```
C:\Code\NuKeeper\NuKeeper>dotnet run repository <GitToken> <RepoUrl>
```

## Organisation mode

Will discover repositories in that Organisation.

```
C:\Code\NuKeeper\NuKeeper>dotnet run organisation <GitToken> https://api.github.com/ <OrgName>
```

Inspired by [Greenkeeper](https://greenkeeper.io/).

