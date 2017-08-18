using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace NuKeeper.NuGet.Api
{
    public class PackageSearchMedatadataWithSource : IPackageSearchMetadata
    {
        private readonly IPackageSearchMetadata _originalMetadata;

        public PackageSearchMedatadataWithSource(string source, IPackageSearchMetadata originalMetadata)
        {
            _originalMetadata = originalMetadata;
            Source = source;
        }

        public string Source { get; }

        public async Task<IEnumerable<VersionInfo>> GetVersionsAsync()
        {
            return await _originalMetadata.GetVersionsAsync();
        }

        public string Authors => _originalMetadata.Authors;

        public IEnumerable<PackageDependencyGroup> DependencySets => _originalMetadata.DependencySets;

        public string Description => _originalMetadata.Description;

        public long? DownloadCount => _originalMetadata.DownloadCount;

        public Uri IconUrl => _originalMetadata.IconUrl;

        public PackageIdentity Identity => _originalMetadata.Identity;

        public Uri LicenseUrl => _originalMetadata.LicenseUrl;

        public Uri ProjectUrl => _originalMetadata.ProjectUrl;

        public Uri ReportAbuseUrl => _originalMetadata.ReportAbuseUrl;

        public DateTimeOffset? Published => _originalMetadata.Published;

        public string Owners => _originalMetadata.Owners;

        public bool RequireLicenseAcceptance => _originalMetadata.RequireLicenseAcceptance;

        public string Summary => _originalMetadata.Summary;

        public string Tags => _originalMetadata.Tags;

        public string Title => _originalMetadata.Title;

        public bool IsListed => _originalMetadata.IsListed;
    }
}