# Getting Started

## Getting NuKeeper

NuKeeper can be found[On NuGet](https://www.nuget.org/packages/NuKeeper/).

Install NuKeeper with:

`dotnet tool install --global NuKeeper`

Update NuKeeper with:

`dotnet tool update --global NuKeeper`

## Examples of using NuKeeper

### Inspect mode

Inspect the current folder

````
cd C:\code\MyApp
NuKeeper
````

produces output e.g.

````

Found 2 package updates
Total package age:
 Days: 12
 LibYears: 0.1

Microsoft.Extensions.Configuration.Json to 2.1.0 from 2.0.2 in 1 place since 13 days ago.
Swashbuckle.AspNetCore to 2.5.0 from 2.4.0 in 1 place since 6 days ago.
````

Inspect a different folder

````
NuKeeper dir=C:\code\MyApp
````

### Update Mode

Apply a selected update to a solution:
````
cd C:\code\MyApp
NuKeeper mode=update
````
or


````
NuKeeper mode=update dir=C:\code\MyApp
````

### Repository mode

In order to work with github repositories, you will first need [a GitHub personal access token](https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/)

````
NuKeeper mode=repository github_repository_uri=https://github.com/NuKeeperDotNet/NuKeeper.git  t=mygithubtoken
````

#### Hidden token

It is often the case that you don't want to put the github token on the command line, e.g. if this command line is itself stored in a public repository. In that case, you can put it in the environment variable `NuKeeper_github_token` and NuKeeper will autopmatically read it from there.

````
set NuKeeper_github_token=mygithubtoken
````

#### Custom github api server

If you have a a different github server, e.g. one hosted inside your organisation, you need to find out where its api root url is. You can specify it to Nukeeper with e.g. `api=https://github.mycompany.com/api/v3`. The default is `https://api.github.com`

#### Custom nuget feeds

If you have an internal nuget package feed, then you can specify a list of nuget sources on the command line. e.g. `sources=https://api.nuget.org/v3/index.json;http://packages.mycompany.com/nugetfeed`

If you override this, you can shose to include the public `api.nuget.org` feed or not. You may not need to, if your internal feed proxies this.

### Organisation mode

````
NuKeeper  mode=org org=myteam
````
