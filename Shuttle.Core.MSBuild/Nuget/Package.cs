namespace Shuttle.Core.MSBuild
{
	public class Package
	{
		public Package(string name, string version)
		{
			Name = name;
			Version = version;
		}

		public string Name { get; private set; }
		public string Version { get; private set; }
	}
}