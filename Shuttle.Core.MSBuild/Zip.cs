using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Shuttle.Core.MSBuild
{
	public class Zip : Task
	{
		[Required]
		public ITaskItem[] Files { get; set; }

		[Required]
		public ITaskItem RelativeFolder { get; set; }

		[Required]
		public ITaskItem ZipFilePath { get; set; }

		public override bool Execute()
		{
			var relativeFolder = Path.GetFullPath(RelativeFolder.ItemSpec);

			if (!relativeFolder.EndsWith("\\"))
			{
				relativeFolder += "\\";
			}

			var relativeFolderUri = new Uri(relativeFolder);

			using (var archiveStream = new FileStream(ZipFilePath.ItemSpec, FileMode.Create))
			using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, false))
			{
				foreach (var file in Files)
				{
					var path = Path.GetFullPath(file.ItemSpec);

					if (File.Exists(path))
					{
						var entry = archive.CreateEntry(relativeFolderUri.MakeRelativeUri(new Uri(path)).ToString());

						using (var fileStream = File.Open(path, FileMode.Open))
						using (var entryStream = entry.Open())
						{
							fileStream.CopyTo(entryStream);
						}
					}
					else
					{
						Log.LogWarning(string.Format("[zip - file missing] : path = '{0}'", path));
					}
				}
			}

			return true;
		}
	}
}