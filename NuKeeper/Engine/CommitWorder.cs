using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.RepositoryInspection;
using System;
using System.Collections.Generic;

namespace NuKeeper.Engine
{
    public class CommitWorder : ICommitWorder
    {
        private readonly IEnrichContext<PackageUpdateSet, UpdateMessageTemplate> _enricher;
        private readonly IEnrichContext<IReadOnlyCollection<PackageUpdateSet>, UpdateMessageTemplate> _multiEnricher;

        public CommitWorder(
            UpdateMessageTemplate commitTemplate,
            UpdateMessageTemplate pullrequestTitleTemplate,
            UpdateMessageTemplate pullrequestBodyTemplate,
            IEnrichContext<PackageUpdateSet, UpdateMessageTemplate> enricher,
            IEnrichContext<IReadOnlyCollection<PackageUpdateSet>, UpdateMessageTemplate> multiEnricher
        )
        {
            CommitTemplate = commitTemplate;
            PullrequestTitleTemplate = pullrequestTitleTemplate;
            PullrequestBodyTemplate = pullrequestBodyTemplate;
            _enricher = enricher;
            _multiEnricher = multiEnricher;
        }

        public UpdateMessageTemplate CommitTemplate { get; }
        public UpdateMessageTemplate PullrequestTitleTemplate { get; }
        public UpdateMessageTemplate PullrequestBodyTemplate { get; }

        public string MakePullRequestTitle(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            if (updates == null)
            {
                throw new ArgumentNullException(nameof(updates));
            }

            var template = PullrequestTitleTemplate;
            template.Clear();
            _multiEnricher.Enrich(updates, template);
            var title = template.Output();
            template.Clear();
            return title;
        }

        public string MakeCommitMessage(PackageUpdateSet updates)
        {
            if (updates == null)
            {
                throw new ArgumentNullException(nameof(updates));
            }

            var template = CommitTemplate;
            template.Clear();
            _enricher.Enrich(updates, template);
            var commitMessage = template.Output();
            template.Clear();
            return commitMessage;
        }

        public string MakeCommitDetails(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            if (updates == null)
            {
                throw new ArgumentNullException(nameof(updates));
            }

            var template = PullrequestBodyTemplate;
            template.Clear();
            _multiEnricher.Enrich(updates, template);
            var body = template.Output();
            template.Clear();
            return body;
        }
    }
}
