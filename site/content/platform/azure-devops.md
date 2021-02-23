---
title: "Azure Devops"
---

{{% notice tip %}}
NuKeeper supports Azure Devops, VSTS and TFS on premise. The same instruction apply for all these platforms!
{{% /notice %}}

NuKeeper supports integration with Azure Devops in two ways. One of them is to use the **repo**, **org** or **global** commands and the other one is through the **extension**. The benefit of the extension is that you can make a build pipeline with the extension and schedule your pipeline. This way you can automate your updating flow.

## Extension

The NuKeeper extension is available in the [marketplace](https://marketplace.visualstudio.com/items?itemName=nukeeper.nukeeper#overview). In order to install it you need to add it to your organisation click the **Get it free** button and follow the installation. Once that is done you can create a new pipeline and use the extension. 

### Installation
Azure Devops gives us the option to do it either in editor mode, or in yaml. 

#### Yaml
1. Add a new build pipeline and select yaml.
1. Add the following **yaml** file and adjust where necessary:
    ```yml
    trigger: none # don't run as CI build

    schedules:
    - cron: "0 0 * * *" # run daily at UTC midnight
      displayName: Check for updated dependencies
      branches:
        include:
        - master
      always: true # regardless of changes

    resources:
       - repo: self # point to its own repo, change it if you don't want that

    pool:
        name: Hosted VS2017 # can be anything you want

    steps:
      - task: nukeeper.nukeeper.nukeeper.NuKeeper@0
        displayName: NuKeeper
    ```    
1. Optionally add extra arguments, The extension just calls the **repo** command, so any arguments extra for your repo command can be added here.
    ```yml
    steps:
    - task: nukeeper.nukeeper.nukeeper.NuKeeper@0
    displayName: NuKeeper
    inputs:
        arguments: '-m 3 -v d'
    ```
1. Finally give the **build service user** the required *[Permissions](#permissions)* and take it for a spin by qeueing a new build!

#### Editor mode
1. Create a new build pipeline for editor mode.
1. Select your repository and select the master branch.
1. Add a new task to your pipeline and select NuKeeper.
1. Optionally, click on the task and add additional arguments to the **Arguments** field. For help on the arguments checkout the [Configuration](/basics/configuration/) page. The extension just call the **repo** command, so any arguments extra for your repo command can be added here.
1. Go to triggers and click the <i class="fas fa-plus"></i> Add button to add your schedule.
1. Enable your agent to access the oauth token:
    ![Oauth checkmark](/img/oauth_checkmark.png)

1. Finally give the **build service user** the required *[Permissions](#permissions)* and take it for a spin by qeueing a new build!
   
#### Permissions
The extensions uses the **build service user** to make a PR. By default this user is not allowed to do that. We have to give it the permissions to do so:

1. Go to "Project settings" in your AzureDevops home screen.
1. Click repositories
1. Click Git repositories
1. Click the build service User(the bottom one, something like: Project collection build service)

Give it the following rights:

- Contribute **Allow**
- Contribute to pull requests **Allow**
- Create branch **Allow**

## Repo command

The repo command works for azure-devops & vsts they same as for the other platforms. You will need a [personal access token](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops) to use the repo command.

```sh
nukeeper repo "https://dev.azure.com/{org}/{project}/_git/{repo}/" <PAT>
```

## Organisation command

Use the organisation command to raise multiple pull requests against multiple git repositories within the same Azure DevOps project.

```sh
nukeeper org project <PAT> --api https://dev.azure.com/myAzureDevOpsorganization
```

## Global command

Use the global command to update a particular package across your entire Azure DevOps organization.

```sh
nukeeper global <PAT> --api https://dev.azure.com/myAzureDevOpsorganization --include PackageToUpdate
```

### Additional arguments
Add any additional arguments that are available for the repo command

```sh
nukeeper repo "https://dev.azure.com/{org}/{project}/_git/{repo}/" <PAT> -m 10
```
The `-m 10` tells NuKeeper that it may update 10 packages. For more parameters checkout the [Configuration](/basics/configuration/) page.

#### Setting a custom limit on the number of open pull requests

You can instruct nukeeper to not create more pull requests than allowed by specifying the `--maxopenpullrequests` parameter. The strategy for figuring out how many active pull requests there are is explained in the [configuration page](/basics/configuration/).

```sh
nukeeper repo "https://dev.azure.com/{org}/{project}/_git/{repo}" <PAT> --maxopenpullrequests 10
```

