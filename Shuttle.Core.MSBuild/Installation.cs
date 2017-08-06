using System;
using System.IO;
using System.Text;
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

	    private const string AssemblyInfoTemplate =
	        @"using System.Reflection;
using System.Runtime.InteropServices;

#if NET40FULL
[assembly: AssemblyTitle(""{package-name} for .NET Framework 4.0"")]
#endif

#if NET45FULL
[assembly: AssemblyTitle(""{package-name} for .NET Framework 4.5"")]
#endif

#if NET451FULL
[assembly: AssemblyTitle(""{package-name} for .NET Framework 4.5.1"")]
#endif

#if NET452FULL
[assembly: AssemblyTitle(""{package-name} for .NET Framework 4.5.2"")]
#endif

#if NET46FULL
[assembly: AssemblyTitle(""{package-name} for .NET Framework 4.6"")]
#endif

#if NET461FULL
[assembly: AssemblyTitle(""{package-name} for .NET Framework 4.6.1"")]
#endif

#if NET462FULL
[assembly: AssemblyTitle(""{package-name} for .NET Framework 4.6.2"")]
#endif

[assembly: AssemblyVersion(""1.0.0.0"")]
[assembly: AssemblyCopyright(""Copyright © Eben Roux {year}"")]
[assembly: AssemblyProduct(""Shuttle"")]
[assembly: AssemblyCompany(""Shuttle"")]
[assembly: AssemblyConfiguration(""Release"")]
[assembly: AssemblyInformationalVersion(""1.0.0"")]
[assembly: ComVisible(false)]
";

	    private const string ProjectFileTermplate =
	        @"  <PropertyGroup>
    <Framework Condition="" '$(Framework)' == '' "">net45-full</Framework>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Framework)' == 'net40-full'"">
    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
    <DefineConstants>$(DefineConstants);NET40FULL</DefineConstants>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Framework)' == 'net45-full'"">
    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>
    <DefineConstants>$(DefineConstants);NET45FULL</DefineConstants>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Framework)' == 'net451-full'"">
    <TargetFrameworkVersion>v4.5.1</TargetFrameworkVersion>
    <DefineConstants>$(DefineConstants);NET451FULL</DefineConstants>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Framework)' == 'net452-full'"">
    <TargetFrameworkVersion>v4.5.2</TargetFrameworkVersion>
    <DefineConstants>$(DefineConstants);NET452FULL</DefineConstants>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Framework)' == 'net46-full'"">
    <TargetFrameworkVersion>v4.6</TargetFrameworkVersion>
    <DefineConstants>$(DefineConstants);NET46FULL</DefineConstants>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Framework)' == 'net461-full'"">
    <TargetFrameworkVersion>v4.6.1</TargetFrameworkVersion>
    <DefineConstants>$(DefineConstants);NET461FULL</DefineConstants>
  </PropertyGroup>
  <PropertyGroup Condition=""'$(Framework)' == 'net462-full'"">
    <TargetFrameworkVersion>v4.6.2</TargetFrameworkVersion>
    <DefineConstants>$(DefineConstants);NET462FULL</DefineConstants>
  </PropertyGroup>
";

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

		    var projectFolder = Path.GetDirectoryName(_vsProject.Properties.Item("FullPath").Value.ToString());

		    if (string.IsNullOrEmpty(projectFolder))
		    {
		        throw new ApplicationException("Could not determine project path.");
		    }

            var buildFolder = Path.Combine(projectFolder, ".build");

			if (!Directory.Exists(buildFolder))
			{
				Directory.CreateDirectory(buildFolder);
			}

			CopyBuildRelatedFile(buildFolder, "Shuttle.Core.MSBuild.dll");
			CopyBuildRelatedFile(buildFolder, "Shuttle.Core.MSBuild.targets");
			ProcessBuildRelatedFile(buildFolder, "package.msbuild.template", "package.msbuild");
			ProcessBuildRelatedFile(buildFolder, "package.nuspec.template", "package.nuspec");

		    ProjectItem buildFolderProjectItem = null;

		    foreach (ProjectItem projectItem in _vsProject.ProjectItems)
		    {
		        if (!projectItem.Name.Equals(".build", StringComparison.OrdinalIgnoreCase))
		        {
		            continue;
		        }

		        buildFolderProjectItem = projectItem;
		        break;
		    }

		    if (buildFolderProjectItem == null)
		    {
		        buildFolderProjectItem = _vsProject.ProjectItems.AddFolder(".build");
		    }

		    buildFolderProjectItem.ProjectItems.AddFromFile(Path.Combine(buildFolder, "Shuttle.Core.MSBuild.dll"));
		    buildFolderProjectItem.ProjectItems.AddFromFile(Path.Combine(buildFolder, "Shuttle.Core.MSBuild.targets"));
		    buildFolderProjectItem.ProjectItems.AddFromFile(Path.Combine(buildFolder, "package.msbuild"));
		    buildFolderProjectItem.ProjectItems.AddFromFile(Path.Combine(buildFolder, "package.nuspec"));

            _vsProject.Save();

		    OverwriteAssemblyInfo(projectFolder);
		    AddFramework(projectFolder);
        }

	    private void AddFramework(string projectFolder)
	    {
	        var projectFilePath = Path.Combine(projectFolder, _vsProject.FileName);

	        if (!File.Exists(projectFilePath))
	        {
	            return;
	        }

	        try
	        {
	            var result = new StringBuilder();

	            using (var sr = new StreamReader(projectFilePath))
	            {
	                string line;
	                var added = false;

	                while (!string.IsNullOrEmpty(line = sr.ReadLine()))
	                {
	                    if (!line.Contains("<TargetFrameworkVersion>"))
	                    {
	                        if (added)
	                        {
	                            result.AppendLine(line);
	                        }
	                        else
	                        {
	                            if (line.Contains("<ItemGroup>"))
	                            {
	                                result.Append(ProjectFileTermplate);

	                                added = true;
	                            }

	                            result.AppendLine(line);
	                        }
	                    }
	                }
	            }

	            File.WriteAllText(projectFilePath, result.ToString());
	        }
	        catch
	        {
	        }
	    }

	    private void OverwriteAssemblyInfo(string projectFolder)
	    {
	        var aseemblyInfoPath = Path.Combine(projectFolder, "Properties\\AssemblyInfo.cs");

	        if (!File.Exists(aseemblyInfoPath))
	        {
	            return;
	        }

	        try
	        {
	            File.WriteAllText(aseemblyInfoPath,
	                AssemblyInfoTemplate.Replace("{package-name}", _vsProject.Name).Replace("{year}", DateTime.Now.ToString("yyyy")));
	        }
	        catch
	        {
	        }
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
			var packageName = packageAssembly;

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