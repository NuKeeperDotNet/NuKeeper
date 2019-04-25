---
title: "Bitbucket"
---

NuKeeper has BitBucket support. This means NuKeeper can create PR's to a *single* bitbucket repository using the repository command and a app password.

## Getting a app password

To get a app password for your *account* go to **bitbucket settings** and click *App password*. 

NuKeeper needs an *App password* to create a PR on your repository. 

1. Go to **bitbucket settings**
1. Select **App password**
1. Click the **Create app password** button
1. Give it the following access rights:
   - Pull request: write(that should also select the other applicable access rights)
1. Store the token and use that for a NuKeeper repo command

## Repo command

The repo command has two required arguments. The first one is the repository that you want to target. For BitBucket **you can use the same url that you use for cloning**. So something like:

```sh
https://{username}@bitbucket.org/{username}/{repositoryname}.git
```

Now to run the command use

```sh
nukeeper repo https://{username}@bitbucket.org/{username}/{repositoryname}.git {AppPassword}
```
Any additional arguments can be added after the app password, for more information checkout the [Configuration](/basics/configuration.md) page.