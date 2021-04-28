using NuKeeper.Abstractions.RepositoryInspection;
using System.Collections.Generic;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface ICommitWorder
    {
        string MakePullRequestTitle(IReadOnlyCollection<PackageUpdateSet> updates);

        string MakePullRequestTitle(IReadOnlyCollection<PackageUpdateSet> updates, string userSettingsPullRequestNameTemplate);

        string MakeCommitMessage(PackageUpdateSet updates);

        string MakeCommitDetails(IReadOnlyCollection<PackageUpdateSet> updates);
    }
}
