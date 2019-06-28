using NuKeeper.Abstractions.Logging;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuKeeper.Git.Tests
{
    public class GitCmdDriverTest
    {
        private INuKeeperLogger _logger;
        private string _pathTogit;

        [OneTimeSetUp]
        public void Setup()
        {
            _logger = new ConfigurableLogger();
            _pathTogit = TestDirectoryHelper.DiscoverPathToGit();
        }

        [TestCase("https://github.com/NuKeeperDotNet/NuKeeper.git")]
        public async Task CloneRepoAndCheckout(string path)
        {
            if (_pathTogit == null)
            {
                Assert.Ignore("no git implementation found!");
            }

            var folder = TestDirectoryHelper.GenerateRandomSubFolder(TestContext.CurrentContext.WorkDirectory);
            var gitDriver = new GitCmdDriver(_pathTogit, _logger, new Folder(_logger, folder), new Abstractions.Git.GitUsernamePasswordCredentials());
            Assert.DoesNotThrowAsync(() => gitDriver.Clone(new Uri(path)));
            Assert.IsTrue(Directory.Exists(Path.Combine(folder.FullName, ".git")), "Local git repo should exist in {0}", folder.FullName);

            // Checkout master branch
            Assert.DoesNotThrowAsync(() => gitDriver.Checkout("master"));
            var head = await gitDriver.GetCurrentHead();
            Assert.AreEqual(head, "master");

            // Checkout new branch
            Assert.DoesNotThrowAsync(() => gitDriver.CheckoutNewBranch("newBranch"));
            head = await gitDriver.GetCurrentHead();
            Assert.AreEqual("newBranch", head);

            TestDirectoryHelper.DeleteDirectory(folder.FullName);
        }
    }
}
