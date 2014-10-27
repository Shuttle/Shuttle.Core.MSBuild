using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace Shuttle.Core.MSBuild
{
	public class PackageFolder
	{
		private static readonly Regex dependencyExpression =
			new Regex(@"(?<package>.*?)\.(?<version>(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?<revision>\.\d+)?)[-\.]?(?<prerelease>.*)",
			          RegexOptions.IgnoreCase);

		private readonly List<Package> _packages = new List<Package>();
		private readonly List<string> _messages = new List<string>();

		public PackageFolder(string packageFolder)
		{
			foreach (var directory in Directory.GetDirectories(packageFolder))
			{
				var directoryName = Path.GetFileName(directory);

				if (string.IsNullOrEmpty(directoryName))
				{
					continue;
				}

				var match = dependencyExpression.Match(directoryName);

				var packageName = match.Groups["package"];
				var packageVersion = match.Groups["version"];

				if (!packageName.Success || !packageVersion.Success)
				{
					_messages.Add(string.Format(
						"Package folder name '{0}' does not match the expected NuGet package structure.", directoryName));

					continue;
				}

				AddPackage(new Package(packageName.Value, packageVersion.Value));
			}
		}

		public void AddPackage(Package package)
		{
			var existing = _packages.Find(candidate => candidate.Name.Equals(package.Name, StringComparison.OrdinalIgnoreCase));

			if (existing == null)
			{
				_packages.Add(package);
			}
			else
			{
				if (StringComparer.OrdinalIgnoreCase.Compare(package.Version, existing.Version) > 0)
				{
					_packages.Remove(existing);
					_packages.Add(package);
				}
			}
		}

		public IEnumerable<Package> Packages
		{
			get { return new ReadOnlyCollection<Package>(_packages); }
		}

		public IEnumerable<string> Messages
		{
			get { return new ReadOnlyCollection<string>(_messages); }
		}
	}
}