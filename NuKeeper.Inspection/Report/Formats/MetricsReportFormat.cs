using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;
using NuKeeper.Abstractions.NuGetApi;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Inspection.Report.Formats
{
    public class MetricsReportFormat : IReportFormat
    {
        private readonly IReportWriter _writer;

        public MetricsReportFormat(IReportWriter writer)
        {
            _writer = writer;
        }

        public void Write(string name, IReadOnlyCollection<PackageUpdateSet> updates)
        {
            if (updates == null)
            {
                throw new ArgumentNullException(nameof(updates));
            }

            _writer.WriteLine($"Packages with updates: {updates.Count}");
            WriteMajorMinorPatchCount(updates);
            WriteProjectCount(updates);
            WriteLibYears(updates);
        }

        private void WriteMajorMinorPatchCount(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var majors = 0;
            var minors = 0;
            var patches = 0;
            foreach (var update in updates)
            {
                var baselineVersion = MinCurrentVersion(update);

                var majorUpdate = FilteredPackageVersion(baselineVersion, update.Packages.Major);
                var minorUpdate = FilteredPackageVersion(baselineVersion, update.Packages.Minor);
                var patchUpdate = FilteredPackageVersion(baselineVersion, update.Packages.Patch);

                if (majorUpdate != null && majorUpdate.Identity.Version.Major > baselineVersion.Major)
                {
                    majors++;
                }
                if (minorUpdate != null && minorUpdate.Identity.Version.Minor > baselineVersion.Minor)
                {
                    minors++;
                }

                if (patchUpdate != null)
                {
                    patches++;
                }
            }

            _writer.WriteLine($"Packages with Major version updates: {majors}");
            _writer.WriteLine($"Packages with Minor version updates: {minors}");
            _writer.WriteLine($"Packages with Patch version updates: {patches}");
        }

        private static NuGetVersion MinCurrentVersion(PackageUpdateSet updates)
        {
            return updates.CurrentPackages
                .Select(p => p.Version)
                .Min();
        }

        private void WriteProjectCount(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var currentPackagesInProjects = updates
                .SelectMany(p => p.CurrentPackages)
                .ToList();

            var projectCount = currentPackagesInProjects
                .Select(c => c.Path.FullName)
                .Distinct()
                .Count();

            _writer.WriteLine($"Projects with updates: {projectCount}");
            _writer.WriteLine($"Updates in projects: {currentPackagesInProjects.Count}");

        }

        private void WriteLibYears(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var totalAge = Age.Sum(updates);
            var displayValue = Age.AsLibYears(totalAge);
            _writer.WriteLine($"LibYears: {displayValue}");
        }

        private static PackageSearchMetadata FilteredPackageVersion(NuGetVersion baseline, PackageSearchMetadata packageVersion)
        {
            if (packageVersion == null)
            {
                return null;
            }

            if (packageVersion.Identity.Version <= baseline)
            {
                return null;
            }

            return packageVersion;
        }

    }
}
