using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Shuttle.Core.MSBuild
{
	public class SetNugetDepencencyVersions : Task
	{
		private readonly Regex dependencyExpression = new Regex(@"(?<dependency>.*)\.(?<version>\d\.\d\.\d)", RegexOptions.IgnoreCase);

		public override bool Execute()
		{
			var openTag = string.IsNullOrEmpty(OpenTag) ? "{" : OpenTag;
			var closeTag = string.IsNullOrEmpty(CloseTag) ? "}" : CloseTag;

			if (!Directory.Exists(PackageFolder.ItemSpec))
			{
				Log.LogError("PackageFolder '{0}' does not exist.", PackageFolder.ItemSpec);

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

			foreach (var directory in Directory.GetDirectories(PackageFolder.ItemSpec))
			{
				var directoryName = Path.GetFileName(directory);

				if (string.IsNullOrEmpty(directoryName))
				{
					continue;
				}

				var match = dependencyExpression.Match(directoryName);

				var dependency = match.Groups["dependency"];
				var version = match.Groups["version"];

				if (!dependency.Success || !version.Success)
				{
					Log.LogMessage("Package dependency folder '{0}' does not match the expected dependency structure.", directoryName);

					return true;
				}

				foreach (var file in files)
				{
					File.WriteAllText(file, File.ReadAllText(file).Replace(string.Format("{0}{1}-version{2}", openTag, dependency.Value, closeTag), version.Value));
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