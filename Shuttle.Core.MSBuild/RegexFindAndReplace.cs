using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Shuttle.Core.MSBuild
{
	public class RegexFindAndReplace : Task
	{
		[Required]
		public ITaskItem[] Files { get; set; }

		[Required]
		public string FindExpression { get; set; }

		public bool IgnoreCase { get; set; }
		public bool Multiline { get; set; }
		public bool Singleline { get; set; }

		public string ReplacementText { get; set; }

		public RegexFindAndReplace()
		{
			ReplacementText = String.Empty;
		}

		public override bool Execute()
		{
			try
			{
				var task = new RegexFindAndReplaceTask()
				{
					FindExpression = FindExpression,
					ReplacementText = ReplacementText,
					IgnoreCase = IgnoreCase,
					Multiline = Multiline,
					Singleline = Singleline
				};

				task.LogMessage += (message) => Log.LogMessage(message);
				task.LogWarning += (warning) => Log.LogWarning(warning);

				foreach (var file in Files)
				{
					task.AddFile(file.ItemSpec);
				}

				task.Execute();

				return true;
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex);

				return false;
			}
		}
	}
}