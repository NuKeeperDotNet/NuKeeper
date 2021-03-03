using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.RepositoryInspection;
using System.Collections.Generic;
using System;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public class PackageUpdateSetsEnricher
        : IEnrichContext<IReadOnlyCollection<PackageUpdateSet>, UpdateMessageTemplate>
    {
        private readonly IEnrichContext<PackageUpdateSet, UpdateMessageTemplate> _enricher;

        public PackageUpdateSetsEnricher(IEnrichContext<PackageUpdateSet, UpdateMessageTemplate> enricher)
        {
            _enricher = enricher;
        }

        public void Enrich(IReadOnlyCollection<PackageUpdateSet> source, UpdateMessageTemplate template)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Count == 0) throw new ArgumentException("Update set must contain at least one update.", nameof(source));
            if (template == null) throw new ArgumentNullException(nameof(template));

            foreach (var update in source)
            {
                _enricher.Enrich(update, template);
            }
        }
    }
}
