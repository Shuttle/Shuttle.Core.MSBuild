using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using NUnit.Framework;

namespace Shuttle.Core.MSBuild.Tests
{
	[TestFixture]
	public class SetNugetDepencencyVersionsTests
	{
		[Test]
		public void Should_be_able_to_set_nuget_dependency_versions_using_defaults()
		{
			var task = new SetNugetDepencencyVersions
			{
				BuildEngine = new Mock<IBuildEngine>().Object,
				PackageFolder = new TaskItem(@".\packages"),
				Files = new List<ITaskItem>
						{
							new TaskItem(@".\files\set-nuget-depencency-versions-default-test.txt")
						}.ToArray()
			};

			File.Copy(@".\files\set-nuget-depencency-versions-default.txt", @".\files\set-nuget-depencency-versions-default-test.txt", true);

			Assert.True(task.Execute());

			var contents = File.ReadAllText(@".\files\set-nuget-depencency-versions-default-test.txt");

			Assert.IsTrue(contents.Contains("<dependency id=\"another.package\" version=\"[3.2.1]\" />"));
			Assert.IsTrue(contents.Contains("<dependency id=\"some-package\" version=\"[1.2.3]\" />"));

			File.Delete(@".\files\set-nuget-depencency-versions-default-test.txt");
		}

		[Test]
		public void Should_be_able_to_set_nuget_dependency_versions_using_custom_values()
		{
		}
	}
}