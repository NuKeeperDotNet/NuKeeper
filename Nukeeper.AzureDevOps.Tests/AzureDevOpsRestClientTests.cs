using NSubstitute;
using NuKeeper.Abstractions.Logging;
using NuKeeper.AzureDevOps;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuKeeper.Abstractions;

namespace Nukeeper.AzureDevOps.Tests
{
    public class AzureDevOpsRestClientTests
    {
        [Test]
        public void InitializesCorrectly()
        {
            var httpClient = new HttpClient();
            var restClient = new AzureDevOpsRestClient(httpClient, Substitute.For<INuKeeperLogger>(), "PAT");

            var encodedToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{string.Empty}:PAT"));
            Assert.IsTrue(httpClient.DefaultRequestHeaders.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/json")));
            Assert.IsTrue(httpClient.DefaultRequestHeaders.Authorization.Equals(new AuthenticationHeaderValue("Basic", encodedToken)));
        }

        [Test]
        public void ThrowsWithBadJson()
        {
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject("<body>Login Page</body>"), Encoding.UTF8, "application/json")
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler) {BaseAddress = new Uri("https://fakebaseAddress.com/")};
            var restClient = new AzureDevOpsRestClient(fakeHttpClient, Substitute.For<INuKeeperLogger>(), "PAT");
            var exception = Assert.ThrowsAsync<NuKeeperException>(async () => await restClient.GetGitRepositories("Project"));
        }

