using System;
using System.Collections.Generic;
using Microsoft.FSharp.Core;
using NuKeeper.Abstractions.Inspections.Files;
using Paket;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public class PaketRepositoryScanner : IRepositoryScanner
    {
        private readonly IReadOnlyCollection<IPackageReferenceFinder> _finders;

        public PaketRepositoryScanner(ProjectFileReader projectFileReader, PackagesFileReader packagesFileReader,
            NuspecFileReader nuspecFileReader, DirectoryBuildTargetsReader directoryBuildTargetsReader)
        {
            _finders = new IPackageReferenceFinder[]
                {projectFileReader, packagesFileReader, nuspecFileReader, directoryBuildTargetsReader};
        }

        public IReadOnlyCollection<PackageInProject> FindAllNuGetPackages(IFolder workingFolder)
        {
            var deps = new Dependencies($"{workingFolder.FullPath}/paket.dependencies");
            try
            {
                var outdated = deps.FindOutdated(false, false, false, FSharpOption<string>.None); //todo see if we can get the current version list immeditaily
                var packages = new List<PackageInProject>();

                foreach (var package in outdated)
                {

                    var path = new PackagePath(workingFolder.FullPath, "paket.dependencies",
                        PackageReferenceType.PaketDependencyFile);

                    var installedVersion = deps.GetInstalledVersion(FSharpOption<string>.None, package.Item2);
                    var packageInfo = new PackageInProject(package.Item2, installedVersion.Value, path);

                    packages.Add(packageInfo);
                }

                return packages;
            }
            catch (Exception e)
            {
                var ex = e.Message;
                return null;
            }
        }
    }
}
