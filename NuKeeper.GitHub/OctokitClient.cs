using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Logging;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Organization = NuKeeper.Abstractions.CollaborationModels.Organization;
using PullRequestRequest = NuKeeper.Abstractions.CollaborationModels.PullRequestRequest;
using Repository = NuKeeper.Abstractions.CollaborationModels.Repository;
using SearchCodeRequest = NuKeeper.Abstractions.CollaborationModels.SearchCodeRequest;
using SearchCodeResult = NuKeeper.Abstractions.CollaborationModels.SearchCodeResult;
using User = NuKeeper.Abstractions.CollaborationModels.User;
using Newtonsoft.Json;

namespace NuKeeper.GitHub
{
    public class OctokitClient : ICollaborationPlatform
    {
        private readonly INuKeeperLogger _logger;
        private bool _initialised;

        private IGitHubClient _client;
        private Uri _apiBase;

        public OctokitClient(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public void Initialise(AuthSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _apiBase = settings.ApiBase;

            _client = new GitHubClient(new ProductHeaderValue("NuKeeper"), _apiBase)
            {
                Credentials = new Credentials(settings.Token)
            };

            _initialised = true;
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

            return await ExceptionHandler(async () =>
            {
                var user = await _client.User.Current();

                var emails = await _client.User.Email.GetAll();
                var primaryEmail = emails.FirstOrDefault(e => e.Primary);

                _logger.Detailed($"Read github user '{user?.Login}'");
                return new User(user?.Login, user?.Name, primaryEmail?.Email ?? user?.Email);
            });
        }

        public async Task<IReadOnlyList<Organization>> GetOrganizations()
        {
            CheckInitialised();

            return await ExceptionHandler(async () =>
            {
                var githubOrgs = await _client.Organization.GetAll();
                _logger.Normal($"Read {githubOrgs.Count} organisations");

                return githubOrgs
                    .Select(org => new Organization(org.Name ?? org.Login))
                    .ToList();
            });
        }

        public async Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organisationName)
        {
            CheckInitialised();

            return await ExceptionHandler(async () =>
            {
                var repos = await _client.Repository.GetAllForOrg(organisationName);
                _logger.Normal($"Read {repos.Count} repos for org '{organisationName}'");
                return repos.Select(repo => new GitHubRepository(repo)).ToList();
            });
        }

        public async Task<Repository> GetUserRepository(string userName, string repositoryName)
        {
            CheckInitialised();

            _logger.Detailed($"Looking for user fork for {userName}/{repositoryName}");

            return await ExceptionHandler(async () =>
            {
                try
                {
                    var result = await _client.Repository.Get(userName, repositoryName);
                    _logger.Normal($"User fork found at {result.GitUrl} for {result.Owner.Login}");
                    return new GitHubRepository(result);
                }
                catch (NotFoundException)
                {
                    _logger.Detailed("User fork not found");
                    return null;
                }
            });
        }

        public async Task<Repository> MakeUserFork(string owner, string repositoryName)
        {
            CheckInitialised();

            _logger.Detailed($"Making user fork for {repositoryName}");

            return await ExceptionHandler(async () =>
            {
                var result = await _client.Repository.Forks.Create(owner, repositoryName, new NewRepositoryFork());
                _logger.Normal($"User fork created at {result.GitUrl} for {result.Owner.Login}");
                return new GitHubRepository(result);
            });
        }

        public async Task<bool> RepositoryBranchExists(string userName, string repositoryName, string branchName)
        {
            CheckInitialised();

            return await ExceptionHandler(async () =>
            {
                try
                {
                    await _client.Repository.Branch.Get(userName, repositoryName, branchName);
                    _logger.Detailed($"Branch found for {userName} / {repositoryName} / {branchName}");
                    return true;
                }
                catch (NotFoundException)
                {
                    _logger.Detailed($"No branch found for {userName} / {repositoryName} / {branchName}");
                    return false;
                }
            });
        }

        public async Task<bool> PullRequestExists(ForkData target, string headBranch, string baseBranch)
        {
            CheckInitialised();

            return await ExceptionHandler(async () =>
            {
                _logger.Normal($"Checking if PR exists onto '{_apiBase} {target.Owner}/{target.Name}: {baseBranch} <= {headBranch}");

                var prRequest = new Octokit.PullRequestRequest
                {
                    State = ItemStateFilter.Open,
                    SortDirection = SortDirection.Descending,
                    SortProperty = PullRequestSort.Created,
                    Head = $"{target.Owner}:{headBranch}",
                };

                var pullReqList = await _client.PullRequest.GetAllForRepository(target.Owner, target.Name, prRequest).ConfigureAwait(false);

                return pullReqList.Any(pr => pr.Base.Ref.EndsWith(baseBranch, StringComparison.InvariantCultureIgnoreCase));
            });
        }

        public async Task OpenPullRequest(ForkData target, PullRequestRequest request, IEnumerable<string> labels)
        {
            CheckInitialised();

            await ExceptionHandler(async () =>
            {
                _logger.Normal($"Making PR onto '{_apiBase} {target.Owner}/{target.Name} from {request.Head}");
                _logger.Detailed($"PR title: {request.Title}");

                var createdPullRequest = await _client.PullRequest.Create(target.Owner, target.Name, new NewPullRequest(request.Title, request.Head, request.BaseRef) { Body = request.Body });

                await AddLabelsToIssue(target, createdPullRequest.Number, labels);

                return Task.CompletedTask;
            });
        }

        public async Task<SearchCodeResult> Search(SearchCodeRequest search)
        {
            CheckInitialised();

            return await ExceptionHandler(async () =>
            {
                var repos = new RepositoryCollection();
                foreach (var repo in search.Repos)
                {
                    repos.Add(repo.Owner, repo.Name);
                }

                var result = await _client.Search.SearchCode(
                    new Octokit.SearchCodeRequest(search.Term)
                    {
                        Repos = repos,
                        In = new[] { CodeInQualifier.Path },
                        PerPage = search.PerPage
                    });
                return new SearchCodeResult(result.TotalCount);
            });
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
                catch (ApiException ex)
                {
                    _logger.Error("Failed to add labels. Continuing", ex);
                }
            }
        }

        private static async Task<T> ExceptionHandler<T>(Func<Task<T>> funcToCheck)
        {
            try
            {
                return await funcToCheck();
            }
            catch (ApiException ex)
            {
                if (ex.HttpResponse?.Body != null)
                {
                    dynamic response = JsonConvert.DeserializeObject(ex.HttpResponse.Body.ToString());
                    if (response?.errors != null && response.errors.Count > 0)
                    {
                        throw new NuKeeperException(response.errors.First.message.ToString(), ex);
                    }
                }

                throw new NuKeeperException(ex.Message, ex);
            }
        }

        public Task<int> GetNumberOfOpenPullRequests(string projectName, string repositoryName)
        {
            return Task.FromResult(0);
        }
    }
}