        [Test]
        public void ThrowsWithUnauthorized()
        {
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler) {BaseAddress = new Uri("https://fakebaseAddress.com/")};
            var restClient = new AzureDevOpsRestClient(fakeHttpClient, Substitute.For<INuKeeperLogger>(), "PAT");
            var exception = Assert.ThrowsAsync<NuKeeperException>(async () => await restClient.GetGitRepositories("Project"));
            Assert.IsTrue(exception.Message.Contains("Unauthorised", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void ThrowsWithForbidden()
        {
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler) {BaseAddress = new Uri("https://fakebaseAddress.com/")};
            var restClient = new AzureDevOpsRestClient(fakeHttpClient, Substitute.For<INuKeeperLogger>(), "PAT");
            var exception = Assert.ThrowsAsync<NuKeeperException>(async () => await restClient.GetGitRepositories("Project"));
            Assert.IsTrue(exception.Message.Contains("Forbidden", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void ThrowsWithBadStatusCode()
        {
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler) {BaseAddress = new Uri("https://fakebaseAddress.com/")};
            var restClient = new AzureDevOpsRestClient(fakeHttpClient, Substitute.For<INuKeeperLogger>(), "PAT");
            var exception = Assert.ThrowsAsync<NuKeeperException>(async () => await restClient.GetGitRepositories("Project"));
            Assert.IsTrue(exception.Message.Contains("Error", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public async Task GetsProjects()
        {
            var projectResource = new ProjectResource
            {
                Count = 3,
                value = new List<Project>
                {
                    new Project
                    {
                        id = "eb6e4656-77fc-42a1-9181-4c6d8e9da5d1",
                        name = "Fabrikam-Fiber-TFVC",
                        description = "Team Foundation Version Control projects.",
                        url = "https://dev.azure.com/fabrikam/_apis/projects/eb6e4656-77fc-42a1-9181-4c6d8e9da5d1",
                        state = "wellFormed",
                        visibility = "private",
                    },
                    new Project
                    {
                        id = "6ce954b1-ce1f-45d1-b94d-e6bf2464ba2c",
                        name = "Fabrikam-Fiber-Git",
                        description = "Git projects.",
                        url = "https://dev.azure.com/fabrikam/_apis/projects/6ce954b1-ce1f-45d1-b94d-e6bf2464ba2c",
                        state = "wellFormed",
                        visibility = "private",
                        revision = 51
                    },
                    new Project
                    {
                        id = "281f9a5b-af0d-49b4-a1df-fe6f5e5f84d0",
                        name = "TestGit",
                        url = "https://dev.azure.com/fabrikam/_apis/projects/281f9a5b-af0d-49b4-a1df-fe6f5e5f84d0",
                        state = "wellFormed",
                        visibility = "private",
                        revision = 2
                    }
                }
            };

            var restClient = GetFakeClient(projectResource);
            var projects = (await restClient.GetProjects()).ToList();

            Assert.IsNotNull(projects);
            Assert.IsTrue(projects.Count == 3);
        }

        [Test]
        public async Task GetsGitRepositories()
        {
            var gitRepositories = new GitRepositories
            {
                count = 3,
                value = new List<AzureRepository>
                {
                    new AzureRepository
                    {
                        id = "5febef5a-833d-4e14-b9c0-14cb638f91e6",
                        name = "AnotherRepository",
                        url = "https://dev.azure.com/fabrikam/_apis/git/repositories/5febef5a-833d-4e14-b9c0-14cb638f91e6",
                        project = new Project
                        {
                            id = "6ce954b1-ce1f-45d1-b94d-e6bf2464ba2c",
                            name = "Fabrikam-Fiber-Git",
                            url = "https://dev.azure.com/fabrikam/_apis/projects/6ce954b1-ce1f-45d1-b94d-e6bf2464ba2c",
                            state = "wellFormed"
                        },
                        remoteUrl = "https://dev.azure.com/fabrikam/Fabrikam-Fiber-Git/_git/AnotherRepository"
                    },
                    new AzureRepository
                    {
                        id = "278d5cd2-584d-4b63-824a-2ba458937249",
                        name = "Fabrikam-Fiber-Git",
                        url = "https://dev.azure.com/fabrikam/_apis/git/repositories/278d5cd2-584d-4b63-824a-2ba458937249",
                        project = new Project
                        {
                            id = "6ce954b1-ce1f-45d1-b94d-e6bf2464ba2c",
                            name = "Fabrikam-Fiber-Git",
                            url = "https://dev.azure.com/fabrikam/_apis/projects/6ce954b1-ce1f-45d1-b94d-e6bf2464ba2c",
                            state = "wellFormed"
                        },
                        remoteUrl = "https://dev.azure.com/fabrikam/_git/Fabrikam-Fiber-Git",
                        defaultBranch = "refs/heads/master"
                    },
                    new AzureRepository
                    {
                        id = "66efb083-777a-4cac-a350-a24b046be6be",
                        name = "AnotherRepository",
                        url = "https://dev.azure.com/fabrikam/_apis/git/repositories/66efb083-777a-4cac-a350-a24b046be6be",
                        project = new Project
                        {
                            id = "281f9a5b-af0d-49b4-a1df-fe6f5e5f84d0",
                            name = "TestGit",
                            url = "https://dev.azure.com/fabrikam/_apis/projects/281f9a5b-af0d-49b4-a1df-fe6f5e5f84d0",
                            state = "wellFormed"
                        },
                        remoteUrl = "https://dev.azure.com/fabrikam/_git/TestGit",
                        defaultBranch = "refs/heads/master"
                    }
                }
            };

            var restClient = GetFakeClient(gitRepositories);
            var azureRepositories = (await restClient.GetGitRepositories("ProjectName")).ToList();

            Assert.IsNotNull(azureRepositories);
            Assert.IsTrue(azureRepositories.Count == 3);
        }

        [Test]
        public async Task GetsGitRefs()
        {
            var gitRefsResource = new GitRefsResource
            {
                count = 3,
                value = new List<GitRefs>
                {
                    new GitRefs
                    {
                        name = "refs/heads/develop",
                        objectId = "67cae2b029dff7eb3dc062b49403aaedca5bad8d",
                        url = "https://dev.azure.com/fabrikam/_apis/git/repositories/278d5cd2-584d-4b63-824a-2ba458937249/refs/heads/develop"
                    },
                    new GitRefs
                    {
                        name = "refs/heads/master",
                        objectId = "23d0bc5b128a10056dc68afece360d8a0fabb014",
                        url = "https://dev.azure.com/fabrikam/_apis/git/repositories/278d5cd2-584d-4b63-824a-2ba458937249/refs/heads/master"
                    },
                    new GitRefs
                    {
                        name = "refs/tags/v1.0",
                        objectId = "23d0bc5b128a10056dc68afece360d8a0fabb014",
                        url = "https://dev.azure.com/fabrikam/_apis/git/repositories/278d5cd2-584d-4b63-824a-2ba458937249/refs/tags/v1.0"
                    }
                }
            };

            var restClient = GetFakeClient(gitRefsResource);
            var gitRefs = (await restClient.GetRepositoryRefs("ProjectName", "RepoId")).ToList();

            Assert.IsNotNull(gitRefs);
            Assert.IsTrue(gitRefs.Count == 3);
        }

        [Test]
        public async Task CreatesPullRequest()
        {
            var pullRequest = new PullRequest
            {
                AzureRepository = new AzureRepository
                {
                    id = "3411ebc1-d5aa-464f-9615-0b527bc66719",
                    name = "2016_10_31",
                    url = "https://dev.azure.com/fabrikam/_apis/git/repositories/3411ebc1-d5aa-464f-9615-0b527bc66719",
                    project = new Project
                    {
                        id = "a7573007-bbb3-4341-b726-0c4148a07853",
                        name = "2016_10_31",
                        description = "test project created on Halloween 2016",
                        url = "https://dev.azure.com/fabrikam/_apis/projects/a7573007-bbb3-4341-b726-0c4148a07853",
                        state = "wellFormed",
                        revision = 7
                    },
                    remoteUrl = "https://dev.azure.com/fabrikam/_git/2016_10_31"
                },
                PullRequestId = 22,
                CodeReviewId = 22,
                Status = "active",
                CreationDate = new DateTime(2016, 11, 01, 16, 30, 31),
                Title = "A new feature",
                Description = "Adding a new feature",
                SourceRefName = "refs/heads/npaulk/my_work",
                TargetRefName = "refs/heads/new_feature",
                MergeStatus = "queued",
                MergeId = "f5fc8381-3fb2-49fe-8a0d-27dcc2d6ef82",
                Url = "https: //dev.azure.com/fabrikam/_apis/git/repositories/3411ebc1-d5aa-464f-9615-0b527bc66719/commits/b60280bc6e62e2f880f1b63c1e24987664d3bda3",
                SupportsIterations = true,
            };

            var restClient = GetFakeClient(pullRequest);
            var request = new PRRequest {title = "A Pr"};
            var createdPullRequest = await restClient.CreatePullRequest(request, "ProjectName", "RepoId");
            Assert.IsNotNull(createdPullRequest);
        }

        [Test]
        public async Task CreatesPullRequestLabel()
        {
            var labelResource = new LabelResource
            {
                value = new List<Label>
                {
                    new Label
                    {
                        active = true,
                        id = "id",
                        name = "nukeeper"
                    }
                }
            };

            var restClient = GetFakeClient(labelResource);
            var request = new LabelRequest {name = "nukeeper"};
            var pullRequestLabel = await restClient.CreatePullRequestLabel(request, "ProjectName", "RepoId", 100);
            Assert.IsNotNull(pullRequestLabel);
        }

        private static AzureDevOpsRestClient GetFakeClient(object returnObject)
        {
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(returnObject), Encoding.UTF8, "application/json")
            });

            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler) {BaseAddress = new Uri("https://fakebaseAddress.com/")};
            return new AzureDevOpsRestClient(fakeHttpClient, Substitute.For<INuKeeperLogger>(), "PAT");
        }
    }
}
