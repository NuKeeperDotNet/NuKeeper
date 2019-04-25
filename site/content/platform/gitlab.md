---
title: "Gitlab"
---

NuKeeper has [GitLab](https://gitlab.com) support. This means NuKeeper can create PR's to a *single* GitLab repository using the repository command and an access token.

 ## Getting the Access Token 

To get an access token for your user account follow these steps:

1. Go to your user profile `https://{YOUR_GITLAB_URL}/profile`
1. Select `Access Tokens` `https://{YOUR_GITLAB_URL}/profile/personal_access_tokens`
1. Create a new `Personal Access Token`
    1. Assign a name.
    1. Select at least the following scopes for the token:
        1. `api` - Grants complete read/write access to the API, including all groups and projects.
        1. `read_user` - Grants read-only access to the authenticated user's profile through the /user API endpoint, which includes username, public email, and full name. Also grants access to read-only API endpoints under /users.
        1. `read_repository` - Grants read-only access to repositories on private projects using Git-over-HTTP (not using the API).
    1. Click `Create personal access token` button
1. Store the token and use that for a NuKeeper repo command.

 ## Repo command

 The repo command has two required arguments. The first one is the repository that you want to target. For GitLab **you can use the same url that you use for cloning**. So something like:

 ```sh
https://{YOUR_GITLAB_URL}/{PROJECT_NAME}/{REPOSITORY_NAME}.git
```

 Now to run the command use

 ```sh
nukeeper repo https://{YOUR_GITLAB_URL}/{PROJECT_NAME}/{REPOSITORY_NAME}.git {Token}
```
Any additional arguments can be added after the app password, for more information checkout the [Configuration](/basics/configuration.md) page.
