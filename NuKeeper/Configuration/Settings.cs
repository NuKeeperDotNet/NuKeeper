using System;

namespace NuKeeper.Configuration
{
    public class Settings
    {
        public Settings(RepositoryModeSettings repositoryModeSettings)
        {
            Repository = repositoryModeSettings;
        }

        public Settings(OrganisationModeSettings organisationModeSettings)
        {
            Organisation = organisationModeSettings;
        }

        public Settings(RepositoryModeSettings repository, OrganisationModeSettings organisation)
        {
            Repository = repository;
            Organisation = organisation;
        }

        public RepositoryModeSettings Repository { get; }
        public OrganisationModeSettings Organisation { get; }
    }
}