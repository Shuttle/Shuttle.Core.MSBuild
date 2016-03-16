using System;
using System.IO;
using EnvDTE;
using EnvDTE80;

namespace Shuttle.Core.MSBuild
{
	public class Installation
	{
		private readonly string _installPath;
		private readonly string _toolsPath;
		private readonly object _nugetPackage;
		private readonly Project _vsProject;

		public Installation(string installPath, string toolsPath, object nugetPackage, object vsProject)
		{
			_installPath = installPath;
			_toolsPath = toolsPath;
			_nugetPackage = nugetPackage;
			_vsProject = (Project) vsProject;
		}

		public void Execute()
		{
			DeleteReadMe();
			ConfigureBuildFolder();
		}

		private void ConfigureBuildFolder()
		{
			var solution = _vsProject.DTE.Solution as Solution2;

			if (solution == null || !solution.IsOpen)
			{
				throw new ApplicationException("No solution appears to be open.");
			}

			var solutionFolder = Path.GetDirectoryName(solution.Properties.Item("Path").Value.ToString());

			if (string.IsNullOrEmpty(solutionFolder))
			{
				throw new ApplicationException("Could not determine solution path.");
			}

			var buildFolder = Path.Combine(solutionFolder, ".build");

			if (!Directory.Exists(buildFolder))
			{
				Directory.CreateDirectory(buildFolder);
			}

			CopyBuildRelatedFile(buildFolder, "Shuttle.Core.MSBuild.dll");
			CopyBuildRelatedFile(buildFolder, "Shuttle.Core.MSBuild.targets");
			ProcessBuildRelatedFile(buildFolder, "package.msbuild.template", "package.msbuild");
			ProcessBuildRelatedFile(buildFolder, "package.nuspec.template", "package.nuspec");

			Project buildFolderProject = null;

			foreach (Project project in solution.Projects)
			{
				if (!project.Name.Equals(".build", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				buildFolderProject = project;
				break;
			}

			if (buildFolderProject == null)
			{
				buildFolderProject = solution.AddSolutionFolder(".build");
			}

			buildFolderProject.ProjectItems.AddFromFile(Path.Combine(buildFolder, "Shuttle.Core.MSBuild.dll"));
			buildFolderProject.ProjectItems.AddFromFile(Path.Combine(buildFolder, "Shuttle.Core.MSBuild.targets"));
			buildFolderProject.ProjectItems.AddFromFile(Path.Combine(buildFolder, "package.msbuild"));
			buildFolderProject.ProjectItems.AddFromFile(Path.Combine(buildFolder, "package.nuspec"));
		}

		public void CopyBuildRelatedFile(string buildFolder, string fileName)
		{
			var sourceFileName = Path.Combine(_toolsPath, fileName);
			var targetFileName = Path.Combine(buildFolder, fileName);

			try
			{
				File.Copy(sourceFileName, targetFileName, true);
			}
			catch (Exception ex)
			{
				Console.WriteLine("[CopyBuildRelatedFile] : could not copy '{0}' tp '{1}' / exception = {2}", sourceFileName,
					targetFileName, ex.Message);
			}
		}

		private void ProcessBuildRelatedFile(string buildFolder, string sourceFileName, string targetFileName)
		{
			var targetPath = Path.Combine(buildFolder, targetFileName);

			if (File.Exists(targetPath))
			{
				return;
			}

			File.Copy(Path.Combine(_toolsPath, sourceFileName), targetPath);

			var packageAssembly = _vsProject.Name;
			var packageName = packageAssembly.ToLower().Replace(".", "-");

			var task = new RegexFindAndReplaceTask();

			task.AddFile(targetPath);

			task.FindExpression = @"\{package-name\}";
			task.ReplacementText = packageName;

			task.Execute();

			task.FindExpression = @"\{package-assembly\}";
			task.ReplacementText = packageAssembly;

			task.Execute();
		}

		private void DeleteReadMe()
		{
			foreach (ProjectItem projectItem in _vsProject.ProjectItems)
			{
				if (!projectItem.Name.Equals("Shuttle.Core.MSBuild-readme.md"))
				{
					continue;
				}

				projectItem.Remove();

				File.Delete(projectItem.FileNames[0]);

				break;
			}
		}
	}
}