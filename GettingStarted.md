# Getting Started

## Getting NuKeeper

NuKeeper can be [found on NuGet as a global tool](https://www.nuget.org/packages/NuKeeper/). The [source is on GitHub](https://github.com/NuKeeperDotNet/NuKeeper).

Install NuKeeper with:

`dotnet tool install --global NuKeeper`

Update NuKeeper with:

`dotnet tool update --global NuKeeper`

## Examples of using NuKeeper

### Inspect mode

Use inspect mode to find updates that can be applied to a solution.

Inspect the current folder:

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

Use update mode to apply an update to code.

Apply a update chosen by NuKeeper to a solution:
````
cd C:\code\MyApp
NuKeeper mode=update
````
or


````
NuKeeper mode=update dir=C:\code\MyApp
````

Apply an update to a particular package:
````
NuKeeper mode=update i=SomePackageName
````

The `include` option actually is a Regular Expression, which by default will match any text that includes that substring. e.g. `i=Newt` matches the package `Newtonsoft.Json`.


### Repository mode

Use repository mode to raise multiple pull requests against a github repository. The repository does not need to present on the file system beforehand. It will be fetched to a temporary folder.

If you run these commandlines regularly, you can automatically get update pull requests [like this one](https://github.com/NuKeeperDotNet/NuKeeper/pull/280).


In order to work with github repositories, you will first need [a GitHub personal access token](https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/)

````
NuKeeper mode=repository github_repository_uri=https://github.com/NuKeeperDotNet/NuKeeper.git t=mygithubtoken
````

#### Hidden token

The github token is a secret; often you don't want to put it on the command line, e.g. if this command line is in a script stored in a public repository. In that case, you can put it in the environment variable `NuKeeper_github_token` and NuKeeper will automatically read it from there.

````
set NuKeeper_github_token=mygithubtoken
````

#### Custom github api server

If you have a a different github server, e.g. one hosted inside your organisation, you need to find out where its api root url is. You can specify it to Nukeeper with e.g. `api=https://github.mycompany.com/api/v3`. The default is `https://api.github.com`

#### Custom nuget feeds

If you have an internal nuget package feed, then you can specify a list of nuget sources on the command line. e.g. `sources=https://api.nuget.org/v3/index.json;http://packages.mycompany.com/nugetfeed`

If you override this, you can shose to include the public `api.nuget.org` feed or not. You may not need to, if your internal feed proxies this.

#### Options for controlling pull requests

todo

### Organisation mode

Use organisation mode to raise multiple PRs against multiple github repositories.

````
NuKeeper  mode=org org=myteam
````

---

For all possible options and modes, see [The ReadMe](README.md)
