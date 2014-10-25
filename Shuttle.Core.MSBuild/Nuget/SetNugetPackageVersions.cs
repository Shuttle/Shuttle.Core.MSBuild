using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Shuttle.Core.MSBuild
{
	public class SetNugetPackageVersions : Task
	{
		public override bool Execute()
		{
			var openTag = string.IsNullOrEmpty(OpenTag) ? "{" : OpenTag;
			var closeTag = string.IsNullOrEmpty(CloseTag) ? "}" : CloseTag;

			var packageFolderPath = PackageFolder.ItemSpec;

			if (!Path.IsPathRooted(packageFolderPath))
			{
				packageFolderPath = Path.GetFullPath(packageFolderPath);
			}

			if (!Directory.Exists(packageFolderPath))
			{
				Log.LogError("PackageFolder '{0}' does not exist.", packageFolderPath);

				return false;
			}

			var files = new List<string>();

			foreach (var file in Files)
			{
				if (File.Exists(file.ItemSpec))
				{
					files.Add(file.ItemSpec);
				}
				else
				{
					Log.LogWarning("TaggedFile '{0}' does not exist.", file.ItemSpec);
				}
			}

			var packageFolder = new PackageFolder(packageFolderPath);

			foreach (var message in packageFolder.Messages)
			{
				Log.LogMessage(message);
			}

			foreach (var package in packageFolder.Packages)
			{
				foreach (var file in files)
				{
					File.WriteAllText(file, File.ReadAllText(file).Replace(string.Format("{0}{1}-version{2}", openTag, package.Name, closeTag), package.Version));
				}
			}

			return true;
		}

		[Required]
		public ITaskItem[] Files { get; set; }

		[Required]
		public ITaskItem PackageFolder { get; set; }

		public string OpenTag { get; set; }
		public string CloseTag { get; set; }
	}
}