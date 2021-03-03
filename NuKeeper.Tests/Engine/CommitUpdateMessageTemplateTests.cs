using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.RepositoryInspection;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace NuKeeper.Tests.Engine
{
    public class CommitUpdateMessageTemplateTests
    {
        private CommitUpdateMessageTemplate _sut;
        private IEnrichContext<PackageUpdateSet, UpdateMessageTemplate> _enricher;
        private IEnrichContext<IReadOnlyCollection<PackageUpdateSet>, UpdateMessageTemplate> _multiEnricher;

        [SetUp]
        public void TestInitialize()
        {
            _sut = new CommitUpdateMessageTemplate();
            _enricher = new PackageUpdateSetEnricher();
            _multiEnricher = new PackageUpdateSetsEnricher(_enricher);
        }

        [Test]
        public void Write_OneUpdate_ReturnsMessageIndicatingPackageAndVersion()
        {
            var updates = PackageUpdates.For(MakePackageForV110());

            _enricher.Enrich(updates, _sut);
            var report = _sut.Output();

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
            Assert.That(report, Is.EqualTo("📦 Automatic update of foo.bar to 1.2.3"));
        }

        [Test]
        public void Write_TwoUpdates_ReturnsMessageIndicatingTheActuallySelectedPackageAndVersion()
        {
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV100());

            _enricher.Enrich(updates, _sut);
            var report = _sut.Output();

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
            Assert.That(report, Is.EqualTo("📦 Automatic update of foo.bar to 1.2.3"));
        }

        [Test]
        public void Write_TwoUpdatesSameVersion_ReturnsMessageIndicatingPackageAndVersion()
        {
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV110InProject3());

            _enricher.Enrich(updates, _sut);
            var report = _sut.Output();

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
            Assert.That(report, Is.EqualTo("📦 Automatic update of foo.bar to 1.2.3"));
        }

        [Test]
        public void Write_MultiplePackages_ReturnsMessageIndicatingNumberOfPackagesUpdated()
        {
            const string packageOne = "foo.bar";
            const string packageTwo = "notfoo.bar";
            var updates = new[] {
                PackageUpdates.For(packageOne, MakePackageForV110(packageOne), MakePackageForV100(packageOne)),
                PackageUpdates.For(packageTwo, MakePackageForV110(packageTwo), MakePackageForV100(packageTwo))
            };

            _multiEnricher.Enrich(updates, _sut);
            var report = _sut.Output();

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
            Assert.That(report, Is.EqualTo("📦 Automatic update of 2 packages"));
        }

        [Test]
        public void Write_CustomTemplate_UsesCustomTemplate()
        {
            _sut.CustomTemplate = "chore: Update {{^multipleChanges}}{{#packages}}{{Name}} to {{Version}}{{/packages}}{{/multipleChanges}}";
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV100());

            _enricher.Enrich(updates, _sut);
            var report = _sut.Output();

            Assert.That(report, Is.EqualTo("chore: Update foo.bar to 1.2.3"));
        }

        [Test]
        public void Write_CustomTemplateAndContext_UsesCustomTemplateAndContext()
        {
            _sut.AddPlaceholderValue("company", "NuKeeper");
            _sut.CustomTemplate =
@"
chore: Update {{^multipleChanges}}{{#packages}}{{Name}} to {{Version}}{{/packages}}{{/multipleChanges}}

We at {{company}} are committed to keeping your software's dependencies up-to-date!
";
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV100());

            _enricher.Enrich(updates, _sut);
            var report = _sut.Output();

            Assert.That(
                report,
                Is.EqualTo(
@"
chore: Update foo.bar to 1.2.3

We at NuKeeper are committed to keeping your software's dependencies up-to-date!
"
               )
            );
        }

        [Test]
        public void Write_CustomTemplateAndContext_SupportsLambdas()
        {
            _sut.AddPlaceholderValue(
                "50char",
                new Func<string, Func<string, string>, object>(
                    (str, render) =>
                    {
                        var rendering = render(str);
                        return rendering.Length > 50 ?
                            rendering.Substring(0, 47).PadRight(50, '.')
                            : rendering;
                    }
                )
            );
            _sut.CustomTemplate = "{{#50char}}chore: Update {{^multipleChanges}}{{#packages}}{{Name}} to {{Version}}{{/packages}}{{/multipleChanges}}{{/50char}}";
            var longPackageName = "ExtremelyLongCompanyName.Package";
            var updates = PackageUpdates.For(longPackageName, MakePackageForV110(longPackageName), MakePackageForV100(longPackageName));

            _enricher.Enrich(updates, _sut);
            var report = _sut.Output();

            Assert.That(report, Is.EqualTo("chore: Update ExtremelyLongCompanyName.Package ..."));
        }

        [TestCase("{{#packages}}{{Name}} to {{Version}}{{/packages}}", "foo.bar to 1.2.3")]
        [TestCase("{{packageEmoji}}", "📦")]
        [TestCase("{{packageCount}}", "1")]
        public void Write_CustomTemplateDefaultContextSinglePackage_ReturnsMessageWithTheExpectedReplacements(string placeHolder, string expectedOutput)
        {
            const string packageName = "foo.bar";
            _sut.CustomTemplate = placeHolder;
            var updates = PackageUpdates.For(
                packageName,
                MakePackageForV110(packageName),
                MakePackageForV100(packageName)
            );

            _enricher.Enrich(updates, _sut);
            var report = _sut.Output();

            Assert.That(report, Is.EqualTo(expectedOutput));
        }

        [TestCase("{{packageEmoji}}", "📦")]
        [TestCase("{{packageCount}}", "2")]
        [TestCase("{{#packages}}{{Name}} to {{Version}}\r\n{{/packages}}", "foo.bar to 1.2.3\r\nnotfoo.bar to 1.2.3\r\n")]
        public void Write_CustomTemplateDefaultContextTwoPackages_ReturnsMessageWithTheExpectedReplacements(string placeHolder, string expectedOutput)
        {
            _sut.CustomTemplate = placeHolder;
            const string packageNameOne = "foo.bar";
            const string packageNameTwo = "notfoo.bar";
            var updates = new[]
            {
                PackageUpdates.For(packageNameOne, MakePackageForV110(packageNameOne), MakePackageForV100(packageNameOne)),
                PackageUpdates.For(packageNameTwo, MakePackageForV110(packageNameTwo), MakePackageForV100(packageNameTwo))
            };

            _multiEnricher.Enrich(updates, _sut);
            var report = _sut.Output();

            Assert.That(report, Is.EqualTo(expectedOutput));
        }

        private static PackageInProject MakePackageForV110(string packageName = "foo.bar")
        {
            var path = new PackagePath("c:\\temp", "folder\\src\\project1\\packages.config",
                PackageReferenceType.PackagesConfig);
            return new PackageInProject(packageName, "1.1.0", path);
        }

        private static PackageInProject MakePackageForV100(string packageName = "foo.bar")
        {
            var path2 = new PackagePath("c:\\temp", "folder\\src\\project2\\packages.config",
                PackageReferenceType.PackagesConfig);
            var currentPackage2 = new PackageInProject(packageName, "1.0.0", path2);
            return currentPackage2;
        }

        private static PackageInProject MakePackageForV110InProject3()
        {
            var path = new PackagePath("c:\\temp", "folder\\src\\project3\\packages.config", PackageReferenceType.PackagesConfig);

            return new PackageInProject("foo.bar", "1.1.0", path);
        }
    }
}
