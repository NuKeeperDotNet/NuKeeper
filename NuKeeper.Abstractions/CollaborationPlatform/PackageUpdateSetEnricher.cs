using NuGet.Packaging.Core;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.NuGetApi;
using NuKeeper.Abstractions.RepositoryInspection;
using System;
using System.Linq;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public class PackageUpdateSetEnricher : IEnrichContext<PackageUpdateSet, UpdateMessageTemplate>
    {
        public void Enrich(PackageUpdateSet source, UpdateMessageTemplate template)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (template == null) throw new ArgumentNullException(nameof(template));

            var package = new PackageTemplate
            {
                Name = source.SelectedId,
                Version = source.SelectedVersion.ToNormalizedString(),
                AllowedChange = source.AllowedChange.ToString(),
                ActualChange = source.ActualChange.ToString(),
                Publication = GetPublicationTemplate(source.Selected),
                Url = source.Selected.Url?.ToString(),
                SourceUrl = source.Selected.Source.SourceUri?.ToString(),
                IsFromNuget = SourceIsPublicNuget(source.Selected.Source.SourceUri),
                Updates = GetUpdates(source)
            };

            if (source.HigherVersionAvailable)
            {
                package.LatestVersion = new LatestPackageTemplate
                {
                    Version = source.HighestVersion?.ToNormalizedString(),
                    Publication = GetPublicationTemplate(source.Highest),
                    Url = source.Highest?.Url?.ToString()
                };
            }

            template.Packages.Add(package);

            template.Footer = new FooterTemplate
            {
                WarningMessage = "This is an automated update. Merge only if it passes tests",
                NuKeeperUrl = "https://github.com/NuKeeperDotNet/NuKeeper"
            };
        }

        private static PublicationTemplate GetPublicationTemplate(
            PackageSearchMetadata metadata
        )
        {
            if (metadata == null) return null;
            if (metadata.Published == null) return null;

            return new PublicationTemplate
            {
                Date = DateFormat.AsUtcIso8601(metadata.Published),
                Ago = TimeSpanFormat.Ago(
                    metadata.Published.Value.UtcDateTime,
                    DateTime.UtcNow
               )
            };
        }

        private static UpdateTemplate[] GetUpdates(
            PackageUpdateSet source
        )
        {
            return source.CurrentPackages
                .Select(p =>
                    {
                        return new UpdateTemplate
                        {
                            SourceFilePath = p.Path.RelativePath,
                            ToVersion = source.SelectedVersion.ToNormalizedString(),
                            FromVersion = p.Version.ToNormalizedString(),
                            FromUrl = SourceIsPublicNuget(source.Selected.Source.SourceUri) ?
                                NuGetVersionPackageLink(p.Identity)
                                : ""
                        };
                    }
                ).ToArray();
        }

        private static bool SourceIsPublicNuget(Uri sourceUrl)
        {
            return
                sourceUrl != null &&
                sourceUrl.ToString().StartsWith("https://api.nuget.org/", StringComparison.OrdinalIgnoreCase);
        }

        private static string NuGetVersionPackageLink(PackageIdentity package)
        {
            return $"https://www.nuget.org/packages/{package.Id}/{package.Version}";
        }
    }
}
