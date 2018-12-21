using NuKeeper.Inspection.RepositoryInspection;
using System.Collections.Generic;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface ICommitWorder
    {
        string MakePullRequestTitle(IReadOnlyCollection<PackageUpdateSet> updates);

        string MakeCommitMessage(PackageUpdateSet updates);

        string MakeCommitDetails(IReadOnlyCollection<PackageUpdateSet> updates);
    }
}
