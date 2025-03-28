﻿using ImGuiNET;

using Pathfinder.Config;
using Pathfinder.Config.Data;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Interface.Components; 

[LocalService(ServiceFlags.Transient)]
public class FiltersPopup {
	public void Draw(string id, ConfigFile config) {
		if (!ImGui.BeginPopup(id)) return;

		var filter = config.Filters;
		var showChara = filter.Flags.HasFlag(WorldObjectType.Chara);

		ImGui.BeginTable("ObjectTypeTable", 2, ImGuiTableFlags.NoSavedSettings);
		ImGui.TableSetupColumn("Object Types");
		ImGui.TableSetupColumn("Character Models", showChara ? ImGuiTableColumnFlags.None : ImGuiTableColumnFlags.Disabled);
		
		ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, 0);
		ImGui.TableHeadersRow();
		ImGui.PopStyleColor();
		ImGui.TableNextRow();

		ImGui.TableSetColumnIndex(0);
		this.DrawFilterFlag(config, WorldObjectType.Terrain);
		this.DrawFilterFlag(config, WorldObjectType.BgObject);
		this.DrawFilterFlag(config, WorldObjectType.Chara, "Characters");

		if (showChara) {
			ImGui.TableSetColumnIndex(1);
			this.DrawFilterFlag(config, WorldObjectType.Human);
			this.DrawFilterFlag(config, WorldObjectType.DemiHuman);
			this.DrawFilterFlag(config, WorldObjectType.Monster);
			this.DrawFilterFlag(config, WorldObjectType.Weapon);
		}

		ImGui.EndTable();
		ImGui.EndPopup();
	}

	private void DrawFilterFlag(ConfigFile config, WorldObjectType flag, string? label = null) {
		label ??= flag.ToString();
		ImGui.PushStyleColor(ImGuiCol.Text, config.GetColor(flag));
		try {
			var value = config.Filters.Flags.HasFlag(flag);
			if (ImGui.Checkbox(label, ref value))
				config.Filters.Flags ^= flag;
		} finally {
			ImGui.PopStyleColor();
		}
	}
}
