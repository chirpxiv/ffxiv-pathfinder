using Dalamud.Configuration;

using Pathfinder.Config.Data;

namespace Pathfinder.Config;

public class ConfigFile : IPluginConfiguration {
	// Version
	
	public const int CurrentVersion = 2;

	public int Version { get; set; } = CurrentVersion;

	// Data

	public ObjectFilters Filters = new();

	public OverlayDisplay Overlay = new();

	public TableDisplay Table = new();
}
