using System.Collections.Generic;
using NuKeeper.Abstractions.CollaborationPlatform;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsCommitWorder : ICommitWorder
    {
        // Azure DevOps allows a maximum of 4000 characters to be used in a pull request description:
        // https://visualstudio.uservoice.com/forums/330519-azure-devops-formerly-visual-studio-team-services/suggestions/20217283-raise-the-character-limit-for-pull-request-descrip
        private const int MaxCharacterCount = 4000;

        public string MakeCommitDetails(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            // TODO: Implement function for Azure DevOps.
            throw new System.NotImplementedException();
        }

        public string MakeCommitMessage(PackageUpdateSet updates)
        {
            // TODO: Implement function for Azure DevOps.
            throw new System.NotImplementedException();
        }

        public string MakePullRequestTitle(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            // TODO: Implement function for Azure DevOps.
            throw new System.NotImplementedException();
        }
    }
}
