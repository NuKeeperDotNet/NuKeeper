﻿using System.Linq;
using System.Threading.Tasks;
using NuGet.Packaging.Core;

namespace NuKeeper.NuGet.Api
{
    public class ApiPackageLookup : IApiPackageLookup
    {
        private readonly IPackageVersionsLookup _packageVersionsLookup;

        public ApiPackageLookup(IPackageVersionsLookup packageVersionsLookup)
        {
            _packageVersionsLookup = packageVersionsLookup;
        }

        public async Task<PackageSearchMedatadataWithSource> FindVersionUpdate(
            PackageIdentity package, VersionChange allowedChange)
        {
            var versions = await _packageVersionsLookup.Lookup(package.Id);
            return versions
                .OrderByDescending(p => p.Identity.Version)
                .FirstOrDefault();
        }
    }
}