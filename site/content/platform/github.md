---
title: "Github"
---

NuKeeper has extended Github support. This means NuKeeper can create PR's to a *single* github repository, update all repositories within a whole organization or on an entire Github server. The only requirement is that we have a valid personal access token(PAT).

## Getting a token

To get a token for your *account* go to [**settings -> developer settings -> personal access token**](https://github.com/settings/tokens) and click *Generate new token*. 

1. Go to **your github settings**
1. Click **developer settings**
1. Click the **personal access token** button
1. Give it the following access rights:
  - Repo command: 
    - [x] Repo
    - [x] user:email
  - Organisation command and Global command:
    - [x] Admin:org (read)
    - [x] Admin:public_key (read)  
1. Store the token and use that for the commands

## Repo command

The repo command has two required arguments. The first one is the repository that you want to target. For github **you can use the same url that you use for cloning**. So something like:

For your own repositories:

```sh
https://github.com/{username}/{repositoryname}.git
```

For organisation repositories:

```sh
https://github.com/{organisation}/{repositoryname}.git
```


Now to run the command use

```sh
nukeeper repo https://github.com/{username}/{repositoryname}.git {PAT}
```
Any additional arguments can be added after the app password, for more information checkout the [Configuration](/basics/configuration.md) page.


## Organisation command

Use the organisation command to raise multiple pull requests against multiple GitHub repositories within the same [GitHub Organisation](https://help.github.com/articles/about-organizations/).

```sh
nukeeper org myteam mygithubtoken
```


## Global command

Use the global command to update a particular package across your entire github server.

```sh
nukeeper global mygithubtoken --include PackageToUpdate --api https://github.mycompany.com/api/v3
```
