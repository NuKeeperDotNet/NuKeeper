---
title: "Gitea"
---

NuKeeper has [Gitea](https://gitea.io/en-us/) support. This means NuKeeper can create PR's to a *single* gitea repository using the repository command and an access token.

## Getting the Access Token 

To get token for your **account** go to **gitea user settings** (https://yourGiteaUrl/user/settings) and click *Applications*. 

NuKeeper needs an *Access Tokens* to create a PR on your repository. 

1. Go to **gitea user settings**
1. Select **Applications**
1. Enter **Token Name** in **Manage Access Tokens**
1. Click the **Generate Token** button
1. Store the token and use that for a NuKeeper repo command

## Repo command

The repo command has two required arguments. The first one is the repository that you want to target. For Gitea **you can use the same url that you use for cloning**. So something like:

```sh
https://{yourGiteaUrl}/{username}/{repositoryname}.git
```

Now to run the command use

```sh
nukeeper repo https://{yourGiteaUrl}/{username}/{repositoryname}.git {Token}
```
Any additional arguments can be added after the app password, for more information checkout the [Configuration](/basics/configuration.md) page.
