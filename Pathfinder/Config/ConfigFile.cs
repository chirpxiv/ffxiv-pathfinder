using System.Linq;
using System.Collections.Generic;

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
	
	// Colors

	public readonly static Dictionary<WorldObjectType, uint> DefaultColors = new() {
		{ WorldObjectType.BgObject, 0xFFFF87B1 },
		{ WorldObjectType.Terrain, 0xFFE87D4E },
		{ WorldObjectType.Human, 0xFF7FE946 },
		{ WorldObjectType.DemiHuman, 0xFF5E8EFF },
		{ WorldObjectType.Monster, 0xFF4D3DFF },
		{ WorldObjectType.Weapon, 0xFF7FE9FF },
		{ WorldObjectType.Chara, 0xFFFFFFFF }
	};

	public Dictionary<WorldObjectType, uint> Colors = DefaultColors
		.ToDictionary(pair => pair.Key, pair => pair.Value);

	public uint GetColor(WorldObjectType key) {
		if (this.Colors.TryGetValue(key, out var result))
			return result;

		if (DefaultColors.TryGetValue(key, out result)) {
			this.Colors.Add(key, result);
			return result;
		}

		return 0xFFFFFFFF;
	}
}
