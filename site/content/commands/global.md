---
title: "Global"
---

{{% notice info %}}
This command is only available for github and Azure DevOps
{{% /notice %}}

Use the global command to update a particular package across your entire github server or Azure DevOps organization.

[Github](https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/)
```sh
nukeeper global token --include PackageToUpdate --api https://github.mycompany.com/api/v3
```

[Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=vsts)
```sh
nukeeper global token --include PackageToUpdate --api https://dev.azure.com/organization
```