using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using Xrm.Framework.CI.Common.Entities;

namespace Xrm.Framework.CI.PowerShell.Cmdlets.Test
{
    [TestFixture]
    class SetXrmWebResourcesFromFolderTest
    {
        string cmdletname = "Set-XrmWebResourcesFromFolder";
        DirectoryInfo tempDir;
        IOrganizationService crm => TestData.XrmFakedContext.GetOrganizationService();

        [SetUp]
        public void Setup()
        {
            tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        }

        [TearDown]
        public void TearDown()
        {
            tempDir.Delete(true);
        }

        [Test]
        public void ShouldUpdateOnlyModifiedContent()
        {
            crm.Create(new WebResource { Id = Guid.NewGuid(), Name = "a_test" });
            crm.Create(new WebResource { Id = Guid.NewGuid(), Name = "a_test1" });
            var id = crm.Create(new WebResource
            {
                Id = Guid.NewGuid(),
                Name = "a_test2",
                Content = Convert.ToBase64String(Encoding.UTF8.GetBytes("a_test2.js"))
            });
            var unmodifiedResource = crm.Retrieve(WebResource.EntityLogicalName, id, new ColumnSet(true)) as WebResource;
            var expectedModifyDate = unmodifiedResource.ModifiedOn;

            var files = new List<string> { "a_test.js", "a_test1.js", "a_test2.js" };
            files.ForEach((file) => File.WriteAllText(Path.Combine(tempDir.FullName, file), file.ToString()));

            var cmd = $"{cmdletname} -Path {tempDir.FullName} -ConnectionString fake";
            using (Pipeline p = TestData.Runspace.CreatePipeline(cmd))
                p.Invoke();

            var webResources = TestData.XrmFakedContext.Data.First(x => x.Key == WebResource.EntityLogicalName).Value.Values;
            Assert.Multiple(() =>
            {
                // check modified content was updated
                files.ForEach((file) =>
                {
                    var res = (WebResource)webResources.First(x => (x as WebResource).Name == Path.GetFileNameWithoutExtension(file));
                    var expected = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(tempDir.FullName, file)));
                    Assert.AreEqual(expected, res.Content);
                });
                // check entity with unmodified content wasn't updated
                unmodifiedResource = crm.Retrieve(WebResource.EntityLogicalName, id, new ColumnSet(true)) as WebResource;
                Assert.AreEqual(expectedModifyDate, unmodifiedResource.ModifiedOn);
            });
        }

        [Test]
        public void ShouldUseSearchPattern()
        {
            crm.Create(new WebResource { Id = Guid.NewGuid(), Name = "b_test" });
            crm.Create(new WebResource { Id = Guid.NewGuid(), Name = "b_test1" });
            crm.Create(new WebResource { Id = Guid.NewGuid(), Name = "b1" });
            crm.Create(new WebResource { Id = Guid.NewGuid(), Name = "b2" });


            var files = new List<string> { "b_test.js", "b_test1.js", "b2.html" };
            files.ForEach((file) => File.WriteAllText(Path.Combine(tempDir.FullName, file), file.ToString()));

            var searchPattern = "b*1.js, *.html";
            var cmd = $"{cmdletname} -Path {tempDir.FullName} -SearchPattern \"{searchPattern}\" -ConnectionString any";
            using (Pipeline p = TestData.Runspace.CreatePipeline(cmd))
                p.Invoke();

            var webResourcesUpdated = TestData.XrmFakedContext.Data.First(x => x.Key == WebResource.EntityLogicalName)
                .Value.Values.Select(x => x as WebResource)
                .Where(x => x.Name == "b_test1" || x.Name == "b2").Select(x => x.Content);

            var webResourcesSkipped = TestData.XrmFakedContext.Data.First(x => x.Key == WebResource.EntityLogicalName)
                .Value.Values.Select(x => x as WebResource)
                .Where(x => x.Name == "b_test" || x.Name == "b1").Select(x => x.Content);

            Assert.Multiple(() =>
            {
                Assert.That(webResourcesUpdated, Has.All.Not.Null);
                Assert.That(webResourcesSkipped, Is.All.Null);

            });
        }

        [Test]
        public void ShouldUseRegularExpression()
        {
            crm.Create(new WebResource { Id = Guid.NewGuid(), Name = "c_test" });
            crm.Create(new WebResource { Id = Guid.NewGuid(), Name = "c_test1" });
            crm.Create(new WebResource { Id = Guid.NewGuid(), Name = "c_1.html" });


            var files = new List<string> { "test.js", "1.html" };
            files.ForEach((file) => File.WriteAllText(Path.Combine(tempDir.FullName, file), file.ToString()));

            var regExToMatchUniqueName = @"c_.*{filename}$";
            var cmd = $"{cmdletname} -RegExToMatchUniqueName \"{regExToMatchUniqueName}\" -ConnectionString any -Path {tempDir.FullName}";
            using (Pipeline p = TestData.Runspace.CreatePipeline(cmd))
                p.Invoke();

            var webResourcesUpdated = TestData.XrmFakedContext.Data.First(x => x.Key == WebResource.EntityLogicalName)
                .Value.Values.Select(x => x as WebResource)
                .Where(x => x.Name == "c_test" || x.Name == "c_test1").Select(x => x.Content);

            var webResourcesSkipped = TestData.XrmFakedContext.Data.First(x => x.Key == WebResource.EntityLogicalName)
                .Value.Values.Select(x => x as WebResource)
                .Where(x => x.Name == "c_1.html").Select(x => x.Content);

            Assert.Multiple(() =>
            {
                Assert.That(webResourcesUpdated, Has.All.Not.Null);
                Assert.That(webResourcesSkipped, Is.All.Null);
            });

        }

        [Test]
        public void ShouldUseRegularExpressionWithFileExtension()
        {
            crm.Create(new WebResource { Id = Guid.NewGuid(), Name = "d_test" });
            crm.Create(new WebResource { Id = Guid.NewGuid(), Name = "d_test1" });
            crm.Create(new WebResource { Id = Guid.NewGuid(), Name = "d_1.html" });


            var files = new List<string> { "test.js", "1.html" };
            files.ForEach((file) => File.WriteAllText(Path.Combine(tempDir.FullName, file), file.ToString()));

            var regExToMatchUniqueName = @"d_.*{filename}$";
            var cmd = $"{cmdletname} -RegExToMatchUniqueName \"{regExToMatchUniqueName}\" -IncludeFileExtensionForUniqueName 1 -ConnectionString any -Path {tempDir.FullName}";
            using (Pipeline p = TestData.Runspace.CreatePipeline(cmd))
                p.Invoke();

            var webResourcesSkipped = TestData.XrmFakedContext.Data.First(x => x.Key == WebResource.EntityLogicalName)
                .Value.Values.Select(x => x as WebResource)
                .Where(x => x.Name == "d_test" || x.Name == "d_test1").Select(x => x.Content);

            var webResourcesUpdated = TestData.XrmFakedContext.Data.First(x => x.Key == WebResource.EntityLogicalName)
                .Value.Values.Select(x => x as WebResource)
                .Where(x => x.Name == "d_1.html").Select(x => x.Content);

            Assert.Multiple(() =>
            {
                Assert.That(webResourcesUpdated, Has.All.Not.Null);
                Assert.That(webResourcesSkipped, Is.All.Null);
            });
        }

        [Test]
        public void ShouldWriteErrorIfWebesourceNotFound()
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, Guid.NewGuid().ToString()), "");
            File.WriteAllText(Path.Combine(tempDir.FullName, Guid.NewGuid().ToString()), "");


            var cmd = $"{cmdletname} -FailIfWebResourceNotFound 1 -ConnectionString any -Path {tempDir.FullName}";
            using (Pipeline p = TestData.Runspace.CreatePipeline(cmd))
            {
                Assert.Throws<CmdletInvocationException>(() => p.Invoke());
            }
        }
    }
}
