using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using NUnit.Framework;

namespace Shuttle.Core.MSBuild.Tests
{
	[TestFixture]
	public class RegexFindAndReplaceTests
	{
		[Test]
		public void Should_be_able_to_find_and_replace_text()
		{
			var task = new RegexFindAndReplace
				{
					BuildEngine = new Mock<IBuildEngine>().Object,
					Files = new List<ITaskItem>
						{
							new TaskItem(FilePathExtensions.BasePath(@".\files\regex-find-and-replace-test.txt"))
						}.ToArray()
				};

			File.Copy(FilePathExtensions.BasePath(@".\files\regex-find-and-replace.txt"),
                FilePathExtensions.BasePath(@".\files\regex-find-and-replace-test.txt"), true);

			task.FindExpression = @"AssemblyInformationalVersion\s*\(\s*"".*""\s*\)";
			task.ReplacementText = "AssemblyInformationalVersion(\"new-version-number\")";

			Assert.True(task.Execute());

			task.FindExpression = @"AssemblyVersion\s*\(\s*"".*""\s*\)";
			task.ReplacementText = "AssemblyVersion(\"new-version-number\")";

			Assert.True(task.Execute());

			task.FindExpression = @"\{semver\}";
			task.ReplacementText = "new-version-number";

			Assert.True(task.Execute());

			var contents = File.ReadAllText(FilePathExtensions.BasePath(@".\files\regex-find-and-replace-test.txt"));

			Assert.IsTrue(contents.Contains("[assembly: AssemblyVersion(\"new-version-number\")]"));
			Assert.IsTrue(contents.Contains("[assembly: AssemblyInformationalVersion(\"new-version-number\")]"));
			Assert.IsTrue(contents.Contains("<version>new-version-number</version>"));

			File.Delete(FilePathExtensions.BasePath(@".\files\regex-find-and-replace-test.txt"));
		}
	}
}