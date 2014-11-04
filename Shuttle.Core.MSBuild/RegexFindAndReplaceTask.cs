using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Shuttle.Core.MSBuild
{
	public delegate void LogDelegate(string message);

	public class RegexFindAndReplaceTask
	{
		private readonly List<string> _files = new List<string>();

		public string FindExpression { get; set; }
		public string ReplacementText { get; set; }

		public bool IgnoreCase { get; set; }
		public bool Multiline { get; set; }
		public bool Singleline { get; set; }

		public event LogDelegate LogMessage = delegate { }; 
		public event LogDelegate LogWarning = delegate { }; 

		public RegexFindAndReplaceTask()
		{
			ReplacementText = String.Empty;
		}

		public void AddFile(string file)
		{
			_files.Add(file);
		}

		public void Execute()
		{
			if (string.IsNullOrEmpty(FindExpression))
			{
				throw new ArgumentException("'FindExpression' is required.");
			}

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

			foreach (var file in _files)
			{
				if (File.Exists(file))
				{
					var contents = File.ReadAllText(file);

					if (replaceRegex.IsMatch(contents) != true)
					{
						LogWarning.Invoke(string.Format("[find/replace - no matches] : file = '{0}'", file));
					}
					else
					{
						contents = replaceRegex.Replace(contents, ReplacementText);

						File.WriteAllText(file, contents);

						LogMessage(string.Format("[find/replace] : file = '{0}'", file));
					}
				}
				else
				{
					LogWarning.Invoke(string.Format("[find/replace - file not found] : file = '{0}'", file));
				}
			}
		}
	}
}