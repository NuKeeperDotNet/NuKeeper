using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Configuration;
using NUnit.Framework;

namespace NuKeeper.Abstractions.Tests.CollaborationModels
{
    [TestFixture]
    public class PullRequestRequestTests
    {
        [Test]
        public void ReplacesRemotesWhenCreatingPullRequestRequestObject()
        {
            var pr = new PullRequestRequest("head", "title", "origin/master", true, true, GitPullRequestMergeStrategy.noFastForward);
            var pr2 = new PullRequestRequest("head", "title", "master", true, true, GitPullRequestMergeStrategy.noFastForward);

            Assert.That(pr.BaseRef, Is.EqualTo("master"));
            Assert.That(pr2.BaseRef, Is.EqualTo("master"));
        }

    }
}
