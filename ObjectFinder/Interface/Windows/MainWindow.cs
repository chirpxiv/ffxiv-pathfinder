using System;
using System.Linq;
using System.Collections.Generic;

using Dalamud.Interface.Windowing;
using ObjectType = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.ObjectType;

using ImGuiNET;

using ObjectFinder.Objects;
using ObjectFinder.Objects.Data;
using ObjectFinder.Services.Core.Attributes;

namespace ObjectFinder.Interface.Windows; 

[LocalService]
public class MainWindow : Window, IDisposable {
	private readonly ObjectService _objects;

	private IObjectClient? _client;
    
	public MainWindow(ObjectService _objects) : base("Object Finder") {
		this._objects = _objects;
	}
	
	// UI draw

	private IObjectClient GetClient()
		=> this._client ??= this._objects.CreateClient();

	public override void Draw() {
        DrawObjectTable();
	}

	private void DrawObjectTable() {
		ImGui.BeginChildFrame(0x0B75, ImGui.GetContentRegionAvail());
        
		ImGui.BeginTable("##ObjectSearchTable.11234", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable | ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable);
		
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
				3 => 0
			};
		});
	}

	public override void OnClose() {
		Dispose();
	}
	
	// Disposal

	public void Dispose() {
		this._client?.Dispose();
		this._client = null;
	}
}
