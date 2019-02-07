using Newtonsoft.Json;
using NuKeeper.AzureDevOps;
using NUnit.Framework;

namespace Nukeeper.AzureDevOps.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class AzureDevopsRestTypesTests
    {
        [TestCase((long)0)]
        [TestCase((long)2147483647)] //int.MaxValue
        [TestCase((long)4294967295)] //uint.MaxValue
        [TestCase((long)9223372036854775807)] //long.MaxValue
        public void AzureRepository_CanBeDeserializedWithSize(long size)
        {
            var json = AzureRepositoryJsonWithSize(size);
            Assert.DoesNotThrow(() => JsonConvert.DeserializeObject<AzureRepository>(json));
        }

        private static string AzureRepositoryJsonWithSize(long size)
        {
            return "" +
            "{" +
                "\"id\": \"a949c96a-9e6b-486f-a41c-90d5e15db277\"," +
                "\"name\": \"Repo.Name\"," +
                "\"url\": \"https://organization.visualstudio.com/a949c96a-9e6b-486f-a41c-90d5e15db277/_apis/git/repositories/a949c96a-9e6b-486f-a41c-90d5e15db277\"," +
                "\"project\": {" +
                    "\"id\": \"a949c96a-9e6b-486f-a41c-90d5e15db277\"," +
                    "\"name\": \"Project\"," +
                    "\"description\": \"A project\"," +
                    "\"url\": \"https://organization.visualstudio.com/_apis/projects/a949c96a-9e6b-486f-a41c-90d5e15db277\"," +
                    "\"state\": \"wellFormed\"," +
                    "\"revision\": 202846384," +
                    "\"visibility\": \"private\"" +
                "}," +
                "\"defaultBranch\": \"refs/heads/master\"," +
                $"\"size\": {size}," +
                "\"remoteUrl\": \"https://organization.visualstudio.com/Project/_git/Repo.Name\"," +
                "\"sshUrl\": \"organization@vs-ssh.visualstudio.com:v3/organization/Project/Repo.Name\"" +
            "}";
        }
    }
}
