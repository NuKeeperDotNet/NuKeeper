using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Inspection
{
    public interface IUpdateFinder
    {
        /// <summary>
        /// Finds the package update sets that are used
        /// </summary>
        /// <param name="workingFolder"></param>
        /// <param name="sources"></param>
        /// <param name="allowedChange"></param>
        /// <param name="usePrerelease"></param>
        /// <param name="throwOnGitError">Should an GIT error be thrown as Exception?</param>
        /// <param name="include">Optional, for no include pass null</param>
        /// <param name="exclude">Optional, for no exclude pass null</param>
        /// <returns></returns>
        Task<IReadOnlyCollection<PackageUpdateSet>> FindPackageUpdateSets(
            IFolder workingFolder,
            NuGetSources sources,
            VersionChange allowedChange,
            UsePrerelease usePrerelease,
            bool throwOnGitError,
            Regex include = null,
            Regex exclude = null);
    }
}
