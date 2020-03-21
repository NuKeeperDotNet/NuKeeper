---
title: "Organisation"
---

{{% notice info %}}
This command is only available for github and Azure DevOps
{{% /notice %}}

Use the organisation command to raise multiple pull requests against multiple GitHub repositories within the same [GitHub Organisation](https://help.github.com/articles/about-organizations/) or within the same Azure DevOps project.

[Github](https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/)
```sh
nukeeper org myteam token
```

[Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=vsts)
```sh
nukeeper org project token --include PackageToUpdate --api https://dev.azure.com/organization
```

