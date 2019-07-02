using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.RepositoryInspection;
using System.Collections.Generic;

namespace NuKeeper.Engine.Packages
{
    public interface IExistingCommitFilter
    {
        IReadOnlyCollection<PackageUpdateSet> Filter(
            IGitDriver git,
            IReadOnlyCollection<PackageUpdateSet> updates,
            string baseBranch,
            string headBranch);
    }
}
