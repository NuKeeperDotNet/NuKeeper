using NuKeeper.Configuration;
using NuKeeper.Engine.Packages;
using NuKeeper.Github;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Creators
{
    public class ExistingBranchFilterCreator : ICreate<IExistingBranchFilter>
    {
        private readonly ICreate<IGithub> _githubCreator;
        private readonly INuKeeperLogger _logger;

        public ExistingBranchFilterCreator(ICreate<IGithub> githubCreator, INuKeeperLogger logger)
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
