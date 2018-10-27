using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstract;
using NuKeeper.Abstract.Engine;
using NuKeeper.Github.Engine;
using NuKeeper.Github.Mappings;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.Logging;
using Octokit;

namespace NuKeeper.GitHub
{
    public class OctokitClient : IClient
    {
        private readonly INuKeeperLogger _logger;
        private bool _initialised = false;

        private IGitHubClient _client;
        private Uri _apiBase;

        public OctokitClient(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public Task Initialise(IAuthSettings settings)
        {
            _apiBase = settings.ApiBase;

            _client = new GitHubClient(new ProductHeaderValue("NuKeeper"), _apiBase)
            {
                Credentials = new Credentials(settings.Token)
            };

            _initialised = true;

            return Task.CompletedTask;
        }

        private void CheckInitialised()
        {
            if (!_initialised)
            {
                throw new NuKeeperException("Github has not been initialised");
            }
        }

        public async Task<IAccount> GetCurrentUser()
        {
            CheckInitialised();

            var user = await _client.User.Current();
            var userLogin = user?.Login;
            _logger.Detailed($"Read github user '{userLogin}'");

            return AutoMapperConfiguration.GithubMappingConfiguration.Map<GithubAccount>(user);
        }

        public async Task CreatePullRequest(IRepositoryData repository, string title, string body, string branchWithChanges,
            IEnumerable<string> labels)
        {
            string qualifiedBranch;
            if (repository.Pull.Owner == repository.Push.Owner)
            {
                qualifiedBranch = branchWithChanges;
            }
            else
            {
                qualifiedBranch = repository.Push.Owner + ":" + branchWithChanges;
            }

            var pr = new GithubPullRequest(title, qualifiedBranch, repository.DefaultBranch) { Body = body };

            await OpenPullRequest(repository.Pull, pr, labels);
        }

        public async Task<IReadOnlyList<IOrganization>> GetOrganizations()
        {
            CheckInitialised();

            var orgs = await _client.Organization.GetAll();
            _logger.Normal($"Read {orgs.Count} organisations");
            return AutoMapperConfiguration.GithubMappingConfiguration.Map<IReadOnlyList<GithubOrganization>>(orgs);
        }

        public async Task<IReadOnlyList<IRepository>> GetRepositoriesForOrganisation(string organisationName)
        {
            CheckInitialised();

            var repos = await _client.Repository.GetAllForOrg(organisationName);
            _logger.Normal($"Read {repos.Count} repos for org '{organisationName}'");
            return AutoMapperConfiguration.GithubMappingConfiguration.Map<IReadOnlyList<GithubRepository>>(repos);
        }

        public async Task<IRepository> GetUserRepository(string userName, string repositoryName)
        {
            CheckInitialised();

            _logger.Detailed($"Looking for user fork for {userName}/{repositoryName}");
            try
            {
                var result = await _client.Repository.Get(userName, repositoryName);
                _logger.Normal($"User fork found at {result.GitUrl} for {result.Owner.Login}");
                return AutoMapperConfiguration.GithubMappingConfiguration.Map<GithubRepository>(result);
            }
            catch (NotFoundException)
            {
                _logger.Detailed("User fork not found");
                return null;
            }
        }

        public async Task<IRepository> MakeUserFork(string owner, string repositoryName)
        {
            CheckInitialised();

            _logger.Detailed($"Making user fork for {repositoryName}");
            try
            {
                var result = await _client.Repository.Forks.Create(owner, repositoryName, new NewRepositoryFork());
                _logger.Normal($"User fork created at {result.GitUrl} for {result.Owner.Login}");
                return AutoMapperConfiguration.GithubMappingConfiguration.Map<GithubRepository>(result);
            }
            catch (Exception ex)
            {
                _logger.Error("User fork not created", ex);
                return null;
            }
        }

        public async Task<IBranch> GetRepositoryBranch(string userName, string repositoryName, string branchName)
        {
            CheckInitialised();

            try
            {
                var result = await _client.Repository.Branch.Get(userName, repositoryName, branchName);
                _logger.Detailed($"Branch found for {userName} / {repositoryName} / {branchName}");
                return AutoMapperConfiguration.GithubMappingConfiguration.Map<GithubBranch>(result);
            }
            catch (NotFoundException)
            {
                _logger.Detailed($"No branch found for {userName} / {repositoryName} / {branchName}");
                return null;
            }
        }

        public async Task<IPullRequest> OpenPullRequest(IForkData target, INewPullRequest request, IEnumerable<string> labels)
        {
            CheckInitialised();

            var githubPullRequest = (GithubPullRequest) request;
            var newPullRequest = AutoMapperConfiguration.GithubMappingConfiguration.Map<NewPullRequest>(githubPullRequest);

            _logger.Normal($"Making PR onto '{_apiBase} {target.Owner}/{target.Name} from {request.Head}");
            _logger.Detailed($"PR title: {request.Title}");
            var createdPullRequest = await _client.PullRequest.Create(target.Owner, target.Name, newPullRequest);

            await AddLabelsToIssue(target, createdPullRequest.Number, labels);

            return AutoMapperConfiguration.GithubMappingConfiguration.Map<GithubPullRequestInfo>(createdPullRequest);
        }

        public async Task<ISearchCodeResult> Search(ISearchCodeRequest search)
        {
            CheckInitialised();

            var request = (GithubSearchCodeRequest) search;

            var mappedRequest = AutoMapperConfiguration.GithubMappingConfiguration.Map<SearchCodeRequest>(request);

            var result = await _client.Search.SearchCode(mappedRequest);

            var mappedResult = AutoMapperConfiguration.GithubMappingConfiguration.Map<GithubSearchCodeResult>(result);

            return mappedResult;
        }

        private async Task AddLabelsToIssue(IForkData target, int issueNumber, IEnumerable<string> labels)
        {
            var labelsToApply = labels?
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            if (labelsToApply != null && labelsToApply.Any())
            {
                _logger.Normal(
                    $"Adding label(s) '{labelsToApply.JoinWithCommas()}' to issue "
                    + $"'{_apiBase} {target.Owner}/{target.Name} {issueNumber}'");

                try
                {
                    await _client.Issue.Labels.AddToIssue(target.Owner, target.Name, issueNumber,
                        labelsToApply);

                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to add labels. Continuing", ex);
                }
            }
        }
    }
}
