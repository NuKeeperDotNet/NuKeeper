using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.RepositoryInspection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuKeeper.Engine.Packages
{
    public class ExistingCommitFilter : IExistingCommitFilter
    {
        private readonly ICollaborationFactory _collaborationFactory;
        private readonly INuKeeperLogger _logger;

        public ExistingCommitFilter(ICollaborationFactory collaborationFactory, INuKeeperLogger logger)
        {
            _collaborationFactory = collaborationFactory;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<PackageUpdateSet>> Filter(IGitDriver git, IReadOnlyCollection<PackageUpdateSet> updates, string baseBranch, string headBranch, string commitMessagePrefix)
        {
            if (git == null)
            {
                throw new ArgumentNullException(nameof(git));
            }

            if (updates == null)
            {
                throw new ArgumentNullException(nameof(updates));
            }

            try
            {
                var filtered = new List<PackageUpdateSet>();
                // commit messages are compared without whitespace because the system tends to add ws.
                var commitMessages = await git.GetNewCommitMessages(baseBranch, headBranch);
                var compactCommitMessages = commitMessages.Select(m => new string(m.Where(c => !char.IsWhiteSpace(c)).ToArray()));

                foreach (var update in updates)
                {
                    var updateCommitMessage = _collaborationFactory.CommitWorder.MakeCommitMessage(update, commitMessagePrefix);
                    var compactUpdateCommitMessage = new string(updateCommitMessage.Where(c => !char.IsWhiteSpace(c)).ToArray());

                    if (!compactCommitMessages.Contains(compactUpdateCommitMessage))
                    {
                        filtered.Add(update);
                    }

                }
                return filtered;
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _logger.Error($"Failed on existing Commit check for {baseBranch} <= {headBranch}", ex);

                return updates;
            }
        }
    }
}
