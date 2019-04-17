using System.Collections.Generic;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public class PackageInProjectReader
    {
        private readonly INuKeeperLogger _logger;

        public PackageInProjectReader(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public PackageInProject Read(
            string id, string version,
            PackagePath path,
            IEnumerable<string> projectReferences)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.Normal($"Skipping package with no id specified in file '{path.FullName}'.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                _logger.Normal($"Skipping package '{id}' with no version specified in file '{path.FullName}'.");
                return null;
            }

            var packageVersionRange = PackageVersionRange.Parse(id, version);

            if (packageVersionRange == null)
            {
                _logger.Normal($"Skipping package '{id}' with version '{version}' that could not be parsed in file '{path.FullName}'.");
                return null;
            }

            var pip = new PackageInProject(packageVersionRange, path, projectReferences);

            var singleVersion = pip.Identity;

            if (singleVersion == null)
            {
                _logger.Normal($"Skipping package '{id}' with version range '{version}' that is not a single version in file '{path.FullName}'.");
                return null;
            }

            return pip;
        }
    }
}
