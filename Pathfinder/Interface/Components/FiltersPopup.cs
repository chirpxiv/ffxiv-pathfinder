using ImGuiNET;

using Pathfinder.Config;
using Pathfinder.Config.Data;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Interface.Components; 

[LocalService(ServiceFlags.Transient)]
public class FiltersPopup {
	public void Draw(string id, ConfigFile config) {
		if (!ImGui.BeginPopup(id)) return;

		var filter = config.Filters;
		var showChara = filter.Flags.HasFlag(ObjectFilterFlags.Chara);

		ImGui.BeginTable("ObjectTypeTable", 2, ImGuiTableFlags.NoSavedSettings);
		ImGui.TableSetupColumn("Object Types");
		ImGui.TableSetupColumn("Character Models", showChara ? ImGuiTableColumnFlags.None : ImGuiTableColumnFlags.Disabled);
		
		ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, 0);
		ImGui.TableHeadersRow();
		ImGui.PopStyleColor();
		ImGui.TableNextRow();

		ImGui.TableSetColumnIndex(0);
		DrawFilterFlag(filter, "BgObject", ObjectFilterFlags.BgObject);
		DrawFilterFlag(filter, "Character", ObjectFilterFlags.Chara);
		DrawFilterFlag(filter, "Terrain", ObjectFilterFlags.Terrain);

		if (showChara) {
			ImGui.TableSetColumnIndex(1);
			DrawFilterFlag(filter, "Human", ObjectFilterFlags.Human);
			DrawFilterFlag(filter, "DemiHuman", ObjectFilterFlags.DemiHuman);
			DrawFilterFlag(filter, "Monster", ObjectFilterFlags.Monster);
			DrawFilterFlag(filter, "Weapon", ObjectFilterFlags.Weapon);
		}

		ImGui.EndTable();
		ImGui.EndPopup();
	}

	private void DrawFilterFlag(ObjectFilters filter, string label, ObjectFilterFlags flag) {
		var value = filter.Flags.HasFlag(flag);
		if (ImGui.Checkbox(label, ref value))
			filter.Flags ^= flag;
	}
}
