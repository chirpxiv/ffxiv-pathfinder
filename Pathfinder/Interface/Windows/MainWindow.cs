using System;
using System.Linq;
using System.Collections.Generic;

using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ObjectType = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.ObjectType;

using ImGuiNET;

using Pathfinder.Config;
using Pathfinder.Config.Data;
using Pathfinder.Objects;
using Pathfinder.Objects.Data;
using Pathfinder.Services.Core.Attributes;

namespace Pathfinder.Interface.Windows; 

[LocalService]
public class MainWindow : Window, IDisposable {
	private readonly ConfigService _config;
	private readonly ObjectService _objects;

	private IObjectClient? _client;
	
	public MainWindow(ConfigService _config, ObjectService _objects) : base(
		"Pathfinder"
	) {
		this._config = _config;
		this._objects = _objects;

		var displaySize = ImGui.GetIO().DisplaySize;
		this.Size = displaySize * 0.325f;
		this.SizeCondition = ImGuiCond.FirstUseEver;
		this.SizeConstraints = new WindowSizeConstraints {
			MinimumSize = displaySize * 0.1f,
			MaximumSize = displaySize
		};
	}
	
	// Object access
	
	private IObjectClient GetClient()
		=> this._client ??= this._objects.CreateClient();
	
	// UI draw

	private const string FilterPopupId = "ObjectFilterPopup";

	public override void Draw() {
		var config = this._config.Get();
		
		DrawButtons();
		DrawObjectTable();
		DrawPopups(config);
	}
	
	// Top window buttons

	private void DrawButtons() {
		if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Filter, "Filters"))
			ImGui.OpenPopup(FilterPopupId);
	}
	
	// Results table

	private void DrawObjectTable() {
		ImGui.BeginChildFrame(0x0B75, ImGui.GetContentRegionAvail());
		
		ImGui.BeginTable("##ObjectSearchTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable);
		
		var avail = ImGui.GetContentRegionAvail().X;
		ImGui.TableSetupColumn("Distance", ImGuiTableColumnFlags.DefaultSort, avail * 0.125f);
		ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.None, avail * 0.175f);
		ImGui.TableSetupColumn("Paths", ImGuiTableColumnFlags.None, avail * 0.7f);
		
		var objects = GetClient().GetObjects().ToList();
		SortTable(ImGui.TableGetSortSpecs().Specs, objects);
		
		ImGui.TableHeadersRow();

		foreach (var info in objects) {
			ImGui.TableNextRow();
			
			ImGui.TableSetColumnIndex(0);
			ImGui.Text($"{info.Distance:0.00}");
			
			ImGui.TableSetColumnIndex(1);
			ImGui.Text(GetItemTypeString(info));

			ImGui.TableSetColumnIndex(2);
			foreach (var path in info.ResourcePaths)
				ImGui.Text(path);
		}
		
		ImGui.EndTable();
		
		ImGui.EndChildFrame();
	}

	private string GetItemTypeString(ObjectInfo info) => info.Type switch {
		ObjectType.CharacterBase => info.ModelType.ToString(),
		var type => type.ToString()
	};

	private void SortTable(ImGuiTableColumnSortSpecsPtr sort, List<ObjectInfo> list) {
		var sortDir = sort.SortDirection == ImGuiSortDirection.Ascending ? -1 : 1;
		list.Sort((a, b) => {
			return sort.ColumnIndex switch {
				0 => a.Distance < b.Distance ? sortDir : -sortDir,
				1 => string.Compare(
					GetItemTypeString(a),
					GetItemTypeString(b),
						StringComparison.Ordinal
					) * sortDir,
				2 => string.Compare(
						string.Join(' ', a.ResourcePaths),
						string.Join(' ', b.ResourcePaths),
						StringComparison.Ordinal
					) * sortDir,
				_ => 0
			};
		});
	}
	
	// Popups

	private void DrawPopups(ConfigFile config) {
		try {
			ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, ImGui.GetStyle().WindowRounding);
			DrawFilters(config);
		} finally {
			ImGui.PopStyleVar();
		}
	}
	
	private void DrawFilters(ConfigFile config) {
		if (!ImGui.BeginPopup(FilterPopupId)) return;

		var filter = config.Filters;
		var chara = filter.Flags.HasFlag(ObjectFilterFlags.Chara);

		ImGui.BeginTable("ObjectTypeTable", 2, ImGuiTableFlags.NoSavedSettings);
		ImGui.TableSetupColumn("Object Types");
		ImGui.TableSetupColumn("Character Models", chara ? ImGuiTableColumnFlags.None : ImGuiTableColumnFlags.Disabled);
		
		ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, 0);
		ImGui.TableHeadersRow();
		ImGui.PopStyleColor();
		ImGui.TableNextRow();

		ImGui.TableSetColumnIndex(0);
		DrawFilterFlag(filter, "BgObject", ObjectFilterFlags.BgObject);
        DrawFilterFlag(filter, "Character", ObjectFilterFlags.Chara);
		DrawFilterFlag(filter, "Terrain", ObjectFilterFlags.Terrain);

		if (chara) {
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
	
	// Window close

	public override void OnClose() {
		Dispose();
	}
	
	// Disposal

	public void Dispose() {
		this._client?.Dispose();
		this._client = null;
	}
}
