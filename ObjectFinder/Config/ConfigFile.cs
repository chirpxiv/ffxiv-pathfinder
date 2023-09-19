using Dalamud.Configuration;

using ObjectFinder.Config.Data;

namespace ObjectFinder.Config;

public class ConfigFile : IPluginConfiguration {
	// Version
	
	public const int CurrentVersion = 1;

	public int Version { get; set; } = CurrentVersion;

	// Data

	public ObjectFilters Filters = new();

	public OverlayDisplay Overlay = new();
}
