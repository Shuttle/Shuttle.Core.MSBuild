using System;
using EnvDTE;
using Debugger = System.Diagnostics.Debugger;

namespace Shuttle.Core.MSBuild
{
	public class Installation
	{
		private readonly string _installPath;
		private readonly string _toolsPath;
		private readonly object _nugetProject;
		private readonly Project _vsProject;

		public Installation(string installPath, string toolsPath, object nugetProject, object vsProject)
		{
			Debugger.Launch();

			_installPath = installPath;
			_toolsPath = toolsPath;
			_nugetProject = nugetProject;
			_vsProject = (Project) vsProject;
		}

		public void Execute()
		{
			Console.WriteLine("I'm in!!!");
		}
	}
}