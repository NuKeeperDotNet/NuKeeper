using NuKeeper.Github.Mappings;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Engine;

namespace NuKeeper.GitHub
{
    public class OctokitClient : IClient
    {
        private readonly INuKeeperLogger _logger;
        private bool _initialised = false;

        private Octokit.IGitHubClient _client;
        private Uri _apiBase;

        public OctokitClient(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public Task Initialise(AuthSettings settings)
        {
            _apiBase = settings.ApiBase;

            _client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("NuKeeper"), _apiBase)
            {
                Credentials = new Octokit.Credentials(settings.Token)
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

        public async Task<User> GetCurrentUser()
        {
            CheckInitialised();

            var user = await _client.User.Current();
            var userLogin = user?.Login;
            _logger.Detailed($"Read github user '{userLogin}'");

            return user != null ? new User(user.Login, user.Name, user.Email) : null;
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
            var request = new Octokit.NewPullRequest(title, qualifiedBranch, repository.DefaultBranch) { Body = body };

            var pr = new GithubPullRequest(request);

            await OpenPullRequest(repository.Pull, pr, labels);
        }

        public async Task<IReadOnlyList<Organization>> GetOrganizations()
        {
            CheckInitialised();

            var orgs = await _client.Organization.GetAll();
            _logger.Normal($"Read {orgs.Count} organisations");
            return orgs.Select(x => new GithubOrganization(x)).ToList();
        }

        public async Task<IReadOnlyList<IRepository>> GetRepositoriesForOrganisation(string organisationName)
        {
            CheckInitialised();

            var repos = await _client.Repository.GetAllForOrg(organisationName);
            _logger.Normal($"Read {repos.Count} repos for org '{organisationName}'");
            return repos.Select(x => new GithubRepository(x)).ToList();
        }

        public async Task<IRepository> GetUserRepository(string userName, string repositoryName)
        {
            CheckInitialised();

            _logger.Detailed($"Looking for user fork for {userName}/{repositoryName}");
            try
            {
                var result = await _client.Repository.Get(userName, repositoryName);
                _logger.Normal($"User fork found at {result.GitUrl} for {result.Owner.Login}");
                return new GithubRepository(result);
            }
            catch (Octokit.NotFoundException)
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
                var result = await _client.Repository.Forks.Create(owner, repositoryName, new Octokit.NewRepositoryFork());
                _logger.Normal($"User fork created at {result.GitUrl} for {result.Owner.Login}");
                return new GithubRepository(result);
            }
            catch (Exception ex)
            {
                _logger.Error("User fork not created", ex);
                return null;
            }
        }

        public async Task<Branch> GetRepositoryBranch(string userName, string repositoryName, string branchName)
        {
            CheckInitialised();

            try
            {
                var result = await _client.Repository.Branch.Get(userName, repositoryName, branchName);
                _logger.Detailed($"Branch found for {userName} / {repositoryName} / {branchName}");
                return new GithubBranch(result);
            }
            catch (Octokit.NotFoundException)
            {
                _logger.Detailed($"No branch found for {userName} / {repositoryName} / {branchName}");
                return null;
            }
        }

        public async Task OpenPullRequest(ForkData target, NewPullRequest request, IEnumerable<string> labels)
        {
            CheckInitialised();
            var newPullRequest = new  Octokit.NewPullRequest(request.Title, request.Head, request.BaseRef);
            _logger.Normal($"Making PR onto '{_apiBase} {target.Owner}/{target.Name} from {request.Head}");
            _logger.Detailed($"PR title: {request.Title}");
            var createdPullRequest = await _client.PullRequest.Create(target.Owner, target.Name, newPullRequest);

            await AddLabelsToIssue(target, createdPullRequest.Number, labels);
        }

        public async Task<SearchCodeResult> Search(SearchCodeRequest search)
        {
            CheckInitialised();

            var repositoryCollection = new Octokit.RepositoryCollection();
            foreach (var repo in search.Repos)
            {
                repositoryCollection.Add(repo.Key, repo.Value);
            }

            var inQualifiers = new List<Octokit.CodeInQualifier>();
            foreach (var qualifier in search.SearchIn)
            {
                if (Enum.TryParse<Octokit.CodeInQualifier>(qualifier.ToString(), out var parsedEnum))
                {
                    inQualifiers.Add(parsedEnum);
                }
            }

            var searchCodeRequest = new Octokit.SearchCodeRequest(search.Term)
            {
                Repos = repositoryCollection,
                PerPage = search.PerPage,
                In = inQualifiers
            };

            var result = await _client.Search.SearchCode(searchCodeRequest);

            return new GithubSearchCodeResult(result);
        }

        private async Task AddLabelsToIssue(ForkData target, int issueNumber, IEnumerable<string> labels)
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
