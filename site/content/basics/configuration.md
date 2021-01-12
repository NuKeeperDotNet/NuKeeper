
---
title: "Configuration"
---


| Long name        | Short name | Commands where it applies | Default value          |
|------------------|-----------|---------------------------|-------------------------|
| help             | h         | _all_                     |                         |
| age              | a         | _all_                     | 7d                      |
| change           | c         | _all_                     | Major                   |
| useprerelease    |           | _all_                     | FromPrerelease          |
| exclude          | e         | _all_                     | _null_                  |
| include          | i         | _all_                     | _null_                  |
| source           | s         | _all_                     |[NuGet.org public api url](https://api.nuget.org/v3/index.json)|
|                  |           |                           |                         |
| verbosity        | v         | _all_                     | Normal                  |
| logdestination   |           | _all_                     | Console                 |
| logfile          |           | _all_                     | 'nukeeper.log'          |
| ouputformat      |           | _all_                     | Text                    |
| ouputdestination |           | _all_                     | Console                 |
| ouputfile        |           | _all_                     | 'nukeeper.out'          |
|                  |           |                           |                         |
| api              | g         | `org`, `global`           |[GitHub.com public API Uri](https://api.github.com/)|
| api              | g         | `repo`                    | Depends on repository Uri|
| fork             | f         | `repo`, `org`, `global`   | PreferFork (Github) <br/> SingleRepository (AzureDevOps) |
| fork             | f         | `repo`, `org`, `global`   | PreferFork              |
| label            | l         | `repo`, `org`, `global`   | 'nukeeper'              |
| maxpackageupdates| m         | `repo`, `org`, `global`, `update`| 3, or when the command is `update`, 1 |
| maxopenpullrequests |           | `repo`, `org`, `global`| 1 if `consolidate`, else `maxpackageupdates` |
| consolidate      | n         | `repo`, `org`, `global`   | false                   |
| platform         |           | `repo`, `org`, `global`   | _null_                  |
| gitclipath       | git       | `repo`, `org`, `global`   | _null_ (use default Lib2Git-Implementation)                  |
|                  |           |                           |                         |
| maxrepo          |           | `org`, `global`           | 10                      |
| includerepos     |           | `org`, `global`           | _null_                  |
| excluderepos     |           | `org`, `global`           | _null_                  |
|                  |           |                           |                         |
| branchnametemplate |           | `repo`, `org`, `global`, `update`| _null_           |
| deletebranchaftermerge | d   | _all_                     | true                    |
| setautomerge |    | `repo`                     | false                    |

* *age* The minimum package age. In order to not consume packages immediately after they are released, exclude updates that do not meet a minimum age.  The default is 7 days. This age is the duration between the published date of the selected package update and now.
 A value can be expressed in command options as an integer and a unit suffix,
where the unit is one of `h` for hour, `d` for days, `w` for weeks. A zero with no unit is also allowed.
Examples: `0` = zero, `12h` = 12 hours, `3d` = 3 days, `2w` = two weeks.

* *change* The greatest level of update that is allowed to packages, based on the version number difference. Values are:  `Major`, `Minor`, `Patch`.
 See [Semver](http://semver.org/) for what these mean.
 The default value `Major` will allow updates to the overall latest version even if it means accepting a new major version.

  For example, if you are currently using package `Foo` at version `1.2.3` and these new versions are available: `1.2.4` - a patch version change, `1.3.0` - a minor version change and `2.0.0` - a new major version.
  * If the allowed version change is `Major` (the default) you will get an update to the overall latest version, i.e. `2.0.0`.
  * If you set the allowed version change to `Minor`, you will get an update to `1.3.0` as now changes to the major version number are not allowed. Version `1.2.4` is also allowed, but the largest allowed update is applied.
  * If the allowed version change is `Patch` you will only get an update to version `1.2.4`.

* *UsePrerelease* Should NuKeeper suggest updates to a pre-release (beta) package version. Values are `Always`, `Never`, `FromPrerelease`. The default is `FromPrerelease`, meaning that only a package that is currently used at a pre-release version will be updated to a later pre-release version.

* *exclude* Do not consider packages matching this regex pattern.
* *include* Only consider packages matching this regex pattern.
* *source* Specifies a NuGet package source to use during the operation. This setting overrides all of the sources specified in the `NuGet.config` files. Multiple sources can be provided by specifying this option multiple times.

* *verbosity*. Controls how much log data is produced. Values are, from least output to most output: `Quiet`, `Minimal`, `Normal`, `Detailed`. `Quiet` should produce no output unless there is an error, and `Detailed` is suitable for debugging issues.  You can also use short names for these log levels: `Q`, `M`, `N`, `D`.
* *logdestination*: Where log data is sent: One of: `Console`, `File`, `Off`. Default is `Console`.
* *logfile* when set, log data will be appended to the named file instead of going to console.
* *outputformat*. Format for output. One of: `None`, `Text`, `Csv`, `Metrics`,`Libyears`. Default is `Text`.
* *outputdestination*: Where output is sent: One of: `Console`, `File`, `Off`. Default is `Console`.
* *outputfile* : Name of file to send output to. Used when output destination is `File`. default is `nukeeper.out`.

* *api* This is the api endpoint for the instance you're targeting. If you are using an internal server and not a public one, you must set it to the api url for your server. The value will be e.g. `https://github.mycompany.com/api/v3`.
* *fork* Values are `PreferFork`, `PreferSingleRepository` and `SingleRepositoryOnly`. Prefer to make branches on a fork of the target repository, or on that repository itself. See the section "Branches, forks and pull requests" below.
* *label* Label to apply to GitHub pull requests. Can be specified multiple times.
* *maxpackageupdates* The maximum number of package updates to apply. In `repo`,`org` and `global` commands, this limits the number of updates per repository. If the `--consolidate` flag is used, these wll be consolidated into one Pull Request. If not, then there will be one Pull Request per update applied. In the `update` command, The default value is 1. When changed, multiple updates can be applied in a single `update` run, up to this number.
* *maxopenpullrequests* The maximum number of pull requests that can be active in one repository at a time. If the `--consolidate` flag is used, the default value is 1. If not, then the default will be `maxpackageupdates`. This currently only works for AzureDevops/TFS.

  To be able to figure out the number of active pull requests, different strategies are used:

  1. First, only pull requests created by the identity linked to the PAT are considered, however this is only possible for Azure Devops Services, and not the on-premise version, as it does not provide any (straightforward) API to be able to figure this out. The cloud version does provide an API that should allow us to figure out the current account, but this requires a token with a scope that can read identities/account information.
  2. If no account or identity can be determined based on the PAT. Then, currently a hardcoded, user `nukeeper@bot.com` is considered. Again this requires a broader scope.
  3. If no user identity was able to be fetched, then it will fetch all active PRs and consider only those with labels `nukeeper` attached to it.

* *maxrepo* The maximum number of repositories to change. Used in Organisation and Global mode.
* *consolidate* Consolidate updates into a single pull request, instead of the default of 1 pull request per package update applied.
* *platform* One of `GitHub`, `AzureDevOps`, `Bitbucket`, `BitbucketLocal`, `Gitlab`, `Gitea`. Determines which kind of source control api will be used. This is typicaly infered from the api url structure, but since this does not always work, it can be specified here if neccessary.
* *gitclipath* Path to `git` command line. Alternative use native git via cli instead of the default lib2gitsharp implementation. In special cases lib2gitsharp is not enough. E.g. in enviroments with company-proxies or self hosted git services signed certificates. For Windows it is typically `C:\Program Files\Git\bin\git.exe`. For Linux it is typically `/usr/bin/git`. 

* *includerepos* A regex to filter repositories by name. Only consider repositories where the name matches this regex pattern. Used in Organisation and Global mode.
* *excluderepos* A regex to filter repositories by name. Do not consider repositories where the name matches this regex pattern. Used in Organisation and Global mode.

* *branchnametemplate* A template that can be used to set the name of the branch NuKeeper creates. Allows you to put those branches in a hierarchy or set your own naming strategy.
  It accepts the following tokens:
  
  |Token       |Replacement                                                                      |
  |------------|---------------------------------------------------------------------------------|
  |`{Default}` |The default Nukeeper Branch Naming                                               |
  |`{Name}`    |The name of the updated Nuget or `Multiple-Packages` if more than one            |
  |`{Version}` |The version of the updated Nuget or `Multiple-Versions` if more than one version |
  |`{Count}`   |The number of updated Nugets                                                     |
  |`{Hash}`    |An Unique hash over the updated nugets.                                          |
  
  The default nukeeper branch naming is:
  
  | | |
  |-|-|
  |Single Nuget   |`nukeeper-update-{Name}-to-{Version}`
  |Multiple Nugets|`nukeeper-update-{Count}-packages-{Hash}`
  
  Note: The old and replaced option `branchnameprefix` can be simulated with `--branchnametemplate "YourBranchPrefix/{Default}"`
  
* *deletebranchaftermerge* Specifies whether a branch should be automatically deleted or not once the branch has been merged. Currently only works with `Platform` equal to `AzureDevOps`, `Gitlab` or `Bitbucket`.
* *setautomerge* Specifies whether a pull request should be merged automatically after passing all checks. Currently only works with `Platform` equal to `AzureDevOps`.
