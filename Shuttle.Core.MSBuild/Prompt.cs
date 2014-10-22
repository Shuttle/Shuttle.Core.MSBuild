using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Shuttle.Core.MSBuild
{
	public class Prompt : Task
	{
		[Required]
		public string Text { get; set; }

		[Output]
		public string UserInput { get; private set; }

		public override bool Execute()
		{
			Console.WriteLine(!string.IsNullOrEmpty(Text) ? Text : "[prompt]");
			UserInput = Console.ReadLine();
			return true;
		}
	}
}