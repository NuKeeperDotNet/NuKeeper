using NuKeeper.Abstractions.CollaborationModels;
using NUnit.Framework;

namespace NuKeeper.Abstractions.Tests.CollaborationModels
{
    [TestFixture]
    public class PullRequestRequestTests
    {
        [Test]
        public void ReplacesRemotesWhenCreatingPullRequestRequestObject()
        {
            var pr = new PullRequestRequest("head", "title", "origin/master", true);
            var pr2 = new PullRequestRequest("head", "title", "master", true);

            Assert.That(pr.BaseRef, Is.EqualTo("master"));
            Assert.That(pr2.BaseRef, Is.EqualTo("master"));
        }

    }
}
