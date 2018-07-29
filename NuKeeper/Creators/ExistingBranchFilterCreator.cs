using NuKeeper.Configuration;
using NuKeeper.Engine.Packages;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Creators
{
    public class ExistingBranchFilterCreator : ICreate<IExistingBranchFilter>
    {
        private readonly ICreate<IGitHub> _githubCreator;
        private readonly INuKeeperLogger _logger;

        public ExistingBranchFilterCreator(ICreate<IGitHub> githubCreator, INuKeeperLogger logger)
        {
            _githubCreator = githubCreator;
            _logger = logger;
        }

        public IExistingBranchFilter Create(SettingsContainer settings)
        {
            return new ExistingBranchFilter(_githubCreator.Create(settings), _logger);
        }
    }
}
