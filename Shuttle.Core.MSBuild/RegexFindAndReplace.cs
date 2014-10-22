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
			var options = RegexOptions.None;

			if (IgnoreCase)
			{
				options |= RegexOptions.IgnoreCase;
			}

			if (Multiline)
			{
				options |= RegexOptions.Multiline;
			}

			if (Singleline)
			{
				options |= RegexOptions.Singleline;
			}

			var replaceRegex = new Regex(FindExpression, options);

			try
			{
				foreach (var file in Files)
				{
					var path = file.ItemSpec;

					var contents = File.ReadAllText(path);

					if (replaceRegex.IsMatch(contents) != true)
					{
						Log.LogWarning(String.Format("[find/replace - no matches] : file = '{0}'", path));
					}
					else
					{
						contents = replaceRegex.Replace(contents, ReplacementText);

						File.WriteAllText(path, contents);

						Log.LogMessage("[find/replace] : file = '{0}'", path);
					}
				}

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